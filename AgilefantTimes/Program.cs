#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using AgilefantTimes.API;
using AgilefantTimes.Output;

#endregion

namespace AgilefantTimes
{
    public static class Program
    {
        /// <summary>
        /// Main entry point for the application.
        /// </summary>
        /// <param name="args">CLI options.</param>
        public static void Main(string[] args)
        {
            try
            {
                var config = Config.Load("aftimes.conf");
                ParseOptions(args, ref config);

                var session = AgilefantLogin.PerformLogin(config.Username, config.Password);
                var users = AgilefantUser.GetAgilefantUsers(ref session);
                var backlogs = AgilefantBacklog.GetAgilefantBacklogs(config.TeamNumber, ref session);
                var sprints = AgilefantSprint.GetAgilefantSprints(backlogs[0].Id, ref session);

                string sprintName;
                int sprintId;
                GetSprint(config, sprints, out sprintId, out sprintName);

                var hours = (from user in users
                             let tasks = AgilefantTime.GetAgilefantTime(config.TeamNumber, backlogs[0].Id, sprintId, user.Id, ref session)
                             select new JsonOutputTime((config.DisplayUsercode ? user.UserCode : user.Name), tasks)).ToList();
                var jsonOutput = new JsonOutput(backlogs[0].Name, sprintName, hours);
                var jsonPrinter = new JsonPrinter();
                jsonPrinter.WriteLine(jsonOutput.ToJson());
            }
            catch (Exception e)
            {
                if (e is OptionException)
                {
                    var name = AppDomain.CurrentDomain.FriendlyName;
                    Console.Error.WriteLine(name + ": " + e.Message);
                }
                else
                {
                    Console.Error.WriteLine("An error occured at runtime:\n" + e.Message);
                }
#if DEBUG
                throw;
#endif
            }

#if DEBUG
            Console.ReadKey();
#endif
        }

        /// <summary>
        /// Gets a sprint ID and name, based on current configuration.
        /// </summary>
        /// <param name="config">Current configuration to use.</param>
        /// <param name="sprintPool">Pool of sprints to select from.</param>
        /// <param name="sprintId">The result sprint ID.</param>
        /// <param name="sprintName">The result sprint name.</param>
        private static void GetSprint(Config config, AgilefantSprint[] sprintPool, out int sprintId, out string sprintName)
        {
            var spId = -1;
            var spName = "Unknown";
            if (config.SprintNumber < 0)
            {
                var currentDate = DateTime.Now.Date;
                foreach (var sprint in sprintPool.Where(sprint => sprint.StartDate <= currentDate && sprint.EndDate >= currentDate))
                {
                    spId = sprint.Id;
                    spName = sprint.Name;
                    break;
                }
            }

            if (spId < 0)
            {
                if (config.SprintNumber <= 0) config.SprintNumber = 1;
                spId = (from agilefantSprint in sprintPool
                            where agilefantSprint.Name.Contains(config.SprintNumber.ToString())
                            select agilefantSprint.Id).First();
                sprintId = spId;
                sprintName = (from sprint in sprintPool
                              where sprint.Id == spId
                              select sprint.Name).First();
                return;
            }
            sprintId = spId;
            sprintName = spName;
        }

        /// <summary>
        /// Parses command line options.
        /// </summary>
        /// <param name="args">CLI arguments to use.</param>
        /// <param name="config">Configuration file loaded.</param>
        /// <exception cref="OptionException">If there was an error parsing command line options.</exception>
        private static void ParseOptions(IEnumerable<string> args, ref Config config)
        {
            var c = config;
            var displayHelp = false;
            var options = new OptionSet
                {
                    {
                        "sprint|s", "The sprint you want to use", s =>
                        {
                            int result;
                            int.TryParse(s, out result);
                            c.SprintNumber = result;
                        }
                    },
                    {
                        "help|h|?", "Displays help", s =>
                        {
                            displayHelp = true;
                        }
                    }
                };
            options.ParseExceptionally(args);
            if (displayHelp) ShowHelp(options);
        }

        /// <summary>
        /// Shows CLI option help.
        /// </summary>
        /// <param name="p">The option set to show help for.</param>
        private static void ShowHelp(OptionSet p)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);

            p.ShowHelp("Get Agilefant sprint times.", "{appName} [OPTION]...",
                       "If no options are specified defaults will be used.",
                       null,
                       "Written by Matthew Knox and contributors.",
                       "Version:\t" + fvi.ProductVersion + " " + ((Environment.Is64BitProcess) ? "x64" : "x32") +
                       "\nCLR Version:\t" + Environment.Version +
                       "\nOS Version:\t" + Environment.OSVersion.VersionString +
                       "\nReport {appName} bugs and above information to the bug tracker at\n" +
                       "<https://github.com/mrkno/AgilefantTimes>",
                       "Copyright © " + DateTime.Now.Year + " Matthew Knox and contributors.\n"
                       + "The MIT License (MIT) <http://opensource.org/licenses/MIT>\n"
                       + "This is free software: you are free to change and redistribute it.\n"
                       + "There is NO WARRANTY, to the extent permitted by law.",
                       false
            );
#if DEBUG
            Console.ReadKey();
#endif
            Environment.Exit(0);
        }
    }
}
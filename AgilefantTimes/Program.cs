#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using AgilefantTimes.API.Agilefant;
using AgilefantTimes.API.Restful;
using AgilefantTimes.Output;

#endregion

namespace AgilefantTimes
{
    public static class Program
    {
        private static Config _config;

        /// <summary>
        /// Main entry point for the application.
        /// </summary>
        /// <param name="args">CLI options.</param>
        public static void Main(string[] args)
        {
            try
            {
                if (!Config.TryLoad("aftimes.conf", out _config) && args.Length == 0)
                {
                    throw new Exception("Could not load configuration file.");
                }
                ParseOptions(args, ref _config);

                var server = new RestApiClient(_config, _config.Port, _config.WebRoot);
                Console.Write("Starting server... ");
                server.Start();
                Console.WriteLine("complete.");

                //Console.ReadKey();

                //server.Stop();
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
                    Console.Error.WriteLine("An error occured at runtime: \r\n" + e.Message);
                }

                if (_config.DebugMode)
                {
                    Console.Error.WriteLine(e.StackTrace);
                    #if DEBUG
                    throw;
                    #endif
                }
            }

            if (_config.DebugMode) Console.ReadKey();
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
                    { "username|u", "{Username} to login with", s => c.Username = s },
                    { "password|p", "{Password} to login with", s => c.Password = s },
                    { "team|t", "Default team {number} to retreive", s => c.TeamNumber = int.Parse(s) },
                    { "sprint|s", "Default sprint {number} to retrieve", s => c.SprintNumber = int.Parse(s) },
                    { "port|p", "{Port} number to host the server on. Defaults to 80.", s => c.Port = int.Parse(s) },
                    { "web|w", "{Directory} that non-API served files are located. Defaults to ./www/", s => c.WebRoot = s },
                    { "usercode|c", "Use usercodes instead of names", s => c.DisplayUsercode = true },
                    { "debug|d", "Enable debugging mode", s => c.DebugMode = true },
                    { "help|h|?", "Displays help", s => displayHelp = true }
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
                       "If no options are specified defaults from your config file will be used.",
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
            if (_config.DebugMode) Console.ReadKey();
            Environment.Exit(0);
        }
    }
}
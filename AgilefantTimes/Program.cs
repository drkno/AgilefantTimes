#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using AgilefantTimes.API.Restful;

#endregion

namespace AgilefantTimes
{
    public static class Program
    {
        private static Config _config;
        private static RestApiClient _server;

        /// <summary>
        /// Main entry point for the application.
        /// </summary>
        /// <param name="args">CLI options.</param>
        public static void Main(string[] args)
        {
            try
            {
                Console.WriteLine(
                    "____ ____ _ _    ____ ____ ____ _  _ ___   ___ _ _  _ ____ ____ \n" +
                    "|__| | __ | |    |___ |___ |__| |\\ |  |     |  | |\\/| |___ [__  \n" +
                    "|  | |__] | |___ |___ |    |  | | \\|  |     |  | |  | |___ ___] \n" +
                    "----------------------------------------------------------------");
                                                              
                Logger.Log("Loading configuration file...", LogLevel.Write);
                if (!Config.TryLoad("aftimes.conf", out _config) && args.Length == 0)
                {
                    throw new Exception("Could not load configuration file.");
                }
                Logger.Log("Parsing options...", LogLevel.Write);
                ParseOptions(args, ref _config);

                Logger.Enabled = _config.DebugMode;
                Console.CancelKeyPress += Console_CancelKeyPress;

                _server = new RestApiClient(_config, _config.Port, _config.WebRoot);
                Logger.Log("Starting server...", LogLevel.Write);
                _server.Start();
                Logger.Log("Server has started. Press CTRL+C (^C) to terminate.", LogLevel.Write);
            }
            catch (Exception e)
            {
                if (e is OptionException)
                {
                    var name = AppDomain.CurrentDomain.FriendlyName;
                    Logger.Log(name + ": " + e.Message, LogLevel.Write);
                }
                else
                {
                    Logger.Log("An error occured at runtime: \r\n" + e.Message, LogLevel.Write);
                }

                
                if (_config.DebugMode)
                {
                    e.StackTrace.Log(LogLevel.Error);
                    throw;
                }
            }

            if (_config.DebugMode) Console.ReadKey();
        }

        /// <summary>
        /// Attempt to safely shutdown the _server when CTRL-C is received.
        /// </summary>
        /// <param name="sender">Caller object.</param>
        /// <param name="e1">Caller event arguments.</param>
        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e1)
        {
            var exitCode = 0;
            try
            {
                Logger.Log("Server shutting down...", LogLevel.Write);
                _server.Stop();
                Logger.Log("Server shutdown complete.", LogLevel.Write);
                
            }
            catch (Exception e)
            {
                if (_config.DebugMode)
                {
                    Logger.Log("Clean shutdown failed. Aborting...", LogLevel.Write);
                }
                else
                {
                    Logger.Log(e.StackTrace, LogLevel.Error);
                }
                exitCode = -1;
            }

            if (_config.DebugMode)
            {
                Logger.Log("Press any key to terminate the program.", LogLevel.Write);
                Console.ReadKey();
            }
            Logger.ShouldLog = false;
            Environment.Exit(exitCode);
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
                    { "port|p", "{Port} number to host the _server on. Defaults to 80.", s => c.Port = int.Parse(s) },
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
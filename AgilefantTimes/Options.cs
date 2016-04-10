/*
 * Knox.Options
 * This is a Mono.Options semi-compatible library for managing CLI
 * arguments and displaying help text for a program. Created as
 * Mono.Options has an issue and was requiring significant
 * modification to meet my needs. It was quicker to write a new
 * version that supported a similar API than to fix the origional.
 * 
 * Copyright © Matthew Knox, Knox Enterprises 2014-Present.
 * This code is avalible under the MIT license in the state
 * that it was avalible on 05/11/2014 from
 * http://opensource.org/licenses/MIT .
*/

#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

#endregion

namespace AgilefantTimes
{
    /// <summary>
    /// Set of CLI interface
    /// </summary>
    public class OptionSet : IEnumerable
    {
        /// <summary>
        /// Index of individual options in option set.
        /// </summary>
        private readonly Dictionary<string, int> _lookupDictionary = new Dictionary<string, int>();
        /// <summary>
        /// List of options contained in this option set.
        /// </summary>
        private readonly List<Option> _options = new List<Option>();

        private static readonly char[] ArgumentSeparators = { '=', ':' };

        private static OptionStyle OptionsStyle { get; } = OptionStyle.Nix;

        /// <summary>
        /// Option prefixes for use with various option styles.
        /// </summary>
        private static readonly string[] OptionsPrefixes = { "-", "--", "/", "/" };

        /// <summary>
        /// Enumerator of this OptionSet
        /// </summary>
        /// <returns>The enumerator</returns>
        public IEnumerator GetEnumerator()
        {
            return _options.GetEnumerator();
        }

        /// <summary>
        /// Add a cli option to the set.
        /// </summary>
        /// <param name="cliOptions">The options to associate this option with.</param>
        /// <param name="description">Description of this option.</param>
        /// <param name="func">Action to run when this option is specified.</param>
        /// <param name="conflictSilent">If a cli option has already been specified by a previous option
        /// handle the error silently rather than throwing an exception.</param>
        public void Add(string cliOptions, string description, Action<string> func, bool conflictSilent = true)
        {
            var option = new Option(cliOptions, description, func, OptionsStyle);
            _options.Add(option);
            var ind = _options.Count - 1;
            foreach (var opt in option.Arguments)
            {
                try
                {
                    _lookupDictionary.Add(opt, ind);    // add reference for quick lookup
                }
                catch (Exception e)
                {
                    if (conflictSilent)
                    {
                        continue;
                    }
                    var opt1 = opt;     // remove all instances of this option, as we want to have a good options state
                    foreach (var op in option.Arguments.TakeWhile(op => op != opt1))
                    {
                        _lookupDictionary.Remove(op);
                    }
                    _options.Remove(option);
                    throw new OptionException("Option " + opt + " already specified for another option.", e);
                }
            }
        }

        /// <summary>
        /// Parses a set of arguments into the option equivilents and calls
        /// the actions of those options.
        /// </summary>
        /// <param name="arguments">Arguments to parse.</param>
        /// <returns>List of arguments that parsing failed for.</returns>
        private List<string> Parse(IEnumerable<string> arguments)
        {
            var optionsInError = new List<string>();
            var temp = new List<string>();
            var readForOption = false;
            var optionRead = -1;

            var enumerable = arguments.ToList();

            for (var i = 0; i <= enumerable.Count; i++)
            {
                if (i == enumerable.Count || enumerable[i].StartsWith(OptionsPrefixes[(int)OptionsStyle]) || !readForOption)
                {
                    if (readForOption)
                    {
                        try
                        {
                            var arg = temp.Aggregate("", (current, t) => current + (t + " "));
                            arg = arg.Trim();
                            if (arg.Length == 0 && _options[optionRead].ExpectsArguments)
                            {
                                throw new OptionException("Option expects arguments and none provided.");
                            }
                            try
                            {
                                _options[optionRead].Action(arg);
                            }
                            catch (Exception)
                            {
                                throw new OptionException("Invalid value for option.");
                            }
                        }
                        catch (OptionException)
                        {
                            optionsInError.Add(enumerable[i - 1 - temp.Count]);
                            optionsInError.AddRange(temp);
                        }
                        finally
                        {
                            temp.Clear();
                        }
                    }

                    if (i == enumerable.Count)
                    {
                        continue;
                    }

                    try
                    {
                        var i1 = i;
                        foreach (var separator in ArgumentSeparators.Where(separator => enumerable[i1].Contains(separator)))
                        {
                            enumerable.RemoveAt(i);
                            enumerable.InsertRange(i, enumerable[i].Split(separator));
                            break;
                        }

                        var ind = _lookupDictionary[enumerable[i]];
                        optionRead = ind;
                        readForOption = true;
                    }
                    catch (Exception)
                    {
                        optionsInError.Add(enumerable[i]);
                        readForOption = false;
                    }
                }
                else
                {
                    temp.Add(enumerable[i]);
                }
            }
            return optionsInError;
        }

        /// <summary>
        /// Parses a set of arguments into the option equivilents and calls
        /// the actions of those options.
        /// </summary>
        /// <param name="arguments">Arguments to parse.</param>
        /// <exception cref="OptionException">On invalid options.</exception>
        public void ParseExceptionally(IEnumerable<string> arguments)
        {
            var result = Parse(arguments);
            if (result.Count <= 0) return;
            var options = "";
            options = result.Aggregate(options, (current, r) => current + (" " + r));
            throw new OptionException("An error occured with option" + (result.Count > 1 ? "s" : "") + " " + options);
        }

        /// <summary>
        /// Style of options to use.
        /// </summary>
        private enum OptionStyle
        {
            Nix = 0
        }

        /// <summary>
        /// Represents an individual option of an OptionSet
        /// </summary>
        private class Option
        {
            /// <summary>
            /// Creates a new option.
            /// </summary>
            /// <param name="options">Cli arguments that use this option.</param>
            /// <param name="description">Description of this option.</param>
            /// <param name="func">Action to perform when this option is specified.</param>
            /// <param name="style">Style of option to use.</param>
            /// <param name="optionalArgs">If this is true, arguments will be treated as non compulsary ones.</param>
            public Option(string options, string description, Action<string> func, OptionStyle style, bool optionalArgs = false)
            {
                Action = func;
                var spl = options.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                Arguments = spl.Select(s => OptionsPrefixes[(int)style + ((s.Length == 1) ? 0 : 1)] + s).ToArray();
                var opts = new List<string>();
                foreach (Match match in Regex.Matches(description, "{[^}]*}"))
                {
                    var val = match.Value.Substring(1, match.Length - 2);
                    opts.Add(val.ToUpper());
                    description = description.Substring(0, match.Index) + val +
                                  description.Substring(match.Index + match.Length);
                }
                Description = description;
                Options = opts.ToArray();
                ExpectsArguments = opts.Count > 0 && !optionalArgs;
            }

            /// <summary>
            /// Arguments that this option provides.
            /// </summary>
            public string[] Arguments { get; }
            /// <summary>
            /// Words to be displayed as options.
            /// </summary>
            public string[] Options { get; }
            /// <summary>
            /// Specifies if this option requires arguments to be passed to it.
            /// </summary>
            public bool ExpectsArguments { get; }
            /// <summary>
            /// Description of this option.
            /// </summary>
            public string Description { get; }
            /// <summary>
            /// Action to perform when this option is specified.
            /// </summary>
            public Action<string> Action { get; }
        }

        #region Help Text
        /// <summary>
        /// Print help.
        /// </summary>
        /// <param name="programNameDescription">Decription to accompany the program name.</param>
        /// <param name="programSynopsis">Synopsis section of the help.</param>
        /// <param name="optionDescriptionPrefix">Text before options.</param>
        /// <param name="optionDescriptionPostfix">Text after options.</param>
        /// <param name="programAuthor">Author section of the help.</param>
        /// <param name="programReportBugs">Bugs section of the help.</param>
        /// <param name="programCopyright">Copyright section of the help.</param>
        /// <param name="confirm">Halt before continuing execution after printing.</param>
        public void ShowHelp(string programNameDescription,
            string programSynopsis,
            string optionDescriptionPrefix,
            string optionDescriptionPostfix,
            string programAuthor,
            string programReportBugs,
            string programCopyright,
            bool confirm)
        {
            WriteProgramName(programNameDescription);
            WriteProgramSynopsis(programSynopsis);
            WriteOptionDescriptions(this, optionDescriptionPrefix, optionDescriptionPostfix);
            WriteProgramAuthor(programAuthor);
            WriteProgramReportingBugs(programReportBugs);
            WriteProgramCopyrightLicense(programCopyright);

            if (confirm)
            {
                Console.ReadKey(true);
            }
        }

        /// <summary>
        /// Print program name and description.
        /// </summary>
        /// <param name="description">Description to print.</param>
        private static void WriteProgramName(string description)
        {
            var origColour = Console.ForegroundColor;
            var appName = AppDomain.CurrentDomain.FriendlyName;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("NAME");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine('\t' + appName + " - " + description + '\n');
            Console.ForegroundColor = origColour;
        }

        /// <summary>
        /// Print the program synopsis.
        /// </summary>
        /// <param name="synopsis">Synopsis to print.</param>
        private static void WriteProgramSynopsis(string synopsis)
        {
            var origColour = Console.ForegroundColor;
            var appName = AppDomain.CurrentDomain.FriendlyName;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("SYNOPSIS");
            Console.ForegroundColor = ConsoleColor.Gray;
            synopsis = synopsis.Replace("{appName}", appName);
            Console.WriteLine('\t' + synopsis + '\n');
            Console.ForegroundColor = origColour;
        }

        /// <summary>
        /// Print the program author.
        /// </summary>
        /// <param name="authorByString">Author string to print.</param>
        private static void WriteProgramAuthor(string authorByString)
        {
            var origColour = Console.ForegroundColor;
            var appName = AppDomain.CurrentDomain.FriendlyName;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("AUTHOR");
            Console.ForegroundColor = ConsoleColor.Gray;
            authorByString = authorByString.Replace("{appName}", appName);
            Console.WriteLine('\t' + authorByString + '\n');
            Console.ForegroundColor = origColour;
        }

        /// <summary>
        /// Print the program reporting bugs section.
        /// </summary>
        /// <param name="reportString">Report bugs string.</param>
        private static void WriteProgramReportingBugs(string reportString)
        {
            var origColour = Console.ForegroundColor;
            var appName = AppDomain.CurrentDomain.FriendlyName;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("REPORTING BUGS");
            Console.ForegroundColor = ConsoleColor.Gray;
            reportString = reportString.Replace("{appName}", appName);
            var spl = reportString.Split(new[] { "\n", "\r\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var s in spl)
            {
                Console.WriteLine('\t' + s);
            }
            Console.WriteLine();
            Console.ForegroundColor = origColour;
        }

        /// <summary>
        /// Print the program copyright license.
        /// </summary>
        /// <param name="copyrightLicense">Copyright license text.</param>
        private static void WriteProgramCopyrightLicense(string copyrightLicense)
        {
            var origColour = Console.ForegroundColor;
            var appName = AppDomain.CurrentDomain.FriendlyName;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("COPYRIGHT");
            Console.ForegroundColor = ConsoleColor.Gray;
            copyrightLicense = copyrightLicense.Replace("{appName}", appName);
            var spl = copyrightLicense.Split(new[] { "\n", "\r\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var s in spl)
            {
                Console.WriteLine('\t' + s);
            }
            Console.WriteLine();
            Console.ForegroundColor = origColour;
        }

        /// <summary>
        /// Prints all the options in an OptionsSet and prefix/postfix text for the description.
        /// </summary>
        /// <param name="os">OptionsSet to use options from.</param>
        /// <param name="prefixText">Text to print before options.</param>
        /// <param name="postText">Text to print after options.</param>
        private static void WriteOptionDescriptions(OptionSet os, string prefixText, string postText)
        {
            var origColour = Console.ForegroundColor;
            var appName = AppDomain.CurrentDomain.FriendlyName;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("DESCRIPTION");
            Console.ForegroundColor = ConsoleColor.Gray;
            if (!string.IsNullOrWhiteSpace(prefixText))
            {
                prefixText = prefixText.Replace("{appName}", appName);
                var spl = prefixText.Split(new[] { "\n", "\r\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var s in spl)
                {
                    Console.WriteLine('\t' + s);
                }
            }

            var buffWid = Console.BufferWidth;
            foreach (var p in os._options)
            {
                Console.Write('\t');
                for (var j = 0; j < p.Arguments.Length; j++)
                {
                    Console.Write(p.Arguments[j]);
                    if (j + 1 != p.Arguments.Length)
                    {
                        Console.Write(", ");
                    }
                    else
                    {
                        if (p.Options.Length > 0)
                        {
                            Console.Write('\t');
                            foreach (var t in p.Options)
                            {
                                Console.Write(" [" + t + "]");
                            }
                        }

                        Console.WriteLine();
                    }
                }

                Console.Write("\t\t");
                var len = buffWid - Console.CursorLeft;

                foreach (var l in p.Description.Split(new[] { "\n", "\r\n", "\r" }, StringSplitOptions.RemoveEmptyEntries)
                    )
                {
                    var lenP = 0;
                    foreach (var w in l.Split(' '))
                    {
                        var word = w;

                        if (lenP != 0 && (lenP + word.Length + 1) > len)
                        {
                            if (lenP != len) Console.Write("\n");
                            Console.Write("\t\t");
                            lenP = 0;
                        }
                        else if (lenP != 0)
                        {
                            word = ' ' + word;
                        }
                        Console.Write(word);
                        lenP += word.Length;
                    }
                    if (lenP != len) Console.Write("\n");
                    Console.Write("\t\t");
                }
                Console.WriteLine();
            }

            if (!string.IsNullOrWhiteSpace(postText))
            {
                postText = postText.Replace("{appName}", appName);
                var spl = postText.Split(new[] { "\n", "\r\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var s in spl)
                {
                    Console.WriteLine('\t' + s);
                }
            }
            Console.WriteLine();
            Console.ForegroundColor = origColour;
        }
        #endregion
    }

    /// <summary>
    /// Exception that is thrown when there is an error with the options specified.
    /// </summary>
    [Serializable]
    public class OptionException : Exception
    {
        /// <summary>
        /// Create a new OptionException.
        /// </summary>
        /// <param name="errorText">The description of this exception.</param>
        public OptionException(string errorText)
            : base(errorText)
        {
        }

        /// <summary>
        /// Create a new OptionException.
        /// </summary>
        /// <param name="errorText">The description of this exception.</param>
        /// <param name="innerException">The inner exception that caused this one to occur.</param>
        public OptionException(string errorText, Exception innerException)
            : base(errorText, innerException)
        {
        }
    }
}
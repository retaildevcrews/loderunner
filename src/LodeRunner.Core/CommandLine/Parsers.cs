// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.Linq;

namespace LodeRunner.Core.CommandLine
{
    /// <summary>
    /// Represents the Parsers implementation.
    /// </summary>
    public static class Parsers
    {
        /// <summary>
        /// Parses the string command line arg.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns>The Argument Value.</returns>
        public static string ParseString(ArgumentResult result)
        {
            string name = result.Parent?.Symbol.Name.ToUpperInvariant().Replace('-', '_');
            if (string.IsNullOrWhiteSpace(name))
            {
                result.ErrorMessage = "result.Parent is null";
                return null;
            }

            string val;

            if (result.Tokens.Count == 0)
            {
                string env = Environment.GetEnvironmentVariable(name);

                if (string.IsNullOrWhiteSpace(env))
                {
                    if (name == "SERVER")
                    {
                        result.ErrorMessage = $"--{result.Parent.Symbol.Name} is required";
                    }

                    return null;
                }
                else
                {
                    val = env.Trim();
                }
            }
            else
            {
                val = result.Tokens[0].Value.Trim();
            }

            if (string.IsNullOrWhiteSpace(val))
            {
                if (name == "SERVER")
                {
                    result.ErrorMessage = $"--{result.Parent.Symbol.Name} is required";
                }

                return null;
            }
            else if (val.Length < 3)
            {
                result.ErrorMessage = $"--{result.Parent.Symbol.Name} must be at least 3 characters";
                return null;
            }
            else if (val.Length > 100)
            {
                result.ErrorMessage = $"--{result.Parent.Symbol.Name} must be 100 characters or less";
            }

            return val;
        }

        /// <summary>
        /// Parses the string list command line arg (--files).
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns>The Argument Value.</returns>
        public static List<string> ParseStringList(ArgumentResult result)
        {
            string name = result.Parent?.Symbol.Name.ToUpperInvariant().Replace('-', '_');
            if (string.IsNullOrWhiteSpace(name))
            {
                result.ErrorMessage = "result.Parent is null";
                return null;
            }

            List<string> val = new ();

            if (result.Tokens.Count == 0)
            {
                string env = Environment.GetEnvironmentVariable(name);

                if (string.IsNullOrWhiteSpace(env))
                {
                    result.ErrorMessage = $"--{result.Argument.Name} is a required parameter";
                    return null;
                }

                string[] files = env.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                foreach (string f in files)
                {
                    val.Add(f.Trim());
                }
            }
            else
            {
                for (int i = 0; i < result.Tokens.Count; i++)
                {
                    val.Add(result.Tokens[i].Value.Trim());
                }
            }

            return val;
        }

        /// <summary>
        /// Parses the boolean command line arg.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns>The Argument Value.</returns>
        public static bool ParseBool(ArgumentResult result)
        {
            string name = result.Parent?.Symbol.Name.ToUpperInvariant().Replace('-', '_');

            if (string.IsNullOrWhiteSpace(name))
            {
                result.ErrorMessage = "result.Parent is null";
                return false;
            }

            string errorMessage = $"--{result.Parent.Symbol.Name} must be true or false";
            bool val;

            // bool options default to true if value not specified (ie -r and -r true)
            if (result.Parent.Parent.Children.FirstOrDefault(c => c.Symbol.Name == result.Parent.Symbol.Name) is OptionResult res &&
                !res.IsImplicit &&
                result.Tokens.Count == 0)
            {
                return true;
            }

            // nothing to validate
            if (result.Tokens.Count == 0)
            {
                string env = Environment.GetEnvironmentVariable(name);

                if (!string.IsNullOrWhiteSpace(env))
                {
                    if (bool.TryParse(env, out val))
                    {
                        return val;
                    }
                    else
                    {
                        result.ErrorMessage = errorMessage;
                        return false;
                    }
                }

                // default to true
                if (result.Parent.Symbol.Name == "verbose-errors")
                {
                    return true;
                }

                if (result.Parent.Symbol.Name == "verbose" &&
                    result.Parent.Parent.Children.FirstOrDefault(c => c.Symbol.Name == "run-loop") is OptionResult resRunLoop &&
                    !resRunLoop.GetValueOrDefault<bool>())
                {
                    return true;
                }

                return false;
            }

            if (!bool.TryParse(result.Tokens[0].Value, out val))
            {
                result.ErrorMessage = errorMessage;
                return false;
            }

            return val;
        }

        /// <summary>
        /// Parser for integer >= 0.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns>The Argument Value.</returns>
        public static int ParseIntGEZero(ArgumentResult result)
        {
            return ParseInt(result, 0);
        }

        /// <summary>
        /// Parser for integer > 0..
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns>The Argument Value.</returns>
        public static int ParseIntGTZero(ArgumentResult result)
        {
            return ParseInt(result, 1);
        }

        /// <summary>
        /// Parser for integer >=-1.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns>The Argument Value.</returns>
        public static int ParseIntGENegOne(ArgumentResult result)
        {
            return ParseInt(result, -1);
        }

        /// <summary>
        /// Parser for Integer.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="minValue">The minimum value.</param>
        /// <returns>The Argument Value.</returns>
        public static int ParseInt(ArgumentResult result, int minValue)
        {
            string name = result.Parent?.Symbol.Name.ToUpperInvariant().Replace('-', '_');

            if (string.IsNullOrWhiteSpace(name))
            {
                result.ErrorMessage = "result.Parent is null";
                return -1;
            }

            string errorMessage = $"--{result.Parent.Symbol.Name} must be an integer >= {minValue}";
            int val;

            // nothing to validate
            if (result.Tokens.Count == 0)
            {
                string env = Environment.GetEnvironmentVariable(name);

                if (string.IsNullOrWhiteSpace(env))
                {
                    return GetCommandDefaultValues(result);
                }
                else
                {
                    if (!int.TryParse(env, out val) || val < minValue)
                    {
                        result.ErrorMessage = errorMessage;
                        return -1;
                    }

                    return val;
                }
            }

            if (!int.TryParse(result.Tokens[0].Value, out val) || val < minValue)
            {
                result.ErrorMessage = errorMessage;
                return -1;
            }

            // if run-loop is set, check for sleep dependency
            if (result.Parent.Symbol.Name == "sleep" &&
                val == 0 &&
                result.Parent.Parent.Children.FirstOrDefault(c => c.Symbol.Name == "run-loop") is OptionResult res &&
                res.GetValueOrDefault<bool>())
            {
                Console.WriteLine(SystemConstants.CmdLineNoticeSleepValueIgnoredMessage);
                return GetCommandDefaultValues(result);
            }

            return val;
        }

        /// <summary>
        /// Gets default values for command line args.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns>the default value.</returns>
        public static int GetCommandDefaultValues(ArgumentResult result)
        {
            switch (result.Parent.Symbol.Name)
            {
                case "max-errors":
                    return 10;
                case "max-concurrent":
                    return 100;
                case "sleep":
                    // check run-loop
                    if (result.Parent.Parent.Children.FirstOrDefault(c => c.Symbol.Name == "run-loop") is OptionResult res && res.GetValueOrDefault<bool>())
                    {
                        return 1000;
                    }

                    return 0;
                case "duration":
                    // default is -1 to differentiate when duration of 0 is passed
                    return -1;
                case "timeout":
                    return 30;
                default:
                    return 0;
            }
        }
    }
}

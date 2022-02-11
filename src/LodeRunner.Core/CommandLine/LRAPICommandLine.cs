// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using LodeRunner.Core;
using Microsoft.Extensions.Logging;

namespace LodeRunner.API
{
    /// <summary>
    /// Main application class.
    /// </summary>
    public sealed class LRAPICommandLine
    {
        // capture parse errors from env vars
        private static readonly List<string> EnvVarErrors = new ();

        /// <summary>
        /// Build the RootCommand for parsing.
        /// </summary>
        /// <returns>RootCommand.</returns>
        public static RootCommand BuildRootCommand()
        {
            RootCommand root = new ()
            {
                Name = "LodeRunner.API",
                Description = "LodeRunner.API Validation App",
                TreatUnmatchedTokensAsErrors = true,
            };

            // We are getting the port number from appsettings, and using that as default value for rootCommand in case --port is not set.
            // if we cannot read it from appsettings. we use DefaultApiWebHostPort
            int defaultApiPortNumber = AppConfigurationHelper.GetLoadRunnerApiPort(SystemConstants.DefaultApiWebHostPort);

            // add the options
            // first option name must be formatted `--[a-z]+`
            // rest of the option names must be formatted `-[a-z]`
            root.AddOption(GetOption(new string[] { "--url-prefix" }, "URL prefix for ingress mapping", string.Empty));
            root.AddOption(GetOption(new string[] { "--port" }, "Listen Port", defaultApiPortNumber, 1, (64 * 1024) - 1));
            root.AddOption(GetOption(new string[] { "--retries" }, "Cosmos 429 retries", 10, 0));
            root.AddOption(GetOption(new string[] { "--timeout" }, "Request timeout", 10, 1));
            root.AddOption(GetOption(new string[] { "--secrets-volume", "-v" }, "Secrets Volume Path", "secrets"));
            root.AddOption(GetOption(new string[] { "--log-level", "-l" }, "Log Level", LogLevel.Error));
            root.AddOption(GetOption(new string[] { "--request-log-level", "-q" }, "Request Log Level", LogLevel.Information));

            // validate dependencies
            root.AddValidator(ValidateDependencies);

            return root;
        }

        // insert env vars as default
        private static Option GetOption<T>(string[] names, string description, T defaultValue, int? minValue = null, int? maxValue = null)
        {
            CheckOptionDetails(names, description);

            // get env variable from formatted first option name i.e. --log-level => LOG_LEVEL
            string key = names[0][2..].Trim().ToUpperInvariant().Replace('-', '_');
            string env = Environment.GetEnvironmentVariable(key);

            T value = GetOptionValue(key, env, defaultValue);

            Option<T> opt = new (names, () => value, description);

            AddIntOptionValidators(opt, minValue, maxValue);

            return opt;
        }

        private static void CheckOptionDetails(string[] names, string description)
        {
            // check has option description
            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentNullException(nameof(description));
            }

            // check option names exist
            if (names == null || names.Length < 1)
            {
                throw new ArgumentNullException(nameof(names));
            }

            // check first option name formatting requirements
            if (names[0].Trim().Length < 4)
            {
                throw new ArgumentNullException(nameof(names), "Invalid command line parameter at position 0");
            }

            // check rest of option names formatting requirements
            for (int i = 1; i < names.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(names[i]) ||
                    names[i].Length != 2 ||
                    names[i][0] != '-')
                {
                    throw new ArgumentException($"Invalid command line parameter at position {i}", nameof(names));
                }
            }
        }

        private static T GetOptionValue<T>(string key, string env, T defaultValue)
        {
            T value = defaultValue;

            // set default to environment value if not set
            if (!string.IsNullOrWhiteSpace(env))
            {
                if (defaultValue.GetType().IsEnum)
                {
                    if (Enum.TryParse(defaultValue.GetType(), env, true, out object result))
                    {
                        value = (T)result;
                    }
                    else
                    {
                        EnvVarErrors.Add($"Environment variable {key} is invalid");
                    }
                }
                else if (typeof(T) == typeof(int))
                {
                    if (!int.TryParse(env, out int temp))
                    {
                        EnvVarErrors.Add($"Environment variable {key} is invalid");
                    }

                    value = (T)Convert.ChangeType(temp, typeof(T));
                }
                else
                {
                    try
                    {
                        value = (T)Convert.ChangeType(env, typeof(T));
                    }
                    catch
                    {
                        EnvVarErrors.Add($"Environment variable {key} is invalid");
                    }
                }
            }

            return value;
        }

        private static void AddIntOptionValidators<T>(Option<T> opt, int? minValue, int? maxValue)
        {
            if (minValue == null)
            {
                return;
            }

            opt.AddValidator((res) =>
            {
                string s = string.Empty;
                int val;

                try
                {
                    val = (int)res.GetValueOrDefault();

                    if (val < minValue)
                    {
                        s = $"{opt.Name[0]} must be >= {minValue}";
                    }
                }
                catch
                {
                }

                return s;
            });

            if (maxValue != null)
            {
                opt.AddValidator((res) =>
                {
                    string s = string.Empty;
                    int val;

                    try
                    {
                        val = (int)res.GetValueOrDefault();

                        if (val > maxValue)
                        {
                            s = $"{opt.Name[0]} must be <= {maxValue}";
                        }
                    }
                    catch
                    {
                    }

                    return s;
                });
            }
        }

        // validate combinations of parameters
        private static string ValidateDependencies(CommandResult result)
        {
            string msg = string.Empty;

            if (EnvVarErrors.Count > 0)
            {
                msg += string.Join('\n', EnvVarErrors) + '\n';
            }

            try
            {
                // get the values to validate
                string secrets = result.Children.FirstOrDefault(c => c.Symbol.Name == "secrets-volume") is OptionResult secretsRes ? secretsRes.GetValueOrDefault<string>() : string.Empty;
                string urlPrefix = result.Children.FirstOrDefault(c => c.Symbol.Name == "urlPrefix") is OptionResult urlRes ? urlRes.GetValueOrDefault<string>() : string.Empty;

                // validate url-prefix
                if (!string.IsNullOrWhiteSpace(urlPrefix))
                {
                    urlPrefix = urlPrefix.Trim();

                    if (urlPrefix.Length < 2)
                    {
                        msg += "--url-prefix is invalid";
                    }

                    if (!urlPrefix.StartsWith('/'))
                    {
                        msg += "--url-prefix must start with /";
                    }
                }

                if (string.IsNullOrWhiteSpace(secrets))
                {
                    msg += "--secrets-volume cannot be empty\n";
                }
                else
                {
                    try
                    {
                        // validate secrets-volume exists
                        if (!Directory.Exists(secrets))
                        {
                            msg += $"--secrets-volume ({secrets}) does not exist\n";
                        }
                    }
                    catch (Exception ex)
                    {
                        msg += $"--secrets-volume exception: {ex.Message}\n";
                    }
                }
            }
            catch
            {
                // system.commandline will catch and display parse exceptions
            }

            // return error message(s) or string.empty
            return msg;
        }
    }
}

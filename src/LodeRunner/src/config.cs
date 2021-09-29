// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text.Json;
using LodeRunner.Core.Interfaces;

namespace LodeRunner
{
    /// <summary>
    /// Web Validation Test Configuration
    /// </summary>
    public class Config : ILRConfig, ICosmosConfig
    {
        /// <summary>
        /// gets or sets the server / url
        /// </summary>
        public List<string> Server { get; set; }

        /// <summary>
        /// gets or sets the list of files to read
        /// </summary>
        public List<string> Files { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets a value indicating whether to the use Prometheus flag
        /// </summary>
        public bool Prometheus { get; set; }

        /// <summary>
        /// gets or sets the zone to log
        /// </summary>
        public string Zone { get; set; }

        /// <summary>
        /// gets or sets the region to log
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        /// gets or sets the tag to log
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether we should run in a loop
        /// </summary>
        public bool RunLoop { get; set; }

        /// <summary>
        /// gets or sets the sleep time between requests in ms
        /// </summary>
        public int Sleep { get; set; }

        /// <summary>
        /// gets or sets the duration of the test in seconds
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether we should use random tests vs. sequential
        /// </summary>
        public bool Random { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether logs should be verbose
        /// </summary>
        public bool Verbose { get; set; }

        /// <summary>
        /// gets or sets the request time out in seconds
        /// </summary>
        public int Timeout { get; set; } = 30;

        /// <summary>
        /// gets or sets the max concurrent requests
        /// </summary>
        public int MaxConcurrent { get; set; }

        /// <summary>
        /// gets or sets the max errors before the test exits with a non-zero response
        /// </summary>
        public int MaxErrors { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether we should do a dry run
        /// </summary>
        public bool DryRun { get; set; }

        /// <summary>
        /// gets or sets the base url for test files
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// gets or sets the summary generation time in minutes
        /// </summary>
        public int SummaryMinutes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether we should display verbose errors or just the count
        /// </summary>
        public bool VerboseErrors { get; set; }

        /// <summary>
        /// gets or sets the seconds to delay before starting the test
        /// </summary>
        public int DelayStart { get; set; }

        /// <summary>
        /// gets or sets a value indicating whether we should use strict json parsing
        /// </summary>
        public bool StrictJson { get; set; }

        /// <summary>Gets or sets the max retry attempts for cosmos requests.</summary>
        /// <value>The retries.</value>
        public int Retries { get; set; } = 10;

        /// <summary>Gets or sets the cosmos max retry wait time for cosmos requests in seconds.</summary>
        /// <value>Time in seconds</value>
        public int CosmosTimeout { get; set; } = 60;

        /// <summary>Gets or sets the secrets.</summary>
        /// <value>The secrets.</value>
        public ISecrets Secrets { get; set; }

        /// <summary>Gets or sets the secrets volume.</summary>
        /// <value>The secrets volume.</value>
        public string SecretsVolume { get; set; } = "secrets";

        /// <summary>Gets or sets the name of the cosmos.</summary>
        /// <value>The name of the cosmos.</value>
        public string CosmosName { get; set; } = string.Empty;

        /// <summary>
        /// gets or sets the seconds to refresh the http client
        /// </summary>
        public int ClientRefresh { get; set; }

        public int WebHostPort { get; set; } = 8080;

        /// <summary>
        /// Set the default config values
        /// </summary>
        public void SetDefaultValues()
        {
            if (Server != null && Server.Count > 0)
            {
                string s;

                for (int i = 0; i < Server.Count; i++)
                {
                    s = Server[i];

                    // make it easier to pass server value
                    if (!s.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        if (s.StartsWith("localhost", StringComparison.OrdinalIgnoreCase) || s.StartsWith("127.0.0.1", StringComparison.OrdinalIgnoreCase))
                        {
                            Server[i] = $"http://{s}";
                        }
                        else
                        {
                            Server[i] = $"https://{s}.azurewebsites.net";
                        }
                    }
                }
            }

            // add a trailing slash if necessary
            if (!string.IsNullOrWhiteSpace(BaseUrl) && !BaseUrl.EndsWith('/'))
            {
                BaseUrl += "/";
            }

            // set json options based on --strict-json
            App.JsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = !StrictJson,
                AllowTrailingCommas = !StrictJson,
                ReadCommentHandling = StrictJson ? JsonCommentHandling.Disallow : JsonCommentHandling.Skip,
            };

            Zone = string.IsNullOrWhiteSpace(Zone) ? string.Empty : Zone;
            Region = string.IsNullOrWhiteSpace(Region) ? string.Empty : Region;
        }
    }
}

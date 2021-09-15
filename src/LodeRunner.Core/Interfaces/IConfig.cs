// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace LodeRunner.Core.Interfaces
{
    /// <summary>
    /// The Config Interface.
    /// </summary>
    public interface IConfig
    {
        /// <summary>
        /// gets or sets the server / url.
        /// </summary>
        List<string> Server { get; set; }

        /// <summary>
        /// gets or sets the list of files to read.
        /// </summary>
        List<string> Files { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to the use Prometheus flag.
        /// </summary>
        public bool Prometheus { get; set; }

        /// <summary>
        /// gets or sets the zone to log.
        /// </summary>
        string Zone { get; set; }

        /// <summary>
        /// gets or sets the region to log.
        /// </summary>
        string Region { get; set; }

        /// <summary>
        /// gets or sets the tag to log.
        /// </summary>
        string Tag { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether we should run in a loop.
        /// </summary>
        bool RunLoop { get; set; }

        /// <summary>
        /// gets or sets the sleep time between requests in ms.
        /// </summary>
        int Sleep { get; set; }

        /// <summary>
        /// gets or sets the duration of the test in seconds.
        /// </summary>
        int Duration { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether we should use random tests vs. sequential.
        /// </summary>
        bool Random { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether logs should be verbose.
        /// </summary>
        bool Verbose { get; set; }

        /// <summary>
        /// gets or sets the request time out in seconds.
        /// </summary>
        int Timeout { get; set; }

        /// <summary>
        /// gets or sets the max concurrent requests.
        /// </summary>
        int MaxConcurrent { get; set; }

        /// <summary>
        /// gets or sets the max errors before the test exits with a non-zero response.
        /// </summary>
        int MaxErrors { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether we should do a dry run.
        /// </summary>
        bool DryRun { get; set; }

        /// <summary>
        /// gets or sets the base url for test files.
        /// </summary>
        string BaseUrl { get; set; }

        /// <summary>
        /// gets or sets the summary generation time in minutes.
        /// </summary>
        int SummaryMinutes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether we should display verbose errors or just the count.
        /// </summary>
        bool VerboseErrors { get; set; }

        /// <summary>
        /// gets or sets the seconds to delay before starting the test.
        /// </summary>
        int DelayStart { get; set; }

        /// <summary>
        /// gets or sets a value indicating whether we should use strict json parsing.
        /// </summary>
        bool StrictJson { get; set; }

        /// <summary>Gets or sets the max retry attempts for cosmos requests.</summary>
        /// <value>The retries.</value>
        int Retries { get; set; }

        /// <summary>Gets or sets the cosmos max retry wait time for cosmos requests in seconds.</summary>
        /// <value>Time in seconds.</value>
        int CosmosTimeout { get; set; }

        /// <summary>Gets or sets the secrets volume.</summary>
        /// <value>The secrets volume.</value>
        string SecretsVolume { get; set; }

        /// <summary>Gets or sets the name of the cosmos.</summary>
        /// <value>The name of the cosmos.</value>
        string CosmosName { get; set; }
    }
}

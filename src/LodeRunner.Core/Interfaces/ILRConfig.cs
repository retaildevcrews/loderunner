// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace LodeRunner.Core.Interfaces
{
    /// <summary>
    /// The Config Interface.
    /// </summary>
    public interface ILRConfig : ICosmosConfig
    {
        /// <summary>
        /// Gets or sets the client status identifier.
        /// </summary>
        /// <value>
        /// The client status identifier.
        /// </value>
        string ClientStatusId { get; set; }

        /// <summary>
        /// gets or sets the Load Client ID which is a GUID.
        /// </summary>
        string LoadClientId { get; set; }

        /// <summary>
        /// gets or sets the guid for the TestRun being executed.
        /// </summary>
        string TestRunId { get; set; }

        /// <summary>
        /// gets or sets the status update interval in seconds.
        /// </summary>
        int StatusUpdateInterval { get; set; }

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

        /// <summary>
        /// Gets or sets the WebHost Port.
        /// </summary>
        /// <value>
        /// The port.
        /// </value>
        int WebHostPort { get; set; }

        /// <summary>
        /// gets or sets the seconds to refresh the http client.
        /// </summary>
        /// <value>
        /// The client refresh.
        /// </value>
        int ClientRefresh { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is client mode.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is client mode; otherwise, <c>false</c>.
        /// </value>
        bool IsClientMode { get; set; }
    }
}

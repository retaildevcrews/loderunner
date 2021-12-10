// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace LodeRunner.API
{
    /// <summary>
    /// System constants.
    /// </summary>
    public class SystemConstants
    {
        /// <summary>
        /// Status code of error.
        /// </summary>
        public const string Error = "Error";

        /// <summary>
        /// Status code of retry.
        /// </summary>
        public const string Retry = "Retry";

        /// <summary>
        /// Status code of retry.
        /// </summary>
        public const string Warn = "Warn";

        /// <summary>
        /// Status code of OK.
        /// </summary>
        public const string OK = "OK";

        /// <summary>
        /// Forwarded for header.
        /// </summary>
        public const string XffHeader = "X-Forwarded-For";

        /// <summary>
        /// Client IP header.
        /// </summary>
        public const string IpHeader = "X-Client-IP";

        /// <summary>
        /// The terminating string.
        /// </summary>
        public const string Terminating = "Terminating";

        /// <summary>
        /// The termination description string.
        /// </summary>
        public const string TerminationDescription = "Termination requested via Cancellation Token.";

        /// <summary>
        /// The unknown string.
        /// </summary>
        public const string Unknown = "unknown";

        /// <summary>
        /// The lode runner UI default port.
        /// </summary>
        public const int LodeRunnerUIDefaultPort = 3000;

        /// <summary>
        /// The client status identifier.
        /// </summary>
        public const string ClientStatusID = "CLIENTSTATUSID";

        /// <summary>
        /// The error message success.
        /// </summary>
        public const string ErrorMessageSuccess = "The parameter 'cientStatusId' should be a non-empty string";

        /// <summary>
        /// The error message unknown.
        /// </summary>
        public const string ErrorMessageUnknownParameter = "Unknown parameter:";

        /// <summary>
        /// The error link.
        /// </summary>
        public const string ErrorLink = "https://github.com/retaildevcrews/loderunner/blob/main/docs/ParameterValidation.md";

        /// <summary>
        /// The error link path.
        /// </summary>
        public const string ErrorLinkPath = "/api/clients/";

        /// <summary>
        /// The error link path anchor.
        /// </summary>
        public const string ErrorLinkPathAnchor = "#clients-direct-read";

        /// <summary>
        /// The category path client with slash.
        /// </summary>
        public const string CategoryPathClientWithSlash = "/api/clients/";

        /// <summary>
        /// The category path client without slash.
        /// </summary>
        public const string CategoryPathClientWithoutSlash = "/api/clients";

        /// <summary>
        /// The category client.
        /// </summary>
        public const string CategoryClient = "Client";

        /// <summary>
        /// The category sub category client.
        /// </summary>
        public const string CategorySubCategoryClient = "Client";

        /// <summary>
        /// The category mode direct.
        /// </summary>
        public const string CategoryModeDirect = "Direct";

        /// <summary>
        /// The category mode static.
        /// </summary>
        public const string CategoryModeStatic = "Static";

        /// <summary>
        /// The category path healthz.
        /// </summary>
        public const string CategoryPathHealthz = "/healthz";

        /// <summary>
        /// The category healthz.
        /// </summary>
        public const string CategoryHealthz = "Healthz";

        /// <summary>
        /// The category sub category healthz.
        /// </summary>
        public const string CategorySubCategoryHealthz = "Healthz";

        /// <summary>
        /// The category mode healthz.
        /// </summary>
        public const string CategoryModeHealthz = "Healthz";

        /// <summary>
        /// The category path metrics.
        /// </summary>
        public const string CategoryPathMetrics = "/metrics";

        /// <summary>
        /// The category metrics.
        /// </summary>
        public const string CategoryMetrics = "Metrics";

        /// <summary>
        /// The category sub category metrics.
        /// </summary>
        public const string CategorySubCategoryMetrics = "Metrics";

        /// <summary>
        /// The category mode metrics.
        /// </summary>
        public const string CategoryModeMetrics = "Metrics";

        /// <summary>
        /// The category static.
        /// </summary>
        public const string CategoryStatic = "Static";

        /// <summary>
        /// The category sub category static.
        /// </summary>
        public const string CategorySubCategoryStatic = "Static";

        /// <summary>
        /// The client identifier cannot be empty.
        /// </summary>
        public const string ClientIdCannotBeEmpty = "ClientStatusId cannot be empty.";

        /// <summary>
        /// The invalid client status identifier.
        /// </summary>
        public const string InvalidClientStatusId = "Invalid Client Status Id.";

        /// <summary>
        /// The unable to create load test configuration.
        /// </summary>
        public const string UnableToCreateLoadTestConfig = "Unable to create Load Test Config item.";

        /// <summary>
        /// The load test configuration successfully deleted.
        /// </summary>
        public const string DeletedLoadTestConfig = "Load Test Config item successfully deleted";

        /// <summary>
        /// The unable to delete load test configuration.
        /// </summary>
        public const string UnableToDeleteLoadTestConfig = "Unable to delete Load Test Config item.";

        /// <summary>
        /// The load test configuration was not found.
        /// </summary>
        public const string NotFoundLoadTestConfig = "Load Test Config item not found.";

        /// <summary>
        /// String for unable to update load test configuration.
        /// </summary>
        public const string UnableToUpdateLoadTestConfig = "Unable to update Load Test Config item.";

        /// <summary>
        /// String for unable to update load test configuration.
        /// </summary>
        public const string UnableToGetLoadTestConfig = "Load Test Config with specified id does not exist.";

        /// <summary>
        /// Invalid Payload data.
        /// </summary>
        public const string InvalidPayloadData = "Invalid Payload data.";
    }
}

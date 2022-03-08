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
        /// The load test config identifier.
        /// </summary>
        public const string LoadTestConfigID = "LOADTESTCONFIGID";

        /// <summary>
        /// The test run identifier.
        /// </summary>
        public const string TestRunID = "TESTRUNID";

        /// <summary>
        /// The error message success.
        /// </summary>
        public const string ErrorMessageGuid = "Value is not a valid GUID.";

        /// <summary>
        /// The error message unknown.
        /// </summary>
        public const string ErrorMessageUnknownParameter = "Unknown parameter:";

        /// <summary>
        /// The error link.
        /// </summary>
        public const string ErrorLink = "https://github.com/retaildevcrews/loderunner/blob/main/docs/ParameterValidation.md";

        /// <summary>
        /// The error link path anchor for direct clients.
        /// </summary>
        public const string ErrorLinkDirectClientsPathAnchor = "#clients-direct-read";

        /// <summary>
        /// The error link path anchor for direct clients.
        /// </summary>
        public const string ErrorLinkDirectLoadTestConfigsPathAnchor = "#loadtestconfigs-direct-read";

        /// <summary>
        /// The error link path anchor for direct clients.
        /// </summary>
        public const string ErrorLinkDirectTestRunsPathAnchor = "#testruns-direct-read";

        /// <summary>
        /// The path for direct client actions.
        /// </summary>
        public const string DirectClientsPath = "/api/clients/";

        /// <summary>
        /// The path for direct load test config actions.
        /// </summary>
        public const string DirectLoadTestConfigsPath = "/api/loadtestconfigs/";

        /// <summary>
        /// The path for direct test run actions.
        /// </summary>
        public const string DirectTestRunsPath = "/api/testruns/";

        /// <summary>
        /// The path for category, metrics.
        /// </summary>
        public const string CategoryMetricsPath = "/metrics";

        /// <summary>
        /// The path for category, healthz.
        /// </summary>
        public const string CategoryHealthzPath = "/healthz";

        /// <summary>
        /// The path for category, clients.
        /// </summary>
        public const string CategoryClientsPath = "/api/clients";

        /// <summary>
        /// The path for category, load test configs.
        /// </summary>
        public const string CategoryLoadTestConfigsPath = "/api/loadtestconfigs";

        /// <summary>
        /// The path for category, test runs.
        /// </summary>
        public const string CategoryTestRunsPath = "/api/testruns";

        /// <summary>
        /// The category static.
        /// </summary>
        public const string CategoryStatic = "Static";

        /// <summary>
        /// The category metrics.
        /// </summary>
        public const string CategoryMetrics = "Metrics";

        /// <summary>
        /// The category healthz.
        /// </summary>
        public const string CategoryHealthz = "Healthz";

        /// <summary>
        /// The category client.
        /// </summary>
        public const string CategoryClient = "Client";

        /// <summary>
        /// The category load test config.
        /// </summary>
        public const string CategoryLoadTestConfig = "LoadTestConfig";

        /// <summary>
        /// The category test run.
        /// </summary>
        public const string CategoryTestRun = "TestRun";

        /// <summary>
        /// The category mode direct.
        /// </summary>
        public const string CategoryModeDirect = "Direct";

        /// <summary>
        /// The category mode static.
        /// </summary>
        public const string CategoryModeStatic = "Static";

        /// <summary>
        /// The category mode metrics.
        /// </summary>
        public const string CategoryModeMetrics = "Metrics";

        /// <summary>
        /// The category mode healthz.
        /// </summary>
        public const string CategoryModeHealthz = "Healthz";

        /// <summary>
        /// Generic not found error message.
        /// </summary>
        public const string NotFoundError = "Not Found.";

        /// <summary>
        /// Generic bad request message.
        /// </summary>
        public const string BadRequest = "Bad Request.";

        /// <summary>
        /// Generic error message for Cosmos response with unhandled status code.
        /// </summary>
        public const string UnhandledCosmosStatusCode = "Unhandled status code from CosmosDB.";

        /// <summary>
        /// Generic parameter data message.
        /// </summary>
        public const string InvalidParameter = "Invalid parameter data.";

        /// <summary>
        /// Generic payload data message.
        /// </summary>
        public const string InvalidPayload = "Invalid payload data.";

        /// <summary>
        /// Generic no returned value from upsert message.
        /// </summary>
        public const string UpsertError = "Upsert did not return a model.";

        /// <summary>
        /// Found all existing clients.
        /// </summary>
        public const string AllClientsFound = "Array of `Client` items.";

        /// <summary>
        /// Found all existing load test configs.
        /// </summary>
        public const string AllLoadTestConfigsFound = "Array of `LoadTestConfig` items.";

        /// <summary>
        /// Found all existing test runs.
        /// </summary>
        public const string AllTestRunsFound = "Array of `TestRun` items.";

        /// <summary>
        /// No clients found.
        /// </summary>
        public const string NoClientsFound = "No `Client` items found.";

        /// <summary>
        /// No load test configs found.
        /// </summary>
        public const string NoLoadTestConfigsFound = "No `LoadTestConfig` items found.";

        /// <summary>
        /// No test runs found.
        /// </summary>
        public const string NoTestRunsFound = "No `TestRun` items found.";

        /// <summary>
        /// Unable to get clients.
        /// </summary>
        public const string UnableToGetClients = "Unable to get `Client` items.";

        /// <summary>
        /// Unable to get load test configs.
        /// </summary>
        public const string UnableToGetLoadTestConfigs = "Unable to get `LoadTestConfig` items.";

        /// <summary>
        /// Unable to get test runs.
        /// </summary>
        public const string UnableToGetTestRuns = "Unable to get `Test Run` items.";

        /// <summary>
        /// Client item was found.
        /// </summary>
        public const string ClientItemFound = "Single `Client` item by clientStatusId.";

        /// <summary>
        /// Load test config item was found.
        /// </summary>
        public const string LoadTestConfigItemFound = "Single `LoadTestConfig` item by loadTestConfigId.";

        /// <summary>
        /// Test run item was found.
        /// </summary>
        public const string TestRunItemFound = "Single `TestRun` item by testRunId.";

        /// <summary>
        /// Client item was not found.
        /// </summary>
        public const string ClientItemNotFound = "Single `Client` item not found by clientStatusId.";

        /// <summary>
        /// Load test config item was not found.
        /// </summary>
        public const string LoadTestConfigItemNotFound = "Single `LoadTestConfig` item not found by loadTestConfigId.";

        /// <summary>
        /// Test run item was not found.
        /// </summary>
        public const string TestRunItemNotFound = "Single `TestRun` item not found by testRunId.";

        /// <summary>
        /// Unable to get client item.
        /// </summary>
        public const string UnableToGetClientItem = "Unable to get single `Client` item.";

        /// <summary>
        /// Unable to get load test config item.
        /// </summary>
        public const string UnableToGetLoadTestConfigItem = "Unable to get single `LoadTestConfig` item.";

        /// <summary>
        /// Unable to get test run item.
        /// </summary>
        public const string UnableToGetTestRunItem = "Unable to get single `TestRun` item.";

        /// <summary>
        /// Created load test config item.
        /// </summary>
        public const string CreatedLoadTestConfig = "Created single `LoadTestConfig` item.";

        /// <summary>
        /// Created test run item.
        /// </summary>
        public const string CreatedTestRun = "Created single `TestRun` item.";

        /// <summary>
        /// Unable to create load test config item.
        /// </summary>
        public const string UnableToCreateLoadTestConfig = "Unable to create single `LoadTestConfig` item.";

        /// <summary>
        /// Unable to create test run item.
        /// </summary>
        public const string UnableToCreateTestRun = "Unable to create single `TestRun` item.";

        /// <summary>
        /// Updated client item.
        /// </summary>
        public const string UpdatedClient = "Updated single `Client` item.";

        /// <summary>
        /// Updated load test config item.
        /// </summary>
        public const string UpdatedLoadTestConfig = "Updated single `LoadTestConfig` item.";

        /// <summary>
        /// Updated test run item.
        /// </summary>
        public const string UpdatedTestRun = "Updated single `TestRun` item.";

        /// <summary>
        /// Unable to update client item.
        /// </summary>
        public const string UnableToUpdateClient = "Unable to update single `Client` item.";

        /// <summary>
        /// Unable to update load test config item.
        /// </summary>
        public const string UnableToUpdateLoadTestConfig = "Unable to update single `LoadTestConfig` item.";

        /// <summary>
        /// Unable to update test run item.
        /// </summary>
        public const string UnableToUpdateTestRun = "Unable to update single `TestRun` item.";

        /// <summary>
        /// Unable to find load test config in order to update.
        /// </summary>
        public const string UnableToUpdateLoadTestConfigNotFound = "Unable to find single `LoadTestConfig` item by loadTestConfigId. Unable to update `LoadTestConfig` item.";

        /// <summary>
        /// Unable to find test run in order to update.
        /// </summary>
        public const string UnableToUpdateTestRunNotFound = "Unable to find single `TestRun` item by testRunId. Unable to update `TestRun` item.";

        /// <summary>
        /// Successfully deleted load test config item.
        /// </summary>
        public const string DeletedLoadTestConfig = "Single `LoadTestConfig` item successfully deleted.";

        /// <summary>
        /// Successfully deleted test run item.
        /// </summary>
        public const string DeletedTestRun = "Single `TestRun` item successfully deleted.";

        /// <summary>
        /// Unable to delete load test config item.
        /// </summary>
        public const string UnableToDeleteLoadTestConfig = "Unable to delete single `LoadTestConfig` item.";

        /// <summary>
        /// Unable to delete test run item.
        /// </summary>
        public const string UnableToDeleteTestRun = "Unable to delete single `TestRun` item.";

        /// <summary>
        /// Unable to find load test config in order to delete.
        /// </summary>
        public const string UnableToDeleteLoadTestConfigNotFound = "Unable to find single `LoadTestConfig` item by loadTestConfigId. Unable to delete `LoadTestConfig` item.";

        /// <summary>
        /// Unable to find test run in order to delete.
        /// </summary>
        public const string UnableToDeleteTestRunNotFound = "Unable to find single `TestRun` item by testRunId. Unable to delete `TestRun` item.";

        /// <summary>
        /// Unable to delete test run item because running.
        /// </summary>
        public const string UnableToDeleteTestRunRunning = "Test is running. Unable to delete single `TestRun` item.";

        /// <summary>
        /// The clientStatusId cannot be empty.
        /// </summary>
        public const string ClientStatusIdCannotBeEmpty = "`clientStatusId` cannot be empty.";

        /// <summary>
        /// The loadTestConfigId cannot be empty.
        /// </summary>
        public const string LoadTestConfigIdCannotBeEmpty = "`loadTestConfigId` cannot be empty.";

        /// <summary>
        /// The testRunId cannot be empty.
        /// </summary>
        public const string TestRunIdCannotBeEmpty = "`testRunId` cannot be empty.";

        /// <summary>
        /// The invalid clientStatusId.
        /// </summary>
        public const string InvalidClientStatusId = "Invalid `clientStatusId`.";

        /// <summary>
        /// The invalid loadTestConfigId.
        /// </summary>
        public const string InvalidLoadTestConfigId = "Invalid `loadTestConfigId`.";

        /// <summary>
        /// The invalid testRunId.
        /// </summary>
        public const string InvalidTestRunId = "Invalid `testRunId`.";

        /// <summary>
        /// The test run item was not found.
        /// </summary>
        public const string TestRunNotFound = "Single `TestRun` document not found by `testRunId`.";

        /// <summary>
        /// The base Url for local host with port param.
        /// </summary>
        public const string BaseUriLocalHostPort = "http://localhost:{0}";
    }
}

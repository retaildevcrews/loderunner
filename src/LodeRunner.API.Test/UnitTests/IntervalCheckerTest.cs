// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using LodeRunner.Core;
using LodeRunner.Core.Extensions;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace LodeRunner.API.Test.UnitTests
{
    /// <summary>
    /// Test Interval Checker class.
    /// </summary>
    public class IntervalCheckerTest
    {
        private readonly ITestOutputHelper output;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntervalCheckerTest"/> class.
        /// </summary>
        /// <param name="output">The output.</param>
        public IntervalCheckerTest(ITestOutputHelper output)
        {
            this.output = output;
            this.logger = CreateLogger(new Config() { LogLevel = LogLevel.Warning });
        }

        /// <summary>
        /// Test for cancellation request was initiated have reached the timeout based on Interval and RetryLimit parameters.
        /// </summary>
        /// <returns>The asynchronous unit test.</returns>
        [Fact]
        [Trait("Category", "Unit")]
        public async Task CanValidateCancellationRequestNotInitiated()
        {
            CancellationTokenSource cancellationTokenSource = new ();

            int intervalSeconds = 4;
            int retryLimit = 3;
            List<string> outputStringList;
            bool cancellationRequested;

            using var consoleIORouter = new ConsoleIORouter();
            try
            {
                consoleIORouter.RedirectOutput();

                using var intervalChecker = new IntervalChecker(this.GetBooleanTrue, this.logger, cancellationTokenSource, intervalSeconds, retryLimit);
                intervalChecker.Start();

                cancellationRequested = await this.WaitForTimeoutAndGetCancellationRequestStatus(cancellationTokenSource, intervalSeconds).ConfigureAwait(false);

                outputStringList = consoleIORouter.GetOutputAsStringList();
            }
            finally
            {
                consoleIORouter.ResetOutput();
            }

            Assert.False(cancellationRequested, "Request cancellation is not expected.");

            Assert.True(outputStringList != null, "Output string list is null.");

            Assert.True(outputStringList.Count == 0, "Output string count should be 0.");

            this.output.WriteLine($"'{outputStringList.Count}' messages found in Console.Output.");
        }

        /// <summary>
        /// Test for cancellation request was not initiated after have reached the timeout based on Interval and RetryLimit parameters.
        /// </summary>
        /// <returns>The asynchronous unit test.</returns>
        [Fact]
        [Trait("Category", "Unit")]
        public async Task CanValidateCancellationRequestInitiated()
        {
            CancellationTokenSource cancellationTokenSource = new ();

            int intervalSeconds = 4;
            int retryLimit = 3;
            List<string> outputStringList;
            bool cancellationRequested;

            using var consoleIORouter = new ConsoleIORouter();
            try
            {
                consoleIORouter.RedirectOutput();

                using var intervalChecker = new IntervalChecker(this.GetBooleanFalse, this.logger, cancellationTokenSource, intervalSeconds, retryLimit);
                intervalChecker.Start();

                cancellationRequested = await this.WaitForTimeoutAndGetCancellationRequestStatus(cancellationTokenSource, intervalSeconds).ConfigureAwait(false);

                outputStringList = consoleIORouter.GetOutputAsStringList();
            }
            finally
            {
                consoleIORouter.ResetOutput();
            }

            Assert.True(cancellationRequested, "Request cancellation expected.");

            Assert.True(outputStringList != null, "Output string list is null.");

            int expectedLogCount = retryLimit + 1;
            Assert.True(outputStringList.Count == expectedLogCount, $"Output string count should be {expectedLogCount}.");

            this.output.WriteLine($"'{outputStringList.Count}' messages found in Console.Output.");

            string messageIfFailed = "Failed to validate Check Failed message for Attempt {0}.";

            // Validate error Messages for 3 attempts.
            for (int i = 1; i <= retryLimit; i++)
            {
                this.ValidateLogMessages(outputStringList, expectedMessageValue: string.Format(LodeRunner.Core.SystemConstants.IntervalCheckFailedAttemptMessage, i, retryLimit), string.Format(messageIfFailed, i), expectedLogLevel: LogLevel.Warning.ToString());
            }

            // Validate App Terminating Error.
            messageIfFailed = "Failed to validate message 'Application Will Terminate'";
            this.ValidateLogMessages(outputStringList, expectedMessageValue: string.Format(LodeRunner.Core.SystemConstants.IntervalCheckErrorMessage, retryLimit), messageIfFailed, expectedLogLevel: LogLevel.Error.ToString());
        }

        /// <summary>
        ///  Create IntervalCheckerTest Logger.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <returns>The logger.</returns>
        private static ILogger<IntervalCheckerTest> CreateLogger(Config config)
        {
            string projectName = Assembly.GetCallingAssembly().GetName().Name;
            using var loggerFactory = LoggerFactory.Create(logger =>
            {
                logger.Setup(logLevelConfig: config, logValues: config, projectName: projectName);
            });

            return loggerFactory.CreateLogger<IntervalCheckerTest>();
        }

        /// <summary>
        /// Calculate the maximum waiting time.
        /// </summary>
        /// <param name="interval">The interval.</param>
        /// <returns>the max waiting time.</returns>
        private static int CalculateTimeout(int interval)
        {
            int newInterval = interval;

            for (int i = 1; i < 3; i++)
            {
                newInterval /= 2;
                interval += newInterval;
            }

            // We will wait for Max Interval time plus 1 extra second.
            return interval + 1;
        }

        /// <summary>
        /// Validates the log messages.
        /// </summary>
        /// <param name="outputStringList">The output string list.</param>
        /// <param name="expectedMessageValue">The expected value.</param>
        /// <param name="messageIfFailed">The message if failed.</param>
        /// <param name="expectedLogLevel">The expected log level.</param>
        private void ValidateLogMessages(List<string> outputStringList, string expectedMessageValue, string messageIfFailed, string expectedLogLevel)
        {
            string actualMessageValue = this.TryParseOutputAndGetValueFromFieldName(outputStringList, "LodeRunner.API.Test.UnitTests.IntervalCheckerTest", expectedMessageValue, "message");

            Assert.True(expectedMessageValue.Equals(actualMessageValue), messageIfFailed);

            string actualLogLevelValue = this.TryParseOutputAndGetValueFromFieldName(outputStringList, "LodeRunner.API.Test.UnitTests.IntervalCheckerTest", expectedMessageValue, "logLevel");

            Assert.True(expectedLogLevel.Equals(actualLogLevelValue), $"Log Level mismatched. Expected '{expectedLogLevel}', but found '{actualLogLevelValue}'");
        }

        /// <summary>
        /// Parses the process output and to get a value for a given field.
        /// </summary>
        /// <param name="outputList">The Process outputList.</param>
        /// <param name="logName">ThelogName to identify the line be parsed.</param>
        /// <param name="marker">The marker string to identify the line be parsed.</param>
        /// <param name="fieldName">The name of the field to get the value from Json object.</param>
        /// <returns>The Task with the FieldValue.</returns>
        private string TryParseOutputAndGetValueFromFieldName(List<string> outputList, string logName, string marker, string fieldName)
        {
            string fieldValue = null;

            // NOTE: we  ignore case sensitive.
            string targetOutputLine = outputList.FirstOrDefault(s => s.Contains($"logName\":\"{logName}\"", StringComparison.InvariantCultureIgnoreCase) && s.Contains(marker, StringComparison.InvariantCultureIgnoreCase));
            if (!string.IsNullOrEmpty(targetOutputLine))
            {
                Newtonsoft.Json.Linq.JObject json = Newtonsoft.Json.Linq.JObject.Parse(targetOutputLine);

                foreach (var e in json)
                {
                    if (e.Key.Equals(fieldName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        fieldValue = e.Value.ToString();
                        this.output.WriteLine($"UTC Time:{DateTime.UtcNow}\tParsing Output.\t'{fieldName}' Found in LogName '{logName}'.\tValue: '{fieldValue}'");
                        break;
                    }
                }
            }
            else
            {
                this.output.WriteLine($"UTC Time:{DateTime.UtcNow}\tParsing Output.\t'{fieldName}' Not Found in LogName '{logName}'.");
            }

            return fieldValue;
        }

        /// <summary>
        /// Waits until timeout is reached and determines if Cancellation request is in place.
        /// </summary>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <param name="intervalSeconds">The interval seconds.</param>
        /// <returns>Whether the cancellation was requested or not.</returns>
        private async Task<bool> WaitForTimeoutAndGetCancellationRequestStatus(CancellationTokenSource cancellationTokenSource, int intervalSeconds)
        {
            int maxWaitingTime = CalculateTimeout(intervalSeconds);
            await Task.Delay(maxWaitingTime * 1000).ConfigureAwait(false);

            bool cancellationRequested = cancellationTokenSource.IsCancellationRequested;
            this.output.WriteLine($"Total Wait time: {maxWaitingTime} secs.\tCancellation Requested: {cancellationRequested}\t");
            return cancellationRequested;
        }

        /// <summary>
        /// Gets a boolean true.
        /// </summary>
        /// <returns>true.</returns>
        private Task<bool> GetBooleanTrue()
        {
            return Task.Run(() => { return true; });
        }

        /// <summary>
        /// Gets a boolean false.
        /// </summary>
        /// <returns>false.</returns>
        private Task<bool> GetBooleanFalse()
        {
            return Task.Run(() => { return false; });
        }
    }
}

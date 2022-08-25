// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace LodeRunner.API.Test.IntegrationTests.Extensions
{
    /// <summary>
    /// Represents LogOutputExtension.
    /// </summary>
    public static class LogOutputExtension
    {
        /// <summary>
        /// Parses the process output and to get Port number that the API is listening on.
        /// </summary>
        /// <param name="outputList">The Process outputList.</param>
        /// <param name="output">The output.</param>
        /// <param name="maxRetries">The maximum retries.</param>
        /// <param name="timeBetweenTriesMs">The time between tries ms.</param>
        /// <returns>port number. 0 if not found.</returns>
        public static async Task<int> TryParseProcessOutputAndGetAPIListeningPort(List<string> outputList, ITestOutputHelper output, int maxRetries = 20, int timeBetweenTriesMs = 1000)
        {
            int portNumber = 0;

            var taskSource = new CancellationTokenSource();
            await LodeRunner.Core.Common.RunAndRetry(maxRetries, timeBetweenTriesMs, taskSource, async (int attemptCount) =>
            {
                await Task.Run(() =>
                {
                    string targetOutputLine = outputList.FirstOrDefault(s => s.Contains("Now listening on:"));
                    if (!string.IsNullOrEmpty(targetOutputLine))
                    {
                        // the line should look like "Now listening on: http://[::]:8080",  since we are splitting on ":" last string in the list will be the port either 8080 (Production) or 8081 (Development)
                        string portNumberString = targetOutputLine.Split(":").ToList().LastOrDefault();

                        if (int.TryParse(portNumberString, out portNumber))
                        {
                            output.WriteLine($"UTC Time:{DateTime.UtcNow}\tParsing LodeRunner API Process Output.\tPort Number Found.\tPort: '{portNumber}'\tAttempts: {attemptCount} [{timeBetweenTriesMs}ms between requests]");
                        }

                        taskSource.Cancel();
                    }
                    else
                    {
                        output.WriteLine($"UTC Time:{DateTime.UtcNow}\tParsing LodeRunner API Process Output.\tPort Number Not Found.\tAttempts: {attemptCount} [{timeBetweenTriesMs}ms between requests]");
                    }
                });
            });

            return portNumber;
        }

        /// <summary>
        /// Parses the process output and to get the specified fieldName value.
        /// </summary>
        /// <param name="outputList">The Process outputList.</param>
        /// <param name="logName">ThelogName to identify the line be parsed.</param>
        /// <param name="marker">The marker string to identify the line be parsed.</param>
        /// <param name="fieldName">The name of the field to get the value from Json object.</param>
        /// <param name="output">The output.</param>
        /// <param name="maxRetries">The maximum retries.</param>
        /// <param name="timeBetweenTriesMs">The time between tries ms.</param>
        /// <returns>The Task with the FieldValue.</returns>
        public static async Task<string> TryParseProcessOutputAndGetValueFromFieldName(List<string> outputList, string logName, string marker, string fieldName, ITestOutputHelper output, int maxRetries = 10, int timeBetweenTriesMs = 500)
        {
            string fieldValue = null;
            var taskSource = new CancellationTokenSource();

            await LodeRunner.Core.Common.RunAndRetry(maxRetries, timeBetweenTriesMs, taskSource, async (int attemptCount) =>
            {
                await Task.Run(() =>
                {
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
                                output.WriteLine($"UTC Time:{DateTime.UtcNow}\tParsing LodeRunner Process Output.\t'{fieldName}' Found in LogName '{logName}'.\tValue: '{fieldValue}'\tAttempts: {attemptCount} [{timeBetweenTriesMs}ms between requests]");
                                break;
                            }
                        }

                        if (fieldValue != null)
                        {
                            taskSource.Cancel();
                        }
                    }
                    else
                    {
                        output.WriteLine($"UTC Time:{DateTime.UtcNow}\tParsing LodeRunner Process Output.\t'{fieldName}' Not Found in LogName '{logName}'.\tAttempts: {attemptCount} [{timeBetweenTriesMs}ms between requests]");
                    }
                });
            });

            return fieldValue;
        }

        /// <summary>
        /// Parses the process errors to determine if errors exist.
        /// </summary>
        /// <param name="errorsList">The Process errorsList.</param>
        /// <param name="output">The output.</param>
        /// <param name="maxRetries">The maximum retries.</param>
        /// <param name="timeBetweenTriesMs">The time between tries ms.</param>
        /// <returns>Task value whether errors found.</returns>
        public static async Task<bool> TryParseProcessErrors(List<string> errorsList, ITestOutputHelper output, int maxRetries = 10, int timeBetweenTriesMs = 500)
        {
            bool errorsFound = false;
            var taskSource = new CancellationTokenSource();

            await LodeRunner.Core.Common.RunAndRetry(maxRetries, timeBetweenTriesMs, taskSource, async (int attemptCount) =>
            {
                await Task.Run(() =>
                {
                    if (errorsList.Count > 0)
                    {
                        output.WriteLine($"UTC Time:{DateTime.UtcNow}\tParsing LodeRunner.API Errors.\tErrors Found.\tErrors Count: '{errorsList.Count}'\tAttempts: {attemptCount} [{timeBetweenTriesMs}ms between requests]");
                        errorsFound = true;
                        taskSource.Cancel();
                    }
                    else
                    {
                        output.WriteLine($"UTC Time:{DateTime.UtcNow}\tParsing LodeRunner.API Errors.\tNo Errors found.\tAttempts: {attemptCount} [{timeBetweenTriesMs}ms between requests]");
                    }
                });
            });

            return errorsFound;
        }
    }
}

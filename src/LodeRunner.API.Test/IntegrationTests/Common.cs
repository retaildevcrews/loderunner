// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace LodeRunner.API.Test.IntegrationTests
{
    /// <summary>
    /// REpresent common use functions.
    /// </summary>
    public static class Common
    {
        /// <summary>
        /// Parses the Output and validates that the Value for a given field is not null.
        /// </summary>
        /// <param name="appName">The name of the application that the log is from.</param>
        /// <param name="outputList">The Process outputList.</param>
        /// <param name="logName">The logName to identify the line be parsed.</param>
        /// <param name="outputMarker">The marker string to identify the line be parsed.</param>
        /// <param name="fieldName">The name of the field to get the value from..</param>
        /// <param name="output">The output.</param>
        /// <param name="instanceId">The intance identifier.</param>
        /// <param name="failErrorMessage">The message to display when validation fails.</param>
        /// <param name="maxRetries">The maximum retries.</param>
        /// <param name="timeBetweenTriesMs">The time between tries ms.</param>
        /// <returns>The field value.</returns>
        public static async Task<string> ParseOutputGetFieldValueAndValidateIsNotNullOrEmpty(string appName, List<string> outputList, string logName, string outputMarker, string fieldName, ITestOutputHelper output, string instanceId = "", string failErrorMessage = null, int maxRetries = 10, int timeBetweenTriesMs = 500)
        {
            var fieldValue = await TryParseProcessOutputAndGetValueFromFieldName(outputList, logName, outputMarker, fieldName, output, instanceId, maxRetries, timeBetweenTriesMs);
            if (string.IsNullOrEmpty(failErrorMessage))
            {
                failErrorMessage = $"Unable to get {fieldName} from {appName}-Command output";
            }

            Assert.False(string.IsNullOrEmpty(fieldValue), failErrorMessage);

            return fieldValue;
        }

        /// <summary>
        /// Parses the process output and to get client status id.
        /// </summary>
        /// <param name="outputList">The Process outputList.</param>
        /// <param name="logName">ThelogName to identify the line be parsed.</param>
        /// <param name="marker">The marker string to identify the line be parsed.</param>
        /// <param name="fieldName">The name of the field to get the value from Json object.</param>
        /// <param name="output">The output.</param>
        /// <param name="instanceId">The intance identifier.</param>
        /// <param name="maxRetries">The maximum retries.</param>
        /// <param name="timeBetweenTriesMs">The time between tries ms.</param>
        /// <returns>The Task with the FieldValue.</returns>
        private static async Task<string> TryParseProcessOutputAndGetValueFromFieldName(List<string> outputList, string logName, string marker, string fieldName, ITestOutputHelper output, string instanceId = "", int maxRetries = 10, int timeBetweenTriesMs = 500)
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
                                output.WriteLine($"UTC Time:{DateTime.UtcNow}\tParsing LodeRunner Process Output.\t{instanceId}\t'{fieldName}' Found in LogName '{logName}'.\tValue: '{fieldValue}'\tAttempts: {attemptCount} [{timeBetweenTriesMs}ms between requests]");
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
                        output.WriteLine($"UTC Time:{DateTime.UtcNow}\tParsing LodeRunner Process Output.\t{instanceId}\t'{fieldName}' Not Found in LogName '{logName}'.\tAttempts: {attemptCount} [{timeBetweenTriesMs}ms between requests]");
                    }
                });
            });

            return fieldValue;
        }
    }
}
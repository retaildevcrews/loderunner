// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using LodeRunner.API.Models;
using LodeRunner.Core.Models;
using Newtonsoft.Json;
using Xunit.Abstractions;

namespace LodeRunner.API.Test.IntegrationTests
{
    /// <summary>
    /// Http Client Extensions.
    /// </summary>
    public static class HttpClientExtension
    {
        /// <summary>
        /// Waits for GetClients Response to match ClientStatus for the given ClientStatusId.
        /// </summary>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="clientsUri">The clients URI.</param>
        /// <param name="clientStatusId">The client status identifier.</param>
        /// <param name="clientStatusType">Type of the client status.</param>
        /// <param name="jsonOptions">The json options.</param>
        /// <param name="output">The output.</param>
        /// <param name="maxRetries">Maximum retries.</param>
        /// <param name="timeBetweenRequestsMs">Wait time betweeen requests.</param>
        /// <returns>HttpResponseMessage and Client from response.</returns>
        public static async Task<(HttpResponseMessage, Client)> GetClientByIdRetriesAsync(this HttpClient httpClient, string clientsUri, string clientStatusId, ClientStatusType clientStatusType, JsonSerializerOptions jsonOptions, ITestOutputHelper output, int maxRetries = 10, int timeBetweenRequestsMs = 100)
        {
            HttpResponseMessage httpResponse = new ();
            Client client = null;

            var taskSource = new CancellationTokenSource();

            await Common.RunAndRetry(maxRetries, timeBetweenRequestsMs, taskSource, async (int attemptCount) =>
            {
                httpResponse = await httpClient.GetAsync($"{clientsUri}/{clientStatusId}");

                if (httpResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: GET Client by ID\tResponse StatusCode: 'NotFound'\tClientStatusId: '{clientStatusId}'\tAttempts: {attemptCount} [{timeBetweenRequestsMs}ms between requests]");
                }
                else if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    client = await httpResponse.Content.ReadFromJsonAsync<Client>(jsonOptions);

                    if (client.Status == clientStatusType)
                    {
                        output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: GET Client by ID\tResponse StatusCode: 'OK'\tClientStatusId: '{clientStatusId}'\tAttempts: {attemptCount} [{timeBetweenRequestsMs}ms between requests]\tStatusType criteria met [{client.Status}]");
                        taskSource.Cancel();
                    }
                    else
                    {
                        output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: GET Client by ID\tResponse StatusCode: 'OK'\tClientStatusId: '{clientStatusId}'\tAttempts: {attemptCount} [{timeBetweenRequestsMs}ms between requests]\tStatusType criteria not met [expected: {clientStatusType}, received: {client.Status}]");
                    }
                }
                else
                {
                    string userMessage = await httpResponse.Content.ReadAsStringAsync();
                    output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: GET Client by ID\tUnhandled Response StatusCode: '{httpResponse.StatusCode}'\tClientStatusId: '{clientStatusId}'\tAttempts: {attemptCount} [{timeBetweenRequestsMs}ms between requests]\tError:{userMessage}");
                    taskSource.Cancel();
                }
            });

            return (httpResponse, client);
        }

        /// <summary>
        /// Retries the GET request until content or unexpected non-200 code is returned.
        /// </summary>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="uri">The URI path.</param>
        /// <param name="action">Action for logging.</param>
        /// <param name="output">The output.</param>
        /// <param name="maxRetries">Maximum retries.</param>
        /// <param name="timeBetweenRequestsMs">Wait time betweeen requests.</param>
        /// <returns>HttpResponseMessage after retries.</returns>
        public static async Task<HttpResponseMessage> GetRetryAsync(this HttpClient httpClient, string uri, string action, ITestOutputHelper output, int maxRetries = 10, int timeBetweenRequestsMs = 100)
        {
            HttpResponseMessage httpResponse = new ();

            var taskSource = new CancellationTokenSource();

            await Common.RunAndRetry(maxRetries, timeBetweenRequestsMs, taskSource, async (int attemptCount) =>
            {
                httpResponse = await httpClient.GetAsync($"{uri}");

                if (httpResponse.StatusCode == HttpStatusCode.NoContent)
                {
                    output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: {action}\tResponse StatusCode: '{httpResponse.StatusCode}'\tAttempts: {attemptCount} [{timeBetweenRequestsMs}ms between requests]");
                }
                else
                {
                    taskSource.Cancel();
                }
            });

            return httpResponse;
        }

        /// <summary>
        /// Get all TestRun.
        /// </summary>
        /// <param name="httpClient">The httpClient.</param>
        /// <param name="testRunsUri">The TestRun Uri.</param>
        /// <param name="output">The output.</param>
        /// <returns>the task.</returns>
        /// <summary>
        /// <returns><see cref="Task"/> representing the asynchronous unit test.</returns>
        public static async Task<HttpResponseMessage> GetTestRuns(this HttpClient httpClient, string testRunsUri, ITestOutputHelper output)
        {
            HttpResponseMessage httpResponse = await httpClient.GetAsync(testRunsUri);

            if (httpResponse.StatusCode == HttpStatusCode.OK || httpResponse.StatusCode == HttpStatusCode.NoContent)
            {
                output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: GET all TestRun\tResponse StatusCode: '{httpResponse.StatusCode}'");
            }
            else
            {
                string userMessage = await httpResponse.Content.ReadAsStringAsync();
                output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: GET all TestRun\tUNEXPECTED Response StatusCode: '{httpResponse.StatusCode}'\tError:{userMessage}");
            }

            return httpResponse;
        }

        /// <summary>
        /// Get TestRun by ID.
        /// </summary>
        /// <param name="httpClient">The httpClient.</param>
        /// <param name="testRunsUri">The TestRun Uri.</param>
        /// <param name="testRunId">The TestRun ID.</param>
        /// <param name="output">The output.</param>
        /// <returns>the task.</returns>
        public static async Task<HttpResponseMessage> GetTestRunById(this HttpClient httpClient, string testRunsUri, string testRunId, ITestOutputHelper output)
        {
            var httpResponse = await httpClient.GetAsync(testRunsUri + "/" + testRunId);

            if (httpResponse.StatusCode == HttpStatusCode.OK || httpResponse.StatusCode == HttpStatusCode.NotFound)
            {
                output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: GET TestRun by ID\tResponse StatusCode: '{httpResponse.StatusCode}'\tTestRunId: '{testRunId}'");
            }
            else
            {
                string userMessage = await httpResponse.Content.ReadAsStringAsync();
                output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: GET TestRun\tUNEXPECTED Response StatusCode: '{httpResponse.StatusCode}'\tTestRunId: '{testRunId}'\tError:{userMessage}");
            }

            return httpResponse;
        }

        /// <summary>
        /// Post TestRuns.
        /// </summary>
        /// <param name="httpClient">The httpClient.</param>
        /// <param name="postTestRunsUri">The post TestRun Uri.</param>
        /// <param name="output">The output.</param>
        /// <returns>the task.</returns>
        public static async Task<HttpResponseMessage> PostTestRun(this HttpClient httpClient, string postTestRunsUri, ITestOutputHelper output)
        {
            TestRunPayload testRunPayload = new ();
            testRunPayload.SetMockData($"Sample TestRun - IntegrationTesting-{nameof(PostTestRun)}-{DateTime.UtcNow:yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK}");

            string jsonTestRun = JsonConvert.SerializeObject(testRunPayload);
            StringContent stringContent = new (jsonTestRun, Encoding.UTF8, "application/json");

            var httpResponse = await httpClient.PostAsync(postTestRunsUri, stringContent);

            if (httpResponse.StatusCode == HttpStatusCode.Created)
            {
                output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: POST TestRun\tResponse StatusCode: '{httpResponse.StatusCode}'");
            }
            else
            {
                string userMessage = await httpResponse.Content.ReadAsStringAsync();
                output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: POST TestRun\tUNEXPECTED Response StatusCode: '{httpResponse.StatusCode}'\tError:{userMessage}");
            }

            return httpResponse;
        }

        /// <summary>
        /// Put Test Run by ID.
        /// </summary>
        /// <param name="httpClient">the httpClient.</param>
        /// <param name="testRunsUri">The TestRun Uri.</param>
        /// <param name="testRunId">The test run ID.</param>
        /// <param name="testRunPayload">the testRunPayload entity.</param>
        /// <param name="output">The output.</param>
        /// <returns>the task.</returns>
        public static async Task<HttpResponseMessage> PutTestRunById(this HttpClient httpClient, string testRunsUri, string testRunId, TestRunPayload testRunPayload, ITestOutputHelper output)
        {
            StringContent stringContent = new (JsonConvert.SerializeObject(testRunPayload), Encoding.UTF8, "application/json");

            // Send Request
            var httpResponse = await httpClient.PutAsync($"{testRunsUri}/{testRunId}", stringContent);

            if (httpResponse.StatusCode == HttpStatusCode.NoContent)
            {
                output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: PUT TestRun\tResponse StatusCode: '{httpResponse.StatusCode}'\tTestRunId: '{testRunId}'");
            }
            else
            {
                string userMessage = await httpResponse.Content.ReadAsStringAsync();
                output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: PUT TestRun\tUNEXPECTED Response StatusCode: '{httpResponse.StatusCode}'\tTestRunId: '{testRunId}'\tError:{userMessage}");
            }

            return httpResponse;
        }

        /// <summary>
        /// Delete a Test Run by Id.
        /// </summary>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="testRunsUri">The base TestRun Uri.</param>
        /// <param name="testRunId">The testRun ID.</param>
        /// <param name="output">The output.</param>
        /// <returns>the successful task value.</returns>
        public static async Task<HttpResponseMessage> DeleteTestRunById(this HttpClient httpClient, string testRunsUri, string testRunId, ITestOutputHelper output)
        {
            var httpResponse = await httpClient.DeleteAsync($"{testRunsUri}/{testRunId}");

            if (httpResponse.StatusCode == HttpStatusCode.NoContent || httpResponse.StatusCode == HttpStatusCode.NotFound)
            {
                output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: DELETE TestRun\tResponse StatusCode: '{httpResponse.StatusCode}'\tTestRunId: '{testRunId}'");
            }
            else
            {
                string userMessage = await httpResponse.Content.ReadAsStringAsync();
                output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: DELETE TestRun\tUNEXPECTED Response StatusCode: '{httpResponse.StatusCode}'\tTestRunId: '{testRunId}'\tError:{userMessage}");
            }

            return httpResponse;
        }

        /// <summary>
        /// Sends request to all TEntity Items.
        /// </summary>
        /// <typeparam name="TEntity">The Entity type.</typeparam>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="baseEntityUri">The endpoint Uri to the GetItem.</param>
        /// <param name="output">The output.</param>
        /// <returns>the task.</returns>
        public static async Task<HttpResponseMessage> GetAllItems<TEntity>(this HttpClient httpClient, string baseEntityUri, ITestOutputHelper output)
            where TEntity : BaseEntityModel
        {
            string entityName = typeof(TEntity).Name;

            HttpResponseMessage httpResponse = await httpClient.GetAsync(baseEntityUri);

            if (httpResponse.StatusCode == HttpStatusCode.OK || httpResponse.StatusCode == HttpStatusCode.NoContent)
            {
                output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: GET all {entityName}\tResponse StatusCode: '{httpResponse.StatusCode}'");
            }
            else
            {
                string userMessage = await httpResponse.Content.ReadAsStringAsync();
                output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: GET all {entityName}\tUNEXPECTED Response StatusCode: '{httpResponse.StatusCode}'\tError:{userMessage}");
            }

            return httpResponse;
        }

        /// <summary>
        /// Post LoadTestConfig.
        /// </summary>
        /// <typeparam name="TEntity">The Entity type.</typeparam>
        /// <typeparam name="TEntityPayload">The EntityPayload type.</typeparam>
        /// <param name="httpClient">The httpClient.</param>
        /// <param name="entityPayload">The Entity Payload.</param>
        /// <param name="baseEntityUri">The LoadTestConfigUri.</param>
        /// <param name="output">The output.</param>
        /// <returns>the task.</returns>
        public static async Task<HttpResponseMessage> PostEntity<TEntity, TEntityPayload>(this HttpClient httpClient, TEntityPayload entityPayload, string baseEntityUri, ITestOutputHelper output)
            where TEntityPayload : BasePayload
            where TEntity : BaseEntityModel
        {
            string entityName = typeof(TEntity).Name;

            string jsonLoadTestConfig = JsonConvert.SerializeObject(entityPayload);

            StringContent stringContent = new (jsonLoadTestConfig, Encoding.UTF8, "application/json");

            var httpResponse = await httpClient.PostAsync(baseEntityUri, stringContent);

            if (httpResponse.StatusCode == HttpStatusCode.Created)
            {
                output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: POST {entityName}\tResponse StatusCode: '{httpResponse.StatusCode}'");
            }
            else
            {
                string userMessage = await httpResponse.Content.ReadAsStringAsync();
                output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: POST {entityName}\tUNEXPECTED Response StatusCode: '{httpResponse.StatusCode}'\tError:{userMessage}");
            }

            return httpResponse;
        }

        /// <summary>
        /// Get Entity Item by ID.
        /// </summary>
        /// <typeparam name="TEntity">The Entity type.</typeparam>
        /// <param name="httpClient">The httpClient.</param>
        /// <param name="baseEntityUri">The endpoint Uri to the GetItem.</param>
        /// <param name="itemId">The LoadTestConfig ID.</param>
        /// <param name="output">The output.</param>
        /// <returns>the task.</returns>
        public static async Task<HttpResponseMessage> GetItemById<TEntity>(this HttpClient httpClient, string baseEntityUri, string itemId, ITestOutputHelper output)
            where TEntity : BaseEntityModel
        {
            string entityName = typeof(TEntity).Name;

            var httpResponse = await httpClient.GetAsync(baseEntityUri + "/" + itemId);

            if (httpResponse.StatusCode == HttpStatusCode.OK || httpResponse.StatusCode == HttpStatusCode.NotFound)
            {
                output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: GET {entityName} by ID\tResponse StatusCode: '{httpResponse.StatusCode}'\t{entityName}Id: '{itemId}'");
            }
            else
            {
                string userMessage = await httpResponse.Content.ReadAsStringAsync();
                output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: GET {entityName}\tUNEXPECTED Response StatusCode: '{httpResponse.StatusCode}'\t{entityName}Id: '{itemId}'\tError:{userMessage}");
            }

            return httpResponse;
        }

        /// <summary>
        /// Delete a Entity Item by Id.
        /// </summary>
        /// <typeparam name="TEntity">The Entity type.</typeparam>
        /// <param name="httpClient">The httpClient.</param>
        /// <param name="baseEntityUri">The endpoint Uri to Delete Item.</param>
        /// <param name="itemId">The LoadTestConfig ID.</param>
        /// <param name="output">The output.</param>
        /// <returns>the successful task value.</returns>
        public static async Task<HttpResponseMessage> DeleteItemById<TEntity>(this HttpClient httpClient, string baseEntityUri, string itemId, ITestOutputHelper output)
            where TEntity : BaseEntityModel
        {
            string entityName = typeof(TEntity).Name;
            var httpResponse = await httpClient.DeleteAsync($"{baseEntityUri}/{itemId}");

            if (httpResponse.StatusCode == HttpStatusCode.NoContent || httpResponse.StatusCode == HttpStatusCode.NotFound)
            {
                output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: DELETE {entityName}\tResponse StatusCode: '{httpResponse.StatusCode}'\t{entityName}Id: '{itemId}'");
            }
            else
            {
                string userMessage = await httpResponse.Content.ReadAsStringAsync();
                output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: DELETE {entityName}\tUNEXPECTED Response StatusCode: '{httpResponse.StatusCode}'\t{entityName}Id: '{itemId}'\tError:{userMessage}");
            }

            return httpResponse;
        }

        /// <summary>
        /// Puts the entity by item identifier.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TEntityPayload">The type of the entity payload.</typeparam>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="baseEntityUri">TThe endpoint Uri to do the  Put.</param>
        /// <param name="entityItemId">The entity item Id.</param>
        /// <param name="entityPayload">The entity payload.</param>
        /// <param name="output">The output.</param>
        /// <returns>the task.</returns>
        public static async Task<HttpResponseMessage> PutEntityByItemId<TEntity, TEntityPayload>(this HttpClient httpClient, string baseEntityUri, string entityItemId, TEntityPayload entityPayload,  ITestOutputHelper output)
             where TEntityPayload : BasePayload
             where TEntity : BaseEntityModel
        {
            string entityName = typeof(TEntity).Name;
            StringContent stringContent = new (JsonConvert.SerializeObject(entityPayload), Encoding.UTF8, "application/json");

            // Send Request
            var httpResponse = await httpClient.PutAsync($"{baseEntityUri}/{entityItemId}", stringContent);

            if (httpResponse.StatusCode == HttpStatusCode.NoContent)
            {
                output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: PUT {entityName}Id\tResponse StatusCode: '{httpResponse.StatusCode}'\t{entityName}Id: '{entityItemId}'");
            }
            else
            {
                string userMessage = await httpResponse.Content.ReadAsStringAsync();
                output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: PUT {entityName}\tUNEXPECTED Response StatusCode: '{httpResponse.StatusCode}'\t{entityName}Id: '{entityItemId}'\tError:{userMessage}");
            }

            return httpResponse;
        }

        /// <summary>
        /// Gets the test run by identifier retries asynchronous.
        /// </summary>
        /// <typeparam name="TEntity">The Entity type.</typeparam>
        /// <param name="httpClient">The httpClient.</param>
        /// <param name="baseEntityUri">The endpoint Uri to the GetItem.</param>
        /// <param name="itemId">The LoadTestConfig ID.</param>
        /// <param name="jsonOptions">The json options.</param>
        /// <param name="output">The output.</param>
        /// <param name="validateCondition">validateCondition delegate.</param>
        /// <param name="maxRetries">The maximum retries.</param>
        /// <param name="timeBetweenRequestsMs">The time between requests ms.</param>
        /// <returns>HttpResponseMessage and TEntity from response.</returns>
        public static async Task<(HttpResponseMessage, TEntity)> GetEntityByIdRetries<TEntity>(this HttpClient httpClient, string baseEntityUri, string itemId, JsonSerializerOptions jsonOptions, ITestOutputHelper output, Func<TEntity, Task<(bool result, string fieldName, string conditionalValue)>> validateCondition, int maxRetries = 10, int timeBetweenRequestsMs = 1000)
            where TEntity : BaseEntityModel
        {
            string entityName = typeof(TEntity).Name;

            HttpResponseMessage httpResponse = new ();
            TEntity testRun = null;

            var taskSource = new CancellationTokenSource();

            await Common.RunAndRetry(maxRetries, timeBetweenRequestsMs, taskSource, async (int attemptCount) =>
            {
                httpResponse = await httpClient.GetAsync($"{baseEntityUri}/{itemId}");

                if (httpResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: GET {entityName} by ID\tResponse StatusCode: 'NotFound'\t{entityName}Id: '{itemId}'\tAttempts: {attemptCount} [{timeBetweenRequestsMs}ms between requests]");
                }
                else if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    testRun = await httpResponse.Content.ReadFromJsonAsync<TEntity>(jsonOptions);

                    (bool validationResult, string fieldName, string actualValue) = await validateCondition(testRun);

                    if (validationResult)
                    {
                        output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: GET {entityName} by ID\tResponse StatusCode: 'OK'\t{entityName}Id: '{itemId}'\tAttempts: {attemptCount} [{timeBetweenRequestsMs}ms between requests]\t{fieldName} criteria met [{actualValue}]");
                        taskSource.Cancel();
                    }
                    else
                    {
                        output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: GET {entityName} by ID\tResponse StatusCode: 'OK'\t{entityName}Id: '{itemId}'\tAttempts: {attemptCount} [{timeBetweenRequestsMs}ms between requests]\t{fieldName} criteria not met [expected: not null, received: {actualValue}]");
                    }
                }
                else
                {
                    string userMessage = await httpResponse.Content.ReadAsStringAsync();
                    output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: GET {entityName} by ID\tUnhandled Response StatusCode: '{httpResponse.StatusCode}'\t{entityName}Id: '{itemId}'\tAttempts: {attemptCount} [{timeBetweenRequestsMs}ms between requests]\tError:{userMessage}");
                    taskSource.Cancel();
                }
            });

            return (httpResponse, testRun);
        }
    }
}

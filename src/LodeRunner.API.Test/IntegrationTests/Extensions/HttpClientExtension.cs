// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LodeRunner.API.Models;
using LodeRunner.API.Test.IntegrationTests.AutoMapper;
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
        /// <returns>HttpStatusCode and Client from response.</returns>
        public static (HttpStatusCode, Client) GetClientByIdRetries(this HttpClient httpClient, string clientsUri, string clientStatusId, ClientStatusType clientStatusType, JsonSerializerOptions jsonOptions, ITestOutputHelper output, int maxRetries = 10, int timeBetweenRequestsMs = 100)
        {
            int attempts = 0;
            HttpResponseMessage httpResponse;
            Client client = null;

            do
            {
                attempts++;
                httpResponse = httpClient.GetAsync($"{clientsUri}/{clientStatusId}").Result;

                if (httpResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: GET Client by ID\tResponse StatusCode: 'NotFound'\tClientStatusId: '{clientStatusId}'\tAttempts: {attempts} [{timeBetweenRequestsMs}ms between requests]");
                    Thread.Sleep(timeBetweenRequestsMs);
                }
                else if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    client = httpResponse.Content.ReadFromJsonAsync<Client>(jsonOptions).Result;

                    if (client.Status == clientStatusType)
                    {
                        output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: GET Client by ID\tResponse StatusCode: 'OK'\tClientStatusId: '{clientStatusId}'\tAttempts: {attempts} [{timeBetweenRequestsMs}ms between requests]\tStatusType criteria met [{client.Status}]");
                        break;
                    }
                    else
                    {
                        output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: GET Client by ID\tResponse StatusCode: 'OK'\tClientStatusId: '{clientStatusId}'\tAttempts: {attempts} [{timeBetweenRequestsMs}ms between requests]\tStatusType criteria not met [expected: {clientStatusType}, received: {client.Status}]");
                        Thread.Sleep(timeBetweenRequestsMs);
                    }
                }
                else
                {
                    output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: GET Client by ID\tUnhandled Response StatusCode: '{httpResponse.StatusCode}'\tClientStatusId: '{clientStatusId}'\tAttempts: {attempts} [{timeBetweenRequestsMs}ms between requests]");
                    break;
                }
            }
            while (attempts <= maxRetries);

            return (httpResponse.StatusCode, client);
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
                output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: GET all TestRun\tUNEXPECTED Response StatusCode: '{httpResponse.StatusCode}'");
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
                output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: GET TestRun\tUNEXPECTED Response StatusCode: '{httpResponse.StatusCode}'\tTestRunId: '{testRunId}'");
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
                output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: POST TestRun\tUNEXPECTED Response StatusCode: '{httpResponse.StatusCode}'");
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
                output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: PUT TestRun\tUNEXPECTED Response StatusCode: '{httpResponse.StatusCode}'\tTestRunId: '{testRunId}'");
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
                output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: DELETE TestRun\tUNEXPECTED Response StatusCode: '{httpResponse.StatusCode}'\tTestRunId: '{testRunId}'");
            }

            return httpResponse;
        }

        ///// <summary>
        ///// Get all LoadTestConfig.
        ///// </summary>
        ///// <param name="httpClient">The httpClient.</param>
        ///// <param name="loadTestConfigsUri">The LoadTestConfig Uri.</param>
        ///// <param name="output">The output.</param>
        ///// <returns>the task.</returns>
        ///// <summary>
        ///// <returns><see cref="Task"/> representing the asynchronous unit test.</returns>
        //public static async Task<HttpResponseMessage> GetLoadTestConfigs(this HttpClient httpClient, string loadTestConfigsUri, ITestOutputHelper output)
        //{
        //    HttpResponseMessage httpResponse = await httpClient.GetAsync(loadTestConfigsUri);

        //    if (httpResponse.StatusCode == HttpStatusCode.OK || httpResponse.StatusCode == HttpStatusCode.NoContent)
        //    {
        //        output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: GET all LoadTestConfig\tResponse StatusCode: '{httpResponse.StatusCode}'");
        //    }
        //    else
        //    {
        //        output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: GET all LoadTestConfig\tUNEXPECTED Response StatusCode: '{httpResponse.StatusCode}'");
        //    }

        //    return httpResponse;
        //}

        /// <summary>
        /// Sends request to all TEntity Items.
        /// </summary>
        /// <typeparam name="TEntity">The Entity type.</typeparam>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="baseEntityUri">The endpoint Uri to the GetItem.</param>
        /// <param name="output">The output.</param>
        /// <returns>the task.</returns>
        public static async Task<HttpResponseMessage> GetAllItems<TEntity>(this HttpClient httpClient, string baseEntityUri, ITestOutputHelper output)
        {
            string entityName = typeof(TEntity).Name;

            HttpResponseMessage httpResponse = await httpClient.GetAsync(baseEntityUri);

            if (httpResponse.StatusCode == HttpStatusCode.OK || httpResponse.StatusCode == HttpStatusCode.NoContent)
            {
                output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: GET all {entityName}\tResponse StatusCode: '{httpResponse.StatusCode}'");
            }
            else
            {
                output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: GET all {entityName}\tUNEXPECTED Response StatusCode: '{httpResponse.StatusCode}'");
            }

            return httpResponse;
        }

        /// <summary>
        /// Post LoadTestConfig.
        /// </summary>
        /// <param name="httpClient">The httpClient.</param>
        /// <param name="loadTestConfigUri">The LoadTestConfigUri.</param>
        /// <param name="output">The output.</param>
        /// <returns>the task.</returns>
        public static async Task<HttpResponseMessage> PostLoadTestConfig(this HttpClient httpClient, string loadTestConfigUri, ITestOutputHelper output)
        {
            LoadTestConfig loadTestConfig = new ();

            loadTestConfig.SetMockData($"Sample LoadTestConfig - IntegrationTesting-{nameof(PostLoadTestConfig)}-{DateTime.UtcNow:yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK}");

            var loadTestConfigPayload = loadTestConfig.AutomapAndGetLoadTestConfigTestPayload();

            string jsonLoadTestConfig = JsonConvert.SerializeObject(loadTestConfigPayload);
            StringContent stringContent = new (jsonLoadTestConfig, Encoding.UTF8, "application/json");

            var httpResponse = await httpClient.PostAsync(loadTestConfigUri, stringContent);

            if (httpResponse.StatusCode == HttpStatusCode.Created)
            {
                output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: POST LoadTestConfig\tResponse StatusCode: '{httpResponse.StatusCode}'");
            }
            else
            {
                output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: POST LoadTestConfig\tUNEXPECTED Response StatusCode: '{httpResponse.StatusCode}'");
            }

            return httpResponse;
        }


        /// <summary>
        /// Post LoadTestConfig.
        /// </summary>
        /// <typeparam name="TEntity">The Entity type.</typeparam>
        /// <typeparam name="TEntityPayload">The Entity Payload type.</typeparam>
        /// <param name="httpClient">The httpClient.</param>
        /// <param name="baseEntityUri">The LoadTestConfigUri.</param>
        /// <param name="output">The output.</param>
        /// <returns>the task.</returns>
        public static async Task<HttpResponseMessage> PostEntity<TEntity, TEntityPayload>(this HttpClient httpClient, string baseEntityUri, ITestOutputHelper output)
            where TEntity : BaseEntityModel, new()
            where TEntityPayload : BasePayload
        {
            string entityName = typeof(TEntity).Name;

            TEntity entitySource = new ();

            // TODO: How to set mock data for Generic BaseEntityModel ???

            //entitySource.SetMockData($"Sample {entityName} - IntegrationTesting-{nameof(PostLoadTestConfig)}-{DateTime.UtcNow:yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK}");

            TEntityPayload entityPayload = BasePayloadAutoMapperHelper<TEntity, TEntityPayload>.Map(entitySource);

            string jsonLoadTestConfig = JsonConvert.SerializeObject(entityPayload);

            StringContent stringContent = new (jsonLoadTestConfig, Encoding.UTF8, "application/json");

            var httpResponse = await httpClient.PostAsync(baseEntityUri, stringContent);

            if (httpResponse.StatusCode == HttpStatusCode.Created)
            {
                output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: POST {entityName}\tResponse StatusCode: '{httpResponse.StatusCode}'");
            }
            else
            {
                output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: POST {entityName}\tUNEXPECTED Response StatusCode: '{httpResponse.StatusCode}'");
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
        {
            string entityName = typeof(TEntity).Name;

            var httpResponse = await httpClient.GetAsync(baseEntityUri + "/" + itemId);

            if (httpResponse.StatusCode == HttpStatusCode.OK || httpResponse.StatusCode == HttpStatusCode.NotFound)
            {
                output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: GET {entityName} by ID\tResponse StatusCode: '{httpResponse.StatusCode}'\t{entityName}Id: '{itemId}'");
            }
            else
            {
                output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: GET {entityName}\tUNEXPECTED Response StatusCode: '{httpResponse.StatusCode}'\t{entityName}Id: '{itemId}'");
            }

            return httpResponse;
        }

        ///// <summary>
        ///// Get LoadTestConfig by ID.
        ///// </summary>
        ///// <param name="httpClient">The httpClient.</param>
        ///// <param name="loadTestConfigsUri">The LoadTestConfigs Uri.</param>
        ///// <param name="loadTestConfigId">The LoadTestConfig ID.</param>
        ///// <param name="output">The output.</param>
        ///// <returns>the task.</returns>
        //public static async Task<HttpResponseMessage> GetLoadTestConfigById(this HttpClient httpClient, string loadTestConfigsUri, string loadTestConfigId, ITestOutputHelper output)
        //{
        //    var httpResponse = await httpClient.GetAsync(loadTestConfigsUri + "/" + loadTestConfigId);

        //    if (httpResponse.StatusCode == HttpStatusCode.OK || httpResponse.StatusCode == HttpStatusCode.NotFound)
        //    {
        //        output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: GET LoadTestConfig by ID\tResponse StatusCode: '{httpResponse.StatusCode}'\tLoadTestConfigId: '{loadTestConfigId}'");
        //    }
        //    else
        //    {
        //        output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: GET LoadTestConfig\tUNEXPECTED Response StatusCode: '{httpResponse.StatusCode}'\tLoadTestConfigId: '{loadTestConfigId}'");
        //    }

        //    return httpResponse;
        //}

        ///// <summary>
        ///// Delete a LoadTestConfig by Id.
        ///// </summary>
        ///// <param name="httpClient">The HTTP client.</param>
        ///// <param name="loadTestConfigsUri">The base LoadTestConfigs Uri.</param>
        ///// <param name="loadTestConfigId">The LoadTestConfig ID.</param>
        ///// <param name="output">The output.</param>
        ///// <returns>the successful task value.</returns>
        //public static async Task<HttpResponseMessage> DeleteLoadTestConfigById(this HttpClient httpClient, string loadTestConfigsUri, string loadTestConfigId, ITestOutputHelper output)
        //{
        //    var httpResponse = await httpClient.DeleteAsync($"{loadTestConfigsUri}/{loadTestConfigId}");

        //    if (httpResponse.StatusCode == HttpStatusCode.NoContent || httpResponse.StatusCode == HttpStatusCode.NotFound)
        //    {
        //        output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: DELETE LoadTestConfig\tResponse StatusCode: '{httpResponse.StatusCode}'\tLoadTestConfigId: '{loadTestConfigId}'");
        //    }
        //    else
        //    {
        //        output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: DELETE LoadTestConfig\tUNEXPECTED Response StatusCode: '{httpResponse.StatusCode}'\tLoadTestConfigId: '{loadTestConfigId}'");
        //    }

        //    return httpResponse;
        //}



        /// <summary>
        /// Delete a Entity Item by Id.
        /// </summary>
        /// <typeparam name="TEntity">The Entity type.</typeparam>
        /// <param name="httpClient">The httpClient.</param>
        /// <param name="baseEntityUri">The endpoint Uri to the GetItem.</param>
        /// <param name="itemId">The LoadTestConfig ID.</param>
        /// <param name="output">The output.</param>
        /// <returns>the successful task value.</returns>
        public static async Task<HttpResponseMessage> DeleteItemById<TEntity>(this HttpClient httpClient, string baseEntityUri, string itemId, ITestOutputHelper output)
        {
            string entityName = typeof(TEntity).Name;
            var httpResponse = await httpClient.DeleteAsync($"{baseEntityUri}/{itemId}");

            if (httpResponse.StatusCode == HttpStatusCode.NoContent || httpResponse.StatusCode == HttpStatusCode.NotFound)
            {
                output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: DELETE {entityName}\tResponse StatusCode: '{httpResponse.StatusCode}'\t{entityName}Id: '{itemId}'");
            }
            else
            {
                output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: DELETE {entityName}\tUNEXPECTED Response StatusCode: '{httpResponse.StatusCode}'\t{entityName}Id: '{itemId}'");
            }

            return httpResponse;
        }



        /// <summary>
        /// Put Load Test Config by ID.
        /// </summary>
        /// <param name="httpClient">the httpClient.</param>
        /// <param name="loadTestConfigUri">The LoadTestConfigId Uri.</param>
        /// <param name="loadTestConfigId">The loadTestConfig ID.</param>
        /// <param name="loadTestConfigPayload">the loadTestConfigPayload entity.</param>
        /// <param name="output">The output.</param>
        /// <returns>the task.</returns>
        public static async Task<HttpResponseMessage> PutLoadTestConfigById(this HttpClient httpClient, string loadTestConfigUri, string loadTestConfigId, LoadTestConfigPayload loadTestConfigPayload, ITestOutputHelper output)
        {
            StringContent stringContent = new (JsonConvert.SerializeObject(loadTestConfigPayload), Encoding.UTF8, "application/json");

            // Send Request
            var httpResponse = await httpClient.PutAsync($"{loadTestConfigUri}/{loadTestConfigId}", stringContent);

            if (httpResponse.StatusCode == HttpStatusCode.NoContent)
            {
                output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: PUT LoadTestConfigId\tResponse StatusCode: '{httpResponse.StatusCode}'\tLoadTestConfigId: '{loadTestConfigId}'");
            }
            else
            {
                output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: PUT LoadTestConfig\tUNEXPECTED Response StatusCode: '{httpResponse.StatusCode}'\tLoadTestConfigId: '{loadTestConfigId}'");
            }

            return httpResponse;
        }
    }
}

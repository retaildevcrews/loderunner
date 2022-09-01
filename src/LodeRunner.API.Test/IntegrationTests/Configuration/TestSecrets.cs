// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using LodeRunner.API.Models;
using LodeRunner.API.Test.IntegrationTests.ExecutingTestRun;
using LodeRunner.API.Test.IntegrationTests.Extensions;
using LodeRunner.Core.Models;
using Xunit;
using Xunit.Abstractions;

namespace LodeRunner.API.Test.IntegrationTests.Configuration
{
    /// <summary>
    /// Represents TestSecrets.
    /// </summary>
    public class TestSecrets : IClassFixture<ApiWebApplicationFactory<Startup>>
    {
        private readonly ApiWebApplicationFactory<Startup> factory;

        private readonly JsonSerializerOptions jsonOptions;

        private readonly ITestOutputHelper output;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestSecrets"/> class.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="output">The output.</param>
        public TestSecrets(ApiWebApplicationFactory<Startup> factory, ITestOutputHelper output)
        {
            this.factory = factory;

            this.output = output;

            this.jsonOptions = new()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
            };
            this.jsonOptions.Converters.Add(new JsonStringEnumConverter());
        }

        /// <summary>
        /// Determines whether the LR.APIinstance [can start since secrets are in place].
        /// </summary>
        /// <param name="apiHostCount">The number API hosts to utilized.</param>
        /// <returns><see cref="Task"/> representing the asynchronous integration test.</returns>
        [Trait("Category", "Integration")]
        [Theory]
        [InlineData(1)]
        public async Task CanStartLodeRunnerAPIInstanceWhenSecretsInPlace(int apiHostCount)
        {
            string secretsVolume = "secrets".GetSecretVolume();

            using var apiProcessContextCollection = new ApiProcessContextCollection(apiHostCount, secretsVolume, this.output);

            try
            {
                Assert.True(apiProcessContextCollection.Start(this.factory.GetNextAvailablePort), $"Api ProcessContext Collection.");

                foreach (var (hostId, portNumber, apiProcessContext) in apiProcessContextCollection)
                {
                    this.output.WriteLine($"Starting LodeRunner API for Host {hostId}.");

                    Assert.True(apiProcessContext.Started, $"Unable to start LodeRunner API Context for Host {hostId}.");

                    bool errorsFound = await LogOutputExtension.TryParseProcessErrors(apiProcessContext.Errors, this.output);

                    Assert.False(errorsFound, $"Errors found in LodeRunner API - Host {hostId} Output.{Environment.NewLine}{string.Join(",", apiProcessContext.Errors)}");

                    int apiListeningOnPort = await LogOutputExtension.TryParseProcessOutputAndGetAPIListeningPort(apiProcessContext.Output, this.output);

                    this.output.WriteLine($"No errors found for API Host {hostId}.");

                    Assert.True(apiListeningOnPort == portNumber, "Unable to get Port Number");
                }

                this.output.WriteLine($"LodeRunner.API is up and running. Passed validation.");
            }
            finally
            {
                apiProcessContextCollection.End();
            }
        }

        /// <summary>
        /// Determines whether the LR.APIinstance [cannot start when secrests are missing].
        /// </summary>
        /// <param name="apiHostCount">The number API hosts to utilized.</param>
        /// <returns><see cref="Task"/> representing the asynchronous integration test.</returns>
        [Trait("Category", "Integration")]
        [Theory]
        [InlineData(1)]
        public async Task CanNotStartLodeRunnerAPIInstanceWhenSecretsAreMissing(int apiHostCount)
        {
            string secretsVolume = SecretVolumeExtension.CreateIntegrationTestSecretsFolder();

            using var apiProcessContextCollection = new ApiProcessContextCollection(apiHostCount, secretsVolume, this.output);

            try
            {
                Assert.True(apiProcessContextCollection.Start(this.factory.GetNextAvailablePort), $"Api ProcessContext Collection.");

                foreach (var (hostId, portNumber, apiProcessContext) in apiProcessContextCollection)
                {
                    this.output.WriteLine($"Starting LodeRunner API for Host {hostId}.");

                    Assert.True(apiProcessContext.Started, $"Unable to start LodeRunner API Context for Host {hostId}.");

                    bool errorsFound = await LogOutputExtension.TryParseProcessErrors(apiProcessContext.Errors, this.output);

                    Assert.True(errorsFound, $"Expected errors, but no Errors found in LodeRunner API - Host {hostId} Output.");

                    // Get Exception message.
                    string exceptionMessage = await LogOutputExtension.TryParseProcessOutputAndGetValueFromFieldName(apiProcessContext.Errors, "LodeRunner.API.Program", "Exception", "Exception", this.output, 10, 1000);

                    bool validMessageFound = ValidateSecretsErrorMessage(exceptionMessage);

                    Assert.True(validMessageFound, $"Expected error message not found in LodeRunner API - Host {hostId} Output.{Environment.NewLine}{string.Join(",", apiProcessContext.Errors)}");
                }
            }
            finally
            {
                apiProcessContextCollection.End();
            }
        }

        /// <summary>
        /// Validate that the exception message correspond to a Secrests validatiion.
        /// </summary>
        /// <param name="exceptionMessage">The exception message.</param>
        /// <returns>whether message is Secrets validation.</returns>
        private static bool ValidateSecretsErrorMessage(string exceptionMessage)
        {
            return exceptionMessage.Contains(LodeRunner.Core.SystemConstants.UnableToReadSecretsFromVolume) ||
                exceptionMessage.Contains(LodeRunner.Core.SystemConstants.CosmosCollectionCannotBeEmpty) ||
                exceptionMessage.Contains(LodeRunner.Core.SystemConstants.CosmosDatabaseCannotBeEmpty) ||
                exceptionMessage.Contains(LodeRunner.Core.SystemConstants.CosmosKeyCannotBeEmpty) ||
                exceptionMessage.Contains(LodeRunner.Core.SystemConstants.CosmosUrlCannotBeEmpty) ||
                exceptionMessage.Contains(LodeRunner.Core.SystemConstants.InvalidCosmosUrl) ||
                exceptionMessage.Contains(LodeRunner.Core.SystemConstants.InvalidCosmosKey);
        }
    }
}

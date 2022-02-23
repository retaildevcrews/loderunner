// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using LodeRunner.Core.Models;
using LodeRunner.Data.Interfaces;
using LodeRunner.Services;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace LodeRunner.Test.UnitTests
{
    public class TestRunServiceTest
    {
        private readonly ITestOutputHelper output;
        private TestRunService service;

        // Sample Test Run Is Serialized To Enable Passing Clones Instead Of References
        private string validSerializedTestRun;

        public TestRunServiceTest(ITestOutputHelper output)
        {
            this.output = output;
            var mockedRepo = new Mock<ICosmosDBRepository>();
            this.service = new TestRunService(mockedRepo.Object);
            mockedRepo.Setup(m => m.IsCosmosDBReady).Returns(true);

            LoadClient validLoadClient = new ();
            validLoadClient.Name = "placeholder client name";
            validLoadClient.Version = "1.0";
            validLoadClient.Region = "placeholder region";
            validLoadClient.Zone = "placeholder zone";
            validLoadClient.Prometheus = true;
            validLoadClient.StartupArgs = "arg1";
            validLoadClient.StartTime = DateTime.UtcNow;

            LoadTestConfig validLoadTestConfig = new ();
            validLoadTestConfig.Files = new List<string> { "abc.txt" };
            validLoadTestConfig.Server = new List<string> { "abc.com" };

            TestRun validTestRun = new ();
            List<LoadClient> validLoadClients = new ();
            validLoadClients.Add(validLoadClient);

            validTestRun.CreatedTime = DateTime.UtcNow;
            validTestRun.StartTime = DateTime.UtcNow;
            validTestRun.Name = "abc";
            validTestRun.LoadTestConfig = validLoadTestConfig;
            validTestRun.LoadClients = validLoadClients;

            this.validSerializedTestRun = JsonSerializer.Serialize(validTestRun);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async void SuccessfulTestRunPost()
        {
            var validTestRun = JsonSerializer.Deserialize<TestRun>(this.validSerializedTestRun);
            var response = await this.service.Post(validTestRun, default(CancellationToken));
            this.output.WriteLine($"Errors: {response.Errors}");
            Assert.Null(response.Errors);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async void FailedTestRunPostInvalidId()
        {
            var invalidTestRun = JsonSerializer.Deserialize<TestRun>(this.validSerializedTestRun);
            invalidTestRun.Id = "abc";
            var response = await this.service.Post(invalidTestRun, default(CancellationToken));
            this.output.WriteLine($"Errors: {response.Errors}");
            Assert.NotEmpty(response.Errors);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async void FailedTestRunPostEmptyId()
        {
            var invalidTestRun = JsonSerializer.Deserialize<TestRun>(this.validSerializedTestRun);
            invalidTestRun.Id = string.Empty;
            var response = await this.service.Post(invalidTestRun, default(CancellationToken));
            this.output.WriteLine($"Errors: {response.Errors}");
            Assert.NotEmpty(response.Errors);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async void FailedTestRunPostEmptyName()
        {
            var invalidTestRun = JsonSerializer.Deserialize<TestRun>(this.validSerializedTestRun);
            invalidTestRun.Name = string.Empty;
            var response = await this.service.Post(invalidTestRun, default(CancellationToken));
            this.output.WriteLine($"Errors: {response.Errors}");
            Assert.NotEmpty(response.Errors);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async void FailedTestRunPostInvalidCreatedTime()
        {
            var invalidTestRun = JsonSerializer.Deserialize<TestRun>(this.validSerializedTestRun);
            invalidTestRun.CreatedTime = DateTime.MinValue;
            var response = await this.service.Post(invalidTestRun, default(CancellationToken));
            this.output.WriteLine($"Errors: {response.Errors}");
            Assert.NotEmpty(response.Errors);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async void FailedTestRunPostInvalidStartTime()
        {
            var invalidTestRun = JsonSerializer.Deserialize<TestRun>(this.validSerializedTestRun);
            invalidTestRun.StartTime = invalidTestRun.CreatedTime.AddDays(-1);
            var response = await this.service.Post(invalidTestRun, default(CancellationToken));
            this.output.WriteLine($"Errors: {response.Errors}");
            Assert.NotEmpty(response.Errors);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async void SuccessfulTestRunPostValidCompletedTime()
        {
            var invalidTestRun = JsonSerializer.Deserialize<TestRun>(this.validSerializedTestRun);
            invalidTestRun.CompletedTime = invalidTestRun.StartTime.AddDays(1);
            var response = await this.service.Post(invalidTestRun, default(CancellationToken));
            this.output.WriteLine($"Errors: {response.Errors}");
            Assert.Null(response.Errors);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async void FailedTestRunPostInvalidCompletedTime()
        {
            var invalidTestRun = JsonSerializer.Deserialize<TestRun>(this.validSerializedTestRun);
            invalidTestRun.CompletedTime = invalidTestRun.StartTime.AddDays(-1);
            var response = await this.service.Post(invalidTestRun, default(CancellationToken));
            this.output.WriteLine($"Errors: {response.Errors}");
            Assert.NotEmpty(response.Errors);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async void FailedTestRunPostInvalidNullLoadTestConfig()
        {
            var invalidTestRun = JsonSerializer.Deserialize<TestRun>(this.validSerializedTestRun);
            invalidTestRun.LoadTestConfig = new LoadTestConfig();
            var response = await this.service.Post(invalidTestRun, default(CancellationToken));
            this.output.WriteLine($"Errors: {response.Errors}");
            Assert.Null(response.Errors);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async void FailedTestRunPostInvalidNullLoadClients()
        {
            var invalidTestRun = JsonSerializer.Deserialize<TestRun>(this.validSerializedTestRun);
            invalidTestRun.LoadClients = new List<LoadClient>();
            var response = await this.service.Post(invalidTestRun, default(CancellationToken));
            this.output.WriteLine($"Errors: {response.Errors}");
            Assert.NotEmpty(response.Errors);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async void FailedTestRunPostDuplicateLoadClients()
        {
            var invalidTestRun = JsonSerializer.Deserialize<TestRun>(this.validSerializedTestRun);
            invalidTestRun.LoadClients.Add(invalidTestRun.LoadClients[0]);
            var response = await this.service.Post(invalidTestRun, default(CancellationToken));
            this.output.WriteLine($"Errors: {response.Errors}");
            Assert.NotEmpty(response.Errors);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async void SuccessfulTestRunPostDistinctLoadClients()
        {
            var invalidTestRun = JsonSerializer.Deserialize<TestRun>(this.validSerializedTestRun);
            LoadClient distinctLoadClient = JsonSerializer.Deserialize<LoadClient>(JsonSerializer.Serialize(invalidTestRun.LoadClients[0]));
            distinctLoadClient.Id = Guid.NewGuid().ToString();
            invalidTestRun.LoadClients.Add(distinctLoadClient);
            var response = await this.service.Post(invalidTestRun, default(CancellationToken));
            this.output.WriteLine($"Errors: {response.Errors}");
            Assert.Null(response.Errors);
        }
    }
}
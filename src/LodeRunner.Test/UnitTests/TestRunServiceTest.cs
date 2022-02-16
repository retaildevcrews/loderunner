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
        TestRunService service;


        // Sample Test Run Is Serialized To Enable Passing Clones Instead Of References
        string validSerializedTestRun;

        List<LoadClient> validLoadClients = new ();

        public TestRunServiceTest(ITestOutputHelper output) {
            this.output = output;
            var mockedRepo = new Mock<ICosmosDBRepository>();
            service = new TestRunService(mockedRepo.Object);
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
            validLoadTestConfig.Files = new List<string>{"abc.txt"};
            validLoadTestConfig.Server = new List<string>{"abc.com"};

            TestRun validTestRun = new ();
            List<LoadClient> validLoadClients = new ();
            validLoadClients.Add(validLoadClient);

            validTestRun.CreatedTime = DateTime.UtcNow;
            validTestRun.StartTime = DateTime.UtcNow;
            validTestRun.Name = "abc";
            validTestRun.LoadTestConfig = validLoadTestConfig;
            validTestRun.LoadClients = validLoadClients;

            validSerializedTestRun = JsonSerializer.Serialize(validTestRun);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async void SuccessfulTestRunPost()
        {
            var validTestRun = JsonSerializer.Deserialize<TestRun>(validSerializedTestRun);
            var response = await service.Post(validTestRun, new CancellationToken());
            output.WriteLine($"Errors: {response.Errors}");
            Assert.Null(response.Errors);

        }

        [Fact]
        [Trait("Category", "Unit")]
        public async void FailedTestRunPostInvalidId()
        {
            var invalidTestRun = JsonSerializer.Deserialize<TestRun>(validSerializedTestRun);
            invalidTestRun.Id = "abc";
            var response = await service.Post(invalidTestRun, new CancellationToken());
            output.WriteLine($"Errors: {response.Errors}");
            Assert.NotEmpty(response.Errors);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async void FailedTestRunPostEmptyId()
        {
            var invalidTestRun = JsonSerializer.Deserialize<TestRun>(validSerializedTestRun);
            invalidTestRun.Id = "";
            var response = await service.Post(invalidTestRun, new CancellationToken());
            output.WriteLine($"Errors: {response.Errors}");
            Assert.NotEmpty(response.Errors);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async void FailedTestRunPostEmptyName()
        {
            var invalidTestRun = JsonSerializer.Deserialize<TestRun>(validSerializedTestRun);
            invalidTestRun.Name = "";
            var response = await service.Post(invalidTestRun, new CancellationToken());
            output.WriteLine($"Errors: {response.Errors}");
            Assert.NotEmpty(response.Errors);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async void FailedTestRunPostInvalidCreatedTime()
        {
            var invalidTestRun = JsonSerializer.Deserialize<TestRun>(validSerializedTestRun);
            invalidTestRun.CreatedTime = DateTime.MinValue;
            var response = await service.Post(invalidTestRun, new CancellationToken());
            output.WriteLine($"Errors: {response.Errors}");
            Assert.NotEmpty(response.Errors);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async void FailedTestRunPostInvalidStartTime()
        {
            var invalidTestRun = JsonSerializer.Deserialize<TestRun>(validSerializedTestRun);
            invalidTestRun.StartTime = invalidTestRun.CreatedTime.AddDays(-1);
            var response = await service.Post(invalidTestRun, new CancellationToken());
            output.WriteLine($"Errors: {response.Errors}");
            Assert.NotEmpty(response.Errors);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async void SuccessfulTestRunPostValidCompletedTime()
        {
            var invalidTestRun = JsonSerializer.Deserialize<TestRun>(validSerializedTestRun);
            invalidTestRun.CompletedTime = invalidTestRun.StartTime.AddDays(1);
            var response = await service.Post(invalidTestRun, new CancellationToken());
            output.WriteLine($"Errors: {response.Errors}");
            Assert.Null(response.Errors);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async void FailedTestRunPostInvalidCompletedTime()
        {
            var invalidTestRun = JsonSerializer.Deserialize<TestRun>(validSerializedTestRun);
            invalidTestRun.CompletedTime = invalidTestRun.StartTime.AddDays(-1);
            var response = await service.Post(invalidTestRun, new CancellationToken());
            output.WriteLine($"Errors: {response.Errors}");
            Assert.NotEmpty(response.Errors);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async void FailedTestRunPostInvalidNullLoadTestConfig()
        {
            var invalidTestRun = JsonSerializer.Deserialize<TestRun>(validSerializedTestRun);
            invalidTestRun.LoadTestConfig = new LoadTestConfig();
            var response = await service.Post(invalidTestRun, new CancellationToken());
            output.WriteLine($"Errors: {response.Errors}");
            Assert.Null(response.Errors);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async void FailedTestRunPostInvalidNullLoadClients()
        {
            var invalidTestRun = JsonSerializer.Deserialize<TestRun>(validSerializedTestRun);
            invalidTestRun.LoadClients = new List<LoadClient>();
            var response = await service.Post(invalidTestRun, new CancellationToken());
            output.WriteLine($"Errors: {response.Errors}");
            Assert.NotEmpty(response.Errors);
        }
    }
}

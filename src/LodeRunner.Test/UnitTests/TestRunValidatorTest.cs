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
    /// <summary>
    /// Unit tests for testRun Validator.
    /// Input for the validator comes from autoMapper.
    /// TestRun instances should be validated before Create and Update.
    /// </summary>
    public class TestRunValidatorTest
    {
        private readonly ITestOutputHelper output;
        private readonly TestRunService service;

        // Sample Test Run Is Serialized To Enable Passing Clones Instead Of References
        private readonly string validSerializedTestRun;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestRunValidatorTest"/> class.
        /// Creates mock data set and repository.
        /// </summary>
        /// <param name="output">Test output handler.</param>
        public TestRunValidatorTest(ITestOutputHelper output)
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

        /// <summary>
        /// Test successful default validation.
        /// </summary>
        [Fact]
        [Trait("Category", "Unit")]
        public void SuccessfulTestRunValidation()
        {
            var validTestRun = JsonSerializer.Deserialize<TestRun>(this.validSerializedTestRun);
            var errors = this.service.Validator.ValidateEntity(validTestRun);
            this.output.WriteLine($"Errors: {string.Join(", ", errors)}");
            Assert.Empty(errors);
        }

        /// <summary>
        /// Test failed validation: Invalid ID.
        /// </summary>
        [Fact]
        [Trait("Category", "Unit")]
        public void FailedTestRunValidationInvalidId()
        {
            var invalidTestRun = JsonSerializer.Deserialize<TestRun>(this.validSerializedTestRun);
            invalidTestRun.Id = "abc";
            var errors = this.service.Validator.ValidateEntity(invalidTestRun);
            this.output.WriteLine($"Errors: {string.Join(", ", errors)}");
            Assert.NotEmpty(errors);
        }

        /// <summary>
        /// Test failed validation: Empty ID.
        /// </summary>
        [Fact]
        [Trait("Category", "Unit")]
        public void FailedTestRunValidationEmptyId()
        {
            var invalidTestRun = JsonSerializer.Deserialize<TestRun>(this.validSerializedTestRun);
            invalidTestRun.Id = string.Empty;
            var errors = this.service.Validator.ValidateEntity(invalidTestRun);
            this.output.WriteLine($"Errors: {string.Join(", ", errors)}");
            Assert.NotEmpty(errors);
        }

        /// <summary>
        /// Test failed validation: Empty "Name" field.
        /// </summary>
        [Fact]
        [Trait("Category", "Unit")]
        public void FailedTestRunValidationEmptyName()
        {
            var invalidTestRun = JsonSerializer.Deserialize<TestRun>(this.validSerializedTestRun);
            invalidTestRun.Name = string.Empty;
            var errors = this.service.Validator.ValidateEntity(invalidTestRun);
            this.output.WriteLine($"Errors: {string.Join(", ", errors)}");
            Assert.NotEmpty(errors);
        }

        /// <summary>
        /// Test failed validation: Invalid "CreatedTime" value.
        /// </summary>
        [Fact]
        [Trait("Category", "Unit")]
        public void FailedTestRunValidationInvalidCreatedTime()
        {
            var invalidTestRun = JsonSerializer.Deserialize<TestRun>(this.validSerializedTestRun);
            invalidTestRun.CreatedTime = DateTime.MinValue;
            var errors = this.service.Validator.ValidateEntity(invalidTestRun);
            this.output.WriteLine($"Errors: {string.Join(", ", errors)}");
            Assert.NotEmpty(errors);
        }

        /// <summary>
        /// Test failed validation: Invalid "StartTime" value.
        /// </summary>
        [Fact]
        [Trait("Category", "Unit")]
        public void FailedTestRunValidationInvalidStartTime()
        {
            var invalidTestRun = JsonSerializer.Deserialize<TestRun>(this.validSerializedTestRun);
            invalidTestRun.StartTime = invalidTestRun.CreatedTime.AddDays(-1);
            var errors = this.service.Validator.ValidateEntity(invalidTestRun);
            this.output.WriteLine($"Errors: {string.Join(", ", errors)}");
            Assert.NotEmpty(errors);
        }

        /// <summary>
        /// Test successful validation: Valid "CompletedTime" value.
        /// </summary>
        [Fact]
        [Trait("Category", "Unit")]
        public void SuccessfulTestRunValidationValidCompletedTime()
        {
            var validTestRun = JsonSerializer.Deserialize<TestRun>(this.validSerializedTestRun);
            validTestRun.CompletedTime = validTestRun.StartTime.AddDays(1);
            var errors = this.service.Validator.ValidateEntity(validTestRun);
            this.output.WriteLine($"Errors: {string.Join(", ", errors)}");
            Assert.Empty(errors);
        }

        /// <summary>
        /// Test failed validation: Invalid "CompletedTime" value.
        /// </summary>
        [Fact]
        [Trait("Category", "Unit")]
        public void FailedTestRunValidationInvalidCompletedTime()
        {
            var invalidTestRun = JsonSerializer.Deserialize<TestRun>(this.validSerializedTestRun);
            invalidTestRun.CompletedTime = invalidTestRun.StartTime.AddDays(-1);
            var errors = this.service.Validator.ValidateEntity(invalidTestRun);
            this.output.WriteLine($"Errors: {string.Join(", ", errors)}");
            Assert.NotEmpty(errors);
        }

        /// <summary>
        /// Test failed validation: Null LoadTestConfig.
        /// </summary>
        [Fact]
        [Trait("Category", "Unit")]
        public void FailedTestRunValidationInvalidNullLoadTestConfig()
        {
            var validTestRun = JsonSerializer.Deserialize<TestRun>(this.validSerializedTestRun);
            validTestRun.LoadTestConfig = new LoadTestConfig();
            var errors = this.service.Validator.ValidateEntity(validTestRun);
            this.output.WriteLine($"Errors: {string.Join(", ", errors)}");
            Assert.Empty(errors);
        }

        /// <summary>
        /// Test failed validation: Null LoadClients.
        /// </summary>
        [Fact]
        [Trait("Category", "Unit")]
        public void FailedTestRunValidationInvalidNullLoadClients()
        {
            var invalidTestRun = JsonSerializer.Deserialize<TestRun>(this.validSerializedTestRun);
            invalidTestRun.LoadClients = new List<LoadClient>();
            var errors = this.service.Validator.ValidateEntity(invalidTestRun);
            this.output.WriteLine($"Errors: {string.Join(", ", errors)}");
            Assert.NotEmpty(errors);
        }

        /// <summary>
        /// Test failed validation: Duplicate LoadClients.
        /// </summary>
        [Fact]
        [Trait("Category", "Unit")]
        public void FailedTestRunValidationDuplicateLoadClients()
        {
            var invalidTestRun = JsonSerializer.Deserialize<TestRun>(this.validSerializedTestRun);
            invalidTestRun.LoadClients.Add(invalidTestRun.LoadClients[0]);
            var errors = this.service.Validator.ValidateEntity(invalidTestRun);
            this.output.WriteLine($"Errors: {string.Join(", ", errors)}");
            Assert.NotEmpty(errors);
        }

        /// <summary>
        /// Test successful validation: Distinct LoadClients.
        /// </summary>
        [Fact]
        [Trait("Category", "Unit")]
        public void SuccessfulTestRunValidationDistinctLoadClients()
        {
            var validTestRun = JsonSerializer.Deserialize<TestRun>(this.validSerializedTestRun);
            LoadClient distinctLoadClient = JsonSerializer.Deserialize<LoadClient>(JsonSerializer.Serialize(validTestRun.LoadClients[0]));
            distinctLoadClient.Id = Guid.NewGuid().ToString();
            validTestRun.LoadClients.Add(distinctLoadClient);
            var errors = this.service.Validator.ValidateEntity(validTestRun);
            this.output.WriteLine($"Errors: {string.Join(", ", errors)}");
            Assert.Empty(errors);
        }
    }
}

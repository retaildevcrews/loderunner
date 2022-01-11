// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using FluentValidation;
using LodeRunner.Core.Interfaces;

namespace LodeRunner.Core.Models.Validators
{
    /// <summary>
    /// TestRunValidator.
    /// </summary>
    public class TestRunValidator : BaseEntityValidator<TestRun>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestRunValidator"/> class.
        /// </summary>
        public TestRunValidator()
        {
            this.RuleFor(m => m.Id).NotEmpty();
            this.RuleFor(m => m.PartitionKey).NotEmpty();
            this.RuleFor(m => m.EntityType).IsInEnum();
            this.RuleFor(m => m.CreatedTime).GreaterThan(DateTime.MinValue).WithMessage("Creation time is required.");
            this.RuleFor(m => m.LoadTestConfig).SetValidator(new LoadTestConfigValidator());
            this.RuleForEach(m => m.LoadClients).SetValidator(new LoadClientValidator());
            this.RuleForEach(m => m.ClientResults).SetValidator(new LoadResultValidator());
        }
    }
}

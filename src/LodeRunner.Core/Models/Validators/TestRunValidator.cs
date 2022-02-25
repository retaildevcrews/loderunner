// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
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
            var minDate = new DateTime(1990, 1, 1);

            // Validations for BaseEntityModel Fields
            this.RuleFor(m => m.Id).NotEmpty();
            this.RuleFor(m => m.Id)
                .Must(id => Guid.TryParse(id, out _))
                .WithMessage("Id must be GUID.");
            this.RuleFor(m => m.PartitionKey).NotEmpty();
            this.RuleFor(m => m.EntityType).IsInEnum();
            this.RuleFor(m => m.Name).NotEmpty();

            // Validation For Non-Entity Properties in TestRun
            this.RuleFor(m => m.CreatedTime).GreaterThan(minDate);
            this.RuleFor(m => new { m.StartTime, m.CreatedTime })
                .Must(m => m.StartTime >= m.CreatedTime)
                .WithMessage("Start Time Must Be Greater Than Or Equal To Created Time.");
            this.RuleFor(m => new { m.CompletedTime, m.StartTime })
                .Must(m => m.CompletedTime >= m.StartTime)
                .When(m => m.CompletedTime != null)
                .WithMessage("Completed Time Must Be Greater Than Or Equal To Start Time.");

            // Validation For Entity Properties in TestRun
            this.RuleFor(m => m.LoadTestConfig)
                .NotEmpty();
            this.RuleFor(m => m.LoadTestConfig).SetValidator(new LoadTestConfigValidator());
            this.RuleFor(m => m.LoadClients)
                .NotEmpty()
                .Must(loadClients =>
                {
                    var loadClientIds = loadClients.Select(loadClient => loadClient.Id);
                    bool isListDistinct = loadClientIds.Distinct().SequenceEqual(loadClientIds);
                    return isListDistinct;
                }).WithMessage("Cannot Have Duplicate Load Clients.");
            this.RuleForEach(m => m.LoadClients).SetValidator(new LoadClientValidator());

            // ClientResults Is Optional
            this.RuleForEach(m => m.ClientResults).SetValidator(new LoadResultValidator());
        }
    }
}

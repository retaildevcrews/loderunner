// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentValidation;

namespace LodeRunner.Core.Models.Validators
{
    /// <summary>
    /// LoadTestConfigValidator.
    /// </summary>
    public class LoadTestConfigValidator : BaseEntityValidator<LoadTestConfig>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadTestConfigValidator"/> class.
        /// </summary>
        public LoadTestConfigValidator()
        {
            this.RuleFor(m => m.Id)
                .NotEmpty();
            this.RuleFor(m => m.PartitionKey)
                .NotEmpty();
            this.RuleFor(m => m.EntityType)
                .IsInEnum();
            this.RuleForEach(m => m.Files)
                .NotNull().WithMessage("Files collection is required.");
            this.RuleForEach(m => m.Server)
                .NotNull().WithMessage("Server collection is required.");
        }
    }
}

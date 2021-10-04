// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using FluentValidation;
using LodeRunner.Core.Interfaces;

namespace LodeRunner.Core.Models.Validators
{
    /// <summary>
    /// LoadClientValidator.
    /// </summary>
    public class LoadClientValidator : BaseEntityValidator<LoadClient>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadClientValidator"/> class.
        /// </summary>
        public LoadClientValidator()
        {
            var minDate = new DateTime(1990, 1, 1);

            this.RuleFor(m => m.Id)
                .NotEmpty();
            this.RuleFor(m => m.EntityType)
                .IsInEnum();
            this.RuleFor(m => m.Version)
                .NotEmpty();
            this.RuleFor(m => m.Region)
                .NotEmpty();
            this.RuleFor(m => m.StartupArgs)
                .NotEmpty();
            this.RuleFor(m => m.StartTime)
                .GreaterThan(minDate);
        }
    }
}

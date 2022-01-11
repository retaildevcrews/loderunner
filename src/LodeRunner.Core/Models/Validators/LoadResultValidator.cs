// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using FluentValidation;

namespace LodeRunner.Core.Models.Validators
{
    /// <summary>
    /// LoadResultValidator.
    /// </summary>
    public class LoadResultValidator : BaseEntityValidator<LoadResult>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadResultValidator"/> class.
        /// </summary>
        public LoadResultValidator()
        {
            this.RuleFor(m => m.LoadClient).SetValidator(new LoadClientValidator());
            this.When(m => m.CompletedTime > DateTime.MinValue, () =>
            {
                this.RuleFor(m => m.TotalRequests).GreaterThan(0);
            });
        }
    }
}

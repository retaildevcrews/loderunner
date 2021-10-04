// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using FluentValidation;
using LodeRunner.Core.Interfaces;

namespace LodeRunner.Core.Models.Validators
{
    /// <summary>
    /// ClientStatusValidator.
    /// </summary>
    public class ClientStatusValidator : BaseEntityValidator<ClientStatus>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClientStatusValidator"/> class.
        /// </summary>
        public ClientStatusValidator()
        {
            var minDate = new DateTime(1990, 1, 1);

            this.RuleFor(m => m.Id)
                .NotEmpty();
            this.RuleFor(m => m.PartitionKey)
                .NotEmpty();
            this.RuleFor(m => m.EntityType)
                .IsInEnum();
            this.RuleFor(m => m.LastUpdated)
                .GreaterThan(minDate);
            this.RuleFor(m => m.StatusDuration)
                .GreaterThan(0);
            this.RuleFor(m => m.Status).
                IsInEnum();
            this.RuleFor(m => m.LoadClient)
                .NotNull()
                .SetValidator(new LoadClientValidator());
        }
    }
}

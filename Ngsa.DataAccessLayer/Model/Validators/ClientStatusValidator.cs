// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Ngsa.LodeRunner.DataAccessLayer.Interfaces;
using Ngsa.LodeRunner.DataAccessLayer.Model;

namespace Ngsa.DataAccessLayer.Model.Validators
{
    /// <summary>
    /// ClientStatusValidator.
    /// </summary>
    public class ClientStatusValidator : AbstractValidator<ClientStatus>, IModelValidator<ClientStatus>
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

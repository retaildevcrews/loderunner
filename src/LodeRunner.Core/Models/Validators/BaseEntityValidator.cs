// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using LodeRunner.Core.Interfaces;

namespace LodeRunner.Core.Models.Validators
{
    /// <summary>
    ///  Represents the Base Entity Validator implementation.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public abstract class BaseEntityValidator<TEntity> : AbstractValidator<TEntity>, IModelValidator<TEntity>
    where TEntity : class
    {
        /// <summary>
        /// Validates entity and returns a list of error messages.
        /// </summary>
        /// <value>
        /// </value>
        /// <returns></returns>
        public IEnumerable<string> ValidateEntity(TEntity entity)
        {
            var errors = this.Validate(entity)?.Errors.ToList();
            List<string> errorMessages = new ();

            if (errors.Count > 0)
            {
                errorMessages = errors.Select(x => $"{x.PropertyName} - {x.ErrorMessage}").ToList<string>();
            }

            return errorMessages;
        }
    }
}

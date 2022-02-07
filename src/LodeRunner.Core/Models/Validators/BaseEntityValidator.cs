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
        /// Gets the error message as a list of strings including the Entity Property-ErrorMessage.
        /// </summary>
        /// <value>
        /// The error message.
        /// </value>
        public IEnumerable<string> ErrorMessages
        {
            get
            {
                var errors = this.ValidationResult?.Errors.ToList();
                List<string> errorMessages = new ();

                if (errors.Count > 0)
                {
                    errorMessages = errors.Select(x => $"{x.PropertyName} - {x.ErrorMessage}").ToList<string>();
                }

                return errorMessages;
            }
        }

        /// <summary>
        /// Gets a value indicating whether validation succeeded.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public bool IsValid => this.ValidationResult != null && this.ValidationResult.IsValid;

        /// <summary>
        /// Gets the validation result.
        /// </summary>
        /// <value>
        /// The validation result.
        /// </value>
        public ValidationResult ValidationResult { get; private set; }

        /// <summary>
        /// Validates the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>
        /// True if valid, otherwise false.
        /// </returns>
        public bool ValidateEntity(TEntity entity)
        {
            this.ValidationResult = this.Validate(entity);
            return this.ValidationResult.IsValid;
        }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentValidation.Results;

namespace LodeRunner.Core.Interfaces
{
    /// <summary>
    /// Model Validator Interface.
    /// </summary>
    /// <typeparam name="TEntity">Entity.</typeparam>
    public interface IModelValidator<TEntity>
    {
        /// <summary>
        /// Gets the error message.
        /// </summary>
        /// <value>
        /// The error message.
        /// </value>
        string ErrorMessage { get;  }

        /// <summary>
        /// Gets a value indicating whether validation succeeded.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        bool IsValid { get;  }

        /// <summary>
        /// Gets the validation result.
        /// </summary>
        /// <value>
        /// The validation result.
        /// </value>
        ValidationResult ValidationResult { get; }

        /// <summary>
        /// Validates the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>True if valid, otherwise false.</returns>
        public bool ValidateEntity(TEntity entity);
    }
}

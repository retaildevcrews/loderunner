// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentValidation.Results;

namespace LodeRunner.Data.Interfaces
{
    /// <summary>
    /// Model Validator Interface.
    /// </summary>
    /// <typeparam name="T">Entity.</typeparam>
    public interface IModelValidator<T>
    {
        /// <summary>
        /// Validates the specified model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>Validation Result.</returns>
        ValidationResult Validate(T model);
    }
}

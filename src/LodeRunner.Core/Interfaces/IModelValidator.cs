// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
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
        /// Validates entity and returns a list of error messages.
        /// </summary>
        /// <value>
        /// The error messages from validation.
        /// </value>
        IEnumerable<string> ValidateEntity(TEntity entity);
    }
}

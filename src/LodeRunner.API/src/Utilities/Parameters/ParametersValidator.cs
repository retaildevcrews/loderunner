// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using LodeRunner.API.Middleware.Validation;

namespace LodeRunner.API.Middleware
{
    /// <summary>
    /// Validates string parameters for the given TEntity used from different controllers.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public class ParametersValidator<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// Validate Id.
        /// </summary>
        /// <param name="entityId">id to validate.</param>
        /// <returns>empty list on valid.</returns>
        public static List<ValidationError> ValidateEntityId(string entityId)
        {
            List<ValidationError> errors = new ();

            string entityIdFieldName = $"{typeof(TEntity).Name}Id";

            if (!IsEntityIdValid(entityId))
            {
                errors.Add(new ValidationError() { Target = entityIdFieldName, Message = ValidationError.GetErrorMessage(entityIdFieldName) });
            }

            return errors;
        }

        /// <summary>
        /// Check if Entity ID is Valid.
        /// </summary>
        /// <param name="entityId">id to validate.</param>
        /// <returns>true on valid.</returns>
        private static bool IsEntityIdValid(string entityId)
        {
            if (string.IsNullOrWhiteSpace(entityId))
            {
                return false;
            }

            Guid guidValue;
            try
            {
                guidValue = Guid.Parse(entityId);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}

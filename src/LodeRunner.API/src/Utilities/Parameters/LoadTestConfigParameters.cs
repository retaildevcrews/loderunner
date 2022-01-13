// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using LodeRunner.API.Middleware.Validation;

namespace LodeRunner.API.Middleware
{
    /// <summary>
    /// Query string parameters for LoadTestConfigs controller.
    /// </summary>
    public sealed class LoadTestConfigParameters
    {
        /// <summary>
        /// Check if LoadTestConfig ID is Valid.
        /// </summary>
        /// <param name="loadTestConfigId">id to validate.</param>
        /// <returns>true on valid.</returns>
        public static bool IsLoadTestConfigIdValid(string loadTestConfigId)
        {
            if (string.IsNullOrWhiteSpace(loadTestConfigId))
            {
                return false;
            }

            Guid guidValue;
            try
            {
               guidValue = Guid.Parse(loadTestConfigId);
            }
            catch (Exception)
            {
               return false;
            }

            return true;
        }

        /// <summary>
        /// Validate Id.
        /// </summary>
        /// <param name="loadTestConfigId">id to validate.</param>
        /// <returns>empty list on valid.</returns>
        public static List<ValidationError> ValidateLoadTestConfigId(string loadTestConfigId)
        {
            List<ValidationError> errors = new ();

            if (!IsLoadTestConfigIdValid(loadTestConfigId))
            {
                errors.Add(new ValidationError() { Target = "loadTestConfigId", Message = ValidationError.GetErrorMessage("loadTestConfigId") });
            }

            return errors;
        }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using LodeRunner.API.Middleware.Validation;

namespace LodeRunner.API.Middleware
{
    /// <summary>
    /// Query string parameters for Clients controller.
    /// </summary>
    public sealed class ClientParameters
    {
        /// <summary>
        /// Check if ClientStatus ID is Valid.
        /// </summary>
        /// <param name="clientStatusId">id to validate.</param>
        /// <returns>true on valid.</returns>
        public static bool IsClientStatusIdValid(string clientStatusId)
        {
            if (string.IsNullOrWhiteSpace(clientStatusId))
            {
                return false;
            }

            Guid guidValue;
            try
            {
               guidValue = Guid.Parse(clientStatusId);
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
        /// <param name="clientStatusId">id to validate.</param>
        /// <returns>empty list on valid.</returns>
        public static List<ValidationError> ValidateClientStatusId(string clientStatusId)
        {
            List<ValidationError> errors = new ();

            if (!IsClientStatusIdValid(clientStatusId))
            {
                errors.Add(new ValidationError() { Target = "clientStatusId", Message = ValidationError.GetErrorMessage("clientStatusId") });
            }

            return errors;
        }
    }
}

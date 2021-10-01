// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
        /// Validate Id.
        /// </summary>
        /// <param name="clientStatusId">id to validate.</param>
        /// <returns>empty list on valid.</returns>
        public static List<ValidationError> ValidateClientStatusId(string clientStatusId)
        {
            List<ValidationError> errors = new ();

            if (string.IsNullOrWhiteSpace(clientStatusId))
            {
                errors.Add(new ValidationError() { Target = "clientStatusId", Message = ValidationError.GetErrorMessage("clientStatusId") });
            }

            return errors;
        }
    }
}

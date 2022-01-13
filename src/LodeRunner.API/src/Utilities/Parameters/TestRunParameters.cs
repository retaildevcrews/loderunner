// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using LodeRunner.API.Middleware.Validation;

namespace LodeRunner.API.Middleware
{
    /// <summary>
    /// Query string parameters for TestRuns controller.
    /// </summary>
    public sealed class TestRunParameters
    {
        /// <summary>
        /// Check if TestRun ID is Valid.
        /// </summary>
        /// <param name="testRunId">id to validate.</param>
        /// <returns>true on valid.</returns>
        public static bool IsTestRunIdValid(string testRunId)
        {
            if (string.IsNullOrWhiteSpace(testRunId))
            {
                return false;
            }

            Guid guidValue;
            try
            {
               guidValue = Guid.Parse(testRunId);
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
        /// <param name="testRunId">id to validate.</param>
        /// <returns>empty list on valid.</returns>
        public static List<ValidationError> ValidateTestRunId(string testRunId)
        {
            List<ValidationError> errors = new ();

            if (!IsTestRunIdValid(testRunId))
            {
                errors.Add(new ValidationError() { Target = "testRunId", Message = ValidationError.GetErrorMessage("testRunId") });
            }

            return errors;
        }
    }
}

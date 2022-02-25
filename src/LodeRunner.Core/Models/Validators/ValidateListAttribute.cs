// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LodeRunner.Core.Models.Validators
{
    /// <summary>
    /// Represents the Custom Attribute Validation for String List.
    /// </summary>
    public class ValidateListAttribute : ValidationAttribute
    {
        /// <summary>
        /// Determines if the associated String List is valid.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="validationContext">The validation context.</param>
        /// <returns>Success if Associated String List passes validation, otherwise error.</returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is IList list && list.Count > 0)
            {
                return ValidationResult.Success;
            }
            else
            {
                return new ValidationResult(this.ErrorMessage);
            }
        }
    }
}

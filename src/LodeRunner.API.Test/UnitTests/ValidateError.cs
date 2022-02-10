// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LodeRunner.API.Middleware.Validation;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace LodeRunner.API.Test.UnitTests
{
    /// <summary>
    /// ValidationError Unit Test.
    /// </summary>
    public class ValidateError
    {
        private const string CategoryMismatch = "Category mismatch.";
        private const string ModeMismatch = "Mode mismatch.";
        private const string RandomPath = "some/random/path";
        private const string InvalidFieldName = "InvalidFieldName";

        /// <summary>
        /// Test the success case of get error message.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="expected">The expected.</param>
        [Theory]
        [Trait("Category", "Unit")]
        [InlineData(SystemConstants.ClientStatusID, SystemConstants.ErrorMessageGuid)]
        [InlineData(SystemConstants.LoadTestConfigID, SystemConstants.ErrorMessageGuid)]
        [InlineData(SystemConstants.TestRunID, SystemConstants.ErrorMessageGuid)]
        [InlineData(InvalidFieldName, SystemConstants.ErrorMessageUnknownParameter)]
        public void GetErrorMessage_Success(string input, string expected)
        {
            string result = ValidationError.GetErrorMessage(input);

            Assert.Contains(expected, result);
        }

        /// <summary>
        /// Test the failure case of get error message.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="expected">The expected.</param>
        [Theory]
        [Trait("Category", "Unit")]
        [InlineData(InvalidFieldName, SystemConstants.ErrorMessageGuid)]
        [InlineData(SystemConstants.ClientStatusID, SystemConstants.ErrorMessageUnknownParameter)]
        [InlineData(SystemConstants.LoadTestConfigID, SystemConstants.ErrorMessageUnknownParameter)]
        [InlineData(SystemConstants.TestRunID, SystemConstants.ErrorMessageUnknownParameter)]
        public void GetErrorMessage_Failure(string input, string expected)
        {
            string result = ValidationError.GetErrorMessage(input);

            Assert.DoesNotContain(expected, result);
        }

        /// <summary>
        /// Test the success case of get error link.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="expected">The expected.</param>
        [Theory]
        [Trait("Category", "Unit")]
        [InlineData(SystemConstants.ErrorLinkPath, SystemConstants.ErrorLink + SystemConstants.ErrorLinkPathAnchor)]
        [InlineData(RandomPath, SystemConstants.ErrorLink)]
        public void GetErrorLink_Success(string input, string expected)
        {
            string result = ValidationError.GetErrorLink(input);
            Assert.StartsWith(expected, result);
        }

        /// <summary>
        /// Test the failure case of get error link.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="expected">The expected.</param>
        [Theory]
        [Trait("Category", "Unit")]
        [InlineData(RandomPath, SystemConstants.ErrorLink + SystemConstants.ErrorLinkPathAnchor)]
        public void GetErrorLink_Failure(string input, string expected)
        {
            string result = ValidationError.GetErrorLink(input);
            Assert.DoesNotContain(expected, result);
        }

        /// <summary>
        /// Test the success case of get category.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="expectedCategory">The expected category.</param>
        /// <param name="expectedMode">The expected mode.</param>
        [Theory]
        [Trait("Category", "Unit")]
        [InlineData(SystemConstants.CategoryPathClientWithSlash, SystemConstants.CategoryClient, SystemConstants.CategoryModeDirect)]
        [InlineData(SystemConstants.CategoryPathClientWithoutSlash, SystemConstants.CategoryClient, SystemConstants.CategoryModeStatic)]
        [InlineData(SystemConstants.CategoryPathHealthz, SystemConstants.CategoryHealthz, SystemConstants.CategoryModeHealthz)]
        [InlineData(SystemConstants.CategoryPathMetrics, SystemConstants.CategoryMetrics, SystemConstants.CategoryModeMetrics)]
        [InlineData(RandomPath, SystemConstants.CategoryStatic, SystemConstants.CategoryModeStatic)]
        public void GetCategory_Success(string path, string expectedCategory, string expectedMode)
        {
            string result = ValidationError.GetCategory(path, out string mode);

            Assert.True(result == expectedCategory, CategoryMismatch);
            Assert.True(mode == expectedMode, ModeMismatch);
        }

        /// <summary>
        /// Test the failure case of get category.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="expectedCategory">The expected category.</param>
        /// <param name="expectedMode">The expected mode.</param>
        [Theory]
        [Trait("Category", "Unit")]
        [InlineData(RandomPath, SystemConstants.CategoryClient, SystemConstants.CategoryModeDirect)]
        [InlineData(RandomPath, SystemConstants.CategoryHealthz, SystemConstants.CategoryModeHealthz)]
        [InlineData(RandomPath, SystemConstants.CategoryMetrics, SystemConstants.CategoryModeMetrics)]
        [InlineData(SystemConstants.CategoryPathClientWithSlash, SystemConstants.CategoryModeStatic)]
        public void GetCategory_Failure(string path, string expectedCategory, string expectedMode)
        {
            string result = ValidationError.GetCategory(path, out string mode);

            Assert.True(result != expectedCategory, CategoryMismatch);
            Assert.True(mode != expectedMode, ModeMismatch);
        }
    }
}

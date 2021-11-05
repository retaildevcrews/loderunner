// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using LodeRunner.API.Middleware.Validation;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace LodeRunner.API.Test.UnitTests
{
    /// <summary>
    /// ValidationError Unit Test.
    /// </summary>
    public class ValidateError
    {
        private const string CategoryMismatch = "Category mismatch.";
        private const string SubCategoryMismatch = "SubCategory mismatch.";
        private const string ModeMismatch = "Mode mismatch.";
        private const string RandomPath = "some/random/path";
        private const string InvalidFieldName = "InvalidFieldName";

        /// <summary>
        /// Test the success case of get error message.
        /// </summary>
        [Theory]
        [Trait("Category", "Unit")]
        [InlineData(SystemConstants.ClientStatusID, SystemConstants.ErrorMessageSuccess)]
        [InlineData(InvalidFieldName, SystemConstants.ErrorMessageUnknownParameter)]
        public void GetErrorMessage_Success(string input, string expected )
        {
            string result = ValidationError.GetErrorMessage(input);

            Assert.StartsWith(expected, result);
        }

        /// <summary>
        /// Test the failure case of get error message.
        /// </summary>
        [Theory]
        [Trait("Category", "Unit")]
        [InlineData(InvalidFieldName, SystemConstants.ErrorMessageSuccess)]
        [InlineData(SystemConstants.ClientStatusID, SystemConstants.ErrorMessageUnknownParameter)]
        public void GetErrorMessage_Failure(string input, string expected)
        {
            string result = ValidationError.GetErrorMessage(input);

            Assert.DoesNotContain(expected, result);

        }

        /// <summary>
        /// Test the success case of get error link.
        /// </summary>
        [Theory]
        [Trait("Category", "Unit")]
        [InlineData(SystemConstants.ErrorLinkPath, SystemConstants.ErrorLink+SystemConstants.ErrorLinkPathAnchor)]
        [InlineData(RandomPath, SystemConstants.ErrorLink)]
        public void GetErrorLink_Success(string input, string expected)
        {
            string result = ValidationError.GetErrorLink(input);
            Assert.StartsWith(expected, result);
        }

        /// <summary>
        /// Test the failure case of get error link.
        /// </summary>
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
        [Theory]
        [Trait("Category", "Unit")]
        [InlineData(SystemConstants.CategoryPathClientWithSlash, SystemConstants.CategoryClient, SystemConstants.CategorySubCategoryClient, SystemConstants.CategoryModeDirect)]
        [InlineData(SystemConstants.CategoryPathClientWithoutSlash, SystemConstants.CategoryClient, SystemConstants.CategorySubCategoryClient, SystemConstants.CategoryModeStatic)]
        [InlineData(SystemConstants.CategoryPathHealthz, SystemConstants.CategoryHealthz, SystemConstants.CategorySubCategoryHealthz, SystemConstants.CategoryModeHealthz)]
        [InlineData(SystemConstants.CategoryPathMetrics, SystemConstants.CategoryMetrics, SystemConstants.CategorySubCategoryMetrics, SystemConstants.CategoryModeMetrics)]
        [InlineData(RandomPath, SystemConstants.CategoryStatic, SystemConstants.CategorySubCategoryStatic, SystemConstants.CategoryModeStatic)]
        public void GetCategory_Success(string path, string expectedCategory, string expectedSubCategory, string expectedMode)
        {
            string result = ValidationError.GetCategory(path, out string subCategory, out string mode);
            
            Assert.True(result == expectedCategory, CategoryMismatch);
            Assert.True(subCategory == expectedSubCategory, SubCategoryMismatch);
            Assert.True(mode == expectedMode, ModeMismatch);
        }

        /// <summary>
        /// Test the failure case of get category.
        /// </summary>
        [Theory]
        [Trait("Category", "Unit")]
        [InlineData(RandomPath, SystemConstants.CategoryClient, SystemConstants.CategorySubCategoryClient, SystemConstants.CategoryModeDirect)]
        [InlineData(RandomPath, SystemConstants.CategoryHealthz, SystemConstants.CategorySubCategoryHealthz, SystemConstants.CategoryModeHealthz)]
        [InlineData(RandomPath, SystemConstants.CategoryMetrics, SystemConstants.CategorySubCategoryMetrics, SystemConstants.CategoryModeMetrics)]
        [InlineData(SystemConstants.CategoryPathClientWithSlash, SystemConstants.CategoryStatic, SystemConstants.CategorySubCategoryStatic, SystemConstants.CategoryModeStatic)]
        public void GetCategory_Failure(string path, string expectedCategory, string expectedSubCategory, string expectedMode)
        {
            string result = ValidationError.GetCategory(path, out string subCategory, out string mode);

            Assert.True(result != expectedCategory, CategoryMismatch);
            Assert.True(subCategory != expectedSubCategory, SubCategoryMismatch);
            Assert.True(mode != expectedMode, ModeMismatch);
        }
    }
}

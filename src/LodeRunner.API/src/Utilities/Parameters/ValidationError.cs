// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Http;

namespace LodeRunner.API.Middleware.Validation
{
    /// <summary>
    /// Validation Error Class.
    /// </summary>
    public class ValidationError
    {
        /// <summary>
        /// Gets or sets error Target.
        /// </summary>
        /// <example>clientStatusId.</example>
        public string Target { get; set; }

        /// <summary>
        /// Gets or sets error Message.
        /// </summary>
        /// <example>The parameter 'cientStatusId' should be a non-empty string.</example>
        public string Message { get; set; }

        /// <summary>
        /// Get standard error message
        ///     changing these will require changes to the json validation tests.
        /// </summary>
        /// <param name="fieldName">field name.</param>
        /// <returns>string.</returns>
        public static string GetErrorMessage(string fieldName)
        {
            return fieldName.ToUpperInvariant() switch
            {
                SystemConstants.ClientStatusID => SystemConstants.ErrorMessageSuccess,
                _ => $"{SystemConstants.ErrorMessageUnknownParameter} {fieldName}",
            };
        }

        /// <summary>
        /// Get the doc link based on request URL.
        /// </summary>
        /// <param name="path">full request path.</param>
        /// <returns>link to doc.</returns>
        public static string GetErrorLink(string path)
        {
            string result = SystemConstants.ErrorLink;

            path = path.ToLowerInvariant();

            if (path.StartsWith(SystemConstants.ErrorLinkPath))
            {
                result += SystemConstants.ErrorLinkPathAnchor;
            }

            return result;
        }

        /// <summary>
        /// Gets category.
        /// </summary>
        /// <param name="context">HttpContext.</param>
        /// <param name="subCategory">subCategory.</param>
        /// <param name="mode">Mode.</param>
        /// <returns>Category.</returns>
        public static string GetCategory(HttpContext context, out string subCategory, out string mode)
        {
            string path = RequestLogger.GetPathAndQuerystring(context.Request).ToLowerInvariant();

            return GetCategory(path, out subCategory, out mode);
        }

        /// <summary>
        /// Gets the category.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="subCategory">The sub category.</param>
        /// <param name="mode">The mode.</param>
        /// <returns>The Category.</returns>
        public static string GetCategory(string path, out string subCategory, out string mode)
        {
            string category;
            if (path.StartsWith(SystemConstants.CategoryPathClientWithSlash))
            {
                category = SystemConstants.CategoryClient;
                subCategory = SystemConstants.CategorySubCategoryClient;
                mode = SystemConstants.CategoryModeDirect;
            }
            else if (path.StartsWith(SystemConstants.CategoryPathClientWithoutSlash))
            {
                category = SystemConstants.CategoryClient;
                subCategory = SystemConstants.CategorySubCategoryClient;
                mode = SystemConstants.CategoryModeStatic;
            }
            else if (path.StartsWith(SystemConstants.CategoryPathHealthz))
            {
                category = SystemConstants.CategoryHealthz;
                subCategory = SystemConstants.CategorySubCategoryHealthz;
                mode = SystemConstants.CategoryModeHealthz;
            }
            else if (path.StartsWith(SystemConstants.CategoryPathMetrics))
            {
                category = SystemConstants.CategoryMetrics;
                subCategory = SystemConstants.CategorySubCategoryMetrics;
                mode = SystemConstants.CategoryModeMetrics;
            }
            else
            {
                category = SystemConstants.CategoryStatic;
                subCategory = SystemConstants.CategorySubCategoryStatic;
                mode = SystemConstants.CategoryModeStatic;
            }

            return category;
        }
    }
}

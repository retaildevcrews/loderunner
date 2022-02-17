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
        /// <example>The parameter 'clientStatusId' should be a non-empty string.</example>
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
                SystemConstants.ClientStatusID => $"'clientStatusId' {SystemConstants.ErrorMessageGuid}",
                SystemConstants.LoadTestConfigID => $"'loadTestConfigId' {SystemConstants.ErrorMessageGuid}",
                SystemConstants.TestRunID => $"'testRunId' {SystemConstants.ErrorMessageGuid}",
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

            if (path.StartsWith(SystemConstants.DirectClientsPath))
            {
                result += SystemConstants.ErrorLinkDirectClientsPathAnchor;
            }
            else if (path.StartsWith(SystemConstants.DirectLoadTestConfigsPath))
            {
                result += SystemConstants.ErrorLinkDirectLoadTestConfigsPathAnchor;
            }
            else if (path.StartsWith(SystemConstants.DirectTestRunsPath))
            {
                result += SystemConstants.ErrorLinkDirectTestRunsPathAnchor;
            }

            return result;
        }

        /// <summary>
        /// Gets category.
        /// </summary>
        /// <param name="context">HttpContext.</param>
        /// <param name="mode">Mode.</param>
        /// <returns>Category.</returns>
        public static string GetCategory(HttpContext context, out string mode)
        {
            string path = RequestLogger.GetPathAndQuerystring(context.Request).ToLowerInvariant();

            return GetCategory(path, out mode);
        }

        /// <summary>
        /// Returns category
        /// </summary>
        /// <param name="path">path</param>
        /// <param name="mode">mode</param>
        /// <returns>category</returns>
        public static string GetCategory(string path, out string mode)
        {
            string category;
            if (path.StartsWith(SystemConstants.DirectClientsPath))
            {
                category = SystemConstants.CategoryClient;
                mode = SystemConstants.CategoryModeDirect;
            }
            else if (path.StartsWith(SystemConstants.DirectLoadTestConfigsPath))
            {
                category = SystemConstants.CategoryLoadTestConfig;
                mode = SystemConstants.CategoryModeDirect;
            }
            else if (path.StartsWith(SystemConstants.DirectTestRunsPath))
            {
                category = SystemConstants.CategoryTestRun;
                mode = SystemConstants.CategoryModeDirect;
            }
            else if (path.StartsWith(SystemConstants.CategoryMetricsPath))
            {
                category = SystemConstants.CategoryMetrics;
                mode = SystemConstants.CategoryModeMetrics;
            }
            else if (path.StartsWith(SystemConstants.CategoryHealthzPath))
            {
                category = SystemConstants.CategoryHealthz;
                mode = SystemConstants.CategoryModeHealthz;
            }
            else if (path.StartsWith(SystemConstants.CategoryClientsPath))
            {
                category = SystemConstants.CategoryClient;
                mode = SystemConstants.CategoryModeStatic;
            }
            else if (path.StartsWith(SystemConstants.CategoryLoadTestConfigsPath))
            {
                category = SystemConstants.CategoryLoadTestConfig;
                mode = SystemConstants.CategoryModeStatic;
            }
            else if (path.StartsWith(SystemConstants.CategoryTestRunsPath))
            {
                category = SystemConstants.CategoryTestRun;
                mode = SystemConstants.CategoryModeStatic;
            }
            else
            {
                category = SystemConstants.CategoryStatic;
                mode = SystemConstants.CategoryModeStatic;
            }

            return category;
        }
    }
}

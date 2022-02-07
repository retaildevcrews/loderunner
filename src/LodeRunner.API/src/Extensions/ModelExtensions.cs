// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using LodeRunner.Core.CommandLine;
using LodeRunner.Core.Extensions;
using LodeRunner.Core.Models;

namespace LodeRunner.API.Middleware
{
    /// <summary>
    /// Provides Extension methods for Models.
    /// </summary>
    public static class ModelExtensions
    {
        /// <summary>
        /// Validates the specified load test payload configuration.
        /// </summary>
        /// <param name="loadTestConfig">The load test configuration.</param>
        /// <param name="errorMessage">Error MEssage String if any.</param>
        /// <param name="payloadPropertiesChanged">Payload Properties Change list.</param>
        /// <returns>Whether or not  the DTO passes validation.</returns>
        public static bool Validate(this LoadTestConfig loadTestConfig, out string errorMessage, List<string> payloadPropertiesChanged = null)
        {
            RootCommand root = LRCommandLine.BuildRootCommandMode();

            string[] args = LoadTestConfigExtensions.GetArgs(loadTestConfig, payloadPropertiesChanged);

            bool result = false;

            try
            {
                var errors = root.Parse(args).Errors;
                if (errors.Count > 0)
                {
                    errorMessage = string.Join(",", errors.Select(e => e.Message).ToList());
                }
                else
                {
                    errorMessage = string.Empty;
                }

                result = errors.Count == 0;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

            return result;
        }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Linq;
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
        /// Validates flag values and combinations in load test configuration payload.
        /// </summary>
        /// <param name="loadTestConfig">The load test configuration.</param>
        /// <returns>Errors when DTO doesn't pass validation.</returns>
        public static IEnumerable<string> FlagValidator(this LoadTestConfig loadTestConfig)
        {
            RootCommand root = LRCommandLine.BuildRootCommandMode();
            string[] args = LoadTestConfigExtensions.GetArgs(loadTestConfig);
            return root.Parse(args).Errors.Select(x => x.Message);
        }
    }
}

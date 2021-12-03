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
using LodeRunner.Core.Models;

namespace LodeRunner.API.Middleware
{
    /// <summary>
    /// Provides Extension methods for Models.
    /// </summary>
    public static class ModelExtensions
    {
        /// <summary>
        /// Validates the specified load test configuration.
        /// </summary>
        /// <param name="loadTestConfig">The load test configuration.</param>
        /// <param name="errorMessage">Error MEssage String if any.</param>
        /// <returns>Whether or not  the DTO passes validation.</returns>
        public static bool Validate(this LoadTestConfig loadTestConfig, out string errorMessage)
        {
            RootCommand root = LRCommandLine.BuildRootCommandMode();

            string[] args = GetArgs(loadTestConfig);

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

        /// <summary>
        /// Gets the arguments.
        /// </summary>
        /// <param name="loadTestConfig">The load test configuration.</param>
        /// <returns>the args.</returns>
        private static string[] GetArgs(LoadTestConfig loadTestConfig)
        {
            var properties = loadTestConfig.GetType().GetProperties().Where(prop => prop.IsDefined(typeof(DescriptionAttribute), false));

            List<string> argsList = new ();

            foreach (var prop in properties)
            {
                var descriptionAttributes = (DescriptionAttribute[])prop.GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (descriptionAttributes.Length > 0)
                {
                    argsList.Add(descriptionAttributes[0].Description);
                    argsList.Add(loadTestConfig.FieldValue(prop.Name));
                }
            }

            return argsList.ToArray();
        }

        /// <summary>
        /// Fields the value.
        /// </summary>
        /// <param name="loadTestConfigDto">The load test configuration.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>The value.</returns>
        private static string FieldValue(this LoadTestConfig loadTestConfigDto, string fieldName)
        {
            string result = string.Empty;

            Type objType = loadTestConfigDto.GetType();

            PropertyInfo[] props = objType.GetProperties();

            PropertyInfo propFound = props.FirstOrDefault(x => x.Name == fieldName);

            if (propFound != null)
            {
                if (propFound.PropertyType == typeof(List<string>))
                {
                    List<string> items = (List<string>)propFound.GetValue(loadTestConfigDto);

                    result = string.Join(" ", items);
                }
                else
                {
                    object propValue = propFound.GetValue(loadTestConfigDto);
                    result = propValue == null ? string.Empty : propValue.ToString();
                }
            }

            return result;
        }
    }
}

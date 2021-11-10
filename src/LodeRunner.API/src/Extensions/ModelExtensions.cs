// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using LodeRunner.API.Data.Dtos;
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
        /// Converts LoadTestConfig model to LoadTestConfigDTO.
        /// </summary>
        /// <param name="loadTestConfigDto">The load test configuration.</param>
        /// <returns>LoadTestConfig.</returns>
        public static LoadTestConfig DtoToModel(this LoadTestConfigDto loadTestConfigDto)
        {
            return new LoadTestConfig
            {
                Files = loadTestConfigDto.Files,

                StrictJson = loadTestConfigDto.StrictJson,

                BaseURL = loadTestConfigDto.BaseURL,

                VerboseErrors = loadTestConfigDto.VerboseErrors,

                Randomize = loadTestConfigDto.Randomize,

                Timeout = loadTestConfigDto.Timeout,

                Server = loadTestConfigDto.Server,

                Tag = loadTestConfigDto.Tag,

                Sleep = loadTestConfigDto.Sleep,

                RunLoop = loadTestConfigDto.RunLoop,

                Duration = loadTestConfigDto.Duration,

                MaxErrors = loadTestConfigDto.MaxErrors,

                DelayStart = loadTestConfigDto.DelayStart,

                DryRun = loadTestConfigDto.DryRun,
            };
        }

        /// <summary>
        /// Validates the specified load test configuration dto.
        /// </summary>
        /// <param name="loadTestConfigDto">The load test configuration dto.</param>
        /// <param name="errorMessage">Error MEssage String if any.</param>
        /// <returns>Whether or not  the DTO passes validation.</returns>
        public static bool Validate(this LoadTestConfigDto loadTestConfigDto, out string errorMessage)
        {
            RootCommand root = LRCommandLine.BuildRootCommand();

            string[] args = GetArgs(loadTestConfigDto);

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
        /// <param name="loadTestConfigDto">The load test configuration dto.</param>
        /// <returns>the args</returns>
        private static string[] GetArgs(LoadTestConfigDto loadTestConfigDto)
        {
            var properties = loadTestConfigDto.GetType().GetProperties().Where(prop => prop.IsDefined(typeof(DescriptionAttribute), false));

            List<string> argsList = new ();

            foreach (var prop in properties)
            {
                var descriptionAttributes = (DescriptionAttribute[])prop.GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (descriptionAttributes.Length > 0)
                {
                    argsList.Add(descriptionAttributes[0].Description);
                    argsList.Add(loadTestConfigDto.FieldValue(prop.Name));
                }
            }

            return argsList.ToArray();
        }

        /// <summary>
        /// Fields the value.
        /// </summary>
        /// <param name="loadTestConfigDto">The load test configuration dto.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>The value.</returns>
        private static string FieldValue(this LoadTestConfigDto loadTestConfigDto, string fieldName)
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
                    result = propFound.GetValue(loadTestConfigDto).ToString();
                }
            }

            return result;
        }
    }
}

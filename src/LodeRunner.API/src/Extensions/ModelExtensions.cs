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

namespace LodeRunner.API.Middleware
{
    /// <summary>
    /// Provides Extension methods for Models.
    /// </summary>
    public static class ModelExtensions
    {
        /// <summary>
        /// Validates the specified model.
        /// </summary>
        /// <typeparam name="TEntity">The model type.</typeparam>
        /// <param name="model">The model.</param>
        /// <param name="errorMessage">Error Message String if any.</param>
        /// <param name="payloadPropertiesChanged">Payload Properties Change list.</param>
        /// <returns>Whether or not  the DTO passes validation.</returns>
        public static bool Validate<TEntity>(this TEntity model, out string errorMessage, List<string> payloadPropertiesChanged = null)
        {
            RootCommand root = LRCommandLine.BuildRootCommandMode();

            string[] args = GetArgs(model, payloadPropertiesChanged);

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
        /// Gets the arguments from properties that exist in changedProperties list.
        /// </summary>
        /// <typeparam name="TEntity">The model type.</typeparam>
        /// <param name="model">The model.</param>
        /// <param name="payloadPropertiesChanged">Changed Properties list.</param>
        /// <returns>the args.</returns>
        private static string[] GetArgs<TEntity>(TEntity model, List<string> payloadPropertiesChanged = null)
        {
            var properties = model.GetType().GetProperties().Where(prop => prop.IsDefined(typeof(DescriptionAttribute), false));

            List<string> argsList = new ();

            foreach (var prop in properties)
            {
                // NOTE: Only convert properties to arguments if the property exist is ChangedProperties list. This list represents the Json data sent as Payload
                if (payloadPropertiesChanged == null || payloadPropertiesChanged.Contains(prop.Name))
                {
                    var descriptionAttributes = (DescriptionAttribute[])prop.GetCustomAttributes(typeof(DescriptionAttribute), false);
                    if (descriptionAttributes.Length > 0)
                    {
                        argsList.Add(descriptionAttributes[0].Description);
                        argsList.Add(model.FieldValue(prop.Name));
                    }
                }
            }

            return argsList.ToArray();
        }

        /// <summary>
        /// Fields the value.
        /// </summary>
        /// <typeparam name="TEntity">The model type.</typeparam>
        /// <param name="modelDto">The model.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>The value.</returns>
        private static string FieldValue<TEntity>(this TEntity modelDto, string fieldName)
        {
            string result = string.Empty;

            Type objType = modelDto.GetType();

            PropertyInfo[] props = objType.GetProperties();

            PropertyInfo propFound = props.FirstOrDefault(x => x.Name == fieldName);

            if (propFound != null)
            {
                if (propFound.PropertyType == typeof(List<string>))
                {
                    List<string> items = (List<string>)propFound.GetValue(modelDto);

                    result = string.Join(" ", items);
                }
                else
                {
                    object propValue = propFound.GetValue(modelDto);
                    result = propValue == null ? string.Empty : propValue.ToString();
                }
            }

            return result;
        }
    }
}

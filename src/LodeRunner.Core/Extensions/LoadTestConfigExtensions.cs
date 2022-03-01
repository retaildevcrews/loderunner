// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using LodeRunner.Core.Models;

namespace LodeRunner.Core.Extensions
{
    /// <summary>
    /// Provides LoadTestConfig extension methods.
    /// </summary>
    public static class LoadTestConfigExtensions
    {
        /// <summary>
        /// Gets the command line arguments from LoadTestConfig properties.
        /// </summary>
        /// <param name="loadTestConfig">The load test configuration.</param>
        /// <returns>The command line arguments.</returns>
        public static string[] GetArgs(LoadTestConfig loadTestConfig)
        {
            var properties = loadTestConfig.GetType().GetProperties().Where(prop => prop.IsDefined(typeof(DescriptionAttribute), false));

            List<string> argsList = new ();

            foreach (var prop in properties)
            {
                var descriptionAttributes = (DescriptionAttribute[])prop.GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (descriptionAttributes.Length > 0)
                {
                    var description = descriptionAttributes[0].Description;

                    var fields = loadTestConfig.FieldValue(prop.Name);
                    if (fields[0] != null)
                    {
                        argsList.Add(description);
                        foreach (var field in fields)
                        {
                            argsList.Add(field.ToString());
                        }
                    }
                }
            }

            return argsList.ToArray();
        }

        /// <summary>
        /// Fields the value.
        /// </summary>
        /// <typeparam name="TPayloadSource">The type of the payload source.</typeparam>
        /// <param name="payloadSource">The payload source.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>The value.</returns>
        public static List<object> FieldValue<TPayloadSource>(this TPayloadSource payloadSource, string fieldName)
        {
            var result = new List<object>();

            Type objType = payloadSource.GetType();

            PropertyInfo[] props = objType.GetProperties();

            PropertyInfo propFound = props.FirstOrDefault(x => x.Name == fieldName);

            if (propFound != null)
            {
                if (propFound.PropertyType == typeof(List<string>))
                {
                    List<string> items = (List<string>)propFound.GetValue(payloadSource);

                    result = items.ToList<object>();
                }
                else
                {
                    result.Add(propFound.GetValue(payloadSource));
                }
            }

            return result;
        }
    }
}

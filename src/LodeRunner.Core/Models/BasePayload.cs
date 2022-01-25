// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LodeRunner.Core.Models
{
    /// <summary>
    /// Base class for payload.
    /// </summary>
    public class BasePayload
    {
        /// <summary>
        /// Gets or sets the properties changed list.
        /// </summary>
        /// <value>
        /// The changed properties.
        /// </value>
        public List<string> PropertiesChanged { get; set; } = new ();

        /// <summary>
        /// Sets the field on the "entityObject".
        /// Assuming property names in this class are the same as in  Derived BasePayload.
        /// We're using the Reflected [CallerMemberName] property, assuming we're calling this from the same property we want to change in LoadTestConfig
        /// If this is called from a method, propertyName should be set explicitly.
        /// </summary>
        /// <typeparam name="T">The property value.</typeparam>
        /// <param name="entityObject">The object representing Derived BasePayload class.</param>
        /// <param name="value">The value.</param>
        /// <param name="propertyName">Name of the caller property.</param>
        protected void SetField<T>(object entityObject, T value, [CallerMemberName] string propertyName = null)
        {
            // NOTE: This method supports wire format for creating /updating concrete implementation of BasePayload.
            // This helps to identify deltas during payload deserialization.

            // Get Reflected property object based on the propertyName
            // Ideally this method should be called from the same property name which needs to updated
            var filesProp = entityObject.GetType().GetProperty(propertyName);

            if (filesProp.PropertyType != typeof(T))
            {
                throw new InvalidCastException($"Cannot cast {typeof(T).FullName} to {filesProp.GetType().FullName}");
            }

            filesProp.SetValue(entityObject, value);
            if (!this.PropertiesChanged.Contains(propertyName))
            {
                this.PropertiesChanged.Add(propertyName);
            }
        }
    }
}

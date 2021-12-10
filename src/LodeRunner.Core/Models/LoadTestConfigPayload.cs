// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using LodeRunner.Core.Extensions;
using LodeRunner.Core.Models.Validators;
using LodeRunner.Core.SchemaFilters;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Swashbuckle.AspNetCore.Annotations;

namespace LodeRunner.Core.Models
{
    /// <summary>
    /// LoadRestConfig Payload.
    /// </summary>
    [SwaggerSchemaFilter(typeof(LoadTestConfigPayloadSchemaFilter))]
    public class LoadTestConfigPayload : INotifyPropertyChanged
    {
        // Composite LoadTestConfig object to hold data
        private readonly LoadTestConfig loadTestConfig = new ();

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets the properties changed list.
        /// </summary>
        /// <value>
        /// The changed properties.
        /// </value>
        public List<string> PropertiesChanged { get; set; } = new ();

        /// <summary>
        /// Gets or sets the files.
        /// </summary>
        /// <value>
        /// The files.
        /// </value>
        public List<string> Files { get => this.loadTestConfig.Files; set => this.SetField(value); }

        /// <summary>
        /// Gets or sets a value indicating whether [strict json].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [strict json]; otherwise, <c>false</c>.
        /// </value>
        public bool StrictJson { get => this.loadTestConfig.StrictJson; set => this.SetField(value); }

        /// <summary>
        /// Gets or sets the base URL.
        /// </summary>
        /// <value>
        /// The base URL.
        /// </value>
        public string BaseURL { get => this.loadTestConfig.BaseURL; set => this.SetField(value); }

        /// <summary>
        /// Gets or sets a value indicating whether [verbose errors].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [verbose errors]; otherwise, <c>false</c>.
        /// </value>
        public bool VerboseErrors { get => this.loadTestConfig.VerboseErrors; set => this.SetField(value); }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="LoadTestConfig"/> is randomize.
        /// </summary>
        /// <value>
        ///   <c>true</c> if randomize; otherwise, <c>false</c>.
        /// </value>
        public bool Randomize { get => this.loadTestConfig.Randomize; set => this.SetField(value); }

        /// <summary>
        /// Gets or sets the timeout.
        /// </summary>
        /// <value>
        /// The timeout.
        /// </value>
        public int Timeout { get => this.loadTestConfig.Timeout; set => this.SetField(value); }

        /// <summary>
        /// Gets or sets the server.
        /// </summary>
        /// <value>
        /// The server.
        /// </value>
        public List<string> Server { get => this.loadTestConfig.Server; set => this.SetField(value); }

        /// <summary>
        /// Gets or sets the tag.
        /// </summary>
        /// <value>
        /// The tag.
        /// </value>
        public string Tag { get => this.loadTestConfig.Tag; set => this.SetField(value); }

        /// <summary>
        /// Gets or sets the sleep.
        /// </summary>
        /// <value>
        /// The sleep.
        /// </value>
        public int Sleep { get => this.loadTestConfig.Sleep; set => this.SetField(value); }

        /// <summary>
        /// Gets or sets a value indicating whether [run loop].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [run loop]; otherwise, <c>false</c>.
        /// </value>
        public bool RunLoop { get => this.loadTestConfig.RunLoop; set => this.SetField(value); }

        /// <summary>
        /// Gets or sets the duration.
        /// </summary>
        /// <value>
        /// The duration.
        /// </value>
        public int Duration { get => this.loadTestConfig.Duration; set => this.SetField(value); }

        /// <summary>
        /// Gets or sets the maximum errors.
        /// </summary>
        /// <value>
        /// The maximum errors.
        /// </value>
        public int MaxErrors { get => this.loadTestConfig.MaxErrors; set => this.SetField(value); }

        /// <summary>
        /// Gets or sets the delay start.
        /// </summary>
        /// <value>
        /// The delay start.
        /// </value>
        public int DelayStart { get => this.loadTestConfig.DelayStart; set => this.SetField(value); }

        /// <summary>
        /// Gets or sets a value indicating whether [dry run].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [dry run]; otherwise, <c>false</c>.
        /// </value>
        public bool DryRun { get => this.loadTestConfig.DryRun; set => this.SetField(value); }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get => this.loadTestConfig.Name; set => this.SetField(value); }

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="fieldName">The field name.</param>
        protected void OnPropertyChanged([CallerMemberName] string fieldName = null)
        {
            if (!this.PropertiesChanged.Contains(fieldName))
            {
                this.PropertiesChanged.Add(fieldName);
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(fieldName));
            }
        }

        /// <summary>
        /// Sets the field on LoadTestConfig based on the field.
        /// Assuming property names in this class are the same as in LoadTestConfig.
        /// We're using the Reflected [CallerMemberName] property, assuming we're calling this from the same property we want to change in LoadTestConfig
        /// If this is called from a method, propertyName should be set explicitly.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="propertyName">Name of the caller propert.</param>
        private void SetField<T>(T value, [CallerMemberName] string propertyName = null)
        {
            // Get Reflected property object based on the propertyName
            // Ideally this method should be called from the same property name which needs to updated
            var filesProp = this.loadTestConfig.GetType().GetProperty(propertyName);
            if (filesProp.PropertyType != typeof(T))
            {
                throw new InvalidCastException($"Cannot cast {typeof(T).FullName} to {filesProp.GetType().FullName}");
            }

            filesProp.SetValue(this.loadTestConfig, value);
            this.OnPropertyChanged(propertyName);
        }
    }
}

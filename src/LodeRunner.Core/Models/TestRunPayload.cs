﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LodeRunner.Core.SchemaFilters;
using Swashbuckle.AspNetCore.Annotations;

namespace LodeRunner.Core.Models
{
    /// <summary>
    /// TestRun Payload.
    /// </summary>
    [SwaggerSchemaFilter(typeof(TestRunPayloadSchemaFilter))]
    public class TestRunPayload : BasePayload, INotifyPropertyChanged
    {
        // Composite TestRun object to hold data
        private readonly TestRun testRun = new ();

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get => this.testRun.Name; set => this.SetField(value); }

        /// <summary>
        /// Gets or sets the LoadTestConfig.
        /// </summary>
        /// <value>
        /// The LoadTestConfig.
        /// </value>
        public LoadTestConfig LoadTestConfig { get => this.testRun.LoadTestConfig; set => this.SetField(value); }

        /// <summary>
        /// Gets or sets the LoadClients.
        /// </summary>
        /// <value>
        /// The LoadClients.
        /// </value>
        public List<LoadClient> LoadClients { get => this.testRun.LoadClients; set => this.SetField(value); }

        /// <summary>
        /// Gets or sets the CreatedTime.
        /// </summary>
        /// <value>
        /// The CreatedTime.
        /// </value>
        public DateTime CreatedTime { get => this.testRun.CreatedTime; set => this.SetField(value); }

        /// <summary>
        /// Gets or sets the StartTime.
        /// </summary>
        /// <value>
        /// The StartTime.
        /// </value>
        public DateTime StartTime { get => this.testRun.StartTime; set => this.SetField(value); }

        /// <summary>
        /// Sets mock TestRun payload data.
        /// </summary>
        /// <param name="name">TestRun name.</param>
        public void SetMockData(string name)
        {
            this.Name = name;
            this.CreatedTime = DateTime.UtcNow;
            this.StartTime = DateTime.UtcNow;

            this.LoadTestConfig = new LoadTestConfig();
            this.LoadTestConfig.SetMockData($"Sample LoadTestConfig - IntegrationTesting-{DateTime.UtcNow:yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK}");

            var mockedLoadClientA = new LoadClient();
            mockedLoadClientA.SetMockData($"Sample LoadClient A - IntegrationTesting-{DateTime.UtcNow:yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK}");

            var mockedLoadClientB = new LoadClient();
            mockedLoadClientB.SetMockData($"Sample LoadClient B - IntegrationTesting-{DateTime.UtcNow:yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK}");

            this.LoadClients = new List<LoadClient>
            {
                mockedLoadClientA,
                mockedLoadClientB,
            };
        }

        /// <summary>
        /// Sets the field on TestRun based on the field.
        /// Assuming property names in this class are the same as in TestRun.
        /// We're using the Reflected [CallerMemberName] property, assuming we're calling this from the same property we want to change in TestRun.
        /// If this is called from a method, propertyName should be set explicitly.
        /// </summary>
        /// <typeparam name="T">The property value.</typeparam>
        /// <param name="value">The value.</param>
        /// <param name="propertyName">Name of the caller property.</param>
        protected override void SetField<T>(T value, [CallerMemberName] string propertyName = null)
        {
            // Get Reflected property object based on the propertyName
            // Ideally this method should be called from the same property name which needs to updated
            var filesProp = this.testRun.GetType().GetProperty(propertyName);
            if (filesProp.PropertyType != typeof(T))
            {
                throw new InvalidCastException($"Cannot cast {typeof(T).FullName} to {filesProp.GetType().FullName}");
            }

            filesProp.SetValue(this.testRun, value);
            this.OnPropertyChanged(propertyName);
        }
    }
}

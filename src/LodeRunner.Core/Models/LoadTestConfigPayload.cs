// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
    public class LoadTestConfigPayload : LoadTestConfig, INotifyPropertyChanged
    {
        private List<string> files;
        private bool strictJson;
        private string baseURL;
        private bool verboseErrors;
        private bool randomize;
        private int timeout;
        private List<string> server;
        private string tag;
        private int sleep;
        private bool runLoop;
        private int duration;
        private int maxErrors;
        private int delayStart;
        private bool dryRun;

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
        public List<string> PropertiesChanged { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the files.
        /// </summary>
        /// <value>
        /// The files.
        /// </value>
        [ValidateNever]
        public override List<string> Files { get => this.files; set => this.SetField(ref this.files, value); }

        /// <summary>
        /// Gets or sets a value indicating whether [strict json].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [strict json]; otherwise, <c>false</c>.
        /// </value>
        public override bool StrictJson { get => this.strictJson; set => this.SetField(ref this.strictJson, value); }

        /// <summary>
        /// Gets or sets the base URL.
        /// </summary>
        /// <value>
        /// The base URL.
        /// </value>
        public override string BaseURL { get => this.baseURL; set => this.SetField(ref this.baseURL, value); }

        /// <summary>
        /// Gets or sets a value indicating whether [verbose errors].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [verbose errors]; otherwise, <c>false</c>.
        /// </value>
        public override bool VerboseErrors { get => this.verboseErrors; set => this.SetField(ref this.verboseErrors, value); }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="LoadTestConfig"/> is randomize.
        /// </summary>
        /// <value>
        ///   <c>true</c> if randomize; otherwise, <c>false</c>.
        /// </value>
        public override bool Randomize { get => this.randomize; set => this.SetField(ref this.randomize, value); }

        /// <summary>
        /// Gets or sets the timeout.
        /// </summary>
        /// <value>
        /// The timeout.
        /// </value>
        public override int Timeout { get => this.timeout; set => this.SetField(ref this.timeout, value); }

        /// <summary>
        /// Gets or sets the server.
        /// </summary>
        /// <value>
        /// The server.
        /// </value>
        [ValidateNever]
        public override List<string> Server { get => this.server; set => this.SetField(ref this.server, value); }

        /// <summary>
        /// Gets or sets the tag.
        /// </summary>
        /// <value>
        /// The tag.
        /// </value>
        public override string Tag { get => this.tag; set => this.SetField(ref this.tag, value); }

        /// <summary>
        /// Gets or sets the sleep.
        /// </summary>
        /// <value>
        /// The sleep.
        /// </value>
        public override int Sleep { get => this.sleep; set => this.SetField(ref this.sleep, value); }

        /// <summary>
        /// Gets or sets a value indicating whether [run loop].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [run loop]; otherwise, <c>false</c>.
        /// </value>
        public override bool RunLoop { get => this.runLoop; set => this.SetField(ref this.runLoop, value); }

        /// <summary>
        /// Gets or sets the duration.
        /// </summary>
        /// <value>
        /// The duration.
        /// </value>
        public override int Duration { get => this.duration; set => this.SetField(ref this.duration, value); }

        /// <summary>
        /// Gets or sets the maximum errors.
        /// </summary>
        /// <value>
        /// The maximum errors.
        /// </value>
        public override int MaxErrors { get => this.maxErrors; set => this.SetField(ref this.maxErrors, value); }

        /// <summary>
        /// Gets or sets the delay start.
        /// </summary>
        /// <value>
        /// The delay start.
        /// </value>
        public override int DelayStart { get => this.delayStart; set => this.SetField(ref this.delayStart, value); }

        /// <summary>
        /// Gets or sets a value indicating whether [dry run].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [dry run]; otherwise, <c>false</c>.
        /// </value>
        public override bool DryRun { get => this.dryRun; set => this.SetField(ref this.dryRun, value); }

        /// <summary>
        /// Gets the type of the entity.
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        public override EntityType EntityType
        {
            get
            {
                if (this.entityType == EntityType.Unassigned)
                {
                    this.entityType = this.GetType().BaseType.Name.As<EntityType>(EntityType.Unassigned);
                }

                return this.entityType;
            }
        }

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
        /// Sets the field.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <param name="callerMemberName">Name of the caller member.</param>
        private void SetField(ref List<string> field, List<string> value, [CallerMemberName] string callerMemberName = null)
        {
            field = value;
            this.OnPropertyChanged(callerMemberName);
        }

        /// <summary>
        /// Sets the field.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <param name="callerMemberName">Name of the caller member.</param>
        private void SetField(ref int field, int value, [CallerMemberName] string callerMemberName = null)
        {
            field = value;
            this.OnPropertyChanged(callerMemberName);
        }

        /// <summary>
        /// Sets the field.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <param name="callerMemberName">Name of the caller member.</param>
        private void SetField(ref string field, string value, [CallerMemberName] string callerMemberName = null)
        {
            field = value;
            this.OnPropertyChanged(callerMemberName);
        }

        /// <summary>
        /// Sets the field.
        /// </summary>
        /// <param name="field">if set to <c>true</c> [field].</param>
        /// <param name="value">if set to <c>true</c> [value].</param>
        /// <param name="callerMemberName">Name of the caller member.</param>
        private void SetField(ref bool field, bool value, [CallerMemberName] string callerMemberName = null)
        {
            field = value;
            this.OnPropertyChanged(callerMemberName);
        }
    }
}

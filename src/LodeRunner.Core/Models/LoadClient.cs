// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using LodeRunner.Core.Interfaces;

namespace LodeRunner.Core.Models
{
    /// <summary>
    /// LoadClient Model.
    /// </summary>
    /// <seealso cref="LodeRunner.Core.Models.BaseEntityModel" />
    public class LoadClient : BaseEntityModel
    {
        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the region.
        /// </summary>
        /// <value>
        /// The region.
        /// </value>
        public string Region { get; set; }

        /// <summary>
        /// Gets or sets the zone.
        /// </summary>
        /// <value>
        /// The zone.
        /// </value>
        public string Zone { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="LoadClient"/> is prometheus.
        /// </summary>
        /// <value>
        ///   <c>true</c> if prometheus; otherwise, <c>false</c>.
        /// </value>
        public bool Prometheus { get; set; }

        /// <summary>
        /// Gets or sets the startup arguments.
        /// </summary>
        /// <value>
        /// The startup arguments.
        /// </value>
        public string StartupArgs { get; set; }

        /// <summary>
        /// Gets or sets the start time.
        /// </summary>
        /// <value>
        /// The start time.
        /// </value>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets the new.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="lastUpdated">The last updated date time.</param>
        /// <returns>a new LoadClient entity.</returns>
        public static LoadClient GetNew(ILRConfig config, DateTime lastUpdated)
        {
            return new LoadClient
            {
                Version = "0.3.0 - 717 - 1030",
                Name = "Central - az - central - us - 2",
                Region = string.IsNullOrWhiteSpace(config.Region) == true ? "Central" : config.Region,
                Zone = string.IsNullOrWhiteSpace(config.Zone) == true ? "az-central-us" : config.Zone,
                Prometheus = config.Prometheus,
                StartupArgs = $"--delay-start {config.DelayStart} --secrets-volume {config.SecretsVolume}",
                StartTime = lastUpdated,
            };
        }
    }
}

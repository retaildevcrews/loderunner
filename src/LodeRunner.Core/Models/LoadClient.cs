﻿// Copyright (c) Microsoft Corporation. All rights reserved.
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
            LoadClient loadClient = new ()
            {
                Version = Core.Version.AssemblyVersion,
                Region = string.IsNullOrWhiteSpace(config.Region) == true ? SystemConstants.Unknown : config.Region,
                Zone = string.IsNullOrWhiteSpace(config.Zone) == true ? SystemConstants.Unknown : config.Zone,
                Prometheus = config.Prometheus,
                StartupArgs = $"--secrets-volume {config.SecretsVolume}",
                StartTime = lastUpdated,
            };
            config.LoadClientId = loadClient.Id;
            return loadClient;
        }

        /// <summary>
        /// Sets mock LoadClient data.
        /// </summary>
        /// <param name="name">LoadClient name.</param>
        public void SetMockData(string name)
        {
            this.Name = name;
            this.Version = "1.0.1";
            this.Region = "Central";
            this.Zone = "central-az-1";
            this.Prometheus = true;
            this.StartupArgs = "--mode Client --region Central --zone central-az-1 --prometheus true";
            this.StartTime = DateTime.UtcNow;
        }
    }
}

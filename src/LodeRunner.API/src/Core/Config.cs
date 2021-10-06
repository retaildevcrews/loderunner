// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using LodeRunner.API.Data;
using LodeRunner.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace LodeRunner.API
{
    /// <summary>
    /// Configurations for App.
    /// </summary>
    public class Config : ICosmosConfig, ICommonConfig
    {
        private string urlPrefix;
        private LogLevel logLevel = LogLevel.Warning;
        private LogLevel requestLogLevel = LogLevel.Information;
        private string secretsVolume = "secrets";
        private string cosmosName = string.Empty;

        /// <summary>
        /// Gets or sets the logLevel.
        /// </summary>
        public LogLevel LogLevel
        {
            get
            {
                return logLevel;
            }

            set
            {
                logLevel = value <= LogLevel.Information ? LogLevel.Information : value;
            }
        }

        /// <summary>
        /// Gets or sets the requestLogLevel.
        /// </summary>
        public LogLevel RequestLogLevel
         {
            get
            {
                return requestLogLevel;
            }

            set
            {
                requestLogLevel = value <= LogLevel.Information ? LogLevel.Information : value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the logLevel is set.
        /// </summary>
        public bool IsLogLevelSet { get; set; }

        /// <summary>
        /// Gets or sets urlPrefix.
        /// </summary>
        public string UrlPrefix
        {
            get
            {
                return urlPrefix;
            }

            set
            {
                urlPrefix = string.IsNullOrWhiteSpace(value) ? string.Empty : value.TrimEnd('/');
            }
        }

        /// <summary>
        /// Gets or sets the secretsVolume that contains the secrets.
        /// </summary>
        public string SecretsVolume
        {
            get
            {
                return secretsVolume;
            }

            set
            {
                secretsVolume = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets cosmosName.
        /// </summary>
        public string CosmosName
        {
            get
            {
                return cosmosName;
            }

            set
            {
                cosmosName = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets Secrets.
        /// </summary>
        public ISecrets Secrets { get; set; }

        /// <summary>
        /// Gets or sets WebHostPort.
        /// </summary>
        public int WebHostPort { get; set; } = 8080;

        /// <summary>
        /// Gets or sets Cosmos max retries.
        /// </summary>
        public int Retries { get; set; } = 10;

        /// <summary>Gets or sets the cosmos max retry wait time for cosmos requests in seconds.</summary>
        /// <value>Time in seconds.</value>
        public int CosmosTimeout { get; set; } = 60;
    }
}

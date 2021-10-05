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

        // ICommonConfig
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

        public bool IsLogLevelSet { get; set; }

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

        // Cosmos IConfig
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

        public ISecrets Secrets { get; set; }

        public int WebHostPort { get; set; } = 8080;

        public int Retries { get; set; } = 10;

        /// <summary>Gets or sets the cosmos max retry wait time for cosmos requests in seconds.</summary>
        /// <value>Time in seconds.</value>
        public int CosmosTimeout { get; set; } = 60;
    }
}

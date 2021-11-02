// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Extensions.Configuration;

namespace LodeRunner.Core
{
    /// <summary>
    /// App Configuration helper class.
    /// </summary>
    public class AppConfigurationHelper
    {
        /// <summary>
        /// Gets the LoadRunner port.
        /// </summary>
        /// <param name="defaultPort">The default port.</param>
        /// <returns>The port number for LodeRunner to run on.</returns>
        public static int GetLoadRunnerPort(int defaultPort)
        {
            return GetPort(defaultPort, SystemConstants.LodeRunnerPortSettingName);
        }

        /// <summary>
        /// Gets the LoadRunner API port.
        /// </summary>
        /// <param name="defaultPort">The default port.</param>
        /// <returns>The port number for LodeRunner.API to run on.</returns>
        public static int GetLoadRunnerApiPort(int defaultPort)
        {
            return GetPort(defaultPort, SystemConstants.LodeRunnerAPIPortSettingName);
        }

        /// <summary>
        /// Gets the load runner UI port.
        /// </summary>
        /// <param name="defaultPort">The default port.</param>
        /// <returns>>The port number for LodeRunner.UI to run on.</returns>
        public static int GetLoadRunnerUIPort(int defaultPort)
        {
            return GetPort(defaultPort, SystemConstants.LodeRunnerUIPortSettingName);
        }

        /// <summary>
        /// Gets the app port.
        /// </summary>
        /// <param name="defaultPort">The default port.</param>
        /// <param name="appName">The name of the application as defined in the configuration json files.</param>
        /// <returns>The port number for LodeRunner to run on.</returns>
        private static int GetPort(int defaultPort, string appName)
        {
            string envName = Environment.GetEnvironmentVariable(SystemConstants.AspNetCoreEnviroment);

            // NOTE: the Option parameter is false, meaning that the file must exist.
            var configBuilder = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.{envName}.json", false);

            var hostConfig = configBuilder.Build();

            if (!int.TryParse(hostConfig[appName], out int lodeRunnerPort))
            {
                lodeRunnerPort = defaultPort;
            }

            return lodeRunnerPort;
        }
    }
}

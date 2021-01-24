// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace Ngsa.LodeRunner
{
    /// <summary>
    /// WebHostBuilder Startup
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">the configuration for WebHost</param>
        public Startup(IConfiguration configuration)
        {
            // keep a local reference
            Configuration = configuration;
        }

        /// <summary>
        /// Gets IConfiguration
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Configure the application builder
        /// </summary>
        /// <param name="app">IApplicationBuilder</param>
        public static void Configure(IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            // version handler
            app.UseVersion();
        }
    }
}

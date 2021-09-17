﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using LodeRunner.API.Models;
using LodeRunner.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace LodeRunner.API.Controllers
{
    /// <summary>
    /// Handle the /healthz* requests
    ///
    /// Cache results to prevent monitoring from overloading service
    /// </summary>
    [Route("[controller]")]
    [ResponseCache(Duration = 60)]
    public class HealthzController : Controller
    {
        private readonly ILogger logger;
        private readonly ILogger<CosmosHealthCheck> hcLogger;

        /// <summary>
        /// Initializes a new instance of the <see cref="HealthzController"/> class.
        /// </summary>
        /// <param name="logger">logger</param>
        /// <param name="dal">data access layer</param>
        /// <param name="hcLogger">HealthCheck logger</param>
        public HealthzController(ILogger<HealthzController> logger, ILogger<CosmosHealthCheck> hcLogger)
        {
            this.logger = logger;
            this.hcLogger = hcLogger;
        }

        /// <summary>
        /// Returns a plain text health status (Healthy, Degraded or Unhealthy)
        /// </summary>
        /// <param name="clientStatusService">The client status service.</param>
        /// <returns>The IActionResult.</returns>
        [HttpGet]
        [Produces("text/plain")]
        [ProducesResponseType(typeof(string), 200)]
        public async Task<IActionResult> RunHealthzAsync([FromServices] IClientStatusService clientStatusService)
        {
            // get list of genres as list of string
            logger.LogInformation(nameof(RunHealthzAsync));

            HealthCheckResult res = await RunCosmosHealthCheck(clientStatusService).ConfigureAwait(false);

            HttpContext.Items.Add(typeof(HealthCheckResult).ToString(), res);

            ContentResult result = new ()
            {
                Content = IetfCheck.ToIetfStatus(res.Status),
                StatusCode = res.Status == HealthStatus.Unhealthy ? (int)System.Net.HttpStatusCode.ServiceUnavailable : (int)System.Net.HttpStatusCode.OK,
            };

            return result;
        }

        /// <summary>
        /// Returns an IETF (draft) health+json representation of the full Health Check
        /// </summary>
        /// <param name="clientStatusService">The client status service.</param>
        /// <returns>IActionResult</returns>
        [HttpGet("ietf")]
        [Produces("application/health+json")]
        [ProducesResponseType(typeof(CosmosHealthCheck), 200)]
        public async Task RunIetfAsync([FromServices] IClientStatusService clientStatusService)
        {
            logger.LogInformation(nameof(RunHealthzAsync));

            DateTime dt = DateTime.UtcNow;

            HealthCheckResult res = await RunCosmosHealthCheck(clientStatusService).ConfigureAwait(false);

            HttpContext.Items.Add(typeof(HealthCheckResult).ToString(), res);

            await CosmosHealthCheck.IetfResponseWriter(HttpContext, res, DateTime.UtcNow.Subtract(dt)).ConfigureAwait(false);
        }

        /// <summary>
        /// Run the health check
        /// </summary>
        /// <param name="clientStatusService">The client status service.</param>
        /// <returns>HealthCheckResult</returns>
        private async Task<HealthCheckResult> RunCosmosHealthCheck(IClientStatusService clientStatusService)
        {
            CosmosHealthCheck chk = new (hcLogger, clientStatusService);

            return await chk.CheckHealthAsync(new HealthCheckContext()).ConfigureAwait(false);
        }
    }
}
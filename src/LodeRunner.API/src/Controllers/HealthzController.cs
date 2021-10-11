// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using LodeRunner.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace LodeRunner.API.Controllers
{
    /// <summary>
    /// Handle the /healthz* requests
    ///
    /// Cache results to prevent monitoring from overloading service.
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
        /// <param name="logger">logger.</param>
        /// <param name="dal">data access layer.</param>
        /// <param name="hcLogger">HealthCheck logger.</param>
        public HealthzController(ILogger<HealthzController> logger, ILogger<CosmosHealthCheck> hcLogger)
        {
            this.logger = logger;
            this.hcLogger = hcLogger;
        }

        /// <summary>
        /// Returns a plain text health status (Healthy, Degraded or Unhealthy).
        /// </summary>
        /// <param name="clientStatusService">The client status service.</param>
        /// <param name="cancellationTokenSource">The cancellation Token Source.</param>
        /// <returns>The IActionResult.</returns>
        [HttpGet]
        [Produces("text/plain")]
        [ProducesResponseType(typeof(string), 200)]
        public async Task<IActionResult> RunHealthzAsync([FromServices] IClientStatusService clientStatusService, [FromServices] CancellationTokenSource cancellationTokenSource)
        {
            // get list of genres as list of string
            this.logger.LogInformation(nameof(this.RunHealthzAsync));

            HealthCheckResult res = await this.RunCosmosHealthCheck(clientStatusService, cancellationTokenSource).ConfigureAwait(false);

            this.HttpContext.Items.Add(typeof(HealthCheckResult).ToString(), res);

            return res.GetContentResult();
        }

        /// <summary>
        /// Returns an IETF (draft) health+json representation of the full Health Check.
        /// </summary>
        /// <param name="clientStatusService">The client status service.</param>
        /// <param name="cancellationTokenSource">The cancellation Token Source.</param>
        /// <returns>IActionResult.</returns>
        [HttpGet("ietf")]
        [Produces("application/health+json")]
        [ProducesResponseType(typeof(CosmosHealthCheck), 200)]
        public async Task RunIetfAsync([FromServices] IClientStatusService clientStatusService,  [FromServices] CancellationTokenSource cancellationTokenSource)
        {
            this.logger.LogInformation(nameof(this.RunHealthzAsync));

            DateTime dt = DateTime.UtcNow;

            HealthCheckResult res = await this.RunCosmosHealthCheck(clientStatusService, cancellationTokenSource).ConfigureAwait(false);

            this.HttpContext.Items.Add(typeof(HealthCheckResult).ToString(), res);

            await CosmosHealthCheck.IetfResponseWriter(this.HttpContext, res, DateTime.UtcNow.Subtract(dt)).ConfigureAwait(false);
        }

        /// <summary>
        /// Run the health check.
        /// </summary>
        /// <param name="clientStatusService">The client status service.</param>
        /// <param name="cancellationTokenSource">The cancellation Token Source.</param>
        /// <returns>HealthCheckResult.</returns>
        private async Task<HealthCheckResult> RunCosmosHealthCheck(IClientStatusService clientStatusService, CancellationTokenSource cancellationTokenSource)
        {
            CosmosHealthCheck chk = new (this.hcLogger, clientStatusService);

            return await chk.CheckHealthAsync(new HealthCheckContext(), cancellationTokenSource.Token).ConfigureAwait(false);
        }
    }
}

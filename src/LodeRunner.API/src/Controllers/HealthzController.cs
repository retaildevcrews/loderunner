// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using LodeRunner.Core.Interfaces;
using LodeRunner.Core.Models;
using LodeRunner.Core.SchemaFilters;
using LodeRunner.Data.Interfaces;
using LodeRunner.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;

namespace LodeRunner.API.Controllers
{
    /// <summary>
    /// Handle the /healthz* requests
    ///
    /// Cache results to prevent monitoring from overloading service.
    /// </summary>
    [Route("[controller]")]
    [ResponseCache(Duration = 60)]
    [SwaggerTag("Handle the /healthz* requests")]
    public class HealthzController : Controller
    {
        private readonly ILogger logger;
        private readonly ILogger<CosmosHealthCheck> hcLogger;

        /// <summary>
        /// Initializes a new instance of the <see cref="HealthzController"/> class.
        /// </summary>
        /// <param name="logger">logger.</param>
        /// <param name="hcLogger">HealthCheck logger.</param>
        public HealthzController(ILogger<HealthzController> logger, ILogger<CosmosHealthCheck> hcLogger)
        {
            this.logger = logger;
            this.hcLogger = hcLogger;
        }

        /// <summary>
        /// Returns a plain text health status (pass, warn or fail).
        /// </summary>
        /// <param name="clientStatusService">The client status service.</param>
        /// <param name="cancellationTokenSource">The cancellation Token Source.</param>
        /// <param name="config">App Configuration.</param>
        /// <returns>The IActionResult.</returns>
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, "`string` (pass, warn or fail)", null, "text/plain")]
        [SwaggerOperation(
            Summary = "Healthz Check (simple)",
            Description = "Returns a text/plain health status (pass, warn or fail)",
            OperationId = "RunHealthzAsync")]
        public async Task<IActionResult> RunHealthzAsync([FromServices] IClientStatusService clientStatusService, [FromServices] CancellationTokenSource cancellationTokenSource, [FromServices] Config config)
        {
            // get list of genres as list of string
            this.logger.LogInformation(nameof(this.RunHealthzAsync));

            HealthCheckResult res = await this.RunCosmosHealthCheck(clientStatusService, cancellationTokenSource, config).ConfigureAwait(false);

            this.HttpContext.Items.Add(typeof(HealthCheckResult).ToString(), res);

            return res.GetContentResult();
        }

        /// <summary>
        /// Returns an IETF (draft) health+json representation of the full Health Check.
        /// </summary>
        /// <param name="clientStatusService">The client status service.</param>
        /// <param name="cancellationTokenSource">The cancellation Token Source.</param>
        /// <param name="config">App Configuration.</param>
        /// <returns>IActionResult.</returns>
        [HttpGet("ietf")]
        [SwaggerResponse((int)HttpStatusCode.OK, "`IetfHealthCheck`", typeof(IetfHealthCheck), "application/health+json")]
        [SwaggerOperation(
            Summary = "Healthz Check (IETF)",
            Description = "Returns an `IetfHealthCheck` document from the Health Check",
            OperationId = "RunIetfAsync")]
        public async Task RunIetfAsync([FromServices] IClientStatusService clientStatusService,  [FromServices] CancellationTokenSource cancellationTokenSource, [FromServices] Config config)
        {
            this.logger.LogInformation(nameof(this.RunHealthzAsync));

            DateTime dt = DateTime.UtcNow;

            HealthCheckResult res = await this.RunCosmosHealthCheck(clientStatusService, cancellationTokenSource, config).ConfigureAwait(false);

            this.HttpContext.Items.Add(typeof(HealthCheckResult).ToString(), res);

            await CosmosHealthCheck.IetfResponseWriter(this.HttpContext, res, DateTime.UtcNow.Subtract(dt)).ConfigureAwait(false);
        }

        /// <summary>
        /// Run the health check.
        /// </summary>
        /// <param name="clientStatusService">The client status service.</param>
        /// <param name="cancellationTokenSource">The cancellation Token Source.</param>
        /// <param name="cosmosConfig">App Cosmos Configuration.</param>
        /// <returns>HealthCheckResult.</returns>
        private async Task<HealthCheckResult> RunCosmosHealthCheck(IClientStatusService clientStatusService, CancellationTokenSource cancellationTokenSource, ICosmosConfig cosmosConfig)
        {
            CosmosHealthCheck chk = new(this.hcLogger, clientStatusService, cosmosConfig);

            return await chk.CheckHealthAsync(new HealthCheckContext(), cancellationTokenSource.Token).ConfigureAwait(false);
        }
    }
}

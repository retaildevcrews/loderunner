// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using LodeRunner.API.Extensions;
using LodeRunner.API.Middleware;
using LodeRunner.API.Models;
using LodeRunner.Core.Models;
using LodeRunner.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;

namespace LodeRunner.API.Controllers
{
    /// <summary>
    /// Handle all of the /api/clients requests.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [SwaggerTag("Read Clients")]
    public class ClientsController : Controller
    {
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientsController"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public ClientsController(ILogger<ClientsController> logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Returns a JSON array of Client objects.
        /// </summary>
        /// <param name="clientStatusService">The ClientStatusService.</param>
        /// <param name="cancellationTokenSource">The cancellation Token Source.</param>
        /// <returns>IActionResult.</returns>
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, "Array of `Client` documents.", typeof(Client[]), "application/json")]
        [SwaggerResponse((int)HttpStatusCode.NoContent, "`Data not found.`", null, "text/plain")]
        [SwaggerResponse((int)HttpStatusCode.ServiceUnavailable, SystemConstants.TerminationDescription)]
        [SwaggerOperation(
            Summary = "Gets a JSON array of Client objects",
            Description = "Returns an array of `Client` documents",
            OperationId = "GetClients")]
        public async Task<ActionResult<IEnumerable<Client>>> GetClients([FromServices] ClientStatusService clientStatusService, [FromServices] CancellationTokenSource cancellationTokenSource)
        {
            if (cancellationTokenSource != null && cancellationTokenSource.IsCancellationRequested)
            {
                return await ResultHandler.CreateCancellationInProgressResult();
            }

            var result = await clientStatusService.GetClients(logger);

            return await ResultHandler.HandleResult(result, logger);
        }

        /// <summary>
        /// Returns a single JSON Client by Parameter, clientStatusId.
        /// </summary>
        /// <param name="clientStatusId">clientStatusId.</param>
        /// <param name="clientStatusService">The ClientStatusService.</param>
        /// <param name="cancellationTokenSource">The cancellation Token Source.</param>
        /// <returns>IActionResult.</returns>
        [HttpGet("{clientStatusId}")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Single `Client` document by clientStatusId.", typeof(Client), "application/json")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, SystemConstants.InvalidClientStatusId, typeof(Middleware.Validation.ValidationError), "application/problem+json")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "`Data not found.`", null, "application/json")]
        [SwaggerResponse((int)HttpStatusCode.ServiceUnavailable, SystemConstants.TerminationDescription)]
        [SwaggerOperation(
            Summary = "Gets a single JSON Client by Parameter, clientStatusId.",
            Description = "Returns a single `Client` document by clientStatusId",
            OperationId = "GetClientByClientStatusId")]
        public async Task<ActionResult<Client>> GetClientByClientStatusId([FromRoute] string clientStatusId, [FromServices] ClientStatusService clientStatusService, [FromServices] CancellationTokenSource cancellationTokenSource)
        {
            if (cancellationTokenSource != null && cancellationTokenSource.IsCancellationRequested)
            {
                return await ResultHandler.CreateCancellationInProgressResult();
            }

            List<Middleware.Validation.ValidationError> errorlist = ParametersValidator<ClientStatus>.ValidateEntityId(clientStatusId);

            if (errorlist.Count > 0)
            {
                logger.LogWarning(new EventId((int)HttpStatusCode.BadRequest, nameof(GetClientByClientStatusId)), $"{SystemConstants.InvalidClientStatusId}");

                return await ResultHandler.CreateBadRequestResult(errorlist, RequestLogger.GetPathAndQuerystring(this.Request));
            }

            var result = await clientStatusService.GetClientByClientStatusId(clientStatusId, logger);

            return await ResultHandler.HandleResult(result, logger);
        }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using LodeRunner.API.Interfaces;
using LodeRunner.API.Middleware;
using LodeRunner.API.Models;
using Microsoft.AspNetCore.Mvc;
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
        private static readonly NgsaLog Logger = new ()
        {
            Name = typeof(ClientsController).FullName,
            ErrorMessage = "ClientsControllerException",
            NotFoundError = "Clients Not Found",
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientsController"/> class.
        /// </summary>
        public ClientsController()
        {
        }

        /// <summary>
        /// Returns a JSON array of Client objects.
        /// </summary>
        /// <param name="appCache">The cache service.</param>
        /// <param name="cancellationTokenSource">The cancellation Token Source.</param>
        /// <returns>IActionResult.</returns>
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, "Array of Client documents or empty array if not found.", typeof(Client[]), "application/json")]
        [SwaggerResponse((int)HttpStatusCode.ServiceUnavailable, SystemConstants.TerminationDescription)]
        [SwaggerOperation(
            Summary = "Gets a JSON array of Client objects",
            Description = "Returns an array of Client documents",
            OperationId = "GetClients")]
        public IActionResult GetClients([FromServices] ILRAPICache appCache, [FromServices] CancellationTokenSource cancellationTokenSource)
        {
            if (cancellationTokenSource != null && cancellationTokenSource.IsCancellationRequested)
            {
                return ResultHandler.CreateCancellationInProgressResult();
            }

            IEnumerable<Client> clients = appCache.GetClients();

            return appCache.HandleCacheResult(clients, Logger);
        }

        /// <summary>
        /// Returns a single JSON Client by Parameter, clientStatusId.
        /// </summary>
        /// <param name="clientStatusId">clientStatusId.</param>
        /// <param name="appCache">The cache service.</param>
        /// <param name="cancellationTokenSource">The cancellation Token Source.</param>
        /// <returns>IActionResult.</returns>
        [HttpGet("{clientStatusId}")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Single Client document by clientStatusId.", typeof(Client), "application/json")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, SystemConstants.InvalidClientStatusId, typeof(Middleware.Validation.ValidationError), "application/json")]
        [SwaggerResponse((int)HttpStatusCode.ServiceUnavailable, SystemConstants.TerminationDescription)]
        [SwaggerOperation(
            Summary = "Gets a single JSON Client by Parameter, clientStatusId.",
            Description = "Returns a single Client document by clientStatusId",
            OperationId = "GetClientByClientStatusId")]
        public IActionResult GetClientByClientStatusId([FromRoute] string clientStatusId, [FromServices] ILRAPICache appCache, [FromServices] CancellationTokenSource cancellationTokenSource)
        {
            if (cancellationTokenSource != null && cancellationTokenSource.IsCancellationRequested)
            {
                return ResultHandler.CreateCancellationInProgressResult();
            }

            List<Middleware.Validation.ValidationError> errorlist = ClientParameters.ValidateClientStatusId(clientStatusId);

            if (errorlist.Count > 0)
            {
                Logger.LogWarning(nameof(this.GetClientByClientStatusId), SystemConstants.InvalidClientStatusId, NgsaLog.LogEvent400, this.HttpContext);

                return ResultHandler.CreateResult(errorlist, RequestLogger.GetPathAndQuerystring(this.Request));
            }

            Client client = appCache.GetClientByClientStatusId(clientStatusId);

            return appCache.HandleCacheResult(client, Logger);
        }
    }
 }

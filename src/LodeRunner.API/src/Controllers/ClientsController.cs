// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using LodeRunner.API.Data;
using LodeRunner.API.Interfaces;
using LodeRunner.API.Middleware;
using LodeRunner.API.Models;
using LodeRunner.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LodeRunner.API.Controllers
{
    /// <summary>
    /// Handle all of the /api/clients requests.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
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
        public IActionResult GetClientByClientStatusId([FromRoute] string clientStatusId, [FromServices] ILRAPICache appCache, [FromServices] CancellationTokenSource cancellationTokenSource)
        {
            if (cancellationTokenSource != null && cancellationTokenSource.IsCancellationRequested)
            {
                return ResultHandler.CreateCancellationInProgressResult();
            }

            if (string.IsNullOrWhiteSpace(clientStatusId))
            {
                return ResultHandler.CreateResult("clientStatusId cannot be empty.", HttpStatusCode.BadRequest);
            }

            List<Middleware.Validation.ValidationError> list = ClientParameters.ValidateClientStatusId(clientStatusId);

            if (list.Count > 0)
            {
                Logger.LogWarning(nameof(this.GetClientByClientStatusId), "Invalid Client Status Id", NgsaLog.LogEvent400, this.HttpContext);

                return ResultHandler.CreateResult(list, RequestLogger.GetPathAndQuerystring(this.Request));
            }

            Client client = appCache.GetClientByClientStatusId(clientStatusId);

            return appCache.HandleCacheResult(client, Logger);
        }
     }
 }

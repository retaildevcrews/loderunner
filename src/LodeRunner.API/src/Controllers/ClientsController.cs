// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
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
        [SwaggerResponse((int)HttpStatusCode.OK, "Array of `Client` documents.", typeof(Client[]), "application/json")]
        [SwaggerResponse((int)HttpStatusCode.NoContent, "`Data not found.`", null, "text/plain")]
        [SwaggerResponse((int)HttpStatusCode.ServiceUnavailable, SystemConstants.TerminationDescription)]
        [SwaggerOperation(
            Summary = "Gets a JSON array of Client objects",
            Description = "Returns an array of `Client` documents",
            OperationId = "GetClients")]
        public async Task<ActionResult<IEnumerable<Client>>> GetClients([FromServices] ILRAPICache appCache, [FromServices] CancellationTokenSource cancellationTokenSource)
        {
            if (cancellationTokenSource != null && cancellationTokenSource.IsCancellationRequested)
            {
                return await ResultHandler.CreateCancellationInProgressResult();
            }

            return await appCache.GetClients();
        }

        /// <summary>
        /// Returns a single JSON Client by Parameter, clientStatusId.
        /// </summary>
        /// <param name="clientStatusId">clientStatusId.</param>
        /// <param name="appCache">The cache service.</param>
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
        public async Task<ActionResult<Client>> GetClientByClientStatusId([FromRoute] string clientStatusId, [FromServices] ILRAPICache appCache, [FromServices] CancellationTokenSource cancellationTokenSource)
        {
            if (cancellationTokenSource != null && cancellationTokenSource.IsCancellationRequested)
            {
                return await ResultHandler.CreateCancellationInProgressResult();
            }

            List<Middleware.Validation.ValidationError> errorlist = ClientParameters.ValidateClientStatusId(clientStatusId);

            if (errorlist.Count > 0)
            {
                await Logger.LogWarning(nameof(this.GetClientByClientStatusId), SystemConstants.InvalidClientStatusId, NgsaLog.LogEvent400, this.HttpContext);

                return await ResultHandler.CreateBadRequestResult(errorlist, RequestLogger.GetPathAndQuerystring(this.Request));
            }

            return await appCache.GetClientByClientStatusId(clientStatusId);
        }
    }
 }

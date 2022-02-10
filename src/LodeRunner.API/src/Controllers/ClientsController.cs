// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using LodeRunner.API.Extensions;
using LodeRunner.API.Middleware;
using LodeRunner.API.Models;
using LodeRunner.Core.Models;
using LodeRunner.Data.Interfaces;
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
        [SwaggerResponse((int)HttpStatusCode.OK, SystemConstants.AllClientsFound, typeof(Client[]), "application/json")]
        [SwaggerResponse((int)HttpStatusCode.NoContent, SystemConstants.NoClientsFound, null, "text/plain")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, SystemConstants.UnableToGetClients, typeof(ErrorResult), "application/problem+json")]
        [SwaggerResponse((int)HttpStatusCode.ServiceUnavailable, SystemConstants.TerminationDescription, typeof(ErrorResult), "application/problem+json")]
        [SwaggerOperation(
            Summary = "Gets a JSON array of Client objects",
            Description = "Returns an array of `Client` documents",
            OperationId = "GetClients")]
        public async Task<ActionResult<IEnumerable<Client>>> GetClients([FromServices] IClientStatusService clientStatusService, [FromServices] CancellationTokenSource cancellationTokenSource)
        {
            if (cancellationTokenSource != null && cancellationTokenSource.IsCancellationRequested)
            {
                return ResultHandler.CreateServiceUnavailableResponse();
            }

            return await ResultHandler.CreateGetResponse<Client>(clientStatusService.GetClients, logger);
        }

        /// <summary>
        /// Returns a single JSON Client by Parameter, clientStatusId.
        /// </summary>
        /// <param name="clientStatusId">clientStatusId.</param>
        /// <param name="clientStatusService">The ClientStatusService.</param>
        /// <param name="cancellationTokenSource">The cancellation Token Source.</param>
        /// <returns>IActionResult.</returns>
        [HttpGet("{clientStatusId}")]
        [SwaggerResponse((int)HttpStatusCode.OK, SystemConstants.ClientItemFound, typeof(Client), "application/json")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, SystemConstants.InvalidClientStatusId, typeof(Dictionary<string, object>), "application/problem+json")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, SystemConstants.ClientItemNotFound, null, "text/plain")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, SystemConstants.UnableToGetClientItem, typeof(ErrorResult), "application/problem+json")]
        [SwaggerResponse((int)HttpStatusCode.ServiceUnavailable, SystemConstants.TerminationDescription)]
        [SwaggerOperation(
            Summary = "Gets a single JSON Client by Parameter, clientStatusId.",
            Description = "Returns a single `Client` document by clientStatusId",
            OperationId = "GetClientByClientStatusId")]
        public async Task<ActionResult<Client>> GetClientByClientStatusId([FromRoute] string clientStatusId, [FromServices] IClientStatusService clientStatusService, [FromServices] CancellationTokenSource cancellationTokenSource)
        {
            if (cancellationTokenSource != null && cancellationTokenSource.IsCancellationRequested)
            {
                return ResultHandler.CreateServiceUnavailableResponse();
            }

            List<string> errorList = ParametersValidator<ClientStatus>.ValidateEntityId(clientStatusId);

            var path = RequestLogger.GetPathAndQuerystring(this.Request);

            return await ResultHandler.CreateGetByIdResponse(clientStatusService.GetClientByClientStatusId, clientStatusId, path, errorList, Logger);
        }
    }
}

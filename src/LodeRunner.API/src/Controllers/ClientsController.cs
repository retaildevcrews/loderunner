// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using LodeRunner.API.Application.Data;
using LodeRunner.API.Middleware;
using LodeRunner.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace LodeRunner.API.Application.Controllers
{
    /// <summary>
    /// Handle all of the /api/clients requests
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

        private readonly ICache cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientsController"/> class.
        /// </summary>
        public ClientsController()
        {
            cache = App.Config.Cache;
        }

        /// <summary>
        /// Returns a JSON array of Client objects
        /// </summary>
        /// <returns>IActionResult</returns>
        [HttpGet]
        public IActionResult GetClients()
        {
            List<Client> clients = (List<Client>)cache.GetClients();
            return Cache.HandleCacheResult<IEnumerable<Client>>(clients, Logger);
        }

         /// <summary>
         /// Returns a single JSON Client by Parameter, clientStatusId
         /// </summary>
         /// <param name="clientStatusId">clientStatusId</param>
         /// <returns>IActionResult</returns>
        [HttpGet("{clientStatusId}")]
        public IActionResult GetClientByClientStatusId([FromRoute] string clientStatusId)
        {
            if (string.IsNullOrWhiteSpace(clientStatusId))
            {
                throw new ArgumentNullException(nameof(clientStatusId));
            }

            List<Middleware.Validation.ValidationError> list = ClientParameters.ValidateClientStatusId(clientStatusId);

            if (list.Count > 0)
            {
                Logger.LogWarning(nameof(GetClientByClientStatusId), "Invalid Client Status Id", NgsaLog.LogEvent400, HttpContext);

                return ResultHandler.CreateResult(list, RequestLogger.GetPathAndQuerystring(Request));
            }

            Client client = cache.GetClientByClientStatusId(clientStatusId);

            return Cache.HandleCacheResult(client, Logger);
        }
     }
 }

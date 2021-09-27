// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using LodeRunner.API.Interfaces;
using LodeRunner.API.Middleware;
using LodeRunner.API.Models;
using LodeRunner.Core.Cache;
using LodeRunner.Core.Models;
using LodeRunner.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace LodeRunner.API.Data
{
    /// <summary>
    /// Cached Data
    /// </summary>
    public class LRAPICache : BaseAppCache, ILRAPCache
    {
        private readonly NgsaLog logger = new ()
        {
            Name = typeof(LRAPICache).FullName,
            ErrorMessage = "CacheException",
            NotFoundError = "Cached values not found",
        };

        private readonly IClientStatusService clientStatusService;
        private readonly ILoadTestConfigService loadTestConfigService;

        public LRAPICache(IClientStatusService clientStatusService, ILoadTestConfigService loadTestConfigService, CancellationTokenSource cancellationTokenSource)
            : base(cancellationTokenSource)
        {
            this.clientStatusService = clientStatusService;
            this.loadTestConfigService = loadTestConfigService;
            SetClientCache();
        }

        public IActionResult HandleCacheResult<TEntity>(TEntity results, NgsaLog logger)
        {
            // log the request
            logger.LogInformation(nameof(HandleCacheResult), "Cache data request");

            // return exception if task is null
            if (results == null)
            {
                logger.LogError(nameof(HandleCacheResult), "Exception: task is null", NgsaLog.LogEvent500, ex: new ArgumentNullException(nameof(results)));

                return ResultHandler.CreateResult(logger.ErrorMessage, HttpStatusCode.InternalServerError);
            }

            try
            {
                // return an OK object result
                return new OkObjectResult(results);
            }
            catch (Exception ex)
            {
                // log and return exception
                logger.LogError(nameof(HandleCacheResult), "Exception", NgsaLog.LogEvent500, ex: ex);

                // return 500 error
                return ResultHandler.CreateResult("Internal Server Error", HttpStatusCode.InternalServerError);
            }
        }

        public Client GetClientByClientStatusId(string clientStatusId)
        {
            this.ValidateEntityId(clientStatusId);

            return this.GetEntry<Client>(clientStatusId);
        }

        public IEnumerable<Client> GetClients()
        {
            return this.GetEntries<Client>();
        }

        public void ProcessClientStatusChange(Document doc)
        {
            ClientStatus clientStatus = (dynamic)doc;

            // TODO: Need to validate clientStatus all together  before to create Client ?

            ValidateEntityId(clientStatus.Id);

            this.SetEntry(clientStatus.Id, new Client(clientStatus), GetMemoryCacheEntryOptions());
        }

        /// <summary>
        /// Provide basic validation for ClientStatus Id .
        /// </summary>
        /// <param name="id">The client status identifier.</param>
        private void ValidateEntityId(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException($"{this.GetType().Name}: clientStatusId cannot be null or empty invalid");
            }

            // TODO : Need to validate Id as GUID
            //Guid guidValue;
            //try
            //{
            //    guidValue = Guid.Parse(id);
            //}
            //catch (Exception ex)
            //{
            //    throw new InvalidDataException($"Invalid GUID value {id}'", ex);
            //}
        }

        /// <summary>
        /// Gets the memory cache entry options.
        /// </summary>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <returns>
        /// The MemoryCacheEntryOptions.
        /// </returns>
        private MemoryCacheEntryOptions GetMemoryCacheEntryOptions()
        {
            return new MemoryCacheEntryOptions()
             .RegisterPostEvictionCallback(async (key, value, reason, state) =>
             {
                 // log the request
                 logger.LogInformation(nameof(LRAPICache), "Cache data request.");

                 // NOTE: EvictionReason.Removed or EvictionReason.Replaced
                 if (reason <= EvictionReason.Replaced)
                 {
                     // NOTE: Scenario LRAPI collection has pending feed change that Calls ProcessClientStatusChange(), then it triggers a key replace event.
                     // Also if we already initiated Cache.Set() to do a replace we do not need to query and replace the key,
                     // neither if key was removed for a EvictionReason other that  Expired,  TokenExpired  or  Capacity.
                     // if so  this will cause a indefinitely loop, since we call this.Cache.Set to update the key.

                     return;
                 }

                 string clientStatusId = key.ToString();
                 try
                 {
                     // Get Client Status from Cosmos
                     ClientStatus clientStatus = await clientStatusService.Get(clientStatusId);

                     // if still exists, update
                     this.SetEntry(key, new Client(clientStatus), GetMemoryCacheEntryOptions());
                 }
                 catch (CosmosException ce)
                 {
                     // log Cosmos status code
                     if (ce.StatusCode == HttpStatusCode.NotFound)
                     {
                         logger.LogInformation("MemoryCacheEntryOptions.RegisterPostEvictionCallback", $"{logger.NotFoundError}: Removing Client {clientStatusId} from Cache");
                     }
                     else
                     {
                         logger.LogError("MemoryCacheEntryOptions.RegisterPostEvictionCallback", ce.ActivityId, new LogEventId((int)ce.StatusCode, "CosmosException"), ex: ce);
                     }
                 }
                 catch (Exception ex)
                 {
                     // log exception
                     logger.LogError("MemoryCacheEntryOptions.RegisterPostEvictionCallback", "Exception", NgsaLog.LogEvent500, ex: ex);
                 }
             })
             .SetSlidingExpiration(TimeSpan.FromSeconds(65))
             .AddExpirationToken(new CancellationChangeToken(this.CancellationTokenSource.Token));
        }

        private void SetClientCache()
        {
            // log the request
            logger.LogInformation(nameof(LRAPICache), "Cache data request");

            try
            {
                // cache client statuses
                var results = clientStatusService.GetAll().Result.ToList();

                foreach (var item in results)
                {
                    var client = new Client(item);

                    this.SetEntry(client.ClientStatusId, new Client(item), GetMemoryCacheEntryOptions());
                }
            }
            catch (CosmosException ce)
            {
                // log and return Cosmos status code
                if (ce.StatusCode == HttpStatusCode.NotFound)
                {
                    logger.LogWarning(nameof(SetClientCache), logger.NotFoundError, new LogEventId((int)ce.StatusCode, string.Empty));
                }

                logger.LogError(nameof(SetClientCache), ce.ActivityId, new LogEventId((int)ce.StatusCode, "CosmosException"), ex: ce);
            }
            catch (Exception ex)
            {
                // log and return exception
                logger.LogError("Handle<T>", "Exception", NgsaLog.LogEvent500, ex: ex);
            }
        }
    }
}

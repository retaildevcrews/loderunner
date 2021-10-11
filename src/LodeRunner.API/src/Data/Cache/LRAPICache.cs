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
using CoreSystemConstants = LodeRunner.Core.SystemConstants;

namespace LodeRunner.API.Data
{
    /// <summary>
    /// Cached Data.
    /// </summary>
    public class LRAPICache : BaseAppCache, ILRAPICache
    {
        private const string CacheDataRequest = "Cache data request.";
        private const string DataNotFound = "Requested data not found in Cache.";

        private readonly NgsaLog logger = new ()
        {
            Name = typeof(LRAPICache).FullName,
            ErrorMessage = "CacheException",
            NotFoundError = "Cached values not found",
        };

        private readonly IClientStatusService clientStatusService;
        private readonly ILoadTestConfigService loadTestConfigService;

        /// <summary>
        /// Initializes a new instance of the <see cref="LRAPICache"/> class.
        /// </summary>
        /// <param name="clientStatusService">Injection of client status service.</param>
        /// <param name="loadTestConfigService">injection of load test config service.</param>
        /// <param name="cancellationTokenSource">Cancellation token source.</param>
        public LRAPICache(IClientStatusService clientStatusService, ILoadTestConfigService loadTestConfigService, CancellationTokenSource cancellationTokenSource)
            : base(cancellationTokenSource)
        {
            this.clientStatusService = clientStatusService;
            this.loadTestConfigService = loadTestConfigService;
            this.SetClientCache();
        }

        /// <summary>
        /// Handles cache results.
        /// </summary>
        /// <typeparam name="TEntity">Entity type of results.</typeparam>
        /// <param name="results">Results from cache get.</param>
        /// <param name="logger">Logger.</param>
        /// <returns>IActionResult.</returns>
        public IActionResult HandleCacheResult<TEntity>(IEnumerable<TEntity> results, NgsaLog logger)
        {
            // log the request
            logger.LogInformation(nameof(this.HandleCacheResult), CacheDataRequest);

            if (!results.Any())
            {
                logger.LogInformation(nameof(this.HandleCacheResult), DataNotFound);

                return ResultHandler.CreateResult(DataNotFound, HttpStatusCode.NoContent);
            }

            return this.InternalReturnOKResult(results);
        }

        /// <inheritdoc/>
        public IActionResult HandleCacheResult<TEntity>(TEntity results, NgsaLog logger)
        {
            // log the request
            logger.LogInformation(nameof(this.HandleCacheResult), CacheDataRequest);

            if (results == null)
            {
                logger.LogInformation(nameof(this.HandleCacheResult), DataNotFound);

                return ResultHandler.CreateResult(DataNotFound, HttpStatusCode.NotFound);
            }

            return this.InternalReturnOKResult(results);
        }

        /// <summary>
        /// Gets client from cache by clientStatusId.
        /// </summary>
        /// <param name="clientStatusId">Client status ID.</param>
        /// <returns>Client.</returns>
        public Client GetClientByClientStatusId(string clientStatusId)
        {
            this.ValidateEntityId(clientStatusId);

            return this.GetEntry<Client>(clientStatusId);
        }

        /// <summary>
        /// Get clients from cache.
        /// </summary>
        /// <returns>clients.</returns>
        public IEnumerable<Client> GetClients()
        {
            return this.GetEntries<Client>();
        }

        /// <summary>
        /// Processes clientStatus changefeed items.
        /// </summary>
        /// <param name="doc">Changefeed item.</param>
        public void ProcessClientStatusChange(Document doc)
        {
            ClientStatus clientStatus = (dynamic)doc;

            // TODO: Need to validate clientStatus all together  before to create Client ?
            this.ValidateEntityId(clientStatus.Id);

            this.SetEntry(clientStatus.Id, new Client(clientStatus), this.GetMemoryCacheEntryOptions());
        }

        /// <summary>
        /// Internals the return OK result.
        /// </summary>
        /// <param name="results">The results.</param>
        /// <returns>The OK Action Result.</returns>
        private IActionResult InternalReturnOKResult(object results)
        {
            try
            {
                // return an OK object result
                return new OkObjectResult(results);
            }
            catch (Exception ex)
            {
                // log and return exception
                this.logger.LogError(nameof(this.HandleCacheResult), "Exception", NgsaLog.LogEvent500, ex: ex);

                // return 500 error
                return ResultHandler.CreateResult("Internal Server Error", HttpStatusCode.InternalServerError);
            }
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
            // Guid guidValue;
            // try
            // {
            //    guidValue = Guid.Parse(id);
            // }
            // catch (Exception ex)
            // {
            //    throw new InvalidDataException($"Invalid GUID value {id}'", ex);
            // }
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
             .SetAbsoluteExpiration(TimeSpan.FromSeconds(CoreSystemConstants.ClientStatusExpirationTime))
             .AddExpirationToken(new CancellationChangeToken(this.CancellationTokenSource.Token))
             .RegisterPostEvictionCallback(async (key, value, reason, state) =>
             {
                 // log the request
                 this.logger.LogInformation(nameof(LRAPICache), CacheDataRequest);

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
                     ClientStatus clientStatus = await this.clientStatusService.Get(clientStatusId);

                     // if still exists, update
                     this.SetEntry(key, new Client(clientStatus), this.GetMemoryCacheEntryOptions());
                 }
                 catch (CosmosException ce)
                 {
                     // log Cosmos status code
                     if (ce.StatusCode == HttpStatusCode.NotFound)
                     {
                         this.logger.LogInformation("MemoryCacheEntryOptions.RegisterPostEvictionCallback", $"{this.logger.NotFoundError}: Removing Client {clientStatusId} from Cache");
                     }
                     else
                     {
                         throw new Exception($"MemoryCacheEntryOptions.RegisterPostEvictionCallback: {ce.Message}", ce);
                     }
                 }
                 catch (Exception ex)
                 {
                     throw new Exception($"MemoryCacheEntryOptions.RegisterPostEvictionCallback: {ex.Message}", ex);
                 }
             });
        }

        private void SetClientCache()
        {
            // log the request
            this.logger.LogInformation(nameof(LRAPICache), CacheDataRequest);

            try
            {
                // cache client statuses
                var results = this.clientStatusService.GetAll().Result.ToList();

                foreach (var item in results)
                {
                    var client = new Client(item);

                    this.SetEntry(client.ClientStatusId, client, this.GetMemoryCacheEntryOptions());
                }
            }
            catch (CosmosException ce)
            {
                // log and return Cosmos status code
                if (ce.StatusCode == HttpStatusCode.NotFound)
                {
                    this.logger.LogWarning(nameof(this.SetClientCache), this.logger.NotFoundError, new LogEventId((int)ce.StatusCode, string.Empty));
                }
                else
                {
                    throw new Exception($"{nameof(this.SetClientCache)}: {ce.Message}", ce);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"{nameof(this.SetClientCache)}: {ex.Message}", ex);
            }
        }
    }
}

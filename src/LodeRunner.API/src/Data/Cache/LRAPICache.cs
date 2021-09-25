// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Caching;
using System.Threading;
using LodeRunner.API.Interfaces;
using LodeRunner.API.Middleware;
using LodeRunner.API.Models;
using LodeRunner.Core.Models;
using LodeRunner.Data.Cache;
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
    public class LRAPICache : BaseCache, ILRAPCache, IDisposable
    {
        private const string ClientPrefix = "client";

        private readonly NgsaLog logger = new ()
        {
            Name = typeof(LRAPICache).FullName,
            ErrorMessage = "CacheException",
            NotFoundError = "Cached values not found",
        };

        private readonly IClientStatusService clientStatusService;
        private readonly ILoadTestConfigService loadTestConfigService;

        private bool disposedValue;

        public LRAPICache(IClientStatusService clientStatusService, ILoadTestConfigService loadTestConfigService, CancellationTokenSource cancellationTokenSource)
            : base(cancellationTokenSource)
        {
            this.clientStatusService = clientStatusService;
            this.loadTestConfigService = loadTestConfigService;
            SetClientCache();
        }

        public IActionResult HandleCacheResult<TFlattenEntity>(TFlattenEntity results, NgsaLog logger)
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

        // implement IDisposable
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public Client GetClientByClientStatusId(string clientStatusId)
        {
            this.ValidateItemId(clientStatusId);

            return this.GetEntryByKey<Client>(ClientPrefix, clientStatusId);
        }

        public IEnumerable<Client> GetClients()
        {
            return this.GetEntries<Client>(ClientPrefix);
        }

        public void ProcessClientStatusChange(Document doc)
        {
            ClientStatus clientStatus = JsonConvert.DeserializeObject<ClientStatus>(doc.ToString());

            // TODO: Validate clientStatus

            string clientKey = $"{ClientPrefix}-{clientStatus.Id}";

            if (this.GetEntry(clientKey) == null)
            {
                List<string> clientStatusIds = (List<string>)this.GetEntry(ClientPrefix);
                clientStatusIds.Add(clientStatus.Id);
                this.SetEntry(ClientPrefix, clientStatusIds, new MemoryCacheEntryOptions());
            }

            // TODO: Need to validate clientStatus before to create Client ?
            //this.SetEntry(clientKey, new Client(clientStatus), GetClientCachePolicy());

            this.SetEntry(clientKey, new Client(clientStatus), GetMemoryCacheEntryOptions(this.CancellationTokenSource));
        }

        /// <summary>
        /// Provide basic validation for ClientStatusId .
        /// </summary>
        /// <param name="id">The client status identifier.</param>
        public override void ValidateItemId(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException($"{this.GetType().Name}: clientStatusId cannot be null or empty invalid");
            }

            Guid guidValue;
            try
            {
                guidValue = Guid.Parse(id);
            }
            catch (Exception ex)
            {
                throw new InvalidDataException($"Invalid GUID value {id}'", ex);
            }
        }

        /// <summary>
        /// Gets the memory cache entry options.
        /// </summary>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <returns>
        /// MemoryCacheEntryOptions.
        /// </returns>
        public override MemoryCacheEntryOptions GetMemoryCacheEntryOptions(CancellationTokenSource cancellationTokenSource)
        {
            string keyClientPrefix = $"{ClientPrefix}-"; // has a dash at the end.
            return new MemoryCacheEntryOptions()
             .RegisterPostEvictionCallback((key, value, reason, state) =>
             {
                 // log the request
                 logger.LogInformation(nameof(LRAPICache), "Cache data request.");

                 if (reason == EvictionReason.Replaced)
                 {
                     // NOTE: Scenario LRAPI collection has pending feed change that Calls ProcessClientStatusChange(), then it trigger a key replace event.
                     // Also if we already initiated case.Set() to do a replace we do not need to query and replace the key
                     // if so  this will cause a indefinitely loop, since we call this.Cache.Set to update the key.

                     return;
                 }

                 string clientStatusId = key.ToString().Replace(keyClientPrefix, string.Empty);

                 try
                 {
                     // Get Client Status from Cosmos
                     ClientStatus clientStatus = clientStatusService.Get(clientStatusId).Result;

                     // if still exists, update
                     this.SetEntry(key, new Client(clientStatus), GetMemoryCacheEntryOptions(cancellationTokenSource));
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

                     // Remove client status ID from cache
                     List<string> clientStatusIds = (List<string>)this.GetEntry(ClientPrefix);

                     clientStatusIds.Remove(clientStatusId);

                     this.SetEntry(ClientPrefix, clientStatusIds, new MemoryCacheEntryOptions());
                 }
                 catch (Exception ex)
                 {
                     // log exception
                     logger.LogError("MemoryCacheEntryOptions.RegisterPostEvictionCallback", "Exception", NgsaLog.LogEvent500, ex: ex);

                     // Remove client status ID from cache
                     List<string> clientStatusIds = (List<string>)this.GetEntry(ClientPrefix);

                     clientStatusIds.Remove(clientStatusId);

                     this.SetEntry(ClientPrefix, clientStatusIds, new MemoryCacheEntryOptions());
                 }
             })
             .SetSlidingExpiration(TimeSpan.FromSeconds(65))
             .AddExpirationToken(new CancellationChangeToken(cancellationTokenSource.Token));
        }

        protected void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                   // objects to dispose
                }

                disposedValue = true;
            }
        }

        private void SetClientCache()
        {
            // log the request
            logger.LogInformation(nameof(LRAPICache), "Cache data request");

            try
            {
                // cache client statuses
                var results = clientStatusService.GetAll().Result.ToList();

                this.SetEntries<ClientStatus, Client>(results, ClientPrefix);
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
            finally
            {
                if (this.GetEntry(ClientPrefix) == null)
                {
                    this.SetEntry(ClientPrefix, new List<string>(), new MemoryCacheEntryOptions());
                }
            }
        }
    }
}

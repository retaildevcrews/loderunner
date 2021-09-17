// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Net;
using System.Runtime.Caching;
using LodeRunner.API.Middleware;
using LodeRunner.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LodeRunner.API.Data
{
    /// <summary>
    /// Cached Data
    /// </summary>
    public partial class Cache : ICache, IDisposable
    {
        private static readonly NgsaLog Logger = new ()
        {
            Name = typeof(Cache).FullName,
            ErrorMessage = "CacheException",
            NotFoundError = "Cached values not found",
        };

        private static IClientStatusService clientStatusService;
        private static ILoadTestConfigService loadTestConfigService;
        private readonly MemoryCache cache;
        private bool disposedValue;

        public Cache(IClientStatusService clientStatusService, ILoadTestConfigService loadTestConfigService)
        {
            cache = new MemoryCache("cache");
            Cache.clientStatusService = clientStatusService;
            Cache.loadTestConfigService = loadTestConfigService;
            SetClientCache();
        }

        public static IActionResult HandleCacheResult<T>(T results, NgsaLog logger)
        {
            // log the request
            logger.LogInformation(nameof(HandleCacheResult), "DS request");

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

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (cache != null)
                    {
                        cache.Dispose();
                    }
                }

                disposedValue = true;
            }
        }
    }
}

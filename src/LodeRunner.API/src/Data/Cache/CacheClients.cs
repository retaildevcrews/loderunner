// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Caching;
using LodeRunner.API.Middleware;
using LodeRunner.API.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents;
using Newtonsoft.Json;

namespace LodeRunner.API.Data
{
    /// <summary>
    /// Data Access Layer for CosmosDB
    /// </summary>
    public partial class Cache
    {
        private static readonly string ClientPrefix = "client";

        public IEnumerable<Client> GetClients()
        {
            List<string> clientStatusIds = (List<string>)cache.Get(ClientPrefix);
            List<Client> clients = new ();
            foreach (string id in clientStatusIds)
            {
                string clientKey = $"{ClientPrefix}-{id}";
                Client client = (Client)cache.Get(clientKey);
                clients.Add(client);
            }

            return clients;
        }

        public Client GetClientByClientStatusId(string clientStatusId)
        {
            if (string.IsNullOrWhiteSpace(clientStatusId))
            {
                throw new ArgumentNullException(nameof(clientStatusId));
            }

            return (Client)cache.Get($"{ClientPrefix}-{clientStatusId}");
        }

        public void ProcessClientStatusChange(Document doc)
        {
            ClientStatus clientStatus = JsonConvert.DeserializeObject<ClientStatus>(doc.ToString());
            string clientKey = $"{ClientPrefix}-{clientStatus.Id}";

            if (cache.Get(clientKey) == null)
            {
                List<string> clientStatusIds = (List<string>)cache.Get(ClientPrefix);
                clientStatusIds.Add(clientStatus.Id);
                cache.Set(ClientPrefix, clientStatusIds, new CacheItemPolicy());
            }

            cache.Set(clientKey, new Client(clientStatus), GetClientCachePolicy());
        }

        private static CacheItemPolicy GetClientCachePolicy()
        {
            return new CacheItemPolicy
            {
                SlidingExpiration = TimeSpan.FromSeconds(65),
                UpdateCallback = async (CacheEntryUpdateArguments args) =>
                {
                    // log the request
                    Logger.LogInformation(nameof(Cache), "DS request");
                    string clientStatusId = args.Key[(ClientPrefix.Length + 1) ..];

                    try
                    {
                        // Get Client Status from Cosmos
                        ClientStatus clientStatus = (ClientStatus)await cosmosDal.GetClientStatusByClientStatusIdAsync(clientStatusId).ConfigureAwait(false);

                        // if still exists, update
                        args.UpdatedCacheItem = new CacheItem(args.Key, new Client(clientStatus));
                    }
                    catch (CosmosException ce)
                    {
                        // log Cosmos status code
                        if (ce.StatusCode == HttpStatusCode.NotFound)
                        {
                            Logger.LogInformation(nameof(CacheItemPolicy.UpdateCallback), $"{Logger.NotFoundError}: Removing Client {clientStatusId} from Cache");
                        }
                        else
                        {
                            Logger.LogError(nameof(CacheItemPolicy.UpdateCallback), ce.ActivityId, new LogEventId((int)ce.StatusCode, "CosmosException"), ex: ce);
                        }

                        // Remove client status ID from cache
                        List<string> clientStatusIds = (List<string>)args.Source.Get(ClientPrefix);
                        clientStatusIds.Remove(args.Key[(ClientPrefix.Length + 1) ..]);
                        args.Source.Set(ClientPrefix, clientStatusIds, new CacheItemPolicy());
                    }
                    catch (Exception ex)
                    {
                        // log exception
                        Logger.LogError(nameof(CacheItemPolicy.UpdateCallback), "Exception", NgsaLog.LogEvent500, ex: ex);

                        // Remove client status ID from cache
                        List<string> clientStatusIds = (List<string>)args.Source.Get(ClientPrefix);
                        clientStatusIds.Remove(args.Key[(ClientPrefix.Length + 1) ..]);
                        args.Source.Set(ClientPrefix, clientStatusIds, new CacheItemPolicy());
                    }
                },
            };
        }

        private async void SetClientCache()
        {
            // log the request
            Logger.LogInformation(nameof(Cache), "DS request");

            try
            {
                // cache client statuses
                List<ClientStatus> results = (List<ClientStatus>)await cosmosDal.GetClientStatusesAsync().ConfigureAwait(false);
                List<string> clientStatusIds = new ();

                foreach (ClientStatus clientStatus in results)
                {
                    clientStatusIds.Add(clientStatus.Id);
                    string clientKey = $"{ClientPrefix}-{clientStatus.Id}";
                    cache.Set(ClientPrefix, clientStatusIds, new CacheItemPolicy());
                    cache.Set(clientKey, new Client(clientStatus), GetClientCachePolicy());
                }
            }
            catch (CosmosException ce)
            {
                // log and return Cosmos status code
                if (ce.StatusCode == HttpStatusCode.NotFound)
                {
                    Logger.LogWarning(nameof(SetClientCache), Logger.NotFoundError, new LogEventId((int)ce.StatusCode, string.Empty));
                }

                Logger.LogError(nameof(SetClientCache), ce.ActivityId, new LogEventId((int)ce.StatusCode, "CosmosException"), ex: ce);
            }
            catch (Exception ex)
            {
                // log and return exception
                Logger.LogError("Handle<T>", "Exception", NgsaLog.LogEvent500, ex: ex);
            }
            finally
            {
                if (cache.Get(ClientPrefix) == null)
                {
                    cache.Set(ClientPrefix, new List<string>(), new CacheItemPolicy());
                }
            }
        }
    }
}

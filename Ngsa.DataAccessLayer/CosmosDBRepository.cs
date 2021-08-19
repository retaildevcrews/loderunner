// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Ngsa.LodeRunner.DataAccessLayer.Interfaces;

namespace Ngsa.LodeRunner.DataAccessLayer
{
    public abstract class CosmosDBRepository<TEntity> : ICosmosDBRepository<TEntity>, IDisposable
                                                where TEntity : class
    {
        private static CosmosClient client;
        private readonly ICosmosDBSettings settings;
        //private readonly CosmosConfig options; this will be use is InternalCosmosDBSqlQuery is implemented
        private readonly object lockObj = new ();
        private Container container;
        private ContainerProperties containerProperties;
        private PropertyInfo partitionKeyPI;

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDBRepository{TEntity}" /> class.
        /// Data Access Layer Constructor
        /// </summary>
        /// <param name="settings">Instance of settings for a ComosDB</param>
        protected CosmosDBRepository(ICosmosDBSettings settings)
        {
            this.settings = settings;

            //this.options = new CosmosConfig
            //{
            //    MaxRows = MaxPageSize,
            //    Timeout = CosmosTimeout,
            //    Retries = settings.Retries,
            //    Timeout = settings.Timeout,
            //};

            CosmosTimeout = settings.Timeout;
            CosmosMaxRetries = settings.Retries;
        }

        public int DefaultPageSize { get; set; } = 100;
        public int MaxPageSize { get; set; } = 1000;
        public int CosmosTimeout { get; set; } = 60;
        public int CosmosMaxRetries { get; set; } = 10;
        public abstract string CollectionName { get; }
        public abstract string ColumnIdValue { get; }
        public abstract string PartitionKeyValue { get; }

        public string DatabaseName => this.settings.DatabaseName;
        public string Id => $"{DatabaseName}:{CollectionName}";

        // NOTE: CosmosDB library currently wraps the Newtonsoft JSON serializer.  Align Attributes and Converters to Newtonsoft on the domain models.
        private CosmosClient Client => client ??= new CosmosClientBuilder(this.settings.Uri, this.settings.Key)
                                                    .WithRequestTimeout(TimeSpan.FromSeconds(CosmosTimeout))
                                                    .WithThrottlingRetryOptions(TimeSpan.FromSeconds(CosmosTimeout), CosmosMaxRetries)
                                                    .WithSerializerOptions(new CosmosSerializationOptions
                                                    {
                                                        PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase,
                                                        Indented = false,
                                                        IgnoreNullValues = true,
                                                    })
                                                    .Build();
        private Container Container
        {
            get
            {
                lock (lockObj)
                {
                    return this.container ??= GetContainer(Client);
                }
            }
        }

        public abstract string GenerateId(TEntity entity);

        public virtual PartitionKey ResolvePartitionKey(TEntity entity)
        {
            try
            {
                var value = new PartitionKey(this.partitionKeyPI.GetValue(entity).ToString());
                return value;
            }
            catch (Exception ex)
            {
                ex.Data["partitionKeyPath"] = this.containerProperties.PartitionKeyPath;
                ex.Data["entityType"] = typeof(TEntity);
                throw;
            }
        }

        /// <summary>
        /// Given a document id and its partition value, retrieve the document, if it exists.
        /// </summary>
        /// <param name="id">ObjectId of the document.</param>
        /// <param name="partitionKey">Value of the partitionkey for the document.</param>
        /// <returns>An instance of the document or null.</returns>
        public async Task<TEntity> GetByIdAsync(string id, string partitionKey)
        {
            var result = await GetByIdWithMetaAsync(id, partitionKey).ConfigureAwait(false);
            return result?.Resource;
        }

        public async Task<ItemResponse<TEntity>> GetByIdWithMetaAsync(string id, string partitionKey)
        {
            return await this.Container.ReadItemAsync<TEntity>(id, new PartitionKey(partitionKey)).ConfigureAwait(false);
        }

        public async Task<TEntity> ReplaceDocumentAsync(string id, TEntity newDocument, ItemRequestOptions reqOptions)
        {
            return await this.Container.ReplaceItemAsync<TEntity>(newDocument, id, ResolvePartitionKey(newDocument), reqOptions).ConfigureAwait(false);
        }

        public async Task<TEntity> CreateDocumentAsync(TEntity newDocument)
        {
            return await this.Container.CreateItemAsync<TEntity>(newDocument, ResolvePartitionKey(newDocument)).ConfigureAwait(false);
        }

        public async Task<TEntity> UpsertDocumentAsync(TEntity newDocument)
        {
            return await this.Container.UpsertItemAsync<TEntity>(newDocument, ResolvePartitionKey(newDocument)).ConfigureAwait(false);
        }

        public async Task<TEntity> DeleteDocumentAsync(string id, string partitionKey)
        {
            return await this.Container.DeleteItemAsync<TEntity>(id, new PartitionKey(partitionKey)).ConfigureAwait(false);
        }

        public virtual async Task<bool> Test()
        {
            if (string.IsNullOrEmpty(this.CollectionName))
            {
                throw new ArgumentException($"CosmosCollection cannot be null");
            }

            //// open and test a new client / container
            //try
            //{
            //    await this.GetByIdAsync(this.ColumnIdValue, this.PartitionKeyValue).ConfigureAwait(false);
            //    return true;
            //}
            //catch (Exception ex)
            //{
            //    return false;
            //}

            // open and test a new client / container
            try
            {
                var containers = await GetContainerNames().ConfigureAwait(false);
                var containerNames = string.Join(',', containers);
                if (containers.Any(x => x == this.CollectionName) == false)
                {
                    throw new ApplicationException();  // use same error path
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Get a proxy to the container.
        /// </summary>
        /// <param name="client">An instance of <see cref="CosmosClient"/></param>
        /// <returns>An instance of <see cref="Container"/>.</returns>
        internal Container GetContainer(CosmosClient client)
        {
            try
            {
                var container = client.GetContainer(this.settings.DatabaseName, this.CollectionName);

                this.containerProperties = GetContainerProperties(container).Result;
                var partitionKeyName = this.containerProperties.PartitionKeyPath.TrimStart('/');
                this.partitionKeyPI = typeof(TEntity).GetProperty(partitionKeyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (this.partitionKeyPI is null)
                {
                    throw new ApplicationException($"Failed to find partition key property {partitionKeyName} on {typeof(TEntity).Name}.  Collection definition does not match Entity definition");
                }

                return container;
            }
            catch (Exception ex)
            {
                var message = $"Failed to connect to CosmosDB {this.settings.DatabaseName}:{this.CollectionName}";

                throw new ApplicationException(message, ex);
            }
        }

        /// <summary>
        /// Get the properties for the container.
        /// </summary>
        /// <param name="container">Instance of a container or null.</param>
        /// <returns>An instance of <see cref="ContainerProperties"/> or null.</returns>
        protected async Task<ContainerProperties> GetContainerProperties(Container container = null)
        {
            return (await (container ?? Container).ReadContainerAsync().ConfigureAwait(false)).Resource;
        }

        /// <summary>
        /// Query the database for all the containers defined and return a list of the container names.
        /// </summary>
        /// <returns>A list of container names present in the configured database.</returns>
        private async Task<IList<string>> GetContainerNames()
        {
            var containerNames = new List<string>();
            var database = this.Client.GetDatabase(this.settings.DatabaseName);
            using var iter = database.GetContainerQueryIterator<ContainerProperties>();
            while (iter.HasMoreResults)
            {
                var response = await iter.ReadNextAsync().ConfigureAwait(false);

                containerNames.AddRange(response.Select(c => c.Id));
            }

            return containerNames;
        }
    }
}

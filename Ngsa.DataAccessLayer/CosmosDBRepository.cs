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
    /// <summary>
    ///   CosmosDBRepository.
    /// </summary>
    public sealed class CosmosDBRepository : ICosmosDBRepository, IDisposable
    {
        private static CosmosClient client;
        private readonly ICosmosDBSettings settings;

        private readonly CosmosConfig options;
        private readonly object lockObj = new ();
        private Container container;
        private ContainerProperties containerProperties;
        private PropertyInfo partitionKeyPI;

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDBRepository"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <exception cref="System.ApplicationException">Repository test for {this.Id} failed.</exception>
        public CosmosDBRepository(ICosmosDBSettings settings)
        {
            this.settings = settings;

            this.options = new CosmosConfig
            {
                MaxRows = this.MaxPageSize,
                Timeout = this.settings.Timeout,
                Retries = this.settings.Retries,
            };

            if (!this.Test().Result)
            {
                throw new ApplicationException($"Repository test for {this.Id} failed.");
            }
        }

        /// <summary>
        /// Gets or sets the default size of the page.
        /// </summary>
        /// <value>
        /// The default size of the page.
        /// </value>
        public int DefaultPageSize { get; set; } = 100;

        /// <summary>
        /// Gets or sets the maximum size of the page.
        /// </summary>
        /// <value>
        /// The maximum size of the page.
        /// </value>
        public int MaxPageSize { get; set; } = 1000;

        /// <summary>
        /// Gets the name of the collection.
        /// </summary>
        /// <value>
        /// The name of the collection.
        /// </value>
        public string CollectionName => this.settings.CollectionName;

        /// <summary>
        /// Gets the name of the database.
        /// </summary>
        /// <value>
        /// The name of the database.
        /// </value>
        public string DatabaseName => this.settings.DatabaseName;

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public string Id => $"{this.DatabaseName}:{this.CollectionName}";

        // NOTE: CosmosDB library currently wraps the Newtonsoft JSON serializer.  Align Attributes and Converters to Newtonsoft on the domain models.

        /// <summary>
        /// Gets the client.
        /// </summary>
        /// <value>
        /// The client.
        /// </value>
        private CosmosClient Client => client ??= new CosmosClientBuilder(this.settings.Uri, this.settings.Key)
                                                    .WithRequestTimeout(TimeSpan.FromSeconds(this.settings.Timeout))
                                                    .WithThrottlingRetryOptions(TimeSpan.FromSeconds(this.settings.Timeout), this.settings.Retries)
                                                    .WithSerializerOptions(new CosmosSerializationOptions
                                                    {
                                                        PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase,
                                                        Indented = false,
                                                        IgnoreNullValues = true,
                                                    })
                                                    .Build();

        /// <summary>
        /// Resolves the partition key.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity.</param>
        /// <returns>The Partition key.</returns>
        public PartitionKey ResolvePartitionKey<TEntity>(TEntity entity)
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
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="id">ObjectId of the document.</param>
        /// <param name="partitionKey">Value of the partitionkey for the document.</param>
        /// <returns>An instance of the document or null.</returns>
        public async Task<TEntity> GetByIdAsync<TEntity>(string id, string partitionKey)
        {
            var result = await this.GetByIdWithMetaAsync<TEntity>(id, partitionKey).ConfigureAwait(false);
            return result.Resource;
        }

        /// <summary>
        /// Gets the by identifier with meta asynchronous.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="id">The identifier.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <returns>An instance of the document or null.</returns>
        public async Task<ItemResponse<TEntity>> GetByIdWithMetaAsync<TEntity>(string id, string partitionKey)
        {
            return await this.Container<TEntity>().ReadItemAsync<TEntity>(id, new PartitionKey(partitionKey)).ConfigureAwait(false);
        }

        /// <summary>
        /// Replaces the document asynchronous.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="id">The identifier.</param>
        /// <param name="newDocument">The new document.</param>
        /// <param name="reqOptions">The req options.</param>
        /// <returns>An instance of the document or null.</returns>
        public async Task<TEntity> ReplaceDocumentAsync<TEntity>(string id, TEntity newDocument, ItemRequestOptions reqOptions)
        {
            return await this.Container<TEntity>().ReplaceItemAsync(newDocument, id, this.ResolvePartitionKey(newDocument), reqOptions).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates the document asynchronous.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="newDocument">The new document.</param>
        /// <returns>An instance of the document or null.</returns>
        public async Task<TEntity> CreateDocumentAsync<TEntity>(TEntity newDocument)
        {
            return await this.Container<TEntity>().CreateItemAsync(newDocument, this.ResolvePartitionKey(newDocument)).ConfigureAwait(false);
        }

        /// <summary>
        /// Upserts the document asynchronous.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="newDocument">The new document.</param>
        /// <returns>An instance of the document.</returns>
        public async Task<TEntity> UpsertDocumentAsync<TEntity>(TEntity newDocument)
        {
            return await this.Container<TEntity>().UpsertItemAsync(newDocument, this.ResolvePartitionKey(newDocument)).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes the document asynchronous.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="id">The identifier.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <returns>An instance of the document or null.</returns>
        public async Task<TEntity> DeleteDocumentAsync<TEntity>(string id, string partitionKey)
        {
            return await this.Container<TEntity>().DeleteItemAsync<TEntity>(id, new PartitionKey(partitionKey)).ConfigureAwait(false);
        }

        /// <summary>
        /// Tests this instance.
        /// </summary>
        /// <returns>
        /// true if passed , otherwise false.
        /// </returns>
        public async Task<bool> Test()
        {
            if (string.IsNullOrEmpty(this.CollectionName))
            {
                throw new ArgumentException($"CosmosCollection cannot be null");
            }

            // open and test a new client / container
            try
            {
                var containers = await this.GetContainerNames().ConfigureAwait(false);
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

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Generic function to be used by subclasses to execute arbitrary queries and return type T.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="sql">Query to be executed.</param>
        /// <param name="options">Query options.</param>
        /// <returns>Enumerable list of objects of type T.</returns>
        public async Task<IEnumerable<TEntity>> InternalCosmosDBSqlQuery<TEntity>(string sql, QueryRequestOptions options = null)
        {
            // run query
            var query = this.Container<TEntity>().GetItemQueryIterator<TEntity>(sql, requestOptions: options ?? this.options.QueryRequestOptions);

            var results = new List<TEntity>();

            while (query.HasMoreResults)
            {
                foreach (var doc in await query.ReadNextAsync().ConfigureAwait(false))
                {
                    results.Add(doc);
                }
            }

            return results;
        }

        /// <summary>
        /// Generic function to be used by subclasses to execute arbitrary queries and return type T.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="queryDefinition">Query to be executed.</param>
        /// <returns>Enumerable list of objects of type T.</returns>
        public async Task<IEnumerable<TEntity>> InternalCosmosDBSqlQuery<TEntity>(QueryDefinition queryDefinition)
        {
            // run query
            var query = this.Container<TEntity>().GetItemQueryIterator<TEntity>(queryDefinition, requestOptions: this.options.QueryRequestOptions);

            var results = new List<TEntity>();

            while (query.HasMoreResults)
            {
                foreach (var doc in await query.ReadNextAsync().ConfigureAwait(false))
                {
                    results.Add(doc);
                }
            }

            return results;
        }

        /// <summary>
        /// Get a proxy to the container.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="client">An instance of <see cref="CosmosClient"/>.</param>
        /// <param name="collectionName">The Collection name.</param>
        /// <returns>An instance of <see cref="Container"/>.</returns>
        internal Container GetContainer<TEntity>(CosmosClient client, string collectionName = null)
        {
            try
            {
                var container = client.GetContainer(this.settings.DatabaseName, collectionName ?? this.CollectionName);

                this.containerProperties = this.GetContainerProperties<TEntity>(container).Result;
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
        /// Containers this instance.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <returns>The Container.</returns>
        private Container Container<TEntity>()
        {
            lock (this.lockObj)
            {
                return this.container ??= this.GetContainer<TEntity>(this.Client);
            }
        }

        /// <summary>
        /// Get the properties for the container.
        /// </summary>
        /// <param name="container">Instance of a container or null.</param>
        /// <returns>An instance of <see cref="ContainerProperties"/> or null.</returns>
        private async Task<ContainerProperties> GetContainerProperties<TEntity>(Container container = null)
        {
            return (await (container ?? this.Container<TEntity>()).ReadContainerAsync().ConfigureAwait(false)).Resource;
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

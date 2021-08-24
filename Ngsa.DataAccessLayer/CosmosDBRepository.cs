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
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public abstract class CosmosDBRepository<TEntity> : ICosmosDBRepository<TEntity>, IDisposable
                                                where TEntity : class
    {
        private static CosmosClient client;
        private readonly ICosmosDBSettings settings;

        // private readonly CosmosConfig options; this will be use is InternalCosmosDBSqlQuery is implemented
        private readonly object lockObj = new ();
        private Container container;
        private ContainerProperties containerProperties;
        private PropertyInfo partitionKeyPI;

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDBRepository{TEntity}" /> class.
        /// Data Access Layer Constructor.
        /// </summary>
        /// <param name="settings">Instance of settings for a ComosDB.</param>
        protected CosmosDBRepository(ICosmosDBSettings settings)
        {
            this.settings = settings;

            // this.options = new CosmosConfig
            // {
            //    MaxRows = MaxPageSize,
            //    Timeout = CosmosTimeout,
            //    Retries = settings.Retries,
            //    Timeout = settings.Timeout,
            // };
            this.CosmosTimeout = settings.Timeout;
            this.CosmosMaxRetries = settings.Retries;
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
        /// Gets or sets the cosmos timeout.
        /// </summary>
        /// <value>
        /// The cosmos timeout.
        /// </value>
        public int CosmosTimeout { get; set; } = 60;

        /// <summary>
        /// Gets or sets the cosmos maximum retries.
        /// </summary>
        /// <value>
        /// The cosmos maximum retries.
        /// </value>
        public int CosmosMaxRetries { get; set; } = 10;

        /// <summary>
        /// Gets the name of the collection.
        /// </summary>
        /// <value>
        /// The name of the collection.
        /// </value>
        public abstract string CollectionName { get; }

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
        private CosmosClient Client => client ??= new CosmosClientBuilder(this.settings.Uri, this.settings.Key)
                                                    .WithRequestTimeout(TimeSpan.FromSeconds(this.CosmosTimeout))
                                                    .WithThrottlingRetryOptions(TimeSpan.FromSeconds(this.CosmosTimeout), this.CosmosMaxRetries)
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
                lock (this.lockObj)
                {
                    return this.container ??= this.GetContainer(this.Client);
                }
            }
        }

        /// <summary>
        /// Generates the identifier.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>the Id.</returns>
        public abstract string GenerateId(TEntity entity);

        /// <summary>
        /// Resolves the partition key.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The Partition key.</returns>
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
            var result = await this.GetByIdWithMetaAsync(id, partitionKey).ConfigureAwait(false);
            return result?.Resource;
        }

        /// <summary>
        /// Gets the by identifier with meta asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <returns>An instance of the document or null.</returns>
        public async Task<ItemResponse<TEntity>> GetByIdWithMetaAsync(string id, string partitionKey)
        {
            return await this.Container.ReadItemAsync<TEntity>(id, new PartitionKey(partitionKey)).ConfigureAwait(false);
        }

        /// <summary>
        /// Replaces the document asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="newDocument">The new document.</param>
        /// <param name="reqOptions">The req options.</param>
        /// <returns>An instance of the document or null.</returns>
        public async Task<TEntity> ReplaceDocumentAsync(string id, TEntity newDocument, ItemRequestOptions reqOptions)
        {
            return await this.Container.ReplaceItemAsync<TEntity>(newDocument, id, this.ResolvePartitionKey(newDocument), reqOptions).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates the document asynchronous.
        /// </summary>
        /// <param name="newDocument">The new document.</param>
        /// <returns>An instance of the document or null.</returns>
        public async Task<TEntity> CreateDocumentAsync(TEntity newDocument)
        {
            return await this.Container.CreateItemAsync<TEntity>(newDocument, this.ResolvePartitionKey(newDocument)).ConfigureAwait(false);
        }

        /// <summary>
        /// Upserts the document asynchronous.
        /// </summary>
        /// <param name="newDocument">The new document.</param>
        /// <returns>An instance of the document.</returns>
        public async Task<TEntity> UpsertDocumentAsync(TEntity newDocument)
        {
            return await this.Container.UpsertItemAsync<TEntity>(newDocument, this.ResolvePartitionKey(newDocument)).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes the document asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <returns>An instance of the document or null.</returns>
        public async Task<TEntity> DeleteDocumentAsync(string id, string partitionKey)
        {
            return await this.Container.DeleteItemAsync<TEntity>(id, new PartitionKey(partitionKey)).ConfigureAwait(false);
        }

        /// <summary>
        /// Tests this instance.
        /// </summary>
        /// <returns>
        /// true if passed , otherwise false.
        /// </returns>
        public virtual async Task<bool> Test()
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
        /// Get a proxy to the container.
        /// </summary>
        /// <param name="client">An instance of <see cref="CosmosClient"/>.</param>
        /// <returns>An instance of <see cref="Container"/>.</returns>
        internal Container GetContainer(CosmosClient client)
        {
            try
            {
                var container = client.GetContainer(this.settings.DatabaseName, this.CollectionName);

                this.containerProperties = this.GetContainerProperties(container).Result;
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
            return (await (container ?? this.Container).ReadContainerAsync().ConfigureAwait(false)).Resource;
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

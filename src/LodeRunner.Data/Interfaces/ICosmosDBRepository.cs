// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace LodeRunner.Data.Interfaces
{
    /// <summary>
    /// CosmosDB Repository Interface.
    /// </summary>
    /// <seealso cref="LodeRunner.Data.Interfaces.IRepository" />
    public interface ICosmosDBRepository : IRepository
    {
        /// <summary>
        /// Gets the name of the database.
        /// </summary>
        /// <value>
        /// The name of the database.
        /// </value>
        string DatabaseName { get; }

        /// <summary>
        /// Gets the name of the collection.
        /// </summary>
        /// <value>
        /// The name of the collection.
        /// </value>
        string CollectionName { get; }

        /// <summary>
        /// Gets a value indicating whether CosmosDB is ready or not.
        /// This caches the ready state from the initial check so it isn't checked every time.
        /// </summary>
        /// <returns>Returns true if ready, otherwise false.</returns>
        bool IsCosmosDBReady { get; }

        /// <summary>
        /// Gets the by identifier asynchronous.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="id">The identifier.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <returns>An instance of the document or null.</returns>
        Task<TEntity> GetByIdAsync<TEntity>(string id, string partitionKey);

        /// <summary>
        /// Gets the by identifier with meta asynchronous.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="id">The identifier.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <returns>An instance of the document or null.</returns>
        Task<ItemResponse<TEntity>> GetByIdWithMetaAsync<TEntity>(string id, string partitionKey);

        /// <summary>
        /// Replaces the document asynchronous.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="id">The identifier.</param>
        /// <param name="newDocument">The new document.</param>
        /// <param name="reqOptions">The req options.</param>
        /// <returns>An instance of the document or null.</returns>
        Task<TEntity> ReplaceDocumentAsync<TEntity>(string id, TEntity newDocument, ItemRequestOptions reqOptions = null);

        /// <summary>
        /// Creates the document asynchronous.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="newDocument">The new document.</param>
        /// <returns>An instance of the document or null.</returns>
        Task<TEntity> CreateDocumentAsync<TEntity>(TEntity newDocument);

        /// <summary>
        /// Upserts the document asynchronous.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="newDocument">The new document.</param>
        /// <param name="cancellationToken">The Cancellation Token.</param>
        /// <param name="requestOptions">The request Options associated with the resource.</param>
        /// <returns>An instance of the document or null.</returns>
        Task<TEntity> UpsertDocumentAsync<TEntity>(TEntity newDocument, CancellationToken cancellationToken = default, ItemRequestOptions requestOptions = null);

        /// <summary>
        /// Deletes the document asynchronous.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="id">The identifier.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <returns>An instance of the document or null.</returns>
        Task<ItemResponse<TEntity>> DeleteDocumentAsync<TEntity>(string id, string partitionKey);

        /// <summary>
        /// Internals the cosmos database SQL query.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="sql">The SQL.</param>
        /// <param name="options">The options.</param>
        /// <returns>The query result.</returns>
        Task<IEnumerable<TEntity>> InternalCosmosDBSqlQuery<TEntity>(string sql, QueryRequestOptions options = null);

        /// <summary>
        /// Internals the cosmos database SQL query.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="queryDefinition">The query definition.</param>
        /// <returns>The query result.</returns>
        Task<IEnumerable<TEntity>> InternalCosmosDBSqlQuery<TEntity>(QueryDefinition queryDefinition);

        /// <summary>
        /// Internals the cosmos database SQL query scalar.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="sql">The SQL.</param>
        /// <param name="defaultValue">The return default value.</param>
        /// <param name="options">The options.</param>
        /// <returns>The Scalar query TResult result.</returns>
        Task<TResult> InternalCosmosDBSqlQueryScalar<TEntity, TResult>(string sql, TResult defaultValue, QueryRequestOptions options = null);

        /// <summary>
        /// Determines whether [is cosmos database ready].
        /// </summary>
        /// <returns>True is Cosmos DB has not exceeded the number of request units per second, otherwise false.</returns>
        Task<bool> CosmosDBReadyCheck();
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace Ngsa.LodeRunner.DataAccessLayer.Interfaces
{
    /// <summary>
    /// CosmosDB Repository Interface.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <seealso cref="Ngsa.LodeRunner.DataAccessLayer.Interfaces.IRepository" />
    public interface ICosmosDBRepository<TEntity> : IRepository
        where TEntity : class
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
        /// Gets the by identifier asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <returns>An instance of the document or null.</returns>
        Task<TEntity> GetByIdAsync(string id, string partitionKey);

        /// <summary>
        /// Gets the by identifier with meta asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <returns>An instance of the document or null.</returns>
        Task<ItemResponse<TEntity>> GetByIdWithMetaAsync(string id, string partitionKey);

        /// <summary>
        /// Generates the identifier.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The Id.</returns>
        string GenerateId(TEntity entity);

        /// <summary>
        /// Replaces the document asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="newDocument">The new document.</param>
        /// <param name="reqOptions">The req options.</param>
        /// <returns>An instance of the document or null.</returns>
        Task<TEntity> ReplaceDocumentAsync(string id, TEntity newDocument, ItemRequestOptions reqOptions = null);

        /// <summary>
        /// Creates the document asynchronous.
        /// </summary>
        /// <param name="newDocument">The new document.</param>
        /// <returns>An instance of the document or null.</returns>
        Task<TEntity> CreateDocumentAsync(TEntity newDocument);

        /// <summary>
        /// Upserts the document asynchronous.
        /// </summary>
        /// <param name="newDocument">The new document.</param>
        /// <returns>An instance of the document or null.</returns>
        Task<TEntity> UpsertDocumentAsync(TEntity newDocument);

        /// <summary>
        /// Deletes the document asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <returns>An instance of the document or null.</returns>
        Task<TEntity> DeleteDocumentAsync(string id, string partitionKey);
    }
}

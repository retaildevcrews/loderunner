﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using LodeRunner.Core.Extensions;
using LodeRunner.Core.Interfaces;
using LodeRunner.Core.Models;
using LodeRunner.Core.Responses;
using LodeRunner.Data.Interfaces;

namespace LodeRunner.Services
{
    /// <summary>
    ///   Client Status Service.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public abstract class BaseService<TEntity> : IBaseService<TEntity>
        where TEntity : class
    {
        private readonly EntityType entityType = EntityType.Unassigned;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseService{TEntity}"/> class.
        /// </summary>
        /// <param name="cosmosDBRepository">The cosmos database repository.</param>
        public BaseService(ICosmosDBRepository cosmosDBRepository)
        {
            this.CosmosDBRepository = cosmosDBRepository;
            this.entityType = typeof(TEntity).Name.As<EntityType>();
        }

        /// <summary>
        /// Gets or sets the validator.
        /// </summary>
        /// <value>
        /// The validator.
        /// </value>
        public IModelValidator<TEntity> Validator { get; protected set; }

        /// <summary>
        /// Gets the cosmos database repository.
        /// </summary>
        /// <value>
        /// The cosmos database repository.
        /// </value>
        protected ICosmosDBRepository CosmosDBRepository { get; private set; }

        /// <summary>
        /// Gets the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>The corresponding entity.</returns>
        public virtual async Task<TEntity> Get(string id)
        {
            return await this.CosmosDBRepository.GetByIdAsync<TEntity>(id, this.entityType.ToString());
        }

        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns>all items for a given type.</returns>
        public virtual async Task<IEnumerable<TEntity>> GetAll()
        {
            string sql = $"SELECT * from e WHERE e.entityType='{this.entityType}' ORDER BY e._ts DESC";

            return await this.CosmosDBRepository.InternalCosmosDBSqlQuery<TEntity>(sql).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the most recent asynchronous.
        /// </summary>
        /// <param name="limit">The limit.</param>
        /// <returns>
        /// all items for a given type.
        /// </returns>
        public virtual async Task<IEnumerable<TEntity>> GetMostRecent(int limit = 1)
        {
            string sql = $"SELECT * FROM e WHERE e.entityType='{this.entityType}' ORDER BY e._ts DESC OFFSET 0 LIMIT {limit}";

            return await this.CosmosDBRepository.InternalCosmosDBSqlQuery<TEntity>(sql).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the count asynchronous.
        /// </summary>
        /// <returns>
        /// Item count that match the Entity type.
        /// </returns>
        public virtual async Task<int> GetCount()
        {
            string sql = $"SELECT VALUE COUNT(1) FROM e where e.entityType='{this.entityType}'";

            int defaultValue = 0;
            return await this.CosmosDBRepository.InternalCosmosDBSqlQueryScalar<TEntity, int>(sql, defaultValue).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>The corresponding entity.</returns>
        public virtual async Task<TEntity> Delete(string id)
        {
            return await this.CosmosDBRepository.DeleteDocumentAsync<TEntity>(id, this.entityType.ToString());
        }

        /// <summary>
        /// Posts the specified entity.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The Created or Updated Entity.</returns>
        public async Task<TEntity> Save(TEntity model, CancellationToken cancellationToken)
        {
           // var returnValue = new Task<TEntity>(() => null);
            if (model != null && !cancellationToken.IsCancellationRequested)
            {
                // Update Entity if CosmosDB connection is ready and the object is valid
                if (this.CosmosDBRepository.IsCosmosDBReady().Result && this.Validator.ValidateEntity(model))
                {
                    return await this.CosmosDBRepository.UpsertDocumentAsync(model, cancellationToken);
                }
                else
                {
                    // TODO: log specific case scenario, even if IsCosmosDBReady() already will do its own logging.

                    // TODO: log validation errors is any  if not  this.validator.IsValid => this.validator.ErrorMessage
                }
            }

            return null;
        }

        /// <summary>
        /// Creates the API response.
        /// </summary>
        /// <param name="returnValue">The return value.</param>
        /// <returns>Api Response.</returns>
        protected ApiResponse<TEntity> CreateApiResponse(TEntity returnValue)
        {
            ApiResponse<TEntity> result = new ();

            if (!this.Validator.IsValid)
            {
                result.Errors = this.Validator.ErrorMessage;
                result.StatusCode = HttpStatusCode.BadRequest;
            }
            else if (returnValue != null)
            {
                result.Model = returnValue;
                result.StatusCode = HttpStatusCode.OK;
            }

            return result;
        }
    }
}

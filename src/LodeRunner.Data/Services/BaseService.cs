// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using LodeRunner.Core.Extensions;
using LodeRunner.Core.Models;
using LodeRunner.Data.Interfaces;

namespace LodeRunner.Services
{
    /// <summary>
    ///   Client Status Service.
    /// </summary>
    public abstract class BaseService : IBaseService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseService"/> class.
        /// </summary>
        /// <param name="cosmosDBRepository">The cosmos database repository.</param>
        public BaseService(ICosmosDBRepository cosmosDBRepository)
        {
            this.CosmosDBRepository = cosmosDBRepository;
        }

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
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="id">The identifier.</param>
        /// <returns>The corresponding entity.</returns>
        public virtual async Task<TEntity> Get<TEntity>(string id)
        {
            EntityType entityType = typeof(TEntity).Name.As<EntityType>();

            return await this.CosmosDBRepository.GetByIdAsync<TEntity>(id, entityType.ToString());
        }

        /// <summary>
        /// Gets all.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <returns>all items for a given type.</returns>
        public virtual async Task<IEnumerable<TEntity>> GetAll<TEntity>()
        {
            EntityType entityType = typeof(TEntity).Name.As<EntityType>();

            string sql = $"SELECT * from e WHERE e.entityType='{entityType}' ORDER BY e._ts DESC";

            return await this.CosmosDBRepository.InternalCosmosDBSqlQuery<TEntity>(sql).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the most recent asynchronous.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="limit">The limit.</param>
        /// <returns>
        /// all items for a given type.
        /// </returns>
        public virtual async Task<IEnumerable<TEntity>> GetMostRecentAsync<TEntity>(int limit = 1)
        {
            EntityType entityType = typeof(TEntity).Name.As<EntityType>();

            string sql = $"SELECT * FROM e WHERE e.entityType='{entityType}' ORDER BY e._ts DESC OFFSET 0 LIMIT {limit}";

            return await this.CosmosDBRepository.InternalCosmosDBSqlQuery<TEntity>(sql).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the count asynchronous.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <returns>
        /// Item count that match the Entity type.
        /// </returns>
        public virtual async Task<int> GetCountAsync<TEntity>()
        {
            EntityType entityType = typeof(TEntity).Name.As<EntityType>();

            string sql = $"SELECT VALUE COUNT(1) FROM e where e.entityType='{entityType}'";

            int defaultValue = 0;
            return await this.CosmosDBRepository.InternalCosmosDBSqlQueryScalar<TEntity, int>(sql, defaultValue).ConfigureAwait(false);
        }
    }
}
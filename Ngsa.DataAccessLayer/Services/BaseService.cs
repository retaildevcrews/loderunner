// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ngsa.DataAccessLayer.Extensions;
using Ngsa.DataAccessLayer.Model.Validators;
using Ngsa.LodeRunner.DataAccessLayer.Interfaces;
using Ngsa.LodeRunner.DataAccessLayer.Model;

namespace Ngsa.LodeRunner.Services
{
    /// <summary>
    ///   Client Status Service.
    /// </summary>
    public partial class BaseService : IBaseService
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

            string sql = $"select * from m where m.entityType='{entityType}'";

            var getAllClientStatusTask = Task.Run(() => this.CosmosDBRepository.InternalCosmosDBSqlQuery<TEntity>(sql).Result);

            getAllClientStatusTask.Wait();

            return await getAllClientStatusTask;
        }
    }

    /// <summary>
    /// Implements the BaseService class, fix StyleCopAnalyzer violation #SA1201.
    /// </summary>
    public partial class BaseService
    {
        /// <summary>
        /// Gets the cosmos database repository.
        /// </summary>
        /// <value>
        /// The cosmos database repository.
        /// </value>
        protected ICosmosDBRepository CosmosDBRepository { get; private set; }
    }
}
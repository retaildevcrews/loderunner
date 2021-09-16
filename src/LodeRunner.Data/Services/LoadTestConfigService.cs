// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using LodeRunner.Core.Models;
using LodeRunner.Data.Interfaces;

namespace LodeRunner.Services
{
    /// <summary>
    ///   Client Status Service.
    /// </summary>
    public class LoadTestConfigService : BaseService, ILoadTestConfigService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadTestConfigService"/> class.
        /// </summary>
        /// <param name="cosmosDBRepository">The client status repository.</param>
        public LoadTestConfigService(ICosmosDBRepository cosmosDBRepository)
            : base(cosmosDBRepository)
        {
        }

        /// <summary>
        /// Gets the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>
        /// The corresponding Entity.
        /// </returns>
        public async Task<LoadTestConfig> Get(string id)
        {
            return await this.Get<LoadTestConfig>(id);
        }

        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns>
        /// all items for a given type.
        /// </returns>
        public async Task<IEnumerable<LoadTestConfig>> GetAll()
        {
            return await this.GetAll<LoadTestConfig>();
        }
    }
}

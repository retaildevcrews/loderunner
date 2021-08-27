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

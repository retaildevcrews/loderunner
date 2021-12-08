// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using LodeRunner.Core.Interfaces;
using LodeRunner.Core.Models;
using LodeRunner.Core.Models.Validators;
using LodeRunner.Data.Interfaces;
using Microsoft.Azure.Cosmos;

namespace LodeRunner.Services
{
    /// <summary>
    ///   Client Status Service.
    /// </summary>
    public class LoadTestConfigService : BaseService, ILoadTestConfigService
    {
        private readonly IModelValidator<LoadTestConfig> validator;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadTestConfigService"/> class.
        /// </summary>
        /// <param name="cosmosDBRepository">The client status repository.</param>
        public LoadTestConfigService(ICosmosDBRepository cosmosDBRepository)
            : base(cosmosDBRepository)
        {
            this.validator = new LoadTestConfigValidator();
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

        /// <summary>
        /// Posts the specified load test configuration.
        /// </summary>
        /// <param name="loadTestConfig">The load test configuration.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The Inserted LoadTestConfig entity.
        /// </returns>
        public Task<LoadTestConfig> Post(LoadTestConfig loadTestConfig, CancellationToken cancellationToken)
        {
            var returnValue = new Task<LoadTestConfig>(() => null);

            if (loadTestConfig != null && !cancellationToken.IsCancellationRequested)
            {
                // Update Entity if CosmosDB connection is ready and the object is valid
                if (this.CosmosDBRepository.IsCosmosDBReady().Result && this.validator.ValidateEntity(loadTestConfig))
                {
                    returnValue = this.CosmosDBRepository.UpsertDocumentAsync(loadTestConfig, cancellationToken);
                }
                else
                {
                    // TODO: log specific case scenario, even if IsCosmosDBReady() already will do its own logging.

                    // TODO: log validation errors is any  if not  this.validator.IsValid => this.validator.ErrorMessage
                }
            }

            return returnValue;
        }

        /// <summary>
        /// Gets the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>
        /// The corresponding Entity.
        /// </returns>
        public async Task<HttpStatusCode> Delete(string id)
        {
            try
            {
                var loadTestConfig = await this.Delete<LoadTestConfig>(id);
                return HttpStatusCode.OK;
            }
            catch (CosmosException ce)
            {
                if (ce.StatusCode == HttpStatusCode.NotFound)
                {
                    return HttpStatusCode.NotFound;
                }
                else
                {
                    return HttpStatusCode.InternalServerError;
                }
            }
            catch (Exception)
            {
                return HttpStatusCode.InternalServerError;
            }
        }
    }
}

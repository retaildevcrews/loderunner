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
    ///   Test Run Service.
    /// </summary>
    public class TestRunService : BaseService, ITestRunService
    {
        private readonly IModelValidator<TestRun> validator;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestRunService"/> class.
        /// </summary>
        /// <param name="cosmosDBRepository">The loderunner repository.</param>
        public TestRunService(ICosmosDBRepository cosmosDBRepository)
            : base(cosmosDBRepository)
        {
            this.validator = new TestRunValidator();
        }

        /// <summary>
        /// Gets the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>
        /// The corresponding Entity.
        /// </returns>
        public async Task<TestRun> Get(string id)
        {
            return await this.Get<TestRun>(id);
        }

        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns>
        /// all items for a given type.
        /// </returns>
        public async Task<IEnumerable<TestRun>> GetAll()
        {
            return await this.GetAll<TestRun>();
        }

        /// <summary>
        /// Posts the specified load test run.
        /// </summary>
        /// <param name="testRun">The load test run.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The Inserted TestRun entity.
        /// </returns>
        public async Task<TestRun> Post(TestRun testRun, CancellationToken cancellationToken)
        {
            var returnValue = new Task<TestRun>(() => null);

            if (testRun != null && !cancellationToken.IsCancellationRequested)
            {
                // Update Entity if CosmosDB connection is ready and the object is valid
                if (this.CosmosDBRepository.IsCosmosDBReady().Result && this.validator.ValidateEntity(testRun))
                {
                    returnValue = this.CosmosDBRepository.UpsertDocumentAsync(testRun, cancellationToken);
                }
                else
                {
                    // TODO: log specific case scenario, even if IsCosmosDBReady() already will do its own logging.

                    // TODO: log validation errors is any  if not  this.validator.IsValid => this.validator.ErrorMessage
                }
            }

            return await returnValue;
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
                var loadTestConfig = await this.Delete<TestRun>(id);
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

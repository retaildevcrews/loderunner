// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using LodeRunner.Core;
using LodeRunner.Core.Models;
using LodeRunner.Core.Models.Validators;
using LodeRunner.Data.Interfaces;
using Microsoft.Azure.Cosmos;

namespace LodeRunner.Services
{
    /// <summary>
    ///   Test Run Service.
    /// </summary>
    public class TestRunService : BaseService<TestRun>, ITestRunService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestRunService"/> class.
        /// </summary>
        /// <param name="cosmosDBRepository">The loderunner repository.</param>
        public TestRunService(ICosmosDBRepository cosmosDBRepository)
            : base(cosmosDBRepository)
        {
            this.Validator = new TestRunValidator();
        }

         /// <summary>
        /// Gets the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>
        /// The corresponding Entity.
        /// </returns>
        public new async Task<HttpStatusCode> Delete(string id)
        {
            try
            {
                var loadTestConfig = await base.Delete(id);
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

        /// <summary>
        /// Gets all available TestRuns for the given client id.
        /// </summary>
        /// <param name="clientId">The client id.</param>
        /// <returns>
        /// List of TestRuns to run on client.
        /// </returns>
        public async Task<IEnumerable<TestRun>> GetNewTestRunsByClientId(string clientId)
        {
            string sql = $"SELECT * FROM e WHERE e.entityType='TestRun' ";
            sql += $"and array_contains(e.loadClients, {{ \"id\": \"{clientId}\"}}, true) ";

            // TODO: Update and add later once LoadResults schema is defined
            // sql + = $"and NOT array_contains(e.ClientResults, {{\"id\": \"{clientId}\"}}, true)";
            sql += $"and (NOT IS_DEFINED(e.completedTime) or IS_NULL(e.completedTime) or e.completedTime = '') ";
            sql += $"ORDER BY e.startTime ASC";

            return await this.CosmosDBRepository.InternalCosmosDBSqlQuery<TestRun>(sql).ConfigureAwait(false);
        }
    }
}

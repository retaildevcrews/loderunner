// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using LodeRunner.Core;
using LodeRunner.Core.Models;
using LodeRunner.Core.Models.Validators;
using LodeRunner.Core.Responses;
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
        /// Posts the specified load test run.
        /// </summary>
        /// <param name="testRun">The load test run.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The Inserted TestRun entity.
        /// </returns>
        public async Task<ApiResponse<TestRun>> Post(TestRun testRun, CancellationToken cancellationToken)
        {
            var returnValue = await this.Save(testRun, cancellationToken);

            ApiResponse<TestRun> result = new ();

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
    }
}

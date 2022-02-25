// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using LodeRunner.Core.Models;
using LodeRunner.Core.Models.Validators;
using LodeRunner.Data.Interfaces;

namespace LodeRunner.Services
{
    /// <summary>
    ///   Load Test Config Service.
    /// </summary>
    public class LoadTestConfigService : BaseService<LoadTestConfig>, ILoadTestConfigService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadTestConfigService"/> class.
        /// </summary>
        /// <param name="cosmosDBRepository">The client status repository.</param>
        public LoadTestConfigService(ICosmosDBRepository cosmosDBRepository)
            : base(cosmosDBRepository)
        {
            this.Validator = new LoadTestConfigValidator();
        }
    }
}

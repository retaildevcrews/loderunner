// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Ngsa.LodeRunner.DataAccessLayer.Interfaces;
using Ngsa.LodeRunner.DataAccessLayer.Model;

namespace Ngsa.LodeRunner.DataAccessLayer
{
    /// <summary>
    /// LoadClient Repository.
    /// </summary>
    public class LoadClientRepository : CosmosDBRepository<LoadClient>, ILoadClientRepository
    {
        // Note: Yes it uses same ClientStatusRepositorySettings.
        private readonly ClientStatusRepositorySettings settings;

        /// <summary>Initializes a new instance of the <see cref="LoadClientRepository" /> class.</summary>
        /// <param name="settings">The settings.</param>
        /// <exception cref="System.ApplicationException">Repository test for {this.Id} failed.</exception>
        public LoadClientRepository(ClientStatusRepositorySettings settings)
            : base(settings)
        {
            this.settings = settings;

            if (!this.Test().Result)
            {
                throw new ApplicationException($"Repository test for {this.Id} failed.");
            }
        }

        /// <inheritdoc/>
        public override string CollectionName => this.settings.CollectionName;

        /// <inheritdoc/>
        public override string GenerateId(LoadClient entity)
        {
            // TODO: Unique Id generated at start-up to differentiate clients located in the same Region and Zone
            if (string.IsNullOrWhiteSpace(entity.Id))
            {
                entity.Id = Guid.NewGuid().ToString();
            }

            return entity.Id;
        }
    }
}

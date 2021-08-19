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
    public class ClientStatusRepository : CosmosDBRepository<ClientStatus>, IClientStatusRepository
    {
        private const string DefaultColumnIdValue = "1";
        private const string DefaultPartitionKeyValue = "ClientStatus";

        private readonly ClientStatusRepositorySettings settings;

        public ClientStatusRepository(ClientStatusRepositorySettings settings)
            : base(settings)
        {
            this.settings = settings;

            if (!Test().Result)
            {
                throw new ApplicationException($"Repository test for {this.Id} failed.");
            }
        }

        public override string CollectionName => settings.CollectionName;
        public override string ColumnIdValue => DefaultColumnIdValue;
        public override string PartitionKeyValue => DefaultPartitionKeyValue;

        public override string GenerateId(ClientStatus entity)
        {
            if (string.IsNullOrWhiteSpace(entity.Id))
            {
                entity.Id = Guid.NewGuid().ToString();
            }

            return entity.Id;
        }
    }
}

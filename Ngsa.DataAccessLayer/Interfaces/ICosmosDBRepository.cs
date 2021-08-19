// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace Ngsa.LodeRunner.DataAccessLayer.Interfaces
{
    public interface ICosmosDBRepository<TEntity> : IRepository
        where TEntity : class
    {
        string DatabaseName { get; }
        string CollectionName { get; }
        Task<TEntity> GetByIdAsync(string id, string partitionKey);
        Task<ItemResponse<TEntity>> GetByIdWithMetaAsync(string id, string partitionKey);
        string GenerateId(TEntity entity);
        Task<TEntity> ReplaceDocumentAsync(string id, TEntity newDocument, ItemRequestOptions reqOptions = null);
        Task<TEntity> CreateDocumentAsync(TEntity newDocument);
        Task<TEntity> UpsertDocumentAsync(TEntity newDocument);
        Task<TEntity> DeleteDocumentAsync(string id, string partitionKey);
    }
}

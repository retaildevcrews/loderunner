// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using LodeRunner.API.Models;

namespace LodeRunner.API.DataAccessLayer
{
    /// <summary>
    /// Data Access Layer for CosmosDB Interface
    /// </summary>
    public interface IDAL
    {
        Task<IEnumerable<ClientStatus>> GetClientStatusesAsync();
        Task<ClientStatus> GetClientStatusByClientStatusIdAsync(string clientStatusId);
    }
}

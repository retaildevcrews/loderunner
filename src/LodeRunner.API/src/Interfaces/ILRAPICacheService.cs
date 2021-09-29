// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using LodeRunner.API.Models;
using LodeRunner.Data.Interfaces;
using Microsoft.Azure.Documents;

namespace LodeRunner.API.Interfaces
{
    /// <summary>
    /// The LRAPI Cache Service
    /// </summary>
    /// <seealso cref="LodeRunner.Data.Interfaces.ICacheService" />
    public interface ILRAPICacheService : ICacheService
    {
        IEnumerable<Client> GetClients();
        Client GetClientByClientStatusId(string clientStatusId);
        void ProcessClientStatusChange(Document doc);
    }
}

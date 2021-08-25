// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Ngsa.LodeRunner.DataAccessLayer.Model;

namespace Ngsa.LodeRunner.DataAccessLayer.Interfaces
{
    /// <summary>
    /// LoadClient Repository Interface.
    /// </summary>
    public interface ILoadClientRepository : ICosmosDBRepository<LoadClient>
    {
    }
}

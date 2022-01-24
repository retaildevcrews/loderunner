// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LodeRunner.Core.Models;

namespace LodeRunner.Data.Interfaces
{
    /// <summary>
    /// ClientStatusService Interface.
    /// </summary>
    public interface IClientStatusService : IBaseService<ClientStatus>
    {
        /// <summary>
        /// Terminates the service.
        /// </summary>
        /// <param name="clientStatus">The ClientStatus entity.</param>
        void TerminateService(ClientStatus clientStatus);
    }
}

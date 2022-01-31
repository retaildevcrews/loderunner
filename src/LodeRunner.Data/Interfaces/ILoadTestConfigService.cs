// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using LodeRunner.Core;
using LodeRunner.Core.Models;
using LodeRunner.Core.Responses;

namespace LodeRunner.Data.Interfaces
{
    /// <summary>
    /// LoadTestConfig Interface.
    /// </summary>
    public interface ILoadTestConfigService : IBaseService<LoadTestConfig>
    {
    }
}

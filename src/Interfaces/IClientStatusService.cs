// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ngsa.LodeRunner.Interfaces
{
    public interface IClientStatusService
    {
        Task PostStarting(string message, DateTime? lastUpdated = null);

        Task PostReady(string message, DateTime? lastUpdated = null);

        Task PostTesting(string message, DateTime? lastUpdated = null);

        Task PostTerminating(string message, DateTime? lastUpdated = null);
    }
}

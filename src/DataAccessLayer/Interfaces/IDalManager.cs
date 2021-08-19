// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Ngsa.LodeRunner.Interfaces;

namespace Ngsa.LodeRunner.DataAccessLayer.Interfaces
{
    public interface IDalManager
    {
        IClientStatusService ClientStatusService { get; }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ngsa.LodeRunner.DataAccessLayer.Interfaces
{
    public interface IRepository
    {
        Task<bool> Test();
    }
}

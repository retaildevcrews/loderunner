// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LodeRunner.Data.Interfaces
{
    /// <summary>
    /// Repository Interface.
    /// </summary>
    public interface IRepository
    {
        /// <summary>
        /// Tests this instance.
        /// </summary>
        /// <returns>true if passed , otherwise false.</returns>
        Task<bool> CreateClient();
    }
}

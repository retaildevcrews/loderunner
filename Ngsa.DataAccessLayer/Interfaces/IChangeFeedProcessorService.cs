// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace Ngsa.LodeRunner.DataAccessLayer.Interfaces
{
    /// <summary>
    /// ChangeFeedProcessor Service Interface.
    /// </summary>
    internal interface IChangeFeedProcessorService
    {
        /// <summary>
        /// Gets the change feed processor.
        /// </summary>
        /// <value>
        /// The change feed processor.
        /// </value>
        ChangeFeedProcessor ChangeFeedProcessor { get; }
    }
}

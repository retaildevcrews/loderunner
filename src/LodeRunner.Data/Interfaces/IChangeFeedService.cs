// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LodeRunner.Core.Models;
using LodeRunner.Data.ChangeFeed;
using Microsoft.Azure.Documents.ChangeFeedProcessor.PartitionManagement;

namespace LodeRunner.Data.Interfaces
{
    /// <summary>
    /// ChangeFeedService Interface.
    /// </summary>
    public interface IChangeFeedService
    {
        /// <summary>
        /// Runs the change feed processor.
        /// </summary>
        /// <returns>The IChangeFeedProcessor task.</returns>
        Task<IChangeFeedProcessor> RunChangeFeedProcessor();
    }
}

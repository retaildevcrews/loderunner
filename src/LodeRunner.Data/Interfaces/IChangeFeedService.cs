// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LodeRunner.Core.Models;
using LodeRunner.Data.ChangeFeed;
using Microsoft.Azure.Documents.ChangeFeedProcessor.PartitionManagement;
using static LodeRunner.Data.ChangeFeed.CustomObserver;

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
        /// <param name="onCustomObserverReadyCallback">The callback when CustomObserver is Ready.</param>
        /// <returns>The IChangeFeedProcessor task.</returns>
        Task<IChangeFeedProcessor> StartChangeFeedProcessor(Action onCustomObserverReadyCallback);
    }
}

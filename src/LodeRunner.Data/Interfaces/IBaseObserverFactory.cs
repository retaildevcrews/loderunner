// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Azure.Documents.ChangeFeedProcessor.FeedProcessing;

namespace LodeRunner.Data.Interfaces
{
    /// <summary>
    /// Represents the Base Change Feed Observer Factory.
    /// </summary>
    public interface IBaseObserverFactory : IChangeFeedObserverFactory
    {
        /// <summary>
        /// Gets the observer instance.
        /// </summary>
        /// <returns>Observer.</returns>
        IChangeFeedObserver GetObserverInstance();
    }
}

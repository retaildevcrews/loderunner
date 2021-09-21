// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using LodeRunner.Data.Interfaces;
using Microsoft.Azure.Documents.ChangeFeedProcessor.FeedProcessing;

namespace LodeRunner.Data.ChangeFeed
{
    /// <summary>
    /// Represents the Relay Runner Observer Factory.
    /// </summary>
    /// <seealso cref="Microsoft.Azure.Documents.ChangeFeedProcessor.FeedProcessing.IChangeFeedObserverFactory" />
    internal class LRObserverFactory : IBaseObserverFactory
    {
        private readonly Action callback;
        private LRObserver lRAPIObserverInstance;

        /// <summary>
        /// Initializes a new instance of the <see cref="LRObserverFactory"/> class.
        /// </summary>
        /// <param name="callback">The callback.</param>
        public LRObserverFactory(Action callback)
        {
            this.callback = callback;
        }

        /// <summary>
        /// Creates an instance of a <see cref="T:Microsoft.Azure.Documents.ChangeFeedProcessor.FeedProcessing.IChangeFeedObserver" />.
        /// </summary>
        /// <returns>
        /// An instance of a <see cref="T:Microsoft.Azure.Documents.ChangeFeedProcessor.FeedProcessing.IChangeFeedObserver" />.
        /// </returns>
        public IChangeFeedObserver CreateObserver()
        {
            if (this.lRAPIObserverInstance == null)
            {
                this.lRAPIObserverInstance = new LRObserver();  // this should be a Generic type

                // We do not want to subscribe Invoke callback if LodeRunner.API.ObserverInstance was already created.

                // This allows Event Registration before ChangeFeedProcessor starts listening.
                this.callback?.Invoke();
            }

            return this.lRAPIObserverInstance;
        }

        /// <summary>
        /// Gets the observer.
        /// </summary>
        /// <returns>The Change Feed Observer Instance.</returns>
        public IChangeFeedObserver GetObserverInstance()
        {
            return this.lRAPIObserverInstance;
        }
    }
}

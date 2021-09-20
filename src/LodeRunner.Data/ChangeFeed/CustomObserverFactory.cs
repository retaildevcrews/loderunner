// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Azure.Documents.ChangeFeedProcessor.FeedProcessing;

namespace LodeRunner.Data.ChangeFeed
{
    /// <summary>
    /// Represents the Custom Observer Factory.
    /// </summary>
    /// <seealso cref="Microsoft.Azure.Documents.ChangeFeedProcessor.FeedProcessing.IChangeFeedObserverFactory" />
    internal class CustomObserverFactory : IChangeFeedObserverFactory
    {
        private readonly Action callback;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomObserverFactory"/> class.
        /// </summary>
        /// <param name="callback">The callback.</param>
        public CustomObserverFactory(Action callback)
        {
            this.callback = callback;
        }

        /// <summary>
        /// Gets the custom observer instance.
        /// </summary>
        /// <value>
        /// The custom observer instance.
        /// </value>
        public CustomObserver CustomObserverInstance { get; private set; }

        /// <summary>
        /// Creates an instance of a <see cref="T:Microsoft.Azure.Documents.ChangeFeedProcessor.FeedProcessing.IChangeFeedObserver" />.
        /// </summary>
        /// <returns>
        /// An instance of a <see cref="T:Microsoft.Azure.Documents.ChangeFeedProcessor.FeedProcessing.IChangeFeedObserver" />.
        /// </returns>
        public IChangeFeedObserver CreateObserver()
        {
            if (this.CustomObserverInstance == null)
            {
                this.CustomObserverInstance = new CustomObserver();

                // We do not want to subscribe Invoke callback if CustomObserverInstance was already created.

                // This allows Event Registration before ChangeFeedProcessor starts listening.
                this.callback?.Invoke();
            }

            return this.CustomObserverInstance;
        }
    }
}

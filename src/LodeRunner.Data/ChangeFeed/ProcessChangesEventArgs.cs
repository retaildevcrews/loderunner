// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Azure.Documents;

namespace LodeRunner.Data.ChangeFeed
{
    /// <summary>
    /// Process Changes EventArgs.
    /// </summary>
    public class ProcessChangesEventArgs
    {
        /// <summary>
        /// Gets or sets the last update.
        /// </summary>
        /// <value>
        /// The last update.
        /// </value>
        public DateTime LastUpdate { get; set; }

        /// <summary>
        /// Gets or sets the document.
        /// </summary>
        /// <value>
        /// The document.
        /// </value>
        public Document Document { get; set; }
    }
}

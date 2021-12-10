// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Timers;

namespace LodeRunner.Events
{
    /// <summary>
    /// Represents the Processing Event Bus.
    /// </summary>
    public static class ProcessingEventBus
    {
        /// <summary>
        /// Occurs when [status update].
        /// </summary>
        public static event EventHandler<ClientStatusEventArgs> StatusUpdate = (sender, e) => { };

        // TODO: Move to LodeRunner.Core.Events namespace in a LodeRunner.Core lib

        /// <summary>
        /// Called when [status update].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="ClientStatusEventArgs"/> instance containing the event data.</param>
        public static void OnStatusUpdate(object sender, ClientStatusEventArgs args)
        {
            StatusUpdate(sender, args);
        }
    }
}

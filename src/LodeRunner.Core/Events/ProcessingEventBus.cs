// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Timers;

namespace LodeRunner.Core.Events
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

        /// <summary>
        /// Occurs when [test run complete].
        /// </summary>
        public static event EventHandler<LoadResultEventArgs> TestRunComplete = (sender, e) => { };

        /// <summary>
        /// Called when [status update].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="ClientStatusEventArgs"/> instance containing the event data.</param>
        public static void OnStatusUpdate(object sender, ClientStatusEventArgs args)
        {
            StatusUpdate(sender, args);
        }

        /// <summary>
        /// Called when [test run complete].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="LoadResultEventArgs"/> instance containing the event data.</param>
        public static void OnTestRunComplete(object sender, LoadResultEventArgs args)
        {
            TestRunComplete(sender, args);
        }
    }
}

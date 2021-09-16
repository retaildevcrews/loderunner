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
        private static Timer statusUpdateTimer = default;
        private static object lastStatusSender = default;
        private static ClientStatusEventArgs lastStatusArgs = default;

        public static event EventHandler<ClientStatusEventArgs> StatusUpdate = (sender, e) => { };

        // TODO: Move to LodeRunner.Core.Events namespace in a LodeRunner.Core lib

        /// <summary>
        /// Called when [status update].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="ClientStatusEventArgs"/> instance containing the event data.</param>
        public static void OnStatusUpdate(object sender, ClientStatusEventArgs args)
        {
            lastStatusSender = sender;
            lastStatusArgs = args;

            if (statusUpdateTimer == default(Timer))
            {
                statusUpdateTimer = new ();
                statusUpdateTimer.Interval = 5000; //TODO change this to App.Config.Frequency * 1000
                statusUpdateTimer.Elapsed += OnStatusTimerEvent;
            }

            statusUpdateTimer.Stop();
            StatusUpdate(sender, args);

            statusUpdateTimer.Start();
        }

        // TODO: Move liveness timer to LodeRunnerService

        /// <summary>
        /// Called when [status timer event].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="ElapsedEventArgs"/> instance containing the event data.</param>
        public static void OnStatusTimerEvent(object sender, ElapsedEventArgs args)
        {
            lastStatusArgs.LastUpdated = DateTime.UtcNow;
            OnStatusUpdate(lastStatusSender, lastStatusArgs);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        public static void Dispose()
        {
            statusUpdateTimer.Stop();
            statusUpdateTimer = null;
            lastStatusSender = null;
            lastStatusArgs = null;
        }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Timers;
namespace Ngsa.LodeRunner.Events
{
    public static class ProcessingEventBus
    {
        private static Timer statusUpdateTimer = default;
        private static object lastStatusSender = default;
        private static ClientStatusEventArgs lastStatusArgs = default;

        public static event EventHandler<ClientStatusEventArgs> StatusUpdate = (sender, e) => { };


        public static void OnStatusUpdate(object sender, ClientStatusEventArgs args)
        {
            lastStatusSender = sender;
            lastStatusArgs = args;

            if (statusUpdateTimer == default(Timer))
            {
                statusUpdateTimer = new Timer();
                statusUpdateTimer.Interval = 5000; //TODO change this to App.Config.Frequency * 1000
                statusUpdateTimer.Elapsed += OnStatusTimerEvent;
            }

            statusUpdateTimer.Stop();
            StatusUpdate(sender, args);

            statusUpdateTimer.Start();
        }


        public static void OnStatusTimerEvent(object sender, ElapsedEventArgs args)
        {
            lastStatusArgs.LastUpdated = DateTime.UtcNow;
            OnStatusUpdate(lastStatusSender, lastStatusArgs);
        }

        public static void Dispose()
        {
            statusUpdateTimer.Stop();
            statusUpdateTimer = null;
            lastStatusSender = null;
            lastStatusArgs = null;
        }
    }
}

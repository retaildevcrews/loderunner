// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
namespace Ngsa.LodeRunner.Events
{
    public static class ProcessingEventBus
    {
        public static event EventHandler<ClientStatusEventArgs> StatusChanged = (sender, e) => { };


        public static void OnStatusUpdate(object sender, ClientStatusEventArgs args)
        {
            StatusChanged(sender, args);
        }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
namespace Ngsa.LodeRunner.Events
{
    public class ClientStatusEventArgs : EventArgs
    {
        public ClientStatusEventArgs(string status, string message)
        {
            LastUpdated = DateTime.UtcNow;
            Status = status;
            Message = message;
        }

        public DateTime LastUpdated { get; set; }
        public string Status { get; set; } //TODO Convert status to enum when merging with dal
        public string Message { get; set; }
    }
}

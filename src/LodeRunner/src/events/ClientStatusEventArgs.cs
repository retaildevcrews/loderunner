// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using LodeRunner.Core.Models;

// TODO: Move to LodeRunner.Core.Events namespace in a LodeRunner.Core lib
namespace LodeRunner.Events
{
    /// <summary>
    /// Represents the ClientStatusEventArgs and contains the main functionality of the class
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public partial class ClientStatusEventArgs : EventArgs
    {
        public ClientStatusEventArgs(ClientStatusType status, string message)
        {
            LastUpdated = DateTime.UtcNow;
            Status = status;
            Message = message;
        }
    }

    /// <summary>
    /// Implements the ClientStatusEventArgs class, fix StyleCopAnalyzer violation #SA1201.
    /// </summary>
    public partial class ClientStatusEventArgs
    {
        public DateTime LastUpdated { get; set; }
        public ClientStatusType Status { get; set; }
        public string Message { get; set; }
    }
}

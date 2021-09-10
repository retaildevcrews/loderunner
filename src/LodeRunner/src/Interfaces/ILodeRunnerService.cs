// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace LodeRunner.Interfaces
{
    /// <summary>
    /// LodeRunnerService interface.
    /// </summary>
    internal interface ILodeRunnerService
    {
        /// <summary>
        /// Gets the service provider.
        /// </summary>
        /// <value>
        /// The service provider.
        /// </value>
        ServiceProvider ServiceProvider { get; }
    }
}

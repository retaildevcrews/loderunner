// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LodeRunner.Core.Interfaces
{
    /// <summary>
    /// Represent the Interface used to inject data Dictionary to be added when NgsaLogger performs a log operation.
    /// </summary>
    public interface ILogValues
    {
        /// <summary>
        /// Gets the log values.
        /// </summary>
        /// <returns>Data Dictionary.</returns>
        Dictionary<string, object> GetLogValues();
    }
}

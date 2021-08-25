// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ngsa.LodeRunner.DataAccessLayer
{
    /// <summary>
    /// ChangeFeedProcessor Settings.
    /// </summary>
    public class ChangeFeedProcessorSettings
    {
        /// <summary>
        /// Gets or sets the name of the instance.
        /// </summary>
        /// <value>
        /// The name of the instance.
        /// </value>
        public string InstanceName { get; set; }

        /// <summary>
        /// Gets or sets the name of the processor.
        /// </summary>
        /// <value>
        /// The name of the processor.
        /// </value>
        public string ProcessorName { get; set; }
    }
}

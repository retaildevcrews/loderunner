// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace LodeRunner.Core.NgsaLogger
{
    /// <summary>
    /// Event Types.
    /// </summary>
    public class EventTypes
    {
        /// <summary>
        ///  Common Events Ids.
        /// </summary>
        public enum CommonEvents
        {
            /// <summary>
            /// The success enum.
            /// </summary>
            Success = 1000,

            /// <summary>
            /// The fail enum.
            /// </summary>
            Fail = 2000,

            /// <summary>
            /// The exception enum.
            /// </summary>
            Exception = 2010,

            /// <summary>
            /// The validation enum.
            /// </summary>
            Validation = 2020,
        }

        // NOTE: We can create enums specifically for LodeRunner and LodeRunner.API
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace LodeRunner.API.Models.Enum
{
    /// <summary>
    /// Types of Entity used to group properties
    /// </summary>
    public enum EntityType
    {
        /// <summary>
        /// Represents a frontend compatible representation of ClientStatus
        /// </summary>
        Client,

        /// <summary>
        /// Configs a load client is started with
        /// </summary>
        LoadClient,

        /// <summary>
        /// Details about a running instance of a load client
        /// </summary>
        ClientStatus,

        /// <summary>
        /// Configs used in test execution
        /// </summary>
        LoadTestConfig,

        /// <summary>
        /// Details about a test execution
        /// </summary>
        TestRun,
    }

    /// <summary>
    /// Values for Client and ClientStatus status
    /// </summary>
    public enum Status
    {
        /// <summary>
        /// When an instance of a load client is starting
        /// </summary>
        Starting,

        /// <summary>
        /// When an instance of a load client is ready to execute tests
        /// </summary>
        Ready,

        /// <summary>
        /// When an instance of a load client is actively executing a test
        /// </summary>
        Testing,

        /// <summary>
        /// When an instance of a load client is shutting down
        /// </summary>
        Terminating,
    }
}

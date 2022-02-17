// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace LodeRunner.API.Test.IntegrationTests.ExecutingTestRun
{
    /// <summary>
    /// Represents the required Parameters for Process Context to Start.
    /// </summary>
    internal class ProcessContextParams
    {
        /// <summary>
        /// Gets or sets the command line.
        /// </summary>
        /// <value>
        /// The command line.
        /// </value>
        public string CommandLine { get; set; } = "dotnet";

        /// <summary>
        /// Gets or sets the command line arguments.
        /// </summary>
        /// <value>
        /// The lode runner arguments.
        /// </value>
        public string CommandLineArgs { get; set; } = $"run --no-build --project";

        /// <summary>
        /// Gets or sets the project base path.
        /// e.g "LodeRunner/LodeRunner.csproj";.
        /// </summary>
        /// <value>
        /// The project path.
        /// </value>
        public string ProjectBasePath { get; set; }

        /// <summary>
        /// Gets or sets the name of the project base parent directory.
        /// </summary>
        /// <value>
        /// The name of the project base parent directory.
        /// </value>
        public string ProjectBaseParentDirectoryName { get; set; } = "src";

        /// <summary>
        /// Gets or sets the project arguments.
        /// e.g "--mode Client --secrets-volume secrets";.
        /// </summary>
        /// <value>
        /// The project arguments.
        /// </value>
        public string ProjectArgs { get; set; }
    }
}

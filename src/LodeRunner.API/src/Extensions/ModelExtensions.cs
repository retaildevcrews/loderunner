// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using LodeRunner.API.Data.Dtos;
using LodeRunner.Core.Models;

namespace LodeRunner.API.Middleware
{
    /// <summary>
    /// Provides Extension methods for Models.
    /// </summary>
    public static class ModelExtensions
    {
        /// <summary>
        /// Converts LoadTestConfig model to LoadTestConfigDTO.
        /// </summary>
        /// <param name="loadTestConfigDto">The load test configuration.</param>
        /// <returns>LoadTestConfig.</returns>
        public static LoadTestConfig DtoToModel(this Data.Dtos.LoadTestConfigDto loadTestConfigDto)
        {
            // TODO: Validate LoadTestConfigDto before to create Model
            return new LoadTestConfig
            {
                Files = loadTestConfigDto.Files,

                StrictJson = loadTestConfigDto.StrictJson,

                BaseURL = loadTestConfigDto.BaseURL,

                VerboseErrors = loadTestConfigDto.VerboseErrors,

                Randomize = loadTestConfigDto.Randomize,

                Timeout = loadTestConfigDto.Timeout,

                Server = loadTestConfigDto.Server,

                Tag = loadTestConfigDto.Tag,

                Sleep = loadTestConfigDto.Sleep,

                RunLoop = loadTestConfigDto.RunLoop,

                Duration = loadTestConfigDto.Duration,

                MaxErrors = loadTestConfigDto.MaxErrors,

                DelayStart = loadTestConfigDto.DelayStart,

                DryRun = loadTestConfigDto.DryRun,
            };
        }

        /// <summary>
        /// Validates the specified load test configuration dto.
        /// </summary>
        /// <param name="loadTestConfigDto">The load test configuration dto.</param>
        /// <returns>Whether or not  the DTO passes validation.</returns>
        public static bool Validate(this Data.Dtos.LoadTestConfigDto loadTestConfigDto)
        {
            return true;
        }
    }
}

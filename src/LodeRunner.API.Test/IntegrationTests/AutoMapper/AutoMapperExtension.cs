// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using AutoMapper;
using LodeRunner.API.Test.IntegrationTests.Payloads;
using LodeRunner.Core.Models;

namespace LodeRunner.API.Test.IntegrationTests.AutoMapper
{
    /// <summary>
    /// AutoMapper Extensions.
    /// </summary>
    public static class AutoMapperExtension
    {
        /// <summary>
        /// Automaps the and get test run payload.
        /// </summary>
        /// <param name="testRunSource">The test run source.</param>
        /// <returns>The Test Payload object.</returns>
        public static TestRunTestPayload AutomapAndGetTestRunPayload(this TestRun testRunSource)
        {
            //// Do the mapping to assure we use the payload class.
            return BasePayloadAutoMapperHelper<TestRun, TestRunTestPayload>.Map(testRunSource);
        }

       /// <summary>
        /// Automaps the and get a new LoadClient.
        /// </summary>
        /// <param name="loadClientSource">The test run source.</param>
        /// <returns>The Load Client object.</returns>
        public static LoadClient AutomapAndGetaNewLoadClient(this LoadClient loadClientSource)
        {
            // Do the mapping to assure we generate a new ID.
            return BaseEntityAutoMapperHelper<LoadClient, LoadClient>.MapIgnoringId(loadClientSource);
        }
    }
}

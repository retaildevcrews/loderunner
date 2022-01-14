// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using AutoMapper;
using LodeRunner.Core.Models;

namespace LodeRunner.API.Test.IntegrationTests
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
        public static TestRunPayload AutomapAndGetTestRunPayload(this TestRun testRunSource)
        {
            // Do the mapping to assure we use the payload class.
            var config = new MapperConfiguration(cfg => cfg.CreateMap<TestRun, TestRunPayload>());

            var mapper = new Mapper(config);

            TestRunPayload testRunPayload = mapper.Map<TestRun, TestRunPayload>(testRunSource);

            return testRunPayload;
        }

        /// <summary>
        /// Automaps the and get a new LoadClient.
        /// </summary>
        /// <param name="loadClientSource">The test run source.</param>
        /// <returns>The Load Client object.</returns>
        public static LoadClient AutomapAndGetaNewLoadClient(this LoadClient loadClientSource)
        {
            // Do the mapping to assure we generate a new ID.
            var config = new MapperConfiguration(cfg => cfg.CreateMap<LoadClient, LoadClient>().ForMember(x => x.Id, opt => opt.Ignore()));

            var mapper = new Mapper(config);

            LoadClient newLoadClient = mapper.Map<LoadClient, LoadClient>(loadClientSource);

            return newLoadClient;
        }
    }
}

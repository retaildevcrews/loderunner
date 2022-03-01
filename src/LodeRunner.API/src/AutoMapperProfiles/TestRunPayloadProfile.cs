// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using AutoMapper;
using LodeRunner.API.Extensions;
using LodeRunner.Core.Models;

namespace LodeRunner.API.AutoMapperProfiles
{
    /// <summary>
    /// Represents the TestRun Auto-mapping Profile.
    /// </summary>
    /// <seealso cref="AutoMapper.Profile" />
    public class TestRunPayloadProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestRunPayloadProfile"/> class.
        /// </summary>
        public TestRunPayloadProfile()
        {
            this.CreateMap<TestRunPayload, TestRun>().ForMember(x => x.Id, opt => opt.Ignore()).IgnoreNullFields();
        }
    }
}

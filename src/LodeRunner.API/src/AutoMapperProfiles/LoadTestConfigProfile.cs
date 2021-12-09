﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using AutoMapper;
using LodeRunner.Core.Models;

namespace LodeRunner.API.AutoMapperProfiles
{
    /// <summary>
    /// Represents the LoadTestConfig Auto-mapping Profile.
    /// </summary>
    /// <seealso cref="AutoMapper.Profile" />
    public class LoadTestConfigProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadTestConfigProfile"/> class.
        /// </summary>
        public LoadTestConfigProfile()
        {
            this.CreateMap<LoadTestConfig, LoadTestConfig>().ForMember(x => x.Id, opt =>
            {
                opt.Ignore();
            });
        }
    }
}

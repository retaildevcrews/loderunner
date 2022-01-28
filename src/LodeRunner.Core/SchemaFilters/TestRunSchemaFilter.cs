﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace LodeRunner.Core.SchemaFilters
{
    /// <summary>
    /// Defines the TestRun Schema.
    /// </summary>
    /// <seealso cref="Swashbuckle.AspNetCore.SwaggerGen.ISchemaFilter" />
    public class TestRunSchemaFilter : ISchemaFilter
    {
        /// <summary>
        /// Applies the specified schema.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <param name="context">The context.</param>
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            schema.Example = new OpenApiObject
            {
                ["id"] = new OpenApiString("58c2053e-cfa6-48ef-b4f2-8a68acded0a4"),
                ["name"] = new OpenApiString("Sample - TestRun"),
                ["entityType"] = new OpenApiString("TestRun"),
                ["partitionKey"] = new OpenApiString("TestRun"),
                ["loadTestConfig"] = new OpenApiObject
                {
                    ["id"] = new OpenApiString("7abcc308-14c4-43eb-b1ee-e351f4db2a08"),
                    ["name"] = new OpenApiString("Sample - LoadTestConfig"),
                    ["entityType"] = new OpenApiString("LoadTestConfig"),
                    ["partitionKey"] = new OpenApiString("LoadTestConfig"),
                    ["files"] = new OpenApiArray
                    {
                        new OpenApiString("baseline.json"),
                        new OpenApiString("benchmark.json"),
                    },
                    ["strictJson"] = new OpenApiBoolean(true),
                    ["baseURL"] = new OpenApiString("Sample BaseURL"),
                    ["verboseErrors"] = new OpenApiBoolean(true),
                    ["randomize"] = new OpenApiBoolean(true),
                    ["timeout"] = new OpenApiInteger(10),
                    ["server"] = new OpenApiArray
                    {
                        new OpenApiString("www.yourprimaryserver.com"),
                        new OpenApiString("www.yoursecondaryserver.com"),
                    },
                    ["tag"] = new OpenApiString("Sample Tag"),
                    ["sleep"] = new OpenApiInteger(5),
                    ["runLoop"] = new OpenApiBoolean(true),
                    ["duration"] = new OpenApiInteger(60),
                    ["maxErrors"] = new OpenApiInteger(10),
                    ["delayStart"] = new OpenApiInteger(5),
                    ["dryRun"] = new OpenApiBoolean(false),
                },
                ["loadClients"] = new OpenApiArray
                {
                    new OpenApiObject
                    {
                        ["id"] = new OpenApiString("13eff199-38ee-46ee-92df-d064025db4e7"),
                        ["version"] = new OpenApiString("1.0.1"),
                        ["region"] = new OpenApiString("Central"),
                        ["zone"] = new OpenApiString("central-az-1"),
                        ["prometheus"] = new OpenApiBoolean(true),
                        ["startupArgs"] = new OpenApiString("--mode Client --region Central --zone central-az-1 --prometheus true"),
                        ["startTime"] = new OpenApiDateTime(new DateTime(2022, 1, 1, 2, 3, 4)),
                    },
                    new OpenApiObject
                    {
                        ["id"] = new OpenApiString("b91dde95-1fb4-4bce-922c-52019d5c0a55"),
                        ["version"] = new OpenApiString("1.0.1"),
                        ["region"] = new OpenApiString("Central"),
                        ["zone"] = new OpenApiString("central-az-2"),
                        ["prometheus"] = new OpenApiBoolean(true),
                        ["startupArgs"] = new OpenApiString("--mode Client --region Central --zone central-az-2 --prometheus true"),
                        ["startTime"] = new OpenApiDateTime(new DateTime(2021, 12, 31, 2, 3, 4)),
                    },
                    new OpenApiObject
                    {
                        ["id"] = new OpenApiString("f7abf4f7-c1a4-4720-bda7-9251276b21f7"),
                        ["version"] = new OpenApiString("1.0.1"),
                        ["region"] = new OpenApiString("Central"),
                        ["zone"] = new OpenApiString("central-az-1"),
                        ["prometheus"] = new OpenApiBoolean(false),
                        ["startupArgs"] = new OpenApiString("--mode Client --region Central --zone central-az-1 --prometheus false"),
                        ["startTime"] = new OpenApiDateTime(new DateTime(2021, 12, 30, 2, 3, 4)),
                    },
                },
                ["createdTime"] = new OpenApiDateTime(new DateTime(2022, 1, 7, 0, 0, 0)),
                ["startTime"] = new OpenApiDateTime(new DateTime(2022, 1, 7, 0, 30, 0)),
                ["completedTime"] = new OpenApiDateTime(new DateTime(2022, 1, 9, 0, 0, 0)),
                ["clientResults"] = new OpenApiArray
                {
                    new OpenApiObject
                    {
                        ["loadClient"] = new OpenApiObject
                        {
                            ["id"] = new OpenApiString("13eff199-38ee-46ee-92df-d064025db4e7"),
                            ["version"] = new OpenApiString("1.0.1"),
                            ["region"] = new OpenApiString("Central"),
                            ["zone"] = new OpenApiString("central-az-1"),
                            ["prometheus"] = new OpenApiBoolean(true),
                            ["startupArgs"] = new OpenApiString("--mode Client --region Central --zone central-az-1 --prometheus true"),
                            ["startTime"] = new OpenApiDateTime(new DateTime(2022, 1, 1, 2, 3, 4)),
                        },
                        ["startTime"] = new OpenApiDateTime(new DateTime(2022, 1, 7, 0, 30, 0)),
                        ["completedTime"] = new OpenApiDateTime(new DateTime(2022, 1, 7, 0, 45, 0)),
                        ["totalRequests"] = new OpenApiInteger(100),
                        ["successfulRequests"] = new OpenApiInteger(100),
                        ["failedRequests"] = new OpenApiInteger(0),
                    },
                    new OpenApiObject
                    {
                        ["loadClient"] = new OpenApiObject
                        {
                            ["id"] = new OpenApiString("b91dde95-1fb4-4bce-922c-52019d5c0a55"),
                            ["version"] = new OpenApiString("1.0.1"),
                            ["region"] = new OpenApiString("Central"),
                            ["zone"] = new OpenApiString("central-az-2"),
                            ["prometheus"] = new OpenApiBoolean(true),
                            ["startupArgs"] = new OpenApiString("--mode Client --region Central --zone central-az-2 --prometheus true"),
                            ["startTime"] = new OpenApiDateTime(new DateTime(2021, 12, 31, 2, 3, 4)),
                        },
                        ["startTime"] = new OpenApiDateTime(new DateTime(2022, 1, 7, 0, 30, 0)),
                        ["completedTime"] = new OpenApiDateTime(new DateTime(2022, 1, 7, 1, 00, 0)),
                        ["totalRequests"] = new OpenApiInteger(100),
                        ["successfulRequests"] = new OpenApiInteger(95),
                        ["failedRequests"] = new OpenApiInteger(5),
                    },
                    new OpenApiObject
                    {
                        ["loadClient"] = new OpenApiObject
                        {
                            ["id"] = new OpenApiString("f7abf4f7-c1a4-4720-bda7-9251276b21f7"),
                            ["version"] = new OpenApiString("1.0.1"),
                            ["region"] = new OpenApiString("Central"),
                            ["zone"] = new OpenApiString("central-az-1"),
                            ["prometheus"] = new OpenApiBoolean(false),
                            ["startupArgs"] = new OpenApiString("--mode Client --region Central --zone central-az-1 --prometheus true"),
                            ["startTime"] = new OpenApiDateTime(new DateTime(2021, 12, 30, 2, 3, 4)),
                        },
                        ["startTime"] = new OpenApiDateTime(new DateTime(2022, 1, 7, 0, 30, 0)),
                        ["completedTime"] = new OpenApiDateTime(new DateTime(2022, 1, 7, 1, 15, 0)),
                        ["totalRequests"] = new OpenApiInteger(11),
                        ["successfulRequests"] = new OpenApiInteger(1),
                        ["failedRequests"] = new OpenApiInteger(10),
                    },
                },
            };
        }
    }
}

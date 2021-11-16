// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using LodeRunner.Core.Extensions;
using LodeRunner.Core.SchemaFilters;
using Swashbuckle.AspNetCore.Annotations;

namespace LodeRunner.Core.Models
{
    /// <summary>
    /// LoadRestConfig Payload.
    /// </summary>
    [SwaggerSchemaFilter(typeof(LoadTestConfigPayloadSchemaFilter))]
    public class LoadTestConfigPayload : LoadTestConfig
    {
        /// <summary>
        /// Gets the type of the entity.
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        public override EntityType EntityType
        {
            get
            {
                if (this.entityType == EntityType.Unassigned)
                {
                    this.entityType = this.GetType().BaseType.Name.As<EntityType>(EntityType.Unassigned);
                }

                return this.entityType;
            }
        }
    }
}

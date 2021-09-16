// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using LodeRunner.Core.Extensions;
using LodeRunner.Core.Interfaces;

namespace LodeRunner.Core.Models
{
    /// <summary>
    ///   BaseEntityModel is base abstract class for all entities and model.
    /// </summary>
    public abstract class BaseEntityModel : IBaseEntityModel
    {
        private EntityType entityType = EntityType.Unassigned;

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public virtual string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets the partition key.
        /// </summary>
        /// <value>
        /// The partition key.
        /// </value>
        public virtual string PartitionKey
        {
            get { return this.EntityType.ToString(); }
        }

        /// <summary>
        /// Gets the type of the entity.
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        public virtual EntityType EntityType
        {
            get
            {
                if (this.entityType == EntityType.Unassigned)
                {
                    this.entityType = this.GetType().Name.As<EntityType>(EntityType.Unassigned);
                }

                return this.entityType;
            }
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public virtual string Name { get; set; }
    }
}

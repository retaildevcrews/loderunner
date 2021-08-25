// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Ngsa.LodeRunner.DataAccessLayer.Interfaces;
using Ngsa.LodeRunner.DataAccessLayer.Model;

namespace Ngsa.LodeRunner.DataAccessLayer
{
    /// <summary>
    ///   ChangeFeedProcessor Repository.
    /// </summary>
    public partial class ChangeFeedProcessorRepository : CosmosDBRepository<ClientStatus>
    {
        private readonly ChangeFeedProcessorRepositorySettings settings;

        /// <summary>Initializes a new instance of the <see cref="ChangeFeedProcessorRepository" /> class.</summary>
        /// <param name="settings">The settings.</param>
        /// <exception cref="System.ApplicationException">Repository test for {this.Id} failed.</exception>
        public ChangeFeedProcessorRepository(ChangeFeedProcessorRepositorySettings settings)
            : base(settings)
        {
            this.settings = settings;

            if (!this.Test().Result)
            {
                throw new ApplicationException($"Repository test for {this.Id} failed.");
            }
        }

        /// <inheritdoc/>
        public override string CollectionName => this.settings.CollectionName;

        /// <inheritdoc/>
        public override string GenerateId(ClientStatus entity)
        {
            if (string.IsNullOrWhiteSpace(entity.Id))
            {
                entity.Id = Guid.NewGuid().ToString();
            }

            return entity.Id;
        }
    }

    /// <summary>
    /// Implements the ILodeRunnerService class, fix StyleCopAnalyzer violation #SA1201.
    /// </summary>
    public partial class ChangeFeedProcessorRepository : IChangeFeedProcessorRepository
    {
        private readonly object lockSourceObj = new ();
        private readonly object lockLeaseObj = new ();

        /// <summary>
        /// Gets the source container.
        /// </summary>
        /// <value>
        /// The source container.
        /// </value>
        public Container SourceContainer
        {
            get
            {
                lock (this.lockSourceObj)
                {
                    return this.GetContainer(this.Client, this.settings.CollectionName);
                }
            }
        }

        /// <summary>
        /// Gets the lease container.
        /// </summary>
        /// <value>
        /// The lease container.
        /// </value>
        public Container LeaseContainer
        {
            get
            {
                lock (this.lockLeaseObj)
                {
                    return this.GetContainer(this.Client, this.settings.CosmosLease);
                }
            }
        }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Ngsa.LodeRunner.DataAccessLayer;
using Ngsa.LodeRunner.DataAccessLayer.Interfaces;

namespace Ngsa.LodeRunner.Services
{
    /// <summary>
    /// Represents the Change Feed Processor Service and contains the main functionality of the class.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public partial class ChangeFeedProcessorService<TEntity>
        where TEntity : class
    {
        private ChangeFeedProcessorRepository changeFeedProcessorRepository;

        private ChangeFeedProcessor changeFeedProcessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeFeedProcessorService{TEntity}"/> class.
        /// </summary>
        /// <param name="cosmosDBSettings">The cosmos database settings.</param>
        /// <param name="changeFeedProcessorSettings">The change feed processor settings.</param>
        public ChangeFeedProcessorService(ChangeFeedProcessorRepositorySettings cosmosDBSettings, ChangeFeedProcessorSettings changeFeedProcessorSettings)
        {
            this.CreateRepository(cosmosDBSettings);

            this.StartChangeFeedProcessorAsync(changeFeedProcessorSettings).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Called when [handle changes asynchronous].
        /// </summary>
        /// <param name="changes">The changes.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns> The HandleChangesAsync implementation Task.</returns>
        protected Task OnHandleChangesAsync(IReadOnlyCollection<TEntity> changes, CancellationToken cancellationToken)
        {
            return this.HandleChangesAsync?.Invoke(changes, cancellationToken);
        }

        private void CreateRepository(ChangeFeedProcessorRepositorySettings cosmosDBSettings)
        {
            cosmosDBSettings.Validate();

            this.changeFeedProcessorRepository = new ChangeFeedProcessorRepository(cosmosDBSettings);
        }

        /// <summary>
        /// Start the Change Feed Processor to listen for changes and process them with the HandleChangesAsync implementation.
        /// </summary>
        /// <returns>ChangeFeedProcessor Task.</returns>
        private async Task<ChangeFeedProcessor> StartChangeFeedProcessorAsync(ChangeFeedProcessorSettings changeFeedProcessorSettings)
        {
            this.changeFeedProcessor = this.changeFeedProcessorRepository.SourceContainer
            .GetChangeFeedProcessorBuilder<TEntity>(processorName: changeFeedProcessorSettings.ProcessorName, onChangesDelegate: this.OnHandleChangesAsync)
            .WithInstanceName(changeFeedProcessorSettings.InstanceName)
            .WithLeaseContainer(this.changeFeedProcessorRepository.LeaseContainer)
            .Build();

            Console.WriteLine("Starting Change Feed Processor...");
            await this.changeFeedProcessor.StartAsync();
            Console.WriteLine("Change Feed Processor started.");
            return this.changeFeedProcessor;
        }
    }

#pragma warning disable CS1710 // XML comment has a duplicate typeparam tag since it is a partial class
    /// <summary>
    /// Implements the ILodeRunnerService class, fix StyleCopAnalyzer violation #SA1201.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public partial class ChangeFeedProcessorService<TEntity> : IChangeFeedProcessorService
#pragma warning restore CS1710 // XML comment has a duplicate typeparam tag since it is a partial class
        where TEntity : class
    {
        /// <summary>
        /// The HandleChangesEventHandler delegate.
        /// </summary>
        /// <param name="changes">The changes.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns> The delegated task.</returns>
        public delegate Task HandleChangesEventHandler(IReadOnlyCollection<TEntity> changes, CancellationToken cancellationToken);

        /// <summary>
        /// Occurs when [handle changes asynchronous].
        /// </summary>
        public event HandleChangesEventHandler HandleChangesAsync;

        /// <summary>
        /// Gets the change feed processor.
        /// </summary>
        /// <value>
        /// The change feed processor.
        /// </value>
        public ChangeFeedProcessor ChangeFeedProcessor { get; private set; }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using LodeRunner.Core.Events;
using LodeRunner.Core.Interfaces;
using LodeRunner.Core.Models;
using LodeRunner.Data.Interfaces;
using Microsoft.Extensions.Logging;

namespace LodeRunner.Subscribers
{
    internal class ClientStatusUpdater : IDisposable
    {
        private const int MaxRetryAttempts = 3;
        private readonly ClientStatus clientStatus;
        private readonly IClientStatusService clientStatusService;
        private readonly ILogger logger;
        private readonly ILRConfig config;
        private int failures = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientStatusUpdater"/> class.
        /// </summary>
        /// <param name="clientStatusService">The client status service.</param>
        /// <param name="clientStatus">The client status.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="config">The config.</param>
        public ClientStatusUpdater(IClientStatusService clientStatusService, ClientStatus clientStatus, ILogger logger, ILRConfig config)
        {
            this.clientStatus = clientStatus;
            this.clientStatusService = clientStatusService;
            this.logger = logger;
            this.config = config;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Dispose any objects here.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Updates the cosmos status.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="ClientStatusEventArgs"/> instance containing the event data.</param>
        public async void UpdateCosmosStatus(object sender, ClientStatusEventArgs args)
        {
            // TODO: do we need a lock here?
            this.clientStatus.Message = args.Message;
            this.clientStatus.Status = args.Status;

            try
            {
                _ = await this.clientStatusService.Post(this.clientStatus, args.CancelTokenSource.Token).ConfigureAwait(false);

                // reset failure count on success
                failures = 0;
            }
            catch (Exception ex)
            {
                failures++;

                if (failures >= MaxRetryAttempts)
                {
                    string baseMessage = $"Unable to Update Client Status after {failures} attempts. Application will Terminate.";

                    logger.LogWarning(new EventId((int)LogLevel.Warning, nameof(UpdateCosmosStatus)), ex, baseMessage);

                    args.CancelTokenSource.Cancel(false);
                }
            }
        }
    }
}

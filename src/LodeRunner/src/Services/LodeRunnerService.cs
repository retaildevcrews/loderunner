// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using LodeRunner.Core;
using LodeRunner.Core.Interfaces;
using LodeRunner.Core.Models;
using LodeRunner.Data;
using LodeRunner.Data.Interfaces;
using LodeRunner.Events;
using LodeRunner.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LodeRunner.Services
{
    /// <summary>
    /// Represents the LodeRunnerService and contains the main functionality of the class.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    /// <seealso cref="LodeRunner.Interfaces.ILodeRunnerService" />
    internal class LodeRunnerService : IDisposable, ILodeRunnerService
    {
        private readonly Config config;
        private readonly LoadClient loadClient;
        private readonly CancellationTokenSource cancellationTokenSource;
        private ClientStatus clientStatus;

        public LodeRunnerService(Config config, CancellationTokenSource cancellationTokenSource)
        {
            this.config = config ?? throw new Exception("CommandOptions is null");

            this.loadClient = LoadClient.GetNew(this.config, DateTime.UtcNow);

            this.clientStatus = new ClientStatus
            {
                // TODO: need to set the correct StatusDuration, this is just an example
                StatusDuration = 1,
                Status = ClientStatusType.Starting,
                LoadClient = this.loadClient,
            };

            this.cancellationTokenSource = cancellationTokenSource;
        }

        /// <summary>
        /// Gets the service provider.
        /// </summary>
        /// <value>
        /// The service provider.
        /// </value>
        public ServiceProvider ServiceProvider { get; private set; }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Starts the service and uses the Configuration to determine the Start mode.
        /// </summary>
        /// <returns>The Task with exit code.</returns>
        public async Task<int> StartService()
        {
            // set any missing values
            config.SetDefaultValues();

            // don't run the test on a dry run
            if (config.DryRun)
            {
                return await StartDryRun();
            }

            try
            {
                if (config.DelayStart == -1)
                {
                    return await StartAndWait();
                }
                else
                {
                    return await Start();
                }
            }
            catch (TaskCanceledException tce)
            {
                // log exception
                if (!tce.Task.IsCompleted)
                {
                    Console.WriteLine($"Exception: {tce}");
                    return SystemConstants.ExitFail;
                }

                // task is completed
                return SystemConstants.ExitSuccess;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nException:{ex.Message}");
                return SystemConstants.ExitFail;
            }
        }

        public IClientStatusService GetClientStatusService()
        {
            return ServiceProvider.GetService<IClientStatusService>();
        }

        public ILoadTestConfigService GetLoadTestConfigService()
        {
            return ServiceProvider.GetService<ILoadTestConfigService>();
        }

        //TODO Move to proper location when merging with DAL
        public void LogStatusChange(object sender, ClientStatusEventArgs args)
        {
            Console.WriteLine($"{args.Message} - {args.LastUpdated:yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK}"); //TODO fix LogStatusChange implementation
        }

        // TODO: Update PostUpdate call to pass clientStatus object from ClientStatusEventArgs
        public async void UpdateCosmosStatus(object sender, ClientStatusEventArgs args)
        {
            // Update Entity, TODO: do we need a lock here?
            clientStatus.LastUpdated = args.LastUpdated;
            clientStatus.Message = args.Message;
            clientStatus.Status = args.Status;

            this.clientStatus = await GetClientStatusService().PostUpdate(this.clientStatus, cancellationTokenSource.Token).ConfigureAwait(false);

            //TODO : Add try catch and write log , then exit App?
        }

        /// <summary>
        /// Validates the settings.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <exception cref="System.ApplicationException">Failed to validate application configuration</exception>
        private static void ValidateSettings(ServiceProvider provider)
        {
            var settings = provider.GetServices<ISettingsValidator>();
            foreach (var validator in settings)
            {
                try
                {
                    validator.Validate();
                }
                catch (Exception ex)
                {
                    throw new ApplicationException($"Failed to validate application configuration", ex);
                }
            }
        }

        /// <summary>
        /// Loads the secrets.
        /// </summary>
        /// <param name="config">The configuration.</param>
        private static void LoadSecrets(Config config)
        {
            config.Secrets = Secrets.GetSecretsFromVolume(config.SecretsVolume);

            // set the Cosmos server name for logging
            config.CosmosName = config.Secrets.CosmosServer.Replace("https://", string.Empty, StringComparison.OrdinalIgnoreCase).Replace("http://", string.Empty, StringComparison.OrdinalIgnoreCase);

            int ndx = config.CosmosName.IndexOf('.', StringComparison.OrdinalIgnoreCase);

            if (ndx > 0)
            {
                config.CosmosName = config.CosmosName.Remove(ndx);
            }
        }

        /// <summary>
        /// Starts a loderunner instance.
        /// </summary>
        /// <returns>The Task with exit code.</returns>
        private async Task<int> Start()
        {
            if (config.DelayStart > 0)
            {
                Console.WriteLine($"Waiting {config.DelayStart} seconds to start test ...\n");

                // wait to start the test run
                await Task.Delay(config.DelayStart * 1000, this.cancellationTokenSource.Token).ConfigureAwait(false);
            }

            ValidationTest lrt = new (config);

            if (config.RunLoop)
            {
                // build and run the web host
                IHost host = App.BuildWebHost(config);
                _ = host.StartAsync(this.cancellationTokenSource.Token);

                // run in a loop
                int res = lrt.RunLoop(config, this.cancellationTokenSource.Token);

                // stop and dispose the web host
                await host.StopAsync(TimeSpan.FromMilliseconds(100)).ConfigureAwait(false);
                host.Dispose();

                //host = null; Remove unnecessary value assignment (IDE0059)

                return res;
            }
            else
            {
                // run one iteration
                return await lrt.RunOnce(config, this.cancellationTokenSource.Token).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Starts a loderunner instance on dry run.
        /// </summary>
        /// <returns>The Task with exit code.</returns>
        private Task<int> StartDryRun()
        {
            return Task.Run(() => App.DoDryRun(config));
        }

        /// <summary>
        /// Starts a loderunner instance and wait to start test.
        /// </summary>
        /// <returns>The Task with exit code.</returns>
        private async Task<int> StartAndWait()
        {
            InitAndRegister();

            ProcessingEventBus.StatusUpdate += UpdateCosmosStatus;
            ProcessingEventBus.StatusUpdate += LogStatusChange;

            ProcessingEventBus.OnStatusUpdate(null, new ClientStatusEventArgs(ClientStatusType.Starting, "Initializing - test init"));

            ProcessingEventBus.OnStatusUpdate(null, new ClientStatusEventArgs(ClientStatusType.Ready, "Ready - test ready"));
            try
            {
                // wait indefinitely
                await Task.Delay(config.DelayStart, this.cancellationTokenSource.Token).ConfigureAwait(false);
            }
            catch (TaskCanceledException tce)
            {
                ProcessingEventBus.OnStatusUpdate(null, new ClientStatusEventArgs(ClientStatusType.Terminating, $"Terminating - {tce.Message}"));
            }
            catch (OperationCanceledException oce)
            {
                ProcessingEventBus.OnStatusUpdate(null, new ClientStatusEventArgs(ClientStatusType.Terminating, $"Terminating - {oce.Message}"));
            }
            finally
            {
                ProcessingEventBus.Dispose();
            }

            return SystemConstants.ExitSuccess;
        }

        /// <summary>
        /// Initializes and Register.
        /// </summary>
        private void InitAndRegister()
        {
            LoadSecrets(config);

            var serviceBuilder = RegisterSystemObjects();

            ValidateSettings(this.ServiceProvider);

            RegisterServices(serviceBuilder);

            RegisterCancellationTokensForServices();
        }

        /// <summary>
        /// Registers system objects.
        /// </summary>
        /// <returns>The Service Collection</returns>
        private ServiceCollection RegisterSystemObjects()
        {
            var serviceBuilder = new ServiceCollection();

            serviceBuilder
                .AddSingleton<Config>(config)
                .AddSingleton<ICosmosConfig>(provider => provider.GetRequiredService<Config>())
                .AddSingleton<CosmosDBSettings>(x => new CosmosDBSettings(x.GetRequiredService<ICosmosConfig>()))
                .AddSingleton<ICosmosDBSettings>(provider => provider.GetRequiredService<CosmosDBSettings>())
                .AddTransient<ISettingsValidator>(provider => provider.GetRequiredService<CosmosDBSettings>());

                // Add other System objects required during Constructor

            // We need to create service provider here since it utilized when Validating Settings
            ServiceProvider = serviceBuilder.BuildServiceProvider();
            return serviceBuilder;
        }

        /// <summary>
        /// Registers the services.
        /// </summary>
        /// <param name="serviceBuilder">The service builder.</param>
        /// <returns>The Service Collection</returns>
        /// <exception cref="System.Exception">serviceBuilder is null</exception>
        private ServiceCollection RegisterServices(ServiceCollection serviceBuilder)
        {
            if (serviceBuilder == null)
            {
                throw new Exception("serviceBuilder is null");
            }

            serviceBuilder

                // Add CosmosDB Repository
                .AddSingleton<CosmosDBRepository>()
                .AddSingleton<ICosmosDBRepository, CosmosDBRepository>(provider => provider.GetRequiredService<CosmosDBRepository>())

                // Add Services
                .AddSingleton<ClientStatusService>()
                .AddSingleton<IClientStatusService>(provider => provider.GetRequiredService<ClientStatusService>())

                .AddSingleton<LoadTestConfigService>()
                .AddSingleton<ILoadTestConfigService>(provider => provider.GetRequiredService<LoadTestConfigService>());

            // We build service provider here since new objects were added to the collection
            ServiceProvider = serviceBuilder.BuildServiceProvider();

            return serviceBuilder;
        }

        /// <summary>
        /// Registers the cancellation tokens for services.
        /// This method will allows to register multiple Cancellation tokens with different purposes for different Services.
        /// </summary>
        private void RegisterCancellationTokensForServices()
        {
            this.cancellationTokenSource.Token.Register(() =>
            {
                GetClientStatusService().TerminateService(this.clientStatus);
            });
        }
    }
}

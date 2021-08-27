// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ngsa.LodeRunner.DataAccessLayer;
using Ngsa.LodeRunner.DataAccessLayer.Interfaces;
using Ngsa.LodeRunner.DataAccessLayer.Model;
using Ngsa.LodeRunner.Events;
using Ngsa.LodeRunner.Interfaces;

namespace Ngsa.LodeRunner.Services
{
    /// <summary>
    /// Represents the LodeRunnerService and contains the main functionality of the class.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    /// <seealso cref="Ngsa.LodeRunner.Interfaces.ILodeRunnerService" />
    internal partial class LodeRunnerService : IDisposable, ILodeRunnerService
    {
        private readonly Config config;
        private ClientStatus clientStatus;
        private bool cosmosConfigInitialized = false;

        public LodeRunnerService(Config config)
        {
            this.config = config ?? throw new Exception("CommandOptions is null");
        }

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
                    return 1;
                }

                // task is completed
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nException:{ex.Message}");
                return 1;
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

        //TODO Move to proper location when merging with DAL
        public async void UpdateCosmosStatus(object sender, ClientStatusEventArgs args)
        {
            this.clientStatus = await GetClientStatusService().Post(args.Message, this.clientStatus, args.LastUpdated, args.Status).ConfigureAwait(false);
        }

        ////TEST only
        //public void TestServicesAndCosmosDBRepository()
        //{
        //    // ClientStatusService
        //    var clientStatus = GetClientStatusService().Get("aae1e564-9505-406d-939d-95d61b066c54").Result;

        //    var clientStatusList = GetClientStatusService().GetAll().Result;

        //    var clientStatusCount = GetClientStatusService().GetCount().Result;

        //    var mostRecentClientStatus = GetClientStatusService().GetMostRecent(5).Result;

        //    // LoadTestConfigService
        //    var loadTestList = GetLoadTestConfigService().GetAll().Result;

        //    var loadTest = GetLoadTestConfigService().Get("a0bfe57b-1535-4d19-aaac-477548ad913f").Result;
        //}

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
                await Task.Delay(config.DelayStart * 1000, App.TokenSource.Token).ConfigureAwait(false);
            }

            ValidationTest lrt = new (config);

            if (config.RunLoop)
            {
                // build and run the web host
                IHost host = App.BuildWebHost();
                _ = host.StartAsync(App.TokenSource.Token);

                // run in a loop
                int res = lrt.RunLoop(config, App.TokenSource.Token);

                // stop and dispose the web host
                await host.StopAsync(TimeSpan.FromMilliseconds(100)).ConfigureAwait(false);
                host.Dispose();

                //host = null; Remove unnecessary value assignment (IDE0059)

                return res;
            }
            else
            {
                // run one iteration
                return await lrt.RunOnce(config, App.TokenSource.Token).ConfigureAwait(false);
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
            InitializeCosmosConfiguration();

            // TestServicesAndCosmosDBRepository();

            ProcessingEventBus.StatusUpdate += UpdateCosmosStatus;
            ProcessingEventBus.StatusUpdate += LogStatusChange;

            ProcessingEventBus.OnStatusUpdate(null, new ClientStatusEventArgs(ClientStatusType.Starting, "Initializing - test init"));

            ProcessingEventBus.OnStatusUpdate(null, new ClientStatusEventArgs(ClientStatusType.Ready, "Ready - test ready"));
            try
            {
                // wait indefinitely
                await Task.Delay(config.DelayStart, App.TokenSource.Token).ConfigureAwait(false);
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

            return 1;
        }

        /// <summary>
        /// Initializes the cosmos configuration.
        /// </summary>
        private void InitializeCosmosConfiguration()
        {
            if (!cosmosConfigInitialized)
            {
                LoadSecrets(config);

                RegisterServices();

                ValidateSettings(this.ServiceProvider);

                cosmosConfigInitialized = true;
            }
        }

        /// <summary>
        /// Registers the services.
        /// </summary>
        private void RegisterServices()
        {
            var serviceBuilder = new ServiceCollection();

            serviceBuilder
                .AddSingleton<CosmosDBSettings>(x => new CosmosDBSettings()
                {
                    CollectionName = config.Secrets.CosmosCollection,
                    Retries = config.Retries,
                    Timeout = config.CosmosTimeout,
                    Uri = config.Secrets.CosmosServer,
                    Key = config.Secrets.CosmosKey,
                    DatabaseName = config.Secrets.CosmosDatabase,
                })
                .AddSingleton<ICosmosDBSettings>(provider => provider.GetRequiredService<CosmosDBSettings>())
                .AddTransient<ISettingsValidator>(provider => provider.GetRequiredService<CosmosDBSettings>())

                // Add CosmosDB Repository
                .AddSingleton<CosmosDBRepository>()
                .AddSingleton<ICosmosDBRepository, CosmosDBRepository>(provider => provider.GetRequiredService<CosmosDBRepository>())

                // Add Services
                .AddSingleton<ClientStatusService>()
                .AddSingleton<IClientStatusService>(provider => provider.GetRequiredService<ClientStatusService>())

                .AddSingleton<LoadTestConfigService>()
                .AddSingleton<ILoadTestConfigService>(provider => provider.GetRequiredService<LoadTestConfigService>());

            ServiceProvider = serviceBuilder.BuildServiceProvider();
        }
    }

    /// <summary>
    /// Implements the ILodeRunnerService class, fix StyleCopAnalyzer violation #SA1201
    /// </summary>
    internal partial class LodeRunnerService : ILodeRunnerService
    {
        public ServiceProvider ServiceProvider { get; private set; }
    }
}

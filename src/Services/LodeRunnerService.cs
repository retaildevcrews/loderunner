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
using Ngsa.LodeRunner.Interfaces;

namespace Ngsa.LodeRunner.Services
{
    /// <summary>
    /// Represents the LodeRunnerService and contains the main functionality of the class.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    /// <seealso cref="Ngsa.LodeRunner.Interfaces.ILodeRunnerService" />
    internal partial class LodeRunnerService : IDisposable
    {
        private readonly Config config;

        public LodeRunnerService(Config config)
        {
            this.config = config ?? throw new Exception("CommandOptions is null");
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

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

        private void LoadSecrets(Config config)
        {
            config.Secrets = Secrets.GetSecretsFromVolume(config.SecretsVolume);

            // set the Cosmos server name for logging
            config.CosmosName = config.Secrets.CosmosServer.Replace("https://", string.Empty, StringComparison.OrdinalIgnoreCase).Replace("http://", string.Empty, StringComparison.OrdinalIgnoreCase);

            int ndx = config.CosmosName.IndexOf('.', StringComparison.OrdinalIgnoreCase);

            if (ndx > 0)
            {
                config.CosmosName = config.CosmosName.Remove(ndx);
            }

            RegisterServices();

            ValidateSettings(this.ServiceProvider);
        }

        private void RegisterServices()
        {
            var serviceBuilder = new ServiceCollection();

            serviceBuilder
                .AddSingleton<ClientStatusRepositorySettings>(x => new ClientStatusRepositorySettings()
                {
                    CollectionName = config.Secrets.CosmosCollection,
                    Retries = config.Retries,
                    Timeout = config.CosmosTimeout,
                    Uri = config.Secrets.CosmosServer,
                    Key = config.Secrets.CosmosKey,
                    DatabaseName = config.Secrets.CosmosDatabase,
                })
                .AddTransient<ISettingsValidator>(provider => provider.GetRequiredService<ClientStatusRepositorySettings>())
                .AddSingleton<ClientStatusRepository>()
                .AddSingleton<IClientStatusRepository, ClientStatusRepository>(provider => provider.GetRequiredService<ClientStatusRepository>())
                .AddSingleton<ClientStatusService>()
                .AddSingleton<IClientStatusService>(provider => provider.GetRequiredService<ClientStatusService>());

            ServiceProvider = serviceBuilder.BuildServiceProvider();
        }

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

        private Task<int> StartDryRun()
        {
            return Task.Run(() => App.DoDryRun(config));
        }

        private async Task<int> StartAndWait()
        {
            LoadSecrets(config);

            Console.WriteLine($"Waiting indefinitely to start test ...\n");

            // **************** Testing Config update  only -- BEGIN ************************

            // Simulate to wait 10 secs then execute a test run

            //await Task.Delay(10000, App.TokenSource.Token).ConfigureAwait(false);

            //string[] argsConfig = "-s https://worka.aks-sb.com -f memory-baseline.json memory-baseline.json --delay-start 5".Split(' ').ToArray();

            //// Test run l8r from run config
            //await App.Main(argsConfig);

            //Console.WriteLine($"\nTest completed, waiting indefinitely to start test ...\n");

            // **************** for testing purpose only -- END ************************


            // ****************** testing Service pattern -- BEGIN**************************
            //await Task.Delay(3000, App.TokenSource.Token).ConfigureAwait(false);

            //await GetClientStatusService().PostStarting("Starting LodeRunner").ConfigureAwait(false);
            // ****************** testing Service pattern -- END **************************

            await Task.Delay(config.DelayStart, App.TokenSource.Token).ConfigureAwait(false);

            return 1;
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

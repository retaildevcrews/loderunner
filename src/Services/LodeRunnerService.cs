// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Ngsa.LodeRunner.DataAccessLayer;

namespace Ngsa.LodeRunner.Services
{
    internal class LodeRunnerService : IDisposable
    {
        private readonly Config config;
        public LodeRunnerService(Config config)
        {
            this.config = config ?? throw new Exception("CommandOptions is null");
        }

        public void Dispose()
        {
            //GC.SuppressFinalize(this);
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

            config.CosmosDalManager = new DalManager(config);

            //config.CosmosDalManager.ClientStatusService?.PostReady("ready");
        }

        private static async Task<int> TestRun(string runConfig)
        {
            // build the System.CommandLine.RootCommand
            RootCommand root = App.BuildRootCommand();
            root.Handler = CommandHandler.Create((Config cfg) => App.Run(cfg));

            string[] argsConfig = runConfig.Split(' ').ToArray();

            // run the command handler
            return await root.InvokeAsync(argsConfig).ConfigureAwait(false);
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

            // **************** for testing purpose only -- BEGIN ************************

            // Simulate to wait 10 secs then execute a test run

            await Task.Delay(10000, App.TokenSource.Token).ConfigureAwait(false);

            string runConfig = "-s https://worka.aks-sb.com -f memory-baseline.json memory-baseline.json --delay-start 5";

            // Test run l8r from run config
            await TestRun(runConfig);

            Console.WriteLine($"\nTest completed, waiting indefinitely to start test ...\n");

            // **************** for testing purpose only -- END ************************

            await Task.Delay(config.DelayStart, App.TokenSource.Token).ConfigureAwait(false);

            return 1;
        }
    }
}

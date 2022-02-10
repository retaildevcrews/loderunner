// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using LodeRunner.Core;
using LodeRunner.Core.CommandLine;
using LodeRunner.Core.Events;
using LodeRunner.Core.Extensions;
using LodeRunner.Core.Interfaces;
using LodeRunner.Core.Models;
using LodeRunner.Core.NgsaLogger;
using LodeRunner.Data;
using LodeRunner.Data.Interfaces;
using LodeRunner.Interfaces;
using LodeRunner.Services.Extensions;
using LodeRunner.Subscribers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LodeRunner.Services
{
    /// <summary>
    /// Represents the LodeRunnerService and contains the main functionality of the class.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    /// <seealso cref="LodeRunner.Interfaces.ILodeRunnerService" />
    public class LodeRunnerService : IDisposable, ILodeRunnerService
    {
        private readonly Config config;
        private readonly LoadClient loadClient;
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly ClientStatus clientStatus;
        private readonly ILogger logger;
        private System.Timers.Timer statusUpdateTimer = default;
        private object lastStatusSender = default;
        private ClientStatusEventArgs lastStatusArgs = default;
        private List<string> pendingTestRuns;

        /// <summary>
        /// Initializes a new instance of the <see cref="LodeRunnerService"/> class.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <param name="cancellationTokenSource">The cancellationTokenSource.</param>
        /// <param name="logger">The logger.</param>
        public LodeRunnerService(Config config, CancellationTokenSource cancellationTokenSource, ILogger<LodeRunnerService> logger)
        {
            Debug.WriteLine("* LodeRunnerService Constructor *");

            this.logger = logger;

            this.config = config ?? throw new Exception("CommandOptions is null");

            this.loadClient = LoadClient.GetNew(this.config, DateTime.UtcNow);

            this.clientStatus = new ClientStatus
            {
                Status = ClientStatusType.Starting,
                LoadClient = this.loadClient,
            };

            this.cancellationTokenSource = cancellationTokenSource;

            this.pendingTestRuns = new List<string>();
        }

        /// <summary>
        /// Gets the service provider.
        /// </summary>
        /// <value>
        /// The service provider.
        /// </value>
        public ServiceProvider ServiceProvider { get; private set; }

        /// <summary>
        /// Gets the ClientStatusId.
        /// </summary>
        /// <value>
        /// The Id.
        /// </value>
        public string ClientStatusId
        {
            get
            {
                return this.clientStatus.Id;
            }
        }

        /// <summary>
        /// Dispose.
        /// </summary>
        public void Dispose()
        {
            if (this.statusUpdateTimer != default(System.Timers.Timer))
            {
                this.statusUpdateTimer.Stop();
            }

            this.statusUpdateTimer = null;
            this.lastStatusSender = null;
            this.lastStatusArgs = null;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Starts the service and uses the Configuration to determine the Start mode.
        /// </summary>
        /// <returns>The Task with exit code.</returns>
        public async Task<int> StartService()
        {
            // set any missing values
            this.config.SetDefaultValues();

            // don't run the test on a dry run
            if (this.config.DryRun)
            {
                return await this.StartDryRun();
            }

            try
            {
                if (this.config.IsClientMode)
                {
                    return await this.StartAndWait();
                }
                else
                {
                    return await this.Start();
                }
            }
            catch (TaskCanceledException tce)
            {
                // log exception
                if (!tce.Task.IsCompleted)
                {
                    this.logger.LogError(new EventId((int)EventTypes.CommonEvents.Exception, nameof(StartService)), tce, "Exception");

                    return Core.SystemConstants.ExitFail;
                }

                // task is completed
                return Core.SystemConstants.ExitSuccess;
            }
            catch (Exception ex)
            {
                this.logger.LogError(new EventId((int)EventTypes.CommonEvents.Exception, nameof(StartService)), ex, "Exception");
                return Core.SystemConstants.ExitFail;
            }
        }

        /// <summary>
        /// Stop Service forcing cancellation and throw On First Exception equals to True.
        /// </summary>
        public void StopService()
        {
            this.cancellationTokenSource.Cancel(false);
        }

        /// <summary>
        /// Gets the client status service.
        /// </summary>
        /// <returns>IClientStatusService.</returns>
        public IClientStatusService GetClientStatusService()
        {
            return this.ServiceProvider.GetService<IClientStatusService>();
        }

        /// <summary>
        /// Gets the load test configuration service.
        /// </summary>
        /// <returns>ILoadTestConfigService.</returns>
        public ILoadTestConfigService GetLoadTestConfigService()
        {
            return this.ServiceProvider.GetService<ILoadTestConfigService>();
        }

        /// <summary>
        /// Gets the TestRun service.
        /// </summary>
        /// <returns>ITestRunConfigService.</returns>
        public ITestRunService GetTestRunService()
        {
            return this.ServiceProvider.GetService<ITestRunService>();
        }

        /// <summary>
        /// Logs the status change.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="ClientStatusEventArgs"/> instance containing the event data.</param>
        public void LogStatusChange(object sender, ClientStatusEventArgs args)
        {
            // TODO Move to proper location when merging with DAL

            Console.WriteLine($"{args.Message} - {args.LastUpdated:yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK}"); // TODO fix LogStatusChange implementation
        }

        /// <summary>
        /// Updates the TestRun with LoadResults.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="LoadResultEventArgs"/> instance containing the event data.</param>
        public async void UpdateTestRun(object sender, LoadResultEventArgs args)
        {
            // TODO: Define expected behavior and handle exceptions when cosmos update fails
            // get TestRun document to update
            var testRun = await GetTestRunService().Get(args.TestRunId);

            LoadResult loadResult = new ();
            loadResult.CompletedTime = args.CompletedTime;
            loadResult.FailedRequests = args.FailedRequests;
            loadResult.SuccessfulRequests = args.SuccessfulRequests;
            loadResult.TotalRequests = args.TotalRequests;
            loadResult.LoadClient = this.loadClient;
            loadResult.StartTime = args.StartTime;
            loadResult.ErrorMessage = args.ErrorMessage;

            testRun.ClientResults.Add(loadResult);

            // update TestRun CompletedTime if last client to report results
            if (testRun.ClientResults.Count == testRun.LoadClients.Count)
            {
                testRun.CompletedTime = args.CompletedTime;
            }

            // post updates
            _ = await GetTestRunService().Post(testRun, this.cancellationTokenSource.Token);

            // remove TestRun from pending list since upload is complete
            this.pendingTestRuns.Remove(testRun.Id);
        }

        /// <summary>
        /// Validates the settings.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <exception cref="System.ApplicationException">Failed to validate application configuration.</exception>
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
        /// Starts a loderunner instance.
        /// </summary>
        /// <returns>The Task with exit code.</returns>
        private async Task<int> Start()
        {
            if (this.config.DelayStart > 0)
            {
                Console.WriteLine($"Waiting {this.config.DelayStart} seconds to start test ...\n");

                // wait to start the test run
                await Task.Delay(this.config.DelayStart * 1000, this.cancellationTokenSource.Token).ConfigureAwait(false);
            }

            ValidationTest lrt = new (this.config, this.logger);

            if (this.config.RunLoop)
            {
                // build and run the web host
                IHost host = BuildWebHost(this.config, this.cancellationTokenSource);
                _ = host.StartAsync(this.cancellationTokenSource.Token);

                // run in a loop
                int res = lrt.RunLoop(this.config, this.cancellationTokenSource.Token);

                // stop and dispose the web host
                await host.StopAsync(TimeSpan.FromMilliseconds(100)).ConfigureAwait(false);
                host.Dispose();

                return res;
            }
            else
            {
                // run one iteration
                return await lrt.RunOnce(this.config, this.cancellationTokenSource.Token).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Starts a loderunner instance on dry run.
        /// </summary>
        /// <returns>The Task with exit code.</returns>
        private Task<int> StartDryRun()
        {
            return Task.Run(() => LRCommandLine.DoDryRun(this.config));
        }

        /// <summary>
        /// Starts a loderunner instance and wait to start test.
        /// </summary>
        /// <returns>The Task with exit code.</returns>
        private async Task<int> StartAndWait()
        {
            this.InitAndRegister();

            // Data connection not available yet, so we'll just update the stdout log
            ProcessingEventBus.StatusUpdate += this.LogStatusChange;
            this.StatusUpdate(this, new ClientStatusEventArgs(ClientStatusType.Starting, $"Initializing Client ({this.ClientStatusId})", this.cancellationTokenSource));

            // InitAndRegister() should have data connection available so we'll attach an event subscription to update the database with client status
            using var clientStatusUpdater = new ClientStatusUpdater(this.GetClientStatusService(), this.clientStatus);

            ProcessingEventBus.StatusUpdate += clientStatusUpdater.UpdateCosmosStatus;

            this.StatusUpdate(this, new ClientStatusEventArgs(ClientStatusType.Ready, $"Client Ready ({this.ClientStatusId})", this.cancellationTokenSource));

            ProcessingEventBus.TestRunComplete += this.UpdateTestRun;
            try
            {
                while (!this.cancellationTokenSource.Token.IsCancellationRequested)
                {
                    Console.WriteLine("Polling for available TestRuns");
                    var testRuns = await this.PollForTestRunsAsync();
                    if (testRuns != null && testRuns.Count > 0)
                    {
                        foreach (var testRun in testRuns)
                        {
                            // skip tests that have been completed but not yet updated with results in cosmos
                            if (!this.pendingTestRuns.Contains(testRun.Id))
                            {
                                // only execute TestRuns scheduled to run before the next minute
                                if (testRun.StartTime < DateTime.UtcNow.AddMinutes(1))
                                {
                                    this.StatusUpdate(null, new ClientStatusEventArgs(ClientStatusType.Testing, $"Received new TestRun ({testRun.Id})", this.cancellationTokenSource));
                                    await this.ExecuteNewTestRunAsync(testRun);
                                }
                            }
                        }

                        this.StatusUpdate(null, new ClientStatusEventArgs(ClientStatusType.Ready, $"Client Ready ({this.ClientStatusId})", this.cancellationTokenSource));
                    }

                    await Task.Delay(this.config.PollingInterval * 1000, this.cancellationTokenSource.Token);
                }
            }
            catch (TaskCanceledException tce)
            {
                this.StatusUpdate(this, new ClientStatusEventArgs(ClientStatusType.Terminating, $"Terminating Client ({this.ClientStatusId}) - {tce.Message}", this.cancellationTokenSource));
            }
            catch (OperationCanceledException oce)
            {
                this.StatusUpdate(this, new ClientStatusEventArgs(ClientStatusType.Terminating, $"Terminating Client ({this.ClientStatusId}) - {oce.Message}", this.cancellationTokenSource));
            }

            return Core.SystemConstants.ExitSuccess;
        }

        /// <summary>
        /// Initializes and Register.
        /// </summary>
        private void InitAndRegister()
        {
            Secrets.LoadSecrets(this.config);

            var serviceBuilder = this.RegisterSystemObjects();

            ValidateSettings(this.ServiceProvider);

            this.RegisterServices(serviceBuilder);

            this.RegisterCancellationTokensForServices();

            this.statusUpdateTimer = new ()
            {
                Interval = this.config.StatusUpdateInterval * 1000,
            };

            this.statusUpdateTimer.Elapsed += this.OnStatusTimerEvent;
        }

        /// <summary>
        /// Registers system objects.
        /// </summary>
        /// <returns>The Service Collection.</returns>
        private ServiceCollection RegisterSystemObjects()
        {
            var serviceBuilder = new ServiceCollection();

            serviceBuilder
                .AddSingleton<Config>(this.config)
                .AddSingleton<ICosmosConfig>(provider => provider.GetRequiredService<Config>())
                .AddSingleton<CosmosDBSettings>(x => new CosmosDBSettings(x.GetRequiredService<ICosmosConfig>()))
                .AddSingleton<ICosmosDBSettings>(provider => provider.GetRequiredService<CosmosDBSettings>())
                .AddTransient<ISettingsValidator>(provider => provider.GetRequiredService<CosmosDBSettings>());

                // Add other System objects required during Constructor

            // We need to create service provider here since it utilized when Validating Settings
            this.ServiceProvider = serviceBuilder.BuildServiceProvider();
            return serviceBuilder;
        }

        /// <summary>
        /// Registers the services.
        /// </summary>
        /// <param name="serviceBuilder">The service builder.</param>
        /// <returns>The Service Collection.</returns>
        /// <exception cref="System.Exception">serviceBuilder is null.</exception>
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
                .AddSingleton<ILoadTestConfigService>(provider => provider.GetRequiredService<LoadTestConfigService>())

                .AddSingleton<TestRunService>()
                .AddSingleton<ITestRunService>(provider => provider.GetRequiredService<TestRunService>());

            // We build service provider here since new objects were added to the collection
            this.ServiceProvider = serviceBuilder.BuildServiceProvider();

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
                this.GetClientStatusService().TerminateService(this.clientStatus);
            });
        }

        /// <summary>
        /// Called when [status timer event].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="ElapsedEventArgs"/> instance containing the event data.</param>
        private void OnStatusTimerEvent(object sender, ElapsedEventArgs args)
        {
            // Ensures that status remains valid and does not expire by updating time.
            // Status value and sender remain the same
            this.lastStatusArgs.LastUpdated = DateTime.UtcNow;
            this.StatusUpdate(this.lastStatusSender, this.lastStatusArgs);
        }

        /// <summary>
        /// Called when [status update].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="ClientStatusEventArgs"/> instance containing the event data.</param>
        private void StatusUpdate(object sender, ClientStatusEventArgs args)
        {
            this.lastStatusSender = sender;
            this.lastStatusArgs = args;

            this.statusUpdateTimer.Stop();
            ProcessingEventBus.OnStatusUpdate(sender, args);
            this.statusUpdateTimer.Start();
        }

        /// <summary>
        /// Polls for TestRuns available to the client.
        /// </summary>
        /// <returns>List of available TestRuns to execute.</returns>
        private async Task<List<TestRun>> PollForTestRunsAsync()
        {
            List<TestRun> testRuns = new ();
            try
            {
                var polledRuns = await GetTestRunService().GetNewTestRunsByClientId(this.ClientStatusId);
                foreach (var item in polledRuns)
                {
                    testRuns.Add((TestRun)item);
                }
            }
            catch (CosmosException ce)
            {
                // TODO: Update logging to handle appropriately
                Console.WriteLine($"CosmosException: {ce}");
            }
            catch (Exception ex)
            {
                // TODO: Update logging to handle appropriately
                Console.WriteLine($"Exception: {ex}");
            }

            return testRuns;
        }

        /// <summary>
        /// Executes the TestRun with a new lode runner instance in command mode.
        /// </summary>
        /// <param name="testRun">TestRun configuration to execute.</param>
        private async Task ExecuteNewTestRunAsync(TestRun testRun)
        {
            this.pendingTestRuns.Add(testRun.Id);
            this.StatusUpdate(null, new ClientStatusEventArgs(ClientStatusType.Testing, $"Executing TestRun ({testRun.Id})", this.cancellationTokenSource));

            // convert TestRun LoadTestConfig object to command line args
            string[] args = LoadTestConfigExtensions.GetArgs(testRun.LoadTestConfig);

            CancellationTokenSource cancel = new ();
            try
            {
                // TODO: Ensure all paths (i.e. with/without errors) with run loop and run once use UpdateTestRun event so cosmos
                // can be updated accordingly
                _ = await ClientModeExtensions.CreateAndStartLodeRunnerCommandMode(args, testRun.Id, cancel, (ILogger<LodeRunnerService>)this.logger);
            }
            catch (Exception ex)
            {
                // TODO: Handle specific exceptions (as needed)
                // TODO: Revisit how to use/where to raise the TestRunComplete event when the test run fails with an exception
                ProcessingEventBus.OnTestRunComplete(null, new LoadResultEventArgs(DateTime.UtcNow, DateTime.UtcNow, testRun.Id, 0, 0, ex.Message));
            }
        }

        /// <summary>
        /// Builds the web host for RunLoop.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <returns>The Host.</returns>
        private IHost BuildWebHost(Config config, CancellationTokenSource cancellationTokenSource)
        {
            int portNumber = AppConfigurationHelper.GetLoadRunnerPort(config.WebHostPort);

            // configure the web host builder
            return Host.CreateDefaultBuilder()
                        .ConfigureWebHostDefaults(webBuilder =>
                        {
                            webBuilder.ConfigureServices(services =>
                            {
                                services.AddSingleton<CancellationTokenSource>(cancellationTokenSource);
                                services.AddSingleton<ICosmosConfig>(provider => provider.GetRequiredService<Config>());
                            });
                            webBuilder.UseStartup<Startup>();
                            webBuilder.UseUrls($"http://*:{portNumber}/");
                        })
                        .ConfigureLogging(logger =>
                        {
                            logger.Setup(config, App.ProjectName);
                        })
                        .UseConsoleLifetime()
                        .Build();
        }
    }
}

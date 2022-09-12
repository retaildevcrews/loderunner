﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LodeRunner.API.Test.IntegrationTests
{
    /// <summary>
    /// Represents the Api Port Pool Manager class.
    /// </summary>
    internal class ApiPortPoolManager : IDisposable
    {
        private static readonly object GetInstanceLock = new();
        private static ApiPortPoolManager instance = null;

        private readonly int lowerPortRange;
        private readonly int upperPortRange;
        private readonly object getNextLock = new();
        private Dictionary<int, bool> portPool = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiPortPoolManager"/> class.
        /// </summary>
        /// <param name="lowerPortRange">The lower port range.</param>
        /// <param name="upperPortRange">The upper port range.</param>
        private ApiPortPoolManager(int lowerPortRange = 8085, int upperPortRange = 8099)
        {
            this.lowerPortRange = lowerPortRange;
            this.upperPortRange = upperPortRange;
            this.BuildPortPool();
        }

        /// <summary>
        /// Get the Singleton instace of ApiPortPoolManager.
        /// We utilized a lock to prevent that when different Integration Tests for TestRunExecution are running simultaneously create more that one ApiPortPoolManager instance when calling GetNextAvailablePort.
        /// </summary>
        /// <returns>ApiPortPoolManager Singleton Instance.</returns>
        public static ApiPortPoolManager GetInstance()
        {
            lock (GetInstanceLock)
            {
                instance ??= new ApiPortPoolManager();
            }

            return instance;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Gets the next available port.
        /// </summary>
        /// <returns>The next available port, 0 if no more ports available.</returns>
        public int GetNextAvailablePort()
        {
            int port = 0;
            lock (this.getNextLock)
            {
                port = this.portPool.FirstOrDefault(entry => EqualityComparer<bool>.Default.Equals(entry.Value, true)).Key;

                if (port != 0)
                {
                    // Set this port as not available
                    this.portPool[port] = false;
                }
            }

            return port;
        }

        /// <summary>
        /// Builds the port pool.
        /// </summary>
        private void BuildPortPool()
        {
            if (this.portPool == null)
            {
                this.portPool = new();
                for (int i = this.lowerPortRange; i <= this.upperPortRange; i++)
                {
                    this.portPool.Add(i, true);
                }
            }
        }
    }
}

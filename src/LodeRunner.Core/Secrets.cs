// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using LodeRunner.Core.Interfaces;

namespace LodeRunner.Core
{
    /// <summary>
    /// Implements Database Secrets.
    /// </summary>
    /// <seealso cref="LodeRunner.Core.Interfaces.ISecrets" />
    public class Secrets : ISecrets
    {
        /// <summary>
        /// Gets the volume.
        /// </summary>
        /// <value>
        /// The volume.
        /// </value>
        public string Volume { get; private set; }

        /// <summary>
        /// Gets the cosmos server.
        /// </summary>
        /// <value>
        /// The cosmos server.
        /// </value>
        public string CosmosServer { get; private set; }

        /// <summary>
        /// Gets the cosmos key.
        /// </summary>
        /// <value>
        /// The cosmos key.
        /// </value>
        public string CosmosKey { get; private set; }

        /// <summary>
        /// Gets the cosmos database.
        /// </summary>
        /// <value>
        /// The cosmos database.
        /// </value>
        public string CosmosDatabase { get; private set; }

        /// <summary>
        /// Gets the cosmos collection.
        /// </summary>
        /// <value>
        /// The cosmos collection.
        /// </value>
        public string CosmosCollection { get; private set; }

        /// <summary>
        /// Get the secrets from the k8s volume.
        /// </summary>
        /// <param name="volume">k8s volume name.</param>
        /// <returns>Secrets or null.</returns>
        public static Secrets GetSecretsFromVolume(string volume)
        {
            if (string.IsNullOrWhiteSpace(volume))
            {
                throw new ArgumentNullException(nameof(volume));
            }

            // throw exception if volume doesn't exist
            if (!Directory.Exists(volume))
            {
                throw new Exception($"Volume '{volume}' does not exist");
            }

            // get secrets from volume
            Secrets sec = new ()
            {
                Volume = volume,
                CosmosCollection = GetSecretFromFile(volume, "CosmosCollection"),
                CosmosDatabase = GetSecretFromFile(volume, "CosmosDatabase"),
                CosmosKey = GetSecretFromFile(volume, "CosmosKey"),
                CosmosServer = GetSecretFromFile(volume, "CosmosUrl"),
            };

            ValidateSecrets(volume, sec);

            return sec;
        }

        // basic validation of Cosmos values
        private static void ValidateSecrets(string volume, Secrets sec)
        {
            if (sec == null)
            {
                throw new Exception($"Unable to read secrets from volume: {volume}");
            }

            if (string.IsNullOrWhiteSpace(sec.CosmosCollection))
            {
                throw new Exception($"CosmosCollection cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(sec.CosmosDatabase))
            {
                throw new Exception($"CosmosDatabase cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(sec.CosmosKey))
            {
                throw new Exception($"CosmosKey cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(sec.CosmosServer))
            {
                throw new Exception($"CosmosUrl cannot be empty");
            }

            if (!sec.CosmosServer.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                !sec.CosmosServer.Contains(".documents.azure.com", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception($"Invalid value for CosmosUrl: {sec.CosmosServer}");
            }

            if (sec.CosmosKey.Length < 64)
            {
                throw new Exception($"Invalid value for CosmosKey: {sec.CosmosKey}");
            }
        }

        // read a secret from a k8s volume
        private static string GetSecretFromFile(string volume, string key)
        {
            string val = string.Empty;

            if (File.Exists($"{volume}/{key}"))
            {
                val = File.ReadAllText($"{volume}/{key}").Trim();
            }

            return val;
        }
    }
}

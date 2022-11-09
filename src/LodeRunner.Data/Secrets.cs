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
        /// Loads the secrets.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public static void LoadSecrets(ICosmosConfig config)
        {
            config.Secrets = Secrets.GetSecretsFromVolume(config.SecretsVolume, skipCosmosKey: config.CosmosAuthType != CosmosAuthType.SecretKey);

            // set the Cosmos server name for logging
            config.CosmosName = config.Secrets.CosmosServer.Replace("https://", string.Empty, StringComparison.OrdinalIgnoreCase).Replace("http://", string.Empty, StringComparison.OrdinalIgnoreCase);
            int ndx = config.CosmosName.IndexOf('.', StringComparison.OrdinalIgnoreCase);
            if (ndx > 0)
            {
                config.CosmosName = config.CosmosName.Remove(ndx);
            }
        }

        /// <summary>
        /// Get the secrets from the k8s volume.
        /// </summary>
        /// <param name="volume">k8s volume name.</param>
        /// <param name="skipCosmosKey ">Skip reading and validating CosmosKey.</param>
        /// <returns>Secrets or null.</returns>
        private static Secrets GetSecretsFromVolume(string volume, bool skipCosmosKey = false)
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
            Secrets sec = new()
            {
                Volume = volume,
                CosmosCollection = GetSecretFromFile(volume, "CosmosCollection"),
                CosmosDatabase = GetSecretFromFile(volume, "CosmosDatabase"),
                CosmosServer = GetSecretFromFile(volume, "CosmosUrl"),
            };

            // Skip if we're using Managed Identity instead of CosmosKey
            if (!skipCosmosKey)
            {
                sec.CosmosKey = GetSecretFromFile(volume, "CosmosKey");
            }

            ValidateSecrets(volume, sec, skipCosmosKey);

            return sec;
        }

        // basic validation of Cosmos values
        private static void ValidateSecrets(string volume, Secrets sec, bool skipCosmosKeyValidation)
        {
            if (sec == null)
            {
                throw new Exception($"{SystemConstants.UnableToReadSecretsFromVolume} {volume}");
            }

            if (string.IsNullOrWhiteSpace(sec.CosmosCollection))
            {
                throw new Exception($"{SystemConstants.CosmosCollectionCannotBeEmpty}");
            }

            if (string.IsNullOrWhiteSpace(sec.CosmosDatabase))
            {
                throw new Exception($"{SystemConstants.CosmosDatabaseCannotBeEmpty}");
            }

            if (!skipCosmosKeyValidation && string.IsNullOrWhiteSpace(sec.CosmosKey))
            {
                throw new Exception($"{SystemConstants.CosmosKeyCannotBeEmpty}");
            }

            if (string.IsNullOrWhiteSpace(sec.CosmosServer))
            {
                throw new Exception($"{SystemConstants.CosmosUrlCannotBeEmpty}");
            }

            if (!sec.CosmosServer.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception($"{SystemConstants.InvalidCosmosUrl} {sec.CosmosServer}");
            }

            if (!skipCosmosKeyValidation && sec.CosmosKey.Length < 64)
            {
                throw new Exception($"{SystemConstants.InvalidCosmosKey} {sec.CosmosKey}");
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

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LodeRunner.Test.Common
{
    /// <summary>
    /// Represents secrets helper class.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    internal class SecretsHelper : IDisposable
    {
        private const string CosmosCollection = "CosmosCollection";
        private const string CosmosDatabase = "CosmosDatabase";
        private const string CosmosKey = "CosmosKey";
        private const string CosmosTestDatabase = "CosmosTestDatabase";
        private const string CosmosUrl = "CosmosUrl";

        private readonly string[] secretFiles = new string[] { CosmosCollection, CosmosDatabase, CosmosKey, CosmosTestDatabase, CosmosUrl };

        /// <summary>
        /// Creates the secrets that will pass Validation.
        /// </summary>
        public void CreateValidSecrets()
        {
            if (!Directory.Exists($"{Directory.GetCurrentDirectory()}/secrets"))
            {
                Directory.CreateDirectory($"{Directory.GetCurrentDirectory()}/secrets");
            }

            foreach (var filename in this.secretFiles)
            {
                using StreamWriter sw = File.AppendText($"{Directory.GetCurrentDirectory()}/secrets/{filename}");
                switch (filename)
                {
                    case CosmosUrl: sw.WriteLine("https://some-random-domain.documents.azure.com"); break;
                    case CosmosKey: sw.WriteLine(RandomString(64)); break;
                    default: sw.WriteLine(filename); break;
                }
            }
        }

        /// <summary>
        /// Creates the secrets that will not pass Validation.
        /// </summary>
        public void CreateEmptySecrets()
        {
            if (!Directory.Exists($"{Directory.GetCurrentDirectory()}/secrets"))
            {
                Directory.CreateDirectory($"{Directory.GetCurrentDirectory()}/secrets");
            }

            foreach (var filename in this.secretFiles)
            {
                using StreamWriter sw = File.CreateText($"{Directory.GetCurrentDirectory()}/secrets/{filename}");
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Dispose any objects here.
        }

        /// <summary>
        /// Deletes the secrets.
        /// </summary>
        internal static void DeleteSecrets()
        {
            Directory.Delete($"{Directory.GetCurrentDirectory()}/secrets", true);
        }

        /// <summary>
        /// Creates a random string.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <returns>Random string.</returns>
        private static string RandomString(int length)
        {
            Random random = new ();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}

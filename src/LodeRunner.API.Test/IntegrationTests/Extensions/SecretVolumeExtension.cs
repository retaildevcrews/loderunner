// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LodeRunner.API.Test.IntegrationTests.Extensions
{
    /// <summary>
    /// Secret Volume Extension methods.
    /// </summary>
    internal static class SecretVolumeExtension
    {
        /// <summary>
        /// Gets the secret volume.
        ///  Note that this logic is utilizing the existing directory convention from "Set Secrets" step in CI/CD.
        ///    - name: Set Secrets.
        ///         mkdir -p /tmp/secrets.
        /// </summary>
        /// <param name="volume">The volume.</param>
        /// <returns>correct secrete folder depending on the OS. Linux or any other.</returns>
        public static string GetSecretVolume(this string volume)
        {
            if (System.OperatingSystem.IsLinux())
            {
                return $"/tmp/secrets";
            }
            else
            {
                return volume;
            }
        }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace LodeRunner.Core.Extensions
{
    /// <summary>
    /// Provides Assembly Extension methods.
    /// </summary>
    public static class AssemblyExtensions
    {
        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <param name="entryAssembly">The entry assembly.</param>
        /// <returns>The Assembly version.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetVersion(this Assembly entryAssembly)
        {
            if (Attribute.GetCustomAttribute(entryAssembly, typeof(AssemblyInformationalVersionAttribute)) is AssemblyInformationalVersionAttribute ver)
            {
                return ver.InformationalVersion;
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <param name="entryAssembly">The entry assembly.</param>
        /// <returns>The Assembly Title.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetTitle(this Assembly entryAssembly)
        {
            if (Attribute.GetCustomAttribute(entryAssembly, typeof(AssemblyTitleAttribute)) is AssemblyTitleAttribute name)
            {
                return name.Title;
            }
            else
            {
                return string.Empty;
            }
        }
    }
}

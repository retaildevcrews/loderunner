// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LodeRunner.Core.Extensions
{
    /// <summary>
    /// Provides String Extension methods.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Get the Enum from string.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="value">The value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="useDefaultIfCannotConvert"> determines id Default Value will be use id in-string cannot be converted to Enum value.</param>
        /// <returns>The Enum type.</returns>
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">
        /// Cannot coerce {value} to {typeof(TEnum).Name}
        /// or
        /// Cannot coerce {value} to {typeof(TEnum).Name}.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TEnum As<TEnum>(this string value, TEnum? defaultValue = null, bool useDefaultIfCannotConvert = false)
            where TEnum : struct, Enum
        {
            if (string.IsNullOrEmpty(value))
            {
                if (defaultValue == null)
                {
                    throw new InvalidEnumArgumentException($"Cannot coerce {value} to {typeof(TEnum).Name}");
                }

                return defaultValue.Value;
            }

            if (Enum.TryParse<TEnum>(value, true, out var enumValue))
            {
                return enumValue;
            }
            else if (useDefaultIfCannotConvert)
            {
                return defaultValue.Value;
            }

            throw new InvalidEnumArgumentException($"Cannot coerce {value} to {typeof(TEnum).Name}");
        }
    }
}

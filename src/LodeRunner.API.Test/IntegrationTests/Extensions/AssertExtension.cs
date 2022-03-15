// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LodeRunner.API.Test.IntegrationTests.Extensions
{
    /// <summary>
    /// Represents additional functionality for XUnit framework.
    /// </summary>
    internal static class AssertExtension
    {
        /// <summary>
        /// Verifies that two objects are equal, using a default comparer.
        /// </summary>
        /// <typeparam name="T">The type of the objects to be compared.</typeparam>
        /// <param name="expected">The expected value.</param>
        /// <param name="actual">The value to be compared against.</param>
        /// <param name="userMessage">Message to show in the error.</param>
        /// <exception cref="EqualException">Thrown when the objects are not equal.</exception>
        public static void Equal<T>(T expected, T actual, string userMessage)
        {
            bool areEqual;

            if (expected == null || actual == null)
            {
                // If either null, equal only if both null
                areEqual = expected == null && actual == null;
            }
            else
            {
                // expected is not null - so safe to call .Equals()
                areEqual = expected.Equals(actual);
            }

            if (!areEqual)
            {
                throw new EqualException(expected, actual, userMessage);
            }
        }

        /// <summary>
        /// Verifies that two objects are equal, using a default comparer.
        /// </summary>
        /// <typeparam name="T">The type of the objects to be compared.</typeparam>
        /// <param name="expected">The expected value.</param>
        /// <param name="response">HttpResponse to get the actual StatusCode and response body from to display as user message.</param>
        /// <exception cref="EqualException">Thrown when the objects are not equal.</exception>
        public static void EqualResponseStatusCode(HttpStatusCode expected, HttpResponseMessage response)
        {
            string userMessage = response.Content.ReadAsStringAsync().Result;

            Equal(expected, response.StatusCode, userMessage);
        }

        /// <summary>
        /// Determines whether this instance contains the object.
        /// </summary>
        /// <typeparam name="T">The type of the objects to be compared.</typeparam>
        /// <param name="expected">The expected.</param>
        /// <param name="collection">The collection to be compared against.</param>
        /// <param name="userMessage">Message to show in the error.</param>
        public static void Contains<T>(T expected, IEnumerable<T> collection, string userMessage)
        {
            bool exists;

            if (expected == null || collection == null)
            {
                exists = false;
            }
            else
            {
                // expected is not null - so safe to call .Contains()
                exists = collection.Contains(expected);
            }

            if (!exists)
            {
                throw new ContainsException(expected, collection, userMessage);
            }
        }
    }
}

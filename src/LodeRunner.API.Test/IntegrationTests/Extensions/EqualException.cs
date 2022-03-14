// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace LodeRunner.API.Test.IntegrationTests.Extensions
{
    /// <summary>
    /// Exception class that allow display a user message when comparing Equals.
    /// </summary>
    /// <seealso cref="Xunit.Sdk.EqualException" />
    public class EqualException : Xunit.Sdk.EqualException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EqualException"/> class.
        /// </summary>
        /// <param name="expected">The expected.</param>
        /// <param name="actual">The actual.</param>
        /// <param name="userMessage">The user message.</param>
        public EqualException(object expected, object actual, string userMessage)
            : base(expected, actual)
        {
            this.UserMessage = userMessage;
        }

        /// <inheritdoc />
        public override string Message => $"{this.UserMessage}\n{base.Message}";
    }
}

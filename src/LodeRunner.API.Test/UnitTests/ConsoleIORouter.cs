// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LodeRunner.API.Test.UnitTests
{
    /// <summary>
    /// Represents the Console helper class.
    /// </summary>
    internal class ConsoleIORouter : IDisposable
    {
        private readonly StringWriter stringWriter = new ();

        private TextWriter savedOut;
        private TextWriter savedError;
        private bool reset;

        /// <summary>
        /// Resets the output.
        /// </summary>
        public void ResetOutput()
        {
            if (!this.reset)
            {
                Console.SetOut(this.savedOut);
                Console.SetError(this.savedError);
                this.reset = true;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.ResetOutput();

            this.stringWriter.Dispose();

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Redirects the output.
        /// </summary>
        public void RedirectOutput()
        {
            this.savedOut = Console.Out;
            this.savedError = Console.Error;

            Console.SetOut(this.stringWriter);
            Console.SetError(this.stringWriter);
        }

        /// <summary>
        /// Gets the output as string list.
        /// </summary>
        /// <param name="removeEmptyStrings">if set to <c>true</c> [remove empty strings from List].</param>
        /// <returns>the string list.</returns>
        internal List<string> GetOutputAsStringList(bool removeEmptyStrings = true)
        {
            string output = this.stringWriter.ToString();

            if (removeEmptyStrings)
            {
                List<string> result = output.Split(Environment.NewLine).ToList();

                return result.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
            }
            else
            {
                return output.Split(Environment.NewLine).ToList();
            }
        }
    }
}

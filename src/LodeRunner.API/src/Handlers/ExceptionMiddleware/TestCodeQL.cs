// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LodeRunner.API.Handlers.ExceptionMiddleware
{
    public class TestCodeQL
    {
        public string Password { get; set; } = "This is a security-event test";

        public void SetPassword(string pwd)
        {
            Password = pwd;
        }

        public string GetPassword()
        {
            return Password;
        }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Net;

namespace LodeRunner.API.Middleware
{
    public class ErrorResult
    {
        public string Message { get; set; }

        public HttpStatusCode Error { get; set; }
    }
}

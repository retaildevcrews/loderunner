// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Ngsa.LodeRunner.DataAccessLayer.Interfaces
{
    public interface ICosmosDBSettings
    {
        int Retries { get; }
        int Timeout { get; }
        string Uri { get; }
        string Key { get; }
        string DatabaseName { get; }
    }
}

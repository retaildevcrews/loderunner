# Data Dictionary for LodeRunner.API

![License](https://img.shields.io/badge/license-MIT-green.svg)

## 1. Introduction

This document is meant to list and define the entities in use in LodeRunner, LodeRunner.API, and LodeRunner.UI.

### 1.1 Primary Entities Used

Below are the primary types of data for the LodeRunner.API (LRAPI).  Those are as follows:

| Type Name       |  Description    |  Notes                             |     LodeRunner.API  | LodeRunner | LodeRunner.UI |
| :-------------- | :-------------- | :--------------------------------- | :----------| :----------| :----------|
| BaseEntity      | Used as the parent for the data classes  | | xxxx | xxxx | xxxx |
| ClientStatus    | This object is used to convey the state of any given LodeRunner client that is configured to use the same data store. | Status documents will be placed in the dabase by LodeRunner and read by LRAPI.  A TTL of **60 seconds will** be given to the records so that if the client doesn't regulary update status will not be visible to the LRAPI or the LodeRunner.UI (LRUI). | xRxx | CRUD | xRxx |
| LoadClient      | Information about the LodeRunner instance | | xRxx | CRUD | xRxx |
| LoadTestConfig  | Used to define the test execution context for the LodeRunner clients. | | CRUD | xxxx | CRUD |
| TestRun         | This is the point in time copy of a load test that serves as a historical record.  It will contain a LoadResults object and have a reference to it's original LoadTest. | | CRUD | xRUx | CRUD |
| LoadResult     | This is the summary information from each client of used in a TestRun and will be a member of TestRun || xRxD | CRUx | xRxD |
| LoadTestConfigPayload     | Used as the Load Test Configuration payload data sent to the server when you make an API request. || xxxx | xxxx | xxxx |
| TestRunPayload     | Used as the TestRun payload data sent to the server when you make an API request. || xxxx | xxxx | xxxx |

`Table 01: Primary LodeRunner Entities`

## 2. Entity Definitions

### 2.1 Base Entity Definition

This entity is the parent of several objects and defines common fields

#### BaseEntityModel

| Property        |      Type       | Description                        | Required | Notes      |
| :-------------- | :-------------- | :--------------------------------- | :--------| :----------|
| PartitionKey    |     String      | Calculated value used for CosmosDB to determine how to allocate and use partitions  | Yes | Initial implementation will use `EntityType` to keep all objects of a similar type in the same partition |
| EntityType      |     String      | Entity type used for filtering  | Yes | |
| Id              |   String   | GUID used to retrieve the object directly | Yes | |
| Name            |   String   | Friendly name so that users may more easily identify a given entity | No | |

`Table 02: Base Definition for Data Entities`

### 2.2 Load Client Information

#### 2.2.1 Example ClientStatus Flow

<!-- markdownlint-disable MD033 -->
<!-- couldn't get sizing to work in standard markdown -->
<img src="diagrams/out/ClientStatus Flow.svg" /> <!-- width="800" height="600"/> -->
<!-- ![ParsingController Sequence](images/sequence-ParsingController.png) -->

`Figure 01: LodeRunner Client Start-up Sequence`

#### 2.2.2 LoadClient

This is an object that represents an instance of LodeRunner and it's initial start-up configuration.

##### LoadClient

| Property        |      Type       | Description                        | Required | Notes      |
| :-------------- | :-------------- | :--------------------------------- | :------- | :--------- |
| PartitionKey    |     String      |                                    |  Yes     | In the current model this value will always be empty for LoadClient and not stored |
| EntityType      |     String      | Entity type used for filtering     |  Yes     |            |
| Version         |     String      | Version of LodeRunner being used   |  Yes     |            |
| Id              |     String      | Unique Id generated at start-up to differentiate clients located in the same Region and Zone | Yes | |
| Name            |     String      | Friendly name so that users may more easily identify a given LoadClient | No | |
| Region          |     String      | The region in which the client is deployed | Yes | |
| Zone            |     String      | The zone in which the client is deployed | Yes | |
| Prometheus      |     Boolean     | Indicates whether or not this instance of LodeRunner is providing Prometheus metrics | No | default is `false` |
| StartupArgs     |     String      | String of arguments passed to LodeRunner at start-up | Yes | |
| StartTime       |     DateTime    | The date and time this instance was started | Yes | |

`Table 03: Load Client Properties`

#### 2.2.3 ClientStatus Definition

This object is primarily for conveying the curent status, time of that status, and the `LoadClient` settings to consuming apps.  It inherits from `BaseEntity` and contains a `LoadClient` member.

##### ClientStatus

| Property        |    Type    | Description                        | Required | Notes      |
| :-------------- | :--------- | :--------------------------------- | :------- | :--------- |
| PartitionKey    |   String   | This value should be populated for `ClientStatus` objects and documents |  Yes | |
| EntityType      |   String   | Entity type used for filtering     |    Yes   | [`ClientStatus`, `LoadTestConfig`, `TestRun`] |
| Id              |   String   | GUID used to retrieve the object directly | Yes | |
| Name            |   String   | Friendly name so that users may more easily identify a given ClientStatus | No | |
| LastUpdated     |   DateTime | This shows the date and time the entity was last updated | Yes | |
| LastStatusChange   |     DateTime    | This shows the date and time the status was last change | Yes | |
| Status          |   String   | Current status of load client      |    Yes   | [`Unknown`, `Starting`, `Ready`, `Testing`, `Terminating`] |
| Message         |   String   | Additional information conveyed as part of the status update | No | |
| LoadClient      | `LoadClient` | A nested object holding the information about the particular client in this status message | Yes | |
| Ttl             |    Int     | Initial time to live in seconds | Yes |  [Expire data in Azure Cosmos DB with Time to Live Docs](https://docs.microsoft.com/en-us/azure/cosmos-db/sql/time-to-live) |

`Table 04: ClientStatus Properties`

### 2.3 Testing and Results Entities

These are used for configuring a testing scenario.  `LoadTestConfig` will contain the settings that dictate what will be tested, with which files, and in what manner.  `TestRun` is used to schedule work with load clients and contains a `LoadTestConfg` and a list of LoadClients to use.

#### 2.3.1 LoadTestConfig

| Property        |    Type    | Description             | Required  | Notes      |
| :-------------- | :--------- | :---------------------- | :-------- | :----------|
| PartitionKey    |   String   | This value should be populated for `LoadTestConfig` objects and documents | Yes | |
| EntityType      |   String   | Entity type used for filtering  | Yes | [`ClientStatus`, `LoadTestConfig`, `TestRun`] |
| Id              |   String   | GUID used to retrieve the object directly. | Yes | |
| Name            |   String   | Friendly name so that users may more easily identify configs | No | |
| Files           |  String[]  | List of files to test   |   Yes     | match `--files` CLI flag |
| StrictJson      |   Boolean  | Use strict json when parsing (default: `False`) | No | match to `--strict-json` CLI flag |
| BaseURL         |   String   | Base url for files (default is empty) | No | match to `--base-url` CLI flag |
| VerboseErrors   |   Boolean  | Displays validation error messages | No | match to `--verbose-errors` CLI flag |
| Randomize       |   Boolean  | Requires `RunLoop` to be true.  Dictates whether to process a load file top to bottom (default: `false`) or randomly | No | match to `--random` CLI flag |
| Timeout         |    Int     | Request timeout in seconds (default: 30) | No | match to `--timeout` CLI flag |
| Server          |  String[]  | Server(s) to test (default is empty) | Yes | match to `--server` CLI flag |
| Tag             |   String   | Tag for log | No | match to `--tag` CLI flag |
| Sleep           |   String   | Sleep (ms) between each request (default: 0) | No | match to `--sleep` CLI flag |
| RunLoop         |   Boolean  | Run test in an infinite loop (default: False) | No | match to `--run-loop` CLI flag |
| Duration        |    Int     | Test duration (seconds) requires `RunLoop=True` (default: 0) | No | match to `--duration` CLI flag |
| MaxErrors       |    Int     | Max validation errors (default: 10) | No | match to `--max-errors` CLI flag |
| DryRun          |   Boolean  | Validate the settings with the target clients (default `false`) | No | match to `--dry-run` CLI flag |

- Removed DelayStart property since StartTime covers intended use case (still flag for command mode).

`Table 05: LoadTestConfig Properties`

#### 2.3.2 LoadTestConfigPayload

This object is utilized as the Load Test Config payload data. It inherits from BasePayload that implements SetField method to support wire format for creating/updating Load Test Configs and helps to identify deltas during payload deserialization.

| Property        |    Type    | Description             | Required  | Notes      |
| :-------------- | :--------- | :---------------------- | :-------- | :----------|
| Name            |   String   | Friendly name so that users may more easily identify configs | No | |
| Files           |  String[]  | List of files to test   |   Yes     | match `--files` CLI flag |
| StrictJson      |   Boolean  | Use strict json when parsing (default: `False`) | No | match to `--strict-json` CLI flag |
| BaseURL         |   String   | Base url for files (default is empty) | No | match to `--base-url` CLI flag |
| VerboseErrors   |   Boolean  | Displays validation error messages | No | match to `--verbose-errors` CLI flag |
| Randomize       |   Boolean  | Requires `RunLoop` to be true.  Dictates whether to process a load file top to bottom (default: `false`) or randomly | No | match to `--random` CLI flag |
| Timeout         |    Int     | Request timeout in seconds (default: 30) | No | match to `--timeout` CLI flag |
| Server          |  String[]  | Server(s) to test (default is empty) | Yes | match to `--server` CLI flag |
| Tag             |   String   | Tag for log | No | match to `--tag` CLI flag |
| Sleep           |   String   | Sleep (ms) between each request (default: 0) | No | match to `--sleep` CLI flag |
| RunLoop         |   Boolean  | Run test in an infinite loop (default: False) | No | match to `--run-loop` CLI flag |
| Duration        |    Int     | Test duration (seconds) requires `RunLoop=True` (default: 0) | No | match to `--duration` CLI flag |
| MaxErrors       |    Int     | Max validation errors (default: 10) | No | match to `--max-errors` CLI flag |
| DelayStart      |    Int     | Delay test start.  Must be `-1` for LodeRunner as that sets LodeRunner into a wait mode | Yes | match to `--delay-start` CLI flag |
| DryRun          |   Boolean  | Validate the settings with the target clients (default `false`) | No | match to `--dry-run` CLI flag |

`Table 06: LoadTestConfigPayload Properties`

#### 2.3.3 TestRun Properties

| Property        |    Type        | Description             | Required  | Notes       |
| :-------------- | :------------- | :---------------------- | :-------- | :---------- |
| PartitionKey    | String         | This value should be populated for `TestRun` objects and documents | Yes | |
| EntityType      | String         | Entity type used for filtering  | Yes | [`ClientStatus`, `LoadTestConfig`, `TestRun`] |
| Id              | String         | GUID used to retrieve the object directly.  Each new run gets a new Id. | Yes | |
| Name            | String         | Friendly name so that users may more easily identify TestRuns | No | |
| LoadTestConfig  | LoadTestConfig | Contains a full copy of the `LoadTestConfig` object to use for the TestRun | Yes | |
| LoadClients     | LoadClient[]   | List of available load clients to use for the TestRun | Yes | Duplicate Load Clients Not Allowed |
| CreatedTime     | DateTime       | Time the TestRun was created | Yes | |
| StartTime       | DateTime       | When to start the TestRun (default current time) | Yes | Cannot occur before CreatedTime |
| CompletedTime   | DateTime       | Time at which all clients completed their executions and reported results | No | This will require the last listed LoadClient to finish the test execution to update the TestRun with its `CompletedTime`. Cannot occur before StartTime |
| ClientResults   |  LoadResult[]  | This is an array of the result output from each client | No | |

`Table 07: TestRun Properties`

#### 2.3.4 TestRunPayload

This object is utilized as the TestRun payload data. It inherits from BasePayload that implements SetField method to support wire format for creating/updating TestRuns and helps to identify deltas during payload deserialization.

| Property        |    Type        | Description             | Required  | Notes      |
| :-------------- | :------------- | :---------------------- | :-------- | :----------|
| Name            |   String   | Friendly name so that users may more easily identify TestRuns | No | |
| LoadTestConfig  | LoadTestConfig | Contains a full copy of the `LoadTestConfig` object to use for the TestRun | Yes | |
| LoadClients     | LoadClient[]   | List of available load clients to use for the TestRun | Yes | |
| CreatedTime     |   DateTime     | Time the TestRun was created | Yes | |
| StartTime       |   DateTime     | When to start the TestRun (default current time) | Yes | |
| CompletedTime   |   DateTime     | Time at which all clients completed their executions and reported results | No | This shouldn't be set while creation of TestRun, this will be used to update the Time only after all tests and clients complete exections |

`Table 08: TestRunPayload Properties`

#### 2.3.5 LoadResult

This entity is still TBD

| Property        |    Type        | Description             | Required  | Notes      |
| :-------------- | :------------- | :---------------------- | :-------- | :----------|
| LoadClient      | `LoadClient`   | A nested object holding the information about the particular client in this status message | Yes | |
| StartTime       |   DateTime     | When the TestRun actually started | Yes  | |
| CompletedTime   |   DateTime     | Time at which the LoadClient completed its execution and reported results | Yes |  |
| TotalRequests   |     Int        |                         | Yes | |
| SuccessfulRequests   |     Int        |                         | Yes | |
| FailedRequests  |     Int        |                         | Yes | |
| ErrorMessage    |   String       | Error message of exception (if thrown) during TestRun | No | |

`Table 09: LoadResult Properties`

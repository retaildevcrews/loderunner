# LodeRunner - A web request validation tool

> Loderunner (L8r) is an internal web request validation tool that we use to run end-to-end tests and long-running smoke tests.
> Loderunner uses both environment variables as well as command line options for configuration. Command flags take precedence over environment variables.

## Running and Debugging LodeRunner via Visual Studio 2019

1. Add CosmosDB secret key ([Instructions](../LodeRunner.Data/README.md#cosmosdb-key))

2. Allow access to CosmosDB through firewall ([Instructions](../LodeRunner.Data/README.md#cosmosdb-firewall-ip-ranges))
    > NOTE: Skip this step if using Cosmos DB Emulator.

To debug LodeRunner app, set command line arguments, otherwise it will only print out usage

1. Click 'Debug'->'LodeRunner Debug Properties'
2. In the 'Application arguments' box set the arguments. Here are some resources.
    - [Example Command Mode Arguments](#example-command-mode-arguments)
    - [Example Client Mode Arguments](#example-client-mode-arguments)
    - [Mode Argument Compatibility Table](#mode-and-argument-compatibility-table)
    - [Command Line Parameter Descriptions](#command-line-parameter-descriptions)
    - [RunLoop Mode Parameters](#runloop-mode-parameters)

## LodeRunner Modes

LodeRunner may be run in **Command** mode or **Client** mode.  If no `--mode` argument is passed then LodeRunner will default to **Command** mode. The argument looks as follows with the default value in bold if unspecified:

--mode=[**Command**|Client]

### Command Mode

**Command** mode is the traditional mode for LodeRunner and will execute based on the arguments passed to via the command line. The default mode processes the input file(s) in sequential order one time and exits. The "run loop" mode runs in a continuous loop until stopped or for the specified duration. Some environment variables and command flags are only valid if run loop is specified and L8r will exit and display usage information. Some parameters have different default values depending on the mode of execution.

#### Example Command Mode Arguments

```bash

-s https://[Testing Target URL] -f memory-baseline.json memory-benchmark.json --delay-start 5 --run-loop true --duration 180

```

Long Running Tests

```bash

# continuously send request every 15 seconds
--sleep 15000 --run-loop --server "https://DomainName" --files benchmark.json

```

Load Testing

```bash

# continuously run testing for 60 seconds
# write all results to console as json

--run-loop --verbose --duration 60

# continuously run testing for 60 seconds sending about 2 requests per second
--run-loop --verbose --duration 60 --sleep 500

```

### Client Mode

**Client** mode is a newly added mode in which LodeRunner will start and wait for a [TestRun](https://github.com/retaildevcrews/loderunner/blob/main/docs/DataDictionary.md#232-testrun) to be assigned for it to execute. The LodeRunner running in **Client** mode will then use the command line arguments specified in the **TestRun** to internally start a LodeRunner service to execute the test.

#### Polling for TestRuns

Once initialized, LodeRunner will continue to poll CosmosDB for available **TestRuns** at a configurable interval (10s default). LodeRunner loops through any available **TestRuns** and executes them in ascending order by StartTime. Because **TestRuns** can have a scheduled StartTime, a **TestRun** that has a StartTime later than 1 minute in the future will be skipped. The skipped **TestRun** will continue to be available in subsequent polling results and will be executed closer to the scheduled StartTime.

In order for a **TestRun** to be available for a LodeRunner client, it must:

- include the clientId in its list of LoadClients
- not have already been executed by the client

#### Executing TestRuns

To execute the TestRuns, the LodeRunner instance running in **Client** mode starts a new instance of LodeRunner running in **Command** mode. The **LoadTestConfig** specified in the **TestRun** is converted to command line arguments. Once a **TestRun** execution is complete, LodeRunner will update the **TestRun** document in CosmosDB with the summarized test results in a **LoadResult** object.

#### Logging Information

Log entries are written when LodeRunner Client is executing under the following conditions

- Scheduled Status Update, logs last Client Status with a frequency defined by `StatusUpdateInterval` (5 seconds)
- Event triggered Status Update, logs Client Status updated by a particular event type (`Starting`, `Ready`, `Testing`, `Terminating`)
  - Starting, when Initializing Client for the very first time.
  - Ready, when Client is ready to perform an action.
  - Testing, when Received or Executing a new TestRun.
  - Terminating, when Client is stopping.


#### Example Client Mode Arguments

TODO: Describe the flags for each one, explain that -s and -f are ignored in **Client** mode

```bash

--mode Client --secrets-volume secrets --prometheus --zone dev --region dev

```

## Mode and Argument Compatibility Table

The following table helps identify which flags are neeed for starting the initial process in each mode.

**Note:** Many flags are not supported for **Client** mode, but they are used within a **TestRun**.  **TestRuns** are equivalent to running LodeRunner in command mode.

Table legend:

- **O** - Optional parameter for the given mode
- **N** - Not supported for a given mode flag
- **R** - Required for for a given mode flag

|                   | Mode        |            |                                                                |
|-------------------|-------------|------------|----------------------------------------------------------------|
| **Argument**      | **Command** | **Client** | **Notes**                                                      |
| --version         | O           | N          | If passed all other parameters are ignored.                    |
| --help            | O           | N          | If passed all other parameters are ignored.                    |
| --dry-run         | O           | N          | Runs arguments through validation, but does not start the app. |
| --server          | R           | N          |                                                                |
| --files           | R           | N          |                                                                |
| --base-url        | O           | N          |                                                                |
| --delay-start     | O           | N          |                                                                |
| --secrets-volume  | N           | R          |                                                                |
| --max-errors      | O           | N          | Not supported when --run-loop is set.                          |
| --sleep           | O           | N          |                                                                |
| --strict-json     | O           | N          |                                                                |
| --summary-minutes | O           | N          |                                                                |
| --tag             | O           | O          |                                                                |
| --timeout         | O           | N          |                                                                |
| --verbose         | O           | N          |                                                                |
| --verbose-errors  | O           | N          |                                                                |
| --zone            | O           | R          | New default is "Unknown".                                      |
| --region          | O           | R          | New default is "Unknown".                                      |
| --run-loop        | O           | N          |                                                                |
| --prometheus      | O           | O          | Requires --run-loop in Command mode, but not in Client mode.   |
| --duration        | O           | N          | Requires --run-loop.                                           |
| --random          | O           | N          | Requires --run-loop.                                           |

### Command Line Parameter Descriptions


> NOTE: Command line arguments take precedence over environment variables


| Parameter (short flag) | Environment Variable | Argument Type(s) | Default | Description                              | Notes      |
|------------------------|----------------------|------------------|---------|------------------------------------------|------------|
| --base-url (-u)        | BASE_URL             | string           | null    | Base URL and optional path to the test files (http or https). |            |
| --delay-start          | DELAY_START          | positive integer | 0       |               |            |
| --dry-run              | N/A                  | bool             | false   | Validates arguments, but does not start the app. |            |
| --duration             | DURATION             | positive integer | 0       | Only valid if --run-loop is specified. Runs the test for specified number of seconds then exits. | Runs until OS signal when set to 0. |
| --files (-f)           | FILES                | file1 [file2 file3 ...]    | N/A   | One or more json test files   | Default test file location is the current directory. |
| --help (-h)            | N/A                  | none             | N/A     | Display LodeRunner command line options.  | If passed all other parameters are ignored. |
| --max-concurrent       | N/A                  | positive integer | 100     | Maximum concurrent requests.            |         |
| --max-errors           | MAX_ERRORS           | positive integer | 10      | End test after max-errors.              | If --max-errors is exceeded, Loderunner will exit with non-zero exit code. Default value is 0 when --run-loop flag is set to true. |
| --prometheus           | PROMETHEUS           | bool             | false   | Expose the /metrics end point for Prometheus. |            |
| --random               | RANDOM               | bool             | false   | Only valid if --run-loop is specified. Randomize requests when running the test. |            |
| --region               | REGION               | string           | "Unknown" | Deployment region for logging (user defined). |            |
| --run-loop (-r)        | RUN_LOOP             | bool             | false   | Runs the test in a continuous loop.              |            |
| --secrets-volume       | SECRETS_VOLUME       | string           | "secrets" | Secrets location (directory name).      | If --secrets-volume is set then secrets directory must exist.   |
| --server (-s)          | SERVER               | string1 [string2 string3 ...] | N/A   | Server URL(s) to test (i.e. `https://MyServerDomainName.com`). |            |
| --sleep (-l)           | SLEEP                | positive integer | 0    | Number of milliseconds to sleep between requests. | Default value is 1000 when --run-loop flag is set to true. |
| --strict-json (-j)     | STRICT_JSON          | bool             | false   | Use strict RFC rules when parsing json. | Json property names are case sensitive, exceptions will occur for trailing commas in json arrays and comments in json. |
| --summary-minutes      | SUMMARY-MINUTES      | positive integer | N/A     | Display summary results (minutes).   | Only valid if --run-loop is specified. Not implemented yet. |
| --tag                  | TAG                  | string           | null    |               |            |
| --timeout (-t)         | TIMEOUT              | int              | 30 seconds | HTTP request timeout in seconds.  |            |
| --verbose              | VERBOSE              | bool             | false   | Log 200 and 300 results as well as errors. | Not implemented yet. |
| --verbose-errors       | VERBOSE_ERRORS       | bool             | false   | Display validation error messages.   |            |
| --version              | N/A                  | none             | N/A     | Display LodeRunner version.               | If passed all other parameters are ignored. |
| --zone                 | ZONE                 | string           | "Unknown" | Deployment zone for logging (user defined). |            |


### Port configuration

- When LodeRunner needs a port to run on, it uses port 8080 by default.
- The port number can be updated by editing the corresponding appsettings file.
  - [`appsettings.Development.json`](../AppSettings/appsettings.Development.json).
  - [`appsettings.Production.json`](../AppSettings/appsettings.Production.json).

## Running as part of an CI-CD pipeline

Loderunner will return a non-zero exit code (fail) under the following conditions

- Error parsing the test files
- If an unhandled exception is thrown during a test
- StatusCode validation fails
- ContentType validation fails
- --max-errors is exceeded
  - To cause the test to fail on any validation error, set --max-errors 1 (default is 10)
- Any validation error on a test that has FailOnValidationError set to true
- Request timeout

## Debugging

> This may occur due to an expired *.cse.ms certificate

```bash

// Load Test Run: logging output
ErrorDetails":"Exception: The SSL connection could not be established, see inner exception."

```

## Contributing

This project welcomes contributions and suggestions. Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit [Microsoft Contributor License Agreement](https://cla.opensource.microsoft.com).

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Trademarks

This project may contain trademarks or logos for projects, products, or services.

Authorized use of Microsoft trademarks or logos is subject to and must follow [Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general).

Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.

Any use of third-party trademarks or logos are subject to those third-party's policies.

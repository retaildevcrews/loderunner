# NGSA Loderunner - A web request validation tool

Loderunner (L8r) is an internal web request validation tool that we use to run end-to-end tests and long-running smoke tests.

## Quick Start

L8r can be run as a docker container, since it is available at gitcr.io, most of its features and functionality are inherited from its parent code base [WebV](https://github.com/microsoft/webvalidate).

TODO - Add descriptive text

[Debugging with Visual Studio 2019](#running-and-debugging-loderunner-via-visual-studio-2019)

## Running as a docker container

```bash

# pull image from GitHub Container Repository 
docker pull ghcr.io/retaildevcrews/ngsa-lr:beta

```

Experiment with L8r

Run L8r with --help option, this should output command line options shown [below](#command-line-parameters)

```bash
docker run ghcr.io/retaildevcrews/ngsa-lr:beta --help

```

Run benchmark and baseline test files

```bash

# Replace domainname sever name with actual ngsa-app url 
docker run --rm ghcr.io/retaildevcrews/ngsa-lr:beta -s "https://DomainName" -f benchmark.json baseline.json

```

Loderunner uses both environment variables as well as command line options for configuration. Command flags take precedence over environment variables.

Loderunner works in two distinct modes. The default mode processes the input file(s) in sequential order one time and exits. The "run loop" mode runs in a continuous loop until stopped or for the specified duration. Some environment variables and command flags are only valid if run loop is specified and L8r will exit and display usage information. Some parameters have different default values depending on the mode of execution.

### Example Arguments for Long Running Tests

```bash
# continuously send request every 15 seconds

docker run --rm ghcr.io/retaildevcrews/ngsa-lr:beta --sleep 15000 --run-loop --server "https://DomainName" --files benchmark.json
```

### Example Arguments for Load Testing

```bash

# continuously run testing for 60 seconds
# write all results to console as json

--run-loop --verbose --duration 60

# continuously run testing for 60 seconds sending about 2 requests per second
--run-loop --verbose --duration 60 --sleep 500

```
## LodeRunner Modes

LodeRunner may be run in **Command** mode or **Client** mode.  If no `--mode` argument is passed then LodeRunner will default to **Command** mode. The argument looks as follows with the default value in bold if unspecified:

--mode=[**Command**|Client]

### Mode Definitions

- **Command** mode is the traditional mode for LodeRunner and will execute based on the arguments passed to via the command line.
- **Client** mode is a newly added mode in which LodeRunner will start and await for a [TestRun](https://github.com/retaildevcrews/loderunner/blob/main/docs/DataDictionary.md#232-testrun) to be assigned for it to execute.  A **TestRun** will contain setting including arguments normally passed to LodeRunner in **Command** mode which will be used to internally start a LodeRunner service to execute the test based on those arguments.

### Mode and Argument Compatibility Table

The following table helps identify which flags are neeed for starting the initial process in each mode.  

**Note:** Many flags are not supported for **client** mode, but they are used within a TestRun.  TestRuns are equivalent to running LodeRunner in client mode.

Table legend:

- **O** - Optional parameter for the given mode
- **N** - Not supported for a given mode flag
- **R** - Required for for a given mode flga

|                   | Mode    |        |                                                                |
|-------------------|---------|--------|----------------------------------------------------------------|
| **Argument**      | **Command** | **Client** | **Notes**                                                          |
| --version         | O           | O          | If passed all other parameters are ignored.            |
| --help            | O       | O      | If passed all other parameters are ignored.                    |
| --dry-run         | O       | O      | Runs arguments through validation, but does not start the app. |
| --server          | R       | N      |                                                                |
| --files           | R       | N      |                                                                |
| --base-url        | O       | N      |                                                                |
| --delay-start     | O       | N      |                                                                |
| --secrets-volume  | N       | R      |                                                                |
| --max-errors      | O       | N      |                                                                |
| --sleep           | O       | N      |                                                                |
| --stric-json      | O       | N      |                                                                |
| --summary-minutes | O       | N      |                                                                |
| --tag             | O       | O      |                                                                |
| --timeout         | O       | N      |                                                                |
| --verbose         | O       | N      |                                                                |
| --verbose-errors  | O       | N      |                                                                |
| --zone            | O       | R      | New default is "Unknown".                                      |
| --region          | O       | R      | New default is "Unknown".                                      |
| --run-loop        | O       | N      |                                                                |
| --prometheus      | O       | O      | Requires --run-loop in Command mode, but not in Client mode.   |
| --duration        | O       | N      | Requires --run-loop.                                           |
| --random          | O       | N      | Requires --run-loop.                                           |
| --sleep           | O       | N      | Requires --run-loop.                                           |

## Command Line Parameters

> Includes short flags and environment variable names where applicable.
>
> Command Line args take precedent over environment variables

- --version
  - other parameters are ignored
  - environment variables are ignored
- --help
  - -h
  - other parameters are ignored
  - environment variables are ignored
- --dry-run bool
  - -d
    - validate parameters but do not execute tests
- --server string1 [string2 string3]
  - -s
  - SERVER
    - server Url (i.e. `https://MyServerDomainName.com`)
- --files file1 [file2 file3 ...]
  - -f
  - FILES
    - one or more json test files
    - default location current directory
- --base-url string
  - -u
  - BASE_URL
    - base URL and optional path to the test files (http or https)
- --delay-start int
  - DELAY_START
    - delay starting the validation test for int seconds
    - default `0`
- --secrets-volume string
  - SECRETS_VOLUME
    - secrets location
    - if --secrets-volume is present then secrets directory must exist.
    - default `secrets`
- --max-errors int
  - MAX_ERRORS
    - end test after max-errors
    - if --max-errors is exceeded, Loderunner will exit with non-zero exit code
    - default `10`
- --region string
  - REGION
    - deployment Region for logging (user defined)
    - default: `unknown`
- --sleep int
  - -l
  - SLEEP
    - number of milliseconds to sleep between requests
    - default `0`
- --strict-json bool
  - -j
  - STRICT_JSON
    - use strict RFC rules when parsing json
    - json property names are case sensitive
    - exceptions will occur for
      - trailing commas in json arrays
      - comments in json
    - default `false`
- --summary-minutes
  - SUMMARY-MINUTES
    - Display summary results (minutes)  (requires --run-loop)
- --tag string
  - TAG
    - user defined tag to include in logs and App Insights
      - can be used to identify location, instance, etc.
- --timeout int
  - -t
  - TIMEOUT
    - HTTP request timeout in seconds
    - default `30 sec`
- --verbose bool
  - VERBOSE
    - log 200 and 300 results as well as errors
    - default `false`
- --verbose-errors bool
  - VERBOSE_ERRORS
    - display validation error messages
    - default `false`
- --zone string
  - ZONE
    - deployment Zone for logging (user defined)
    - default: `unknown`

### RunLoop Mode Parameters

- Some parameters are only valid if `--run-loop` is specified
- Some parameters have different defaults if `--run-loop` is specified

- --run-loop bool
  - -r
  - RUN_LOOP
    - runs the test in a continuous loop
- --duration int
  - DURATION
    - run test for duration seconds then exit
    - default `0 (run until OS signal)`
- --prometheus bool
  - PROMETHEUS
    - expose the /metrics end point for Prometheus
    - default: `false`
- --random bool
  - RANDOM
    - randomize requests
    - default `false`
- --sleep int
  - -l
  - SLEEP
    - number of milliseconds to sleep between requests
    - default `1000`

### Port configuration

- When LodeRunner needs a port to run on, it uses port 8080 by default.
- The port number can be updated by editing the corresponding appsettings file.
  - [`appsettings.Development.json`](../AppSettings/appsettings.Development.json).
  - [`appsettings.Production.json`](../AppSettings/appsettings.Production.json).

## Running and Debugging LodeRunner via Visual Studio 2019

In order to debug LodeRunner app we must first set some of the command line arguments needed in order for it to execute.  If the correct arguments aren't passed or no arguments it will only print out usage. To run LodeRunner as client awaiting commmands from the LodeRunner.API then we will need arguments such as:

1. Click 'Debug'->'LodeRunner Debug Properties'
2. In the 'Application arguments' box you may use one of these two sets of properties:

<!-- markdownlint-disable MD034 MD036 -->
**Client Mode**

   >--mode client --secrets-volume secrets --region centralus --zone azure --prometheus

OR

**Command Mode**

   >-s https://[Testing Target URL] -f memory-baseline.json memory-benchmark.json --delay-start 5 --run-loop true --duration 180
<!-- markdownlint-enable MD034 MD036 -->

TODO - Describe the flags for each one, explain that -s and -f are ignored in client mode
TODO - Create a legend for the `secrets` files

The project is already configured with most of the required files and information the the /secrets folder. The existing files are the `CosmosCollection`, `CosmosDatabase`, `CosmosTestDatabase`, and `CosmosURL` files.  However, in order to access the database located at the URL in `CosmosURL` we will need the `CosmosKey` file created and populated. That is not included in the project as it is a secret.  You may simply create a file named `CosmosKey` and populate it with a Read-Write key generated from CosmosDB.

The last thing you'll need to do in order to run the app against your CosmosDB instance is to allow access through the CosmosDB firewall.  Navigate to the `Settings` -> `Firewall and virtual networks` section of the Azure Portal CosmosDB Account UI.  Once there it should automatically have a link to [+ Add my current IP (your IP)](No-link).  Click that link in the portal and then click the Save button at the bottom of the screen.  Once clicked a notification should appear at the top stating "Updating Firewall configuration".  It will take a little time to update.  Once the firewall is updated you will be able to run the app.

TODO - Replace the Add IP instructions with CLI command if possible

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

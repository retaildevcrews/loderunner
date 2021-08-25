# NGSA Loderunner - A web request validation tool

Loderunner (L8r) is an internal web request validation tool that we use to run end-to-end tests and long-running smoke tests.

## Quick Start

L8r can be run as a docker container, since it is available at gitcr.io, most of its features and functionality are inherited from its parent code base [WebV](https://github.com/microsoft/webvalidate).

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
    - `required`
- --files file1 [file2 file3 ...]
  - -f
  - FILES
    - one or more json test files
    - default location current directory
    - `required`
- --base-url string
  - -u
  - BASE_URL
    - base URL and optional path to the test files (http or https)
- --delay-start int
  - DELAY_START
    - delay starting the validation test for int seconds
    - if --delay-start is equals to `-1` then --secrets-volume must be present and exist
    - if --delay-start is anything other than `-1` then secrets are ignored
    - if --delay-start is set to `-1` and --secrets-volume exists, Loderunner will start and await indefinitely to start test
    - default `0`
- --secrets-volume string
  - SECRETS_VOLUME
    - secrets location
    - if --secrets-volume is present then --delay-start must be equals to `-1`
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
    - default: `null`
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
    - default: `null`

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

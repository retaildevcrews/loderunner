# API Parameter Validation

The following documentation describes the types of Query String and route parameters available in the REST API. The categories are broken down by the [Clients API](#Clients-API), [Load Test Configs API](#Load-Test-Configs-API), and [Test Runs API](#Test-Runs-API).

## Overview

The following section describes the following interactions with the API; 'direct reads'. A direct read involves specifying an ID in the route path without Query String parameters.

### Direct Reads

- Valid single read returns `HTTP/200` with item and content-type of `application/json`
- Valid single read with no results returns `HTTP/404` not found and content-type of `text/plain`
- Invalid single read returns a `HTTP/400` error response with a `application/problem+json` content type

### Error Handling

The error handling details including the response to parameter or route path validation errors uses a combination of RFC 7807 and the Microsoft REST API guidelines and can be found on the [HttpErrorResponses](HttpErrorResponses.md) page.

## Clients API

### Clients Direct Read

This applies to the following API route:

- /api/clients/{clientStatusId}

|   Name            |  Description                 |  Type    |  Valid Input              |  Response Body      |  Notes  |
|   ----            |  -----------                 |  ----    |  -----------              |  -------------      |  -----  |
|   clientStatusId  |  Return a specific client    |  string  |  a non-empty GUID string  |  Single `Client`    |         |

## LoadTestConfigs API

### LoadTestConfigs Direct Read

This applies to the following API route:

- /api/loadtestconfigs/{loadTestConfigId}

|   Name            |  Description                             |  Type    |  Valid Input              |  Response Body            |  Notes  |
|   ----            |  -----------                             |  ----    |  -----------              |  -------------            |  -----  |
|   loadTestConfigId  |  Return a specific load test config    |  string  |  a non-empty GUID string  |  Single `LoadTestConfig`  |         |

## TestRuns API

### TestRuns Direct Read

This applies to the following API route:

- /api/testruns/{testRunId}

|   Name       |  Description                   |  Type    |  Valid Input              |  Response Body       |  Notes  |
|   ----       |  -----------                   |  ----    |  -----------              |  -------------       |  -----  |
|   testRunId  |  Return a specific test run    |  string  |  a non-empty GUID string  |  Single `TestRun`    |         |

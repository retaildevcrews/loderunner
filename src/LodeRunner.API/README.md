# LodeRunner.API

LodeRunner.API is intended to facility testing in controlled environments by adding the capability to update load test configs without restarting load clients.

## Running the API

1. Clone the Repo:
      `git clone https://github.com/retaildevcrews/loderunner.git`

2. Add CosmosDB secret key ([Instructions](../LodeRunner.Data/README.md#cosmosdb-key))

3. Allow access to CosmosDB through firewall ([Instructions](../LodeRunner.Data/README.md#cosmosdb-firewall-ip-ranges))
      > NOTE: Skip this step if using Cosmos DB Emulator.

4. Change into the API directory:
      `cd src/LodeRunner.API`

5. Run the Application
      `dotnet run`

You should see the following response:

```bash

Hosting environment: Development or Production
Content root path: /src/LodeRunner.API
Now listening on: http://[::]:8081
Application started. Press Ctrl+C to shut down.

```

### Port configuration

- LodeRunner.API runs on port 8081 by default.
- The port number can be updated by editing the corresponding appsettings file.
  - [`appsettings.Development.json`](../AppSettings/appsettings.Development.json).
  - [`appsettings.Production.json`](../AppSettings/appsettings.Production.json).

## Testing the API

```bash

# test using httpie (installed automatically in Codespaces)
http localhost:8081/version

# test using curl
curl localhost:8081/version

# testing Health Check -  IetfHealthCheck
curl --request GET http://localhost:8081/Healthz/ietf

      # Overall Response Status
            # The least successful status from Checks performed.

      # Check Response time
            # targetValue: The expected duration time (e.g 200ms)
            # observedValue: The actual duration time (ms)
            # CosmosTimeout: The configured CosmosDB timeout value (default 60 seconds)

      # Check Status
            # Pass, if observedValue is less or equal to targetValue
            # Warn, if observedValue is greater than targetValue and less than CosmosTimeout
            # Fail, if observedValue is greater than CosmosTimeout

```

Stop API application by typing Ctrl-C or the stop button if run via F5

## API Object Validation Flow

Upon requesting **LodeRunner.API** endpoint, ASP.NET validates payload uing `ComponentModel`, if Component Model attributes are present. Once the payload is verified, the controller will invoke object validation by using [ModelExtensions](src/Extensions/ModelExtensions.cs) class under **LodeRunner.API.Extensions**. This will call **LodeRunner.Core** to build `RootCommand` and use it to validate the payload against the passed args.

Object --> `ComponentModel` validation --> ModelExtensions Object validation --> Build `RootCommand` object --> Execute Command Line Parser Validator

## Expected API Responses

The expected responses and returned status codes for various types of HTTP requests to selected API paths are shown in the table below. The selected paths include:

* /api/clients (C)
* /api/loadtestconfigs (L)
* /api/testruns (T)

| Reason	| Response	| Status Code	| GET	| GETByID	| POST	| PUT	| DELETE 
| ---- | ---- | ---- | ---- | ---- | ---- | ---- | ---- |
| CosmosException |						
| |	Not Found |	404 |	C,L,T |	C,L,T |	L,T |	L,T	| L,T
| | Determined by CosmosException |	Varies (e.g., 408, 429)	| C,L,T	| C,L,T	| L,T	| L,T	| L,T
| Exception	
| |	Internal Server Error	| 500	| C,L,T	| C,L,T	| L,T	| L,T	| L,T
| Successful HTTP Method Execution							
| |	Created	| 201		| |	| L,T		
| |	Internal Server Error (no returned value) | 500 |	| | L,T	| L,T	| L,T
| |	No Content	| 204	| C,L,T		| |	| L,T	| L,T
| |	Not Found	| 404	|	| C,L,T	|	| L,T	| L,T
| |	OK	| 200	| C,L,T	| C,L,T	|	|	| L,T
| Failed Pre-Execution Entity Id Validation						
| |	Bad Request	| 400	|	| C,L,T	|	| L,T	| L,T
| Failed Pre-Execution Entity Validation							
| | Bad Request	| 400	| 	| |	L,T	| L,T	
| Failed Pre-Execution Condition							
| | Conflict	| 409	| | | |	| T
| | Not Found	| 404	| | | | | T


## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us the rights to use your contribution. For details, visit <https://cla.opensource.microsoft.com>

When you submit a pull request, a CLA bot will automatically determine whether you need to provide a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Trademarks

This project may contain trademarks or logos for projects, products, or services.

Authorized use of Microsoft trademarks or logos is subject to and must follow [Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general).

Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.

Any use of third-party trademarks or logos are subject to those third-party's policies.

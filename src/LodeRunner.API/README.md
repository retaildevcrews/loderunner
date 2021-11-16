# LodeRunner.API

LodeRunner.API is intended to facility testing in controlled environments by adding the capability to update load test configs without restarting load clients.

## Running the API

1. Clone the Repo:
      `git clone https://github.com/retaildevcrews/loderunner.git`

2. Change into the API directory:
      `cd src/LodeRunner.API`

3. Run the Application
      `dotnet run`

You should see the following response:
> Hosting environment: Development or Production
Content root path: /src/LodeRunner.API
Now listening on: http://[::]:8081
Application started. Press Ctrl+C to shut down.

### Port configuration

- LodeRunner.API runs on port 8081 by default.
- The port number can be updated by editing the corresponding appsettings file.
  - [`appsettings.Development.json`](../AppSettings/appsettings.Development.json).
  - [`appsettings.Production.json`](../AppSettings/appsettings.Production.json).

## Testing the API

```bash
 
# test using httpie (installed automatically in Codespaces)
http localhost:8081/api/version
 
# test using curl
curl localhost:8081/api/version

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

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us the rights to use your contribution. For details, visit <https://cla.opensource.microsoft.com>

When you submit a pull request, a CLA bot will automatically determine whether you need to provide a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Trademarks

This project may contain trademarks or logos for projects, products, or services.

Authorized use of Microsoft trademarks or logos is subject to and must follow [Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general).

Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.

Any use of third-party trademarks or logos are subject to those third-party's policies.

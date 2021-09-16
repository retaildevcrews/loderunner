# RelayRunner

RelayRunner is intended to facility testing in controlled environments by adding the capability to update load test configs without restarting load clients.

## Running the Backend Application

1. Clone the Repo:
      `git clone https://github.com/retaildevcrews/relayrunner.git`

2. Change into the backend directory:
      `cd backend`

3. Run the Application
      `dotnet run`

You should see the following response:
> Hosting environment: Production
Content root path: /workspaces/relayrunner/backend
Now listening on: http://[::]:8080
Application started. Press Ctrl+C to shut down.

## Testing the Backend Application

```bash
 
# test using httpie (installed automatically in Codespaces)
http localhost:8080/api/version
 
# test using curl
curl localhost:8080/api/version
 
```

Stop RelayRunner by typing Ctrl-C or the stop button if run via F5

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us the rights to use your contribution. For details, visit <https://cla.opensource.microsoft.com>

When you submit a pull request, a CLA bot will automatically determine whether you need to provide a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Trademarks

This project may contain trademarks or logos for projects, products, or services.

Authorized use of Microsoft trademarks or logos is subject to and must follow [Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general).

Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.

Any use of third-party trademarks or logos are subject to those third-party's policies.

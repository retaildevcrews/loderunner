# RelayRunner

RelayRunner is intended to facility testing in controlled environments by adding the capability to update load test configs without restarting load clients.

## Client Application Scripts

In the project directory, you can run:

- `npm clean-install`: Installs npm dependecies.

- `npm start` : Runs the app in development mode

- `npm run lint` : Runs Linter and automatically fixes Linter recommended changes

- `npm test` : Launches the test runner, jest, in the interactive watch mode.

- `npm run build` : Builds the app for production to the `build` folder.

- `npm run eject` : Removes the single build dependency from project and copies all the configuration files and the transitive dependencies (webpack, Babel, ESLint, etc) right into your project so you have full control over them.
  - *Note: this is a one-way operation. Once you `eject`, you can’t go back!*

## Running the Client Application Locally

1. Clone the Repo `git clone https://github.com/retaildevcrews/relayrunner.git`
2. Change into the relayrunner directory `cd relayrunner`
3. Start the k3d cluster `make create`
4. Deploy a pod with relayrunner-backend `make rrapi`
5. Change into the client directory `cd client`
6. Install node dependencies `npm clean-install`
7. Start the client `npm start`

## Testing the Client Application

Run tests on Client components and functions using `npm test`

## Serve the Client Application on NGINX

```bash
// Use k3d to create a cluster
make create
// Build the client in production mode and serve on a pod running nginx
make rrui
```

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us the rights to use your contribution. For details, visit <https://cla.opensource.microsoft.com>

When you submit a pull request, a CLA bot will automatically determine whether you need to provide a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Trademarks

This project may contain trademarks or logos for projects, products, or services.

Authorized use of Microsoft trademarks or logos is subject to and must follow [Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general).

Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.

Any use of third-party trademarks or logos are subject to those third-party's policies.

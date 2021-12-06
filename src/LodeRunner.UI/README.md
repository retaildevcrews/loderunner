# LodeRunner.UI

LodeRunner.UI is intended to facility testing in controlled environments by adding the capability to update load test configs without restarting load clients.

## Development Experience

### Initial Setup

1. Setup LodeRunner and LodeRunner.API pods ([Instructions](../../README.md#running-the-system-via-codespaces))
2. Set `src/LodeRunner.UI/.env.development` REACT_APP_SERVER to LodeRunner.API URL
   - To prevent accidental commits `git update-index --assume-unchanged .env.development`
3. Change into the LodeRunner.UI directory `cd src/LodeRunner.UI`
4. Install node dependencies in `npm install`
5. Start the client `npm start`

### Re-run

1. Verify in loderunner directory
2. Set environmental variables for K8S generic secret via `source ~/.lr.env`
3. Start the k3d cluster `make create`
4. Deploy LodeRunner, LodeRunner.API, LodeRunner.UI `make lr-local`
5. In ports, set LodeRunner.API port visibility to public
6. Change into the LodeRunner.UI directory `cd src/LodeRunner.UI`
7. Start the client `npm start`

## Testing the Client Application

Launch the test runner, jest, in the interactive watch mode: `npm test`

## Linters

- `npm run lint`: Runs ESLint and automatically fixes recommended changes
- `npm run lint-audit`: Runs ESLint and lists recommended changes
- `npm run format`: Runs Prettier and automatically fixes recommended changes

## Fix dependency vulnerabilities

1. Verify dependencies that require fixing
   - `npm audit --production`
   - Explanation: <https://github.com/facebook/create-react-app/issues/11174>
   - NOTE: Please do not move react-scripts to devDependencies as suggested in the above article. This will break the docker image build.
2. Automatically fix dependencies: `npm audit fix`

## Production Build

- `npm run build`: Builds app for production and saves in the `build` folder
- `npm run build-prod`: Builds app for production and saves in the `build` folder without ESLint errors

## Contributing

This project welcomes contributions and suggestions. Most contributions require you to agree to a Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us the rights to use your contribution. For details, visit <https://cla.opensource.microsoft.com>

When you submit a pull request, a CLA bot will automatically determine whether you need to provide a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Trademarks

This project may contain trademarks or logos for projects, products, or services.

Authorized use of Microsoft trademarks or logos is subject to and must follow [Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general).

Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.

Any use of third-party trademarks or logos are subject to those third-party's policies.

/* eslint-disable react/prop-types */
import ConfigPage  from "../ConfigPage";
import ResultPage from "../ResultPage";
import ResultsOverviewPage from "../ResultsOverviewPage";
import TestPage from "../TestPage";

const routes = {
  "/": () => <TestPage />,
  "/results": () => <ResultsOverviewPage />,
  "/results/:testRunId": ({ testRunId }) => (
    <ResultPage testRunId={testRunId} />
  ),
  "/configs/:configId": ({ configId }) => <ConfigPage configId={configId} />,
};

export default routes;

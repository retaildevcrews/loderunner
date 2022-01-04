/* eslint-disable react/prop-types */
import ResultPage from "../ResultPage";
import ResultsOverviewPage from "../ResultsOverviewPage";
import TestPage from "../TestPage";

const routes = {
  "/": () => <TestPage />,
  "/results": () => <ResultsOverviewPage />,
  "/results/:testRunId": ({ testRunId }) => (
    <ResultPage testRunId={testRunId} />
  ),
};

export default routes;

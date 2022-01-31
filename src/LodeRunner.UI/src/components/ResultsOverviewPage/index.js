import { useContext, useEffect, useState } from "react";
import { A } from "hookrouter";
import RefreshIcon from "../RefreshIcon";
import { AppContext } from "../../contexts";
import { getResults } from "../../services/results";
import getMMMDYYYYhmma from "../../utilities/datetime";
import { RESULT, TEST_RUN } from "../../models";
import "./styles.css";

const ResultsOverviewPage = () => {
  const [fetchResultsTrigger, setFetchResultsTrigger] = useState(0);
  const [results, setResults] = useState([]);
  const { setIsPending } = useContext(AppContext);

  useEffect(() => {
    setIsPending(true);
    getResults()
      .then((r) => setResults(r))
      .catch(() => setResults([]))
      .finally(() => setIsPending(false));
  }, [fetchResultsTrigger]);

  return (
    <div className="resultsoverview">
      <div className="page-header">
        <h1>
          <button
            type="button"
            aria-label="Refresh Load Test Results List"
            className="unset refresh"
            onClick={() => setFetchResultsTrigger(Date.now())}
          >
            <RefreshIcon height="0.8em" />
          </button>
          Test Run Overview
        </h1>
        <A href="/" className="unset navigation">
          Load Test Submission
        </A>
      </div>
      {results.map(
        ({
          [TEST_RUN.id]: testId,
          [TEST_RUN.name]: testName,
          [TEST_RUN.createdTime]: createdTime,
          [TEST_RUN.scheduledStartTime]: scheduledStartTime,
          [TEST_RUN.finalCompletionTime]: finalCompletionTime,
          [TEST_RUN.results]: clientResults,
        }) => {
          let [totalRequest, totalSuccessfulRequest, totalFailedRequest] =
            Array(3).fill("--");

          if (clientResults.length > 0) {
            [totalRequest, totalSuccessfulRequest, totalFailedRequest] =
              clientResults.reduce(
                (
                  [aggTotal, aggSuccess, aggFail],
                  {
                    [RESULT.requestCount]: total,
                    [RESULT.successfulRequestCount]: success,
                    [RESULT.failedRequestCount]: fail,
                  }
                ) => [aggTotal + total, aggSuccess + success, aggFail + fail],
                [0, 0, 0]
              );

            totalRequest = totalRequest.toString();
            totalSuccessfulRequest = totalSuccessfulRequest.toString();
            totalFailedRequest = totalFailedRequest.toString();
          }

          return (
            <A href={`/results/${testId}`} key={testId} className="unset card">
              <div>
                <div>
                  <span className="card-key">Name:</span> {testName}
                </div>
                <div>
                  <span className="card-key">Creation Time:</span>&nbsp;
                  {getMMMDYYYYhmma(createdTime)}
                </div>
                <div>
                  <span className="card-key">Scheduled Start Time:</span>&nbsp;
                  {getMMMDYYYYhmma(scheduledStartTime)}
                </div>
                <div>
                  <span className="card-key">Final Completion Time:</span>&nbsp;
                  {getMMMDYYYYhmma(finalCompletionTime) || "--"}
                </div>
              </div>
              <div className="resultsoverview-item-counts">
                <div>
                  <span className="card-key">Request Count:</span>&nbsp;
                  {totalRequest}
                </div>
                <div>
                  <span className="card-key">Successful Request Count:</span>
                  &nbsp;
                  {totalSuccessfulRequest}
                </div>
                <div>
                  <span className="card-key">Failed Request Count:</span>&nbsp;
                  {totalFailedRequest}
                </div>
              </div>
            </A>
          );
        }
      )}
    </div>
  );
};

export default ResultsOverviewPage;

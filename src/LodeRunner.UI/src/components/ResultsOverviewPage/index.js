import { useContext, useEffect, useState } from "react";
import { A } from "hookrouter";
import RefreshIcon from "../RefreshIcon";
import { AppContext } from "../../contexts";
import { getResults } from "../../services/results";
import getMMMDYYYYhmma from "../../utilities/datetime";
import { CLIENT, RESULT, TEST_RUN, CONFIG } from "../../models";
import "./styles.css";

const ResultsOverviewPage = () => {
  const [fetchResultsTrigger, setFetchResultsTrigger] = useState(0);
  const [results, setResults] = useState([]);
  const { setIsPending } = useContext(AppContext);

  useEffect(() => {
    setIsPending(true);
    getResults()
      .then((r) => setResults(r))
      .catch(() => {
        setResults([
          {
            [TEST_RUN.id]: "001",
            [TEST_RUN.name]: "Test Run 1",
            [TEST_RUN.config]: {
              [CONFIG.baseUrl]: "",
              [CONFIG.dryRun]: false,
              [CONFIG.duration]: 3000,
              [CONFIG.files]: ["file001", "file002"],
              [CONFIG.id]: "config-001",
              [CONFIG.name]: "Config 1",
              [CONFIG.maxErrors]: 10,
              [CONFIG.strictJson]: true,
              [CONFIG.verbose]: true,
              [CONFIG.verboseErrors]: true,
              [CONFIG.randomize]: false,
              [CONFIG.runLoop]: false,
              [CONFIG.servers]: ["server001", "server002"],
              [CONFIG.sleep]: 0,
              [CONFIG.tag]: "",
              [CONFIG.timeout]: 1000,
            },
            [RESULT.client]: {
              [CLIENT.loadClientId]: "001",
              [CLIENT.name]: "Load Client 1",
              [CLIENT.prometheus]: true,
              [CLIENT.region]: "dev",
              [CLIENT.tag]: "tag-001",
              [CLIENT.version]: "verion-001",
              [CLIENT.zone]: "dev",
            },
            [TEST_RUN.createdTime]: new Date("2021-01-01 9:00:00"),
            [TEST_RUN.scheduledStartTime]: new Date("2021-01-02 9:00:00"),
            [TEST_RUN.totalCompletionTime]: new Date("2021-01-02 10:30:00"),
            [RESULT.requestCount]: 100,
            [RESULT.successfulRequestCount]: 95,
            [RESULT.failedRequestCount]: 5,
          },
          {
            [TEST_RUN.id]: "002",
            [TEST_RUN.name]: "Test Run 2",
            [TEST_RUN.config]: {
              [CONFIG.baseUrl]: "base-url-002",
              [CONFIG.dryRun]: true,
              [CONFIG.duration]: 3000,
              [CONFIG.files]: ["file001", "file002"],
              [CONFIG.id]: "config-002",
              [CONFIG.name]: "Config 2",
              [CONFIG.maxErrors]: 10,
              [CONFIG.strictJson]: false,
              [CONFIG.verbose]: false,
              [CONFIG.verboseErrors]: false,
              [CONFIG.randomize]: true,
              [CONFIG.runLoop]: true,
              [CONFIG.servers]: ["server001", "server002"],
              [CONFIG.sleep]: 10,
              [CONFIG.tag]: "tag-002",
              [CONFIG.timeout]: 1000,
            },
            [RESULT.client]: {
              [CLIENT.loadClientId]: "002",
              [CLIENT.name]: "Load Client 2",
              [CLIENT.prometheus]: false,
              [CLIENT.region]: "dev",
              [CLIENT.tag]: "",
              [CLIENT.version]: "version-002",
              [CLIENT.zone]: "dev",
            },
            [TEST_RUN.createdTime]: new Date("2021-01-02 9:00:00"),
            [TEST_RUN.scheduledStartTime]: new Date("2021-01-03 9:00:00"),
            [TEST_RUN.totalCompletionTime]: new Date("2021-01-03 10:30:00"),
            [RESULT.requestCount]: 100,
            [RESULT.successfulRequestCount]: 95,
            [RESULT.failedRequestCount]: 5,
          },
        ]);
      })
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
          [TEST_RUN.createdTime]: testCreatedTime,
          [TEST_RUN.scheduledStartTime]: testScheduledStartTime,
          [TEST_RUN.totalCompletionTime]: testTotalCompletionTime,
          [RESULT.requestCount]: testRequestCount,
          [RESULT.successfulRequestCount]: testSuccessfulRequestCount,
          [RESULT.failedRequestCount]: testFailedRequestCount,
        }) => (
          <A href={`/results/${testId}`} key={testId} className="unset card">
            <div>
              <div>
                <span className="card-key">Name:</span> {testName}
              </div>
              <div>
                <span className="card-key">Creation Time:</span>&nbsp;
                {getMMMDYYYYhmma(testCreatedTime)}
              </div>
              <div>
                <span className="card-key">Scheduled Start Time:</span>&nbsp;
                {getMMMDYYYYhmma(testScheduledStartTime)}
              </div>
              <div>
                <span className="card-key">Completion Time:</span>&nbsp;
                {getMMMDYYYYhmma(testTotalCompletionTime)}
              </div>
            </div>
            <div>
              <span className="card-key">Request Count:</span>&nbsp;
              {testRequestCount}
            </div>
            <div>
              <span className="card-key">Successful Request Count:</span>&nbsp;
              {testSuccessfulRequestCount}
            </div>
            <div>
              <span className="card-key">Failed Request Count:</span>&nbsp;
              {testFailedRequestCount}
            </div>
          </A>
        )
      )}
    </div>
  );
};

export default ResultsOverviewPage;

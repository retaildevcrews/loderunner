import { useContext, useEffect, useState } from "react";
import PropTypes from "prop-types";
import { A } from "hookrouter";
import Modal from "../Modal";
import PencilIcon from "../PencilIcon";
import PendingFeature from "../PendingFeature";
import RefreshIcon from "../RefreshIcon";
import { AppContext } from "../../contexts";
import { getResultById } from "../../services/results";
import getMMMDYYYYhmma from "../../utilities/datetime";
import { CLIENT, RESULT, TEST_RUN, CONFIG } from "../../models";
import { MODAL_CONTENT } from "../../utilities/constants";
import "./styles.css";

const ResultPage = ({ testRunId }) => {
  const defaultResultValues = {
    [TEST_RUN.id]: testRunId,
    [TEST_RUN.name]: undefined,
    [TEST_RUN.config]: {
      [CONFIG.baseUrl]: undefined,
      [CONFIG.dryRun]: undefined,
      [CONFIG.duration]: undefined,
      [CONFIG.files]: [],
      [CONFIG.id]: undefined,
      [CONFIG.name]: undefined,
      [CONFIG.maxErrors]: undefined,
      [CONFIG.strictJson]: undefined,
      [CONFIG.verbose]: undefined,
      [CONFIG.verboseErrors]: undefined,
      [CONFIG.randomize]: undefined,
      [CONFIG.runLoop]: undefined,
      [CONFIG.servers]: [],
      [CONFIG.sleep]: undefined,
      [CONFIG.tag]: undefined,
      [CONFIG.timeout]: undefined,
    },
    [RESULT.client]: {
      [CLIENT.loadClientId]: undefined,
      [CLIENT.name]: undefined,
      [CLIENT.prometheus]: undefined,
      [CLIENT.region]: undefined,
      [CLIENT.tag]: undefined,
      [CLIENT.version]: undefined,
      [CLIENT.zone]: undefined,
    },
    [TEST_RUN.createdTime]: undefined,
    [TEST_RUN.scheduledStartTime]: undefined,
    [TEST_RUN.totalCompletionTime]: undefined,
    [RESULT.requestCount]: undefined,
    [RESULT.successfulRequestCount]: undefined,
    [RESULT.failedRequestCount]: undefined,
  };

  const [modalContent, setModalContent] = useState(MODAL_CONTENT.closed);
  const [result, setResult] = useState(defaultResultValues);
  const [fetchResultTrigger, setFetchResultTrigger] = useState(0);
  const { setIsPending } = useContext(AppContext);

  useEffect(() => {
    setIsPending(true);
    getResultById(testRunId)
      .then((r) => setResult(r))
      .catch(() => {
        if (testRunId === "001") {
          setResult({
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
              [CLIENT.startTime]: "2021-01-01 8:00:00",
              [CLIENT.tag]: "tag-001",
              [CLIENT.version]: "verion-001",
              [CLIENT.zone]: "dev",
            },
            [TEST_RUN.createdTime]: "2021-01-01 9:00:00",
            [TEST_RUN.scheduledStartTime]: "2021-01-02 9:00:00",
            [TEST_RUN.totalCompletionTime]: "2021-01-02 10:30:00",
            [RESULT.requestCount]: 100,
            [RESULT.successfulRequestCount]: 95,
            [RESULT.failedRequestCount]: 5,
          });
        } else if (testRunId === "002") {
          setResult({
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
              [CLIENT.startTime]: "2021-01-01 8:00:00",
              [CLIENT.tag]: "",
              [CLIENT.version]: "version-002",
              [CLIENT.zone]: "dev",
            },
            [TEST_RUN.createdTime]: "2021-01-02 9:00:00",
            [TEST_RUN.scheduledStartTime]: "2021-01-03 9:00:00",
            [TEST_RUN.totalCompletionTime]: "2021-01-03 10:30:00",
            [RESULT.requestCount]: 100,
            [RESULT.successfulRequestCount]: 95,
            [RESULT.failedRequestCount]: 5,
          });
        }
      })
      .finally(() => setIsPending(false));
  }, [fetchResultTrigger]);

  const {
    [TEST_RUN.id]: testId,
    [TEST_RUN.name]: testName,
    [TEST_RUN.config]: {
      [CONFIG.baseUrl]: configBaseUrl,
      [CONFIG.dryRun]: configDryRun,
      [CONFIG.duration]: configDuration,
      [CONFIG.files]: configFiles,
      [CONFIG.id]: configId,
      [CONFIG.name]: configName,
      [CONFIG.maxErrors]: configMaxErrors,
      [CONFIG.strictJson]: configStrictJson,
      [CONFIG.verbose]: configVerbose,
      [CONFIG.verboseErrors]: configVerboseErrors,
      [CONFIG.randomize]: configRandomize,
      [CONFIG.runLoop]: configRunLoop,
      [CONFIG.servers]: configServers,
      [CONFIG.sleep]: configSleep,
      [CONFIG.tag]: configTag,
      [CONFIG.timeout]: configTimeout,
    },
    [RESULT.client]: {
      [CLIENT.loadClientId]: clientId,
      [CLIENT.name]: clientName,
      [CLIENT.prometheus]: clientPrometheus,
      [CLIENT.region]: clientRegion,
      [CLIENT.startTime]: clientStartTime,
      [CLIENT.tag]: clientTag,
      [CLIENT.version]: clientVersion,
      [CLIENT.zone]: clientZone,
    },
    [TEST_RUN.createdTime]: testCreatedTime,
    [TEST_RUN.scheduledStartTime]: testScheduledStartTime,
    [TEST_RUN.totalCompletionTime]: testTotalCompletionTime,
    [RESULT.requestCount]: testRequestCount,
    [RESULT.successfulRequestCount]: testSuccessfulRequestCount,
    [RESULT.failedRequestCount]: testFailedRequestCount,
  } = result;

  return (
    <div className="result">
      {modalContent && (
        <Modal content={modalContent} setContent={setModalContent}>
          {modalContent === MODAL_CONTENT.pendingFeature && <PendingFeature />}
        </Modal>
      )}
      <div className="page-header">
        <h1>
          <button
            type="button"
            aria-label="Refresh Load Test Result"
            className="unset refresh"
            onClick={() => setFetchResultTrigger(Date.now())}
          >
            <RefreshIcon height="0.8em" />
          </button>
          Load Test
        </h1>
        <A href="/results" className="unset navigation">
          Test Run Overview
        </A>
      </div>
      <div className="result-section">
        <div className="result-item-name">
          Result Name: {testName} ({testId})
          <button
            type="button"
            className="unset"
            onClick={() => setModalContent(MODAL_CONTENT.pendingFeature)}
          >
            <PencilIcon width="1.7em" />
          </button>
        </div>
        <div>
          <div className="result-item">
            <span>Creation Time:</span>
            <span className="result-item-value">
              {testCreatedTime ? getMMMDYYYYhmma(testCreatedTime) : "--"}
            </span>
          </div>
          <div className="result-item">
            <span>Scheduled Start Time:</span>
            <span className="result-item-value">
              {testScheduledStartTime
                ? getMMMDYYYYhmma(testScheduledStartTime)
                : "--"}
            </span>
          </div>
          <div className="result-item">
            <span>Completion Time:</span>
            <span className="result-item-value">
              {testTotalCompletionTime
                ? getMMMDYYYYhmma(testTotalCompletionTime)
                : "--"}
            </span>
          </div>
        </div>
        <div>
          <div className="result-item">
            <span>Request Count:</span>
            <span className="result-item-value">
              {testRequestCount === undefined ? "--" : testRequestCount}
            </span>
          </div>
          <div className="result-item">
            <span>Successful Request Count:</span>
            <span className="result-item-value">
              {testSuccessfulRequestCount === undefined
                ? "--"
                : testSuccessfulRequestCount}
            </span>
          </div>
          <div className="result-item">
            <span>Failed Request Count:</span>
            <span className="result-item-value">
              {testFailedRequestCount === undefined
                ? "--"
                : testFailedRequestCount}
            </span>
          </div>
        </div>
      </div>
      <div className="result-section">
        <div className="result-item">
          <span>Client Name: </span>
          <span>{clientName || "--"}</span>
        </div>
        <div>
          <div className="result-item">
            <span>Region:</span>
            <span className="result-item-value">{clientRegion || "--"}</span>
          </div>
          <div className="result-item">
            <span>Zone:</span>
            <span className="result-item-value">{clientZone || "--"}</span>
          </div>
          <div className="result-item">
            <span>Prometheus Enabled:</span>
            <span className="result-item-value">
              {clientPrometheus === undefined
                ? "--"
                : clientPrometheus.toString()}
            </span>
          </div>
          <div className="result-item">
            <span>Log & App Insights Tag:</span>
            <span className="result-item-value">{clientTag || "--"}</span>
          </div>
        </div>
        <div>
          <div className="result-item">
            <span>Deployed:</span>
            <span className="result-item-value">
              {clientStartTime ? getMMMDYYYYhmma(clientStartTime) : "--"}
            </span>
          </div>
          <div className="result-item">
            <span>LodeRunner Version:</span>
            <span className="result-item-value">{clientVersion || "--"}</span>
          </div>
          <div className="result-item">
            <span>LodeRunner ID:</span>
            <span className="result-item-value">{clientId || "--"}</span>
          </div>
        </div>
      </div>
      <div className="result-section">
        <div
          title="User friendly name for config settings"
          className="result-item"
        >
          <span>Config Name: </span>
          <span>
            {configName || "--"} ({configId || "--"})
          </span>
        </div>
        <div title="Servers to test">
          Servers:
          <ul>
            {configServers.map((s, index) => (
              <li
                // eslint-disable-next-line react/no-array-index-key
                key={`${configId}-server-${index}`}
                className="result-item-value"
              >
                {s}
              </li>
            ))}
          </ul>
        </div>
        <div>
          <div
            title=" Use strict  rules when parsing json. JSON property names are case sensitive. Exceptions will occur for trailing commas and comments in JSON."
            className="result-item"
          >
            <span>Parse with Strict JSON:</span>
            <span className="result-item-value">
              {configStrictJson === undefined
                ? "--"
                : configStrictJson.toString()}
            </span>
          </div>
          <div title="Base URL for load test files" className="result-item">
            <span>Base URL:</span>
            <span className="result-item-value">{configBaseUrl || "--"}</span>
          </div>
          <div title="Load test file to test">
            Load Test Files:
            <ul>
              {configFiles.map((f, index) => (
                <li
                  // eslint-disable-next-line react/no-array-index-key
                  key={`${configId}-file-${index}`}
                  className="result-item-value"
                >
                  {f}
                </li>
              ))}
            </ul>
          </div>
        </div>
        <div>
          <div title="Add a tag to the log" className="result-item">
            <span>Tag:</span>
            <span className="result-item-value">{configTag || "--"}</span>
          </div>
          <div
            title="Validate settings with target clients without running load test"
            className="result-item"
          >
            <span>Dry Run:</span>
            <span className="result-item-value">
              {configDryRun === undefined ? "--" : configDryRun.toString()}
            </span>
          </div>
          <div
            title="Display 200 and 300 results as well as errors"
            className="result-item"
          >
            <span>Verbose:</span>
            <span className="result-item-value">
              {configVerbose === undefined ? "--" : configVerbose.toString()}
            </span>
          </div>
          <div
            title="Display validation error messages"
            className="result-item"
          >
            <span>Verbose Errors:</span>
            <span className="result-item-value">
              {configVerboseErrors === undefined
                ? "--"
                : configVerboseErrors.toString()}
            </span>
          </div>
        </div>
        <div>
          <div title="Test duration" className="result-item">
            <span>Duration:</span>
            <span className="result-item-value">
              {configDuration === undefined ? "--" : configDuration} s
            </span>
          </div>
          <div title="Request timeout" className="result-item">
            <span>Timeout:</span>
            <span className="result-item-value">
              {configTimeout === undefined ? "--" : configTimeout} s
            </span>
          </div>
          <div title="Sleep between each request" className="result-item">
            <span>Sleep:</span>
            <span className="result-item-value">
              {configSleep === undefined ? "--" : configSleep} ms
            </span>
          </div>
          <div title="Maximum validation errors" className="result-item">
            <span>Max Errors:</span>
            <span className="result-item-value">{configMaxErrors}</span>
          </div>
          <div title="Run test in an infinite loop" className="result-item">
            <span>Run Loop:</span>
            <span className="result-item-value">
              {configRunLoop === undefined ? "--" : configRunLoop.toString()}
            </span>
          </div>
          <div
            title="Processes load file randomly instead of from top to bottom"
            className="result-item"
          >
            <span>Randomize:</span>
            <span className="result-item-value">
              {configRandomize === undefined
                ? "--"
                : configRandomize.toString()}
            </span>
          </div>
        </div>
      </div>
    </div>
  );
};

ResultPage.propTypes = {
  testRunId: PropTypes.string.isRequired,
};

export default ResultPage;

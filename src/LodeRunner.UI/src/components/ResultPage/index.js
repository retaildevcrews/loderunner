import { useContext, useEffect, useState } from "react";
import PropTypes from "prop-types";
import { A } from "hookrouter";
import Modal from "../Modal";
import NotFound from "../NotFoundPage";
import PencilIcon from "../PencilIcon";
import PendingFeature from "../PendingFeature";
import RefreshIcon from "../RefreshIcon";
import { AppContext } from "../../contexts";
import { getResultById } from "../../services/results";
import getMMMDYYYYhmma from "../../utilities/datetime";
import { CONFIG, LOAD_CLIENT, RESULT, TEST_RUN } from "../../models";
import { MODAL_CONTENT } from "../../utilities/constants";
import "./styles.css";

const ResultPage = ({ testRunId }) => {
  const [modalContent, setModalContent] = useState(MODAL_CONTENT.closed);
  const [test, setTest] = useState();
  const [fetchTestTrigger, setFetchTestTrigger] = useState(0);
  const { setIsPending } = useContext(AppContext);

  useEffect(() => {
    setIsPending(true);
    getResultById(testRunId)
      .then((r) => setTest(r))
      .catch(() => setTest())
      .finally(() => setIsPending(false));
  }, [fetchTestTrigger]);

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
            onClick={() => setFetchTestTrigger(Date.now())}
          >
            <RefreshIcon height="0.8em" />
          </button>
          Load Test
        </h1>
        <A href="/results" className="unset navigation">
          Test Run Overview
        </A>
      </div>
      {test ? (
        <div>
          <div className="result-section">
            <div>
              <div>
                <span className="result-item-label">Load Test Name:</span>&nbsp;
                {test[TEST_RUN.name]}
                <button
                  type="button"
                  className="unset"
                  onClick={() => setModalContent(MODAL_CONTENT.pendingFeature)}
                >
                  <PencilIcon width="1.5em" />
                </button>
              </div>
              <div>
                <span className="result-item-label">Load Test ID:</span>&nbsp;
                {test[TEST_RUN.id]}
              </div>
            </div>
            <div>
              <div>
                <span className="result-item-label">Creation Time:</span>&nbsp;
                {getMMMDYYYYhmma(test[TEST_RUN.createdTime]) || "--"}
              </div>
              <div>
                <span className="result-item-label">Scheduled Start Time:</span>
                &nbsp;
                {getMMMDYYYYhmma(test[TEST_RUN.scheduledStartTime]) || "--"}
              </div>
              <div>
                <span className="result-item-label">
                  Overall Completion Time:
                </span>
                &nbsp;
                {getMMMDYYYYhmma(test[RESULT.completionTime]) || "--"}
              </div>
            </div>
          </div>
          <div className="result-section-loadclients">
            {test[TEST_RUN.clients].map(
              ({
                [LOAD_CLIENT.id]: clientId,
                [LOAD_CLIENT.name]: clientName,
                [LOAD_CLIENT.region]: clientRegion,
                [LOAD_CLIENT.zone]: clientZone,
                [LOAD_CLIENT.prometheus]: clientPrometheus,
                [LOAD_CLIENT.tag]: clientTag,
                [LOAD_CLIENT.startTime]: clientStart,
                [LOAD_CLIENT.version]: clientVersion,
              }) => (
                <div key={`${test[TEST_RUN.id]}-${clientId}`}>
                  <div>
                    <div>
                      <span className="result-item-label">
                        Load Client Name:
                      </span>
                      &nbsp;
                      {clientName || "--"}
                    </div>
                    <div>
                      <span className="result-item-label">Load Client ID:</span>
                      &nbsp;
                      {clientId || "--"}
                    </div>
                  </div>
                  <RequestCounts
                    counts={test[TEST_RUN.results].find(
                      (counts) =>
                        counts[RESULT.client][LOAD_CLIENT.id] === clientId
                    )}
                  />
                  <div>
                    <div>
                      <span className="result-item-label">Region:</span>&nbsp;
                      {clientRegion || "--"}
                    </div>
                    <div>
                      <span className="result-item-label">Zone:</span>&nbsp;
                      {clientZone || "--"}
                    </div>
                    <div>
                      <span className="result-item-label">
                        Prometheus Enabled:
                      </span>
                      &nbsp;
                      {clientPrometheus === undefined
                        ? "--"
                        : clientPrometheus.toString()}
                    </div>
                    <div>
                      <span className="result-item-label">
                        Log & App Insights Tag:
                      </span>
                      &nbsp;
                      {clientTag || "--"}
                    </div>
                  </div>
                  <div>
                    <div>
                      <span className="result-item-label">Deployed:</span>&nbsp;
                      {getMMMDYYYYhmma(clientStart) || "--"}
                    </div>
                    <div>
                      <span className="result-item-label">
                        LodeRunner Version:
                      </span>
                      &nbsp;
                      {clientVersion || "--"}
                    </div>
                  </div>
                </div>
              )
            )}
          </div>
          <div className="result-section">
            <div title="User friendly name for config settings">
              <div>
                <span className="result-item-label">Config Name:</span>&nbsp;
                {test[TEST_RUN.config][CONFIG.name] || "--"}
              </div>
              <div>
                <span className="result-item-label">Config ID:</span>&nbsp;
                {test[TEST_RUN.config][CONFIG.id] || "--"})
              </div>
            </div>
            <div title="Servers to test">
              <span className="result-item-label">Servers:</span>
              <ul>
                {test[TEST_RUN.config][CONFIG.servers].map((s, index) => (
                  <li
                    // eslint-disable-next-line react/no-array-index-key
                    key={`${test[TEST_RUN.config][CONFIG.id]}-server-${index}`}
                  >
                    {s}
                  </li>
                ))}
              </ul>
            </div>
            <div>
              <div title=" Use strict  rules when parsing json. JSON property names are case sensitive. Exceptions will occur for trailing commas and comments in JSON.">
                <span className="result-item-label">
                  Parse with Strict JSON:
                </span>
                &nbsp;
                {test[TEST_RUN.config][CONFIG.strictJson] === undefined
                  ? "--"
                  : test[TEST_RUN.config][CONFIG.strictJson].toString()}
              </div>
              <div title="Base URL for load test files">
                <span className="result-item-label">Base URL:</span>&nbsp;
                {test[TEST_RUN.config][CONFIG.baseUrl] || "--"}
              </div>
              <div title="Load test file to test">
                <span className="result-item-label">Load Test Files:</span>
                <ul>
                  {test[TEST_RUN.config][CONFIG.files].map((f, index) => (
                    <li
                      // eslint-disable-next-line react/no-array-index-key
                      key={`${test[TEST_RUN.config][CONFIG.id]}-file-${index}`}
                    >
                      {f}
                    </li>
                  ))}
                </ul>
              </div>
            </div>
            <div>
              <div title="Add a tag to the log">
                <span className="result-item-label">Tag:</span>&nbsp;
                {test[TEST_RUN.config][CONFIG.tag] || "--"}
              </div>
              <div title="Validate settings with target clients without running load test">
                <span className="result-item-label">Dry Run:</span>&nbsp;
                {test[TEST_RUN.config][CONFIG.dryRun] === undefined
                  ? "--"
                  : test[TEST_RUN.config][CONFIG.dryRun].toString()}
              </div>
              <div title="Display validation error messages">
                <span className="result-item-label">Verbose Errors:</span>&nbsp;
                {test[TEST_RUN.config][CONFIG.verboseErrors] === undefined
                  ? "--"
                  : test[TEST_RUN.config][CONFIG.verboseErrors].toString()}
              </div>
            </div>
            <div>
              <div title="Test duration">
                <span className="result-item-label">Duration:</span>&nbsp;
                {test[TEST_RUN.config][CONFIG.duration] === undefined
                  ? "--"
                  : test[TEST_RUN.config][CONFIG.duration]}
                &nbsp;s
              </div>
              <div title="Request timeout">
                <span className="result-item-label">Timeout:</span>&nbsp;
                {test[TEST_RUN.config][CONFIG.timeout] === undefined
                  ? "--"
                  : test[TEST_RUN.config][CONFIG.timeout]}
                &nbsp;s
              </div>
              <div title="Sleep between each request">
                <span className="result-item-label">Sleep:</span>&nbsp;
                {test[TEST_RUN.config][CONFIG.sleep] === undefined
                  ? "--"
                  : test[TEST_RUN.config][CONFIG.sleep]}
                &nbsp;ms
              </div>
              <div title="Maximum validation errors">
                <span className="result-item-label">Max Errors:</span>&nbsp;
                {test[TEST_RUN.config][CONFIG.maxErrors]}
              </div>
              <div title="Run test in an infinite loop">
                <span className="result-item-label">Run Loop:</span>&nbsp;
                {test[TEST_RUN.config][CONFIG.runLoop] === undefined
                  ? "--"
                  : test[TEST_RUN.config][CONFIG.runLoop].toString()}
              </div>
              <div title="Processes load file randomly instead of from top to bottom">
                <span className="result-item-label">Randomize:</span>&nbsp;
                {test[TEST_RUN.config][CONFIG.randomize] === undefined
                  ? "--"
                  : test[TEST_RUN.config][CONFIG.randomize].toString()}
              </div>
            </div>
          </div>
        </div>
      ) : (
        <NotFound />
      )}
    </div>
  );
};

ResultPage.propTypes = {
  testRunId: PropTypes.string.isRequired,
};

export default ResultPage;

const RequestCounts = ({ counts }) => {
  const {
    [RESULT.completionTime]: completionTime,
    [RESULT.requestCount]: requestCount,
    [RESULT.successfulRequestCount]: successCount,
    [RESULT.failedRequestCount]: failedCount,
  } = counts;

  return (
    <div>
      <div>
        <span className="result-item-label">Load Test Completion Time:</span>
        &nbsp;{getMMMDYYYYhmma(completionTime) || "--"}
      </div>
      <div>
        <span className="result-item-label">Total Requests:</span>&nbsp;
        {requestCount === undefined ? "--" : requestCount}
      </div>
      <div>
        <span className="result-item-label">Successful Requests:</span>&nbsp;
        {successCount === undefined ? "--" : successCount}
      </div>
      <div>
        <span className="result-item-label">Failed Requests:</span>&nbsp;
        {failedCount === undefined ? "--" : failedCount}
      </div>
    </div>
  );
};

RequestCounts.propTypes = {
  counts: PropTypes.shape({
    [RESULT.completionTime]: PropTypes.string,
    [RESULT.requestCount]: PropTypes.number,
    [RESULT.successfulRequestCount]: PropTypes.number,
    [RESULT.failedRequestCount]: PropTypes.number,
  }),
};

RequestCounts.defaultProps = {
  counts: {},
};

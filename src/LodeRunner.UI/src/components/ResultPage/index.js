import { useContext, useEffect, useState } from "react";
import PropTypes from "prop-types";
import { A } from "hookrouter";
import Modal from "../Modal";
import NotFound from "../NotFoundPage";
import PencilIcon from "../PencilIcon";
import PendingFeature from "../PendingFeature";
import RefreshIcon from "../RefreshIcon";
import { AppContext } from "../../contexts";
import { getTestRunById } from "../../services/testRuns";
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
    getTestRunById(testRunId)
      .then((r) => setTest(r))
      .catch(() => setTest())
      .finally(() => setIsPending(false));
    // eslint-disable-next-line
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
            <RefreshIcon height="0.9em" />
          </button>
          Load Test
        </h1>
        <A href="/results" className="unset navigation">
          Test Run Overview
        </A>
      </div>
      {test ? (
        <>
          <div className="result-section">
            <TestRun test={test} setModalContent={setModalContent} />
          </div>
          <div className="result-section">
            <Config test={test} />
          </div>
          <div className="result-section">
            {test[TEST_RUN.clients].map((client) => (
              <div
                key={`${test[TEST_RUN.id]}-${client[LOAD_CLIENT.id]}`}
                className="result-client"
              >
                <Client
                  client={client}
                  results={test[TEST_RUN.results].find(
                    (counts) =>
                      counts[RESULT.client][LOAD_CLIENT.id] ===
                      client[LOAD_CLIENT.id]
                  )}
                />
              </div>
            ))}
          </div>
        </>
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

const TableWrapper = ({ children }) => (
  <table>
    <tbody>{children}</tbody>
  </table>
);

TableWrapper.propTypes = {
  children: PropTypes.oneOfType([
    PropTypes.arrayOf(PropTypes.node),
    PropTypes.node,
  ]).isRequired,
};

const TestRun = ({ test, setModalContent }) => (
  <TableWrapper>
    <tr>
      <th>Test Run Name:</th>
      <td className="result-column-spaced">
        {test[TEST_RUN.name]}
        <button
          type="button"
          className="unset"
          onClick={() => setModalContent(MODAL_CONTENT.pendingFeature)}
        >
          <PencilIcon width="1em" />
        </button>
      </td>
      <th>Creation Time:</th>
      <td>{getMMMDYYYYhmma(test[TEST_RUN.createdTime]) || "--"}</td>
    </tr>
    <tr>
      <th>Test Run ID:</th>
      <td className="result-column-spaced">{test[TEST_RUN.id]}</td>
      <th>Scheduled Start Time:</th>
      <td>{getMMMDYYYYhmma(test[TEST_RUN.scheduledStartTime]) || "--"}</td>
    </tr>
    <tr>
      <td />
      <td />
      <th>Total Completion Time:</th>
      <td>{getMMMDYYYYhmma(test[TEST_RUN.finalCompletionTime]) || "--"}</td>
    </tr>
    {!test[TEST_RUN.finalCompletionTime] &&
      test[TEST_RUN.config][CONFIG.runLoop] && (
        <tr>
          <td />
          <td />
          <th>Duration:</th>
          <td>
            {test[TEST_RUN.config][CONFIG.duration] === 0
              ? "Infinite"
              : `${test[TEST_RUN.config][CONFIG.duration]} s`}
          </td>
        </tr>
      )}
  </TableWrapper>
);

TestRun.propTypes = {
  test: PropTypes.shape({
    [TEST_RUN.name]: PropTypes.string,
    [TEST_RUN.config]: PropTypes.shape({
      [CONFIG.runLoop]: PropTypes.bool,
      [CONFIG.duration]: PropTypes.number,
    }),
    [TEST_RUN.createdTime]: PropTypes.string,
    [TEST_RUN.scheduledStartTime]: PropTypes.string,
    [TEST_RUN.finalCompletionTime]: PropTypes.string,
  }).isRequired,
  setModalContent: PropTypes.func.isRequired,
};

const Config = ({ test }) => {
  let duration = "";

  if (test[TEST_RUN.config][CONFIG.duration] === undefined) {
    duration = "--";
  } else if (test[TEST_RUN.config][CONFIG.duration] === 0) {
    duration = "Infinite";
  } else {
    duration = `${test[TEST_RUN.config][CONFIG.duration]} s`;
  }

  return (
    <>
      <TableWrapper>
        <tr>
          <th>Config Name:</th>
          <td>{test[TEST_RUN.config][CONFIG.name] || "--"}</td>
        </tr>
        <tr>
          <th>Config ID:</th>
          <td>{test[TEST_RUN.config][CONFIG.id] || "--"}</td>
        </tr>
      </TableWrapper>
      <br />
      <TableWrapper>
        <tr>
          <th className="result-column-spaced">Servers:</th>
          <th>Load Test Files:</th>
        </tr>
        <tr>
          <td className="result-column-spaced">
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
          </td>
          <td>
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
          </td>
        </tr>
      </TableWrapper>
      <br />
      <TableWrapper>
        <tr>
          <td className="result-column-spaced">
            <span className="result-item-label">Run Loop:&nbsp;</span>
            {test[TEST_RUN.config][CONFIG.runLoop] === undefined
              ? "--"
              : test[TEST_RUN.config][CONFIG.runLoop].toString()}
          </td>
          <td className="result-column-spaced">
            <span className="result-item-label">Tag:&nbsp;</span>
            {test[TEST_RUN.config][CONFIG.tag] || "--"}
          </td>
          <td>
            <span className="result-item-label">Verbose Errors:&nbsp;</span>
            {test[TEST_RUN.config][CONFIG.verboseErrors] === undefined
              ? "--"
              : test[TEST_RUN.config][CONFIG.verboseErrors].toString()}
          </td>
        </tr>
        <tr>
          <td className="result-column-spaced">
            <span className="result-item-label">Duration:&nbsp;</span>
            {duration}
          </td>
          <td className="result-column-spaced">
            <span className="result-item-label">Sleep:&nbsp;</span>
            {test[TEST_RUN.config][CONFIG.sleep] === undefined
              ? "--"
              : test[TEST_RUN.config][CONFIG.sleep]}
            &nbsp;ms
          </td>
          <td>
            <span className="result-item-label">
              Parse with Strict JSON:&nbsp;
            </span>
            {test[TEST_RUN.config][CONFIG.strictJson] === undefined
              ? "--"
              : test[TEST_RUN.config][CONFIG.strictJson].toString()}
          </td>
        </tr>
        <tr>
          <td className="result-column-spaced">
            <span className="result-item-label">Max Errors: &nbsp;</span>
            {test[TEST_RUN.config][CONFIG.maxErrors] === undefined
              ? "--"
              : test[TEST_RUN.config][CONFIG.maxErrors]}
          </td>
          <td className="result-column-spaced">
            <span className="result-item-label">Randomize:&nbsp;</span>
            {test[TEST_RUN.config][CONFIG.randomize] === undefined
              ? "--"
              : test[TEST_RUN.config][CONFIG.randomize].toString()}
          </td>
          <td>
            <span className="result-item-label">Dry Run:&nbsp;</span>
            {test[TEST_RUN.config][CONFIG.dryRun] === undefined
              ? "--"
              : test[TEST_RUN.config][CONFIG.dryRun].toString()}
          </td>
        </tr>
        <tr>
          <td className="result-column-spaced">
            <span className="result-item-label">Timeout:&nbsp;</span>
            {test[TEST_RUN.config][CONFIG.timeout] === undefined
              ? "--"
              : test[TEST_RUN.config][CONFIG.timeout]}
            &nbsp;s
          </td>
          <td className="result-column-spaced">
            <span className="result-item-label">Base URL:&nbsp;</span>
            {test[TEST_RUN.config][CONFIG.baseUrl] || "--"}
          </td>
        </tr>
      </TableWrapper>
    </>
  );
};

Config.propTypes = {
  test: PropTypes.shape({
    [TEST_RUN.config]: PropTypes.shape({
      [CONFIG.name]: PropTypes.string,
      [CONFIG.id]: PropTypes.string,
      [CONFIG.files]: PropTypes.arrayOf(PropTypes.string),
      [CONFIG.runLoop]: PropTypes.bool,
      [CONFIG.tag]: PropTypes.string,
      [CONFIG.verboseErrors]: PropTypes.bool,
      [CONFIG.duration]: PropTypes.number,
      [CONFIG.sleep]: PropTypes.number,
      [CONFIG.strictJson]: PropTypes.bool,
    }).isRequired,
  }).isRequired,
};

const Client = ({
  client: {
    [LOAD_CLIENT.id]: clientId,
    [LOAD_CLIENT.name]: clientName,
    [LOAD_CLIENT.region]: clientRegion,
    [LOAD_CLIENT.zone]: clientZone,
    [LOAD_CLIENT.prometheus]: clientPrometheus,
    [LOAD_CLIENT.tag]: clientTag,
    [LOAD_CLIENT.startTime]: clientStart,
    [LOAD_CLIENT.version]: clientVersion,
  },
  results,
}) => (
  <>
    <TableWrapper>
      <tr>
        <th>Client Name:</th>
        <td className="result-column-spaced">{clientName || "--"}</td>
        <th>Completion Time:</th>
        <td className="result-column-spaced">
          {getMMMDYYYYhmma(results[RESULT.completionTime]) || "--"}
        </td>
        <th>Deployed:</th>
        <td>{getMMMDYYYYhmma(clientStart) || "--"}</td>
      </tr>
      <tr>
        <th>Client ID:</th>
        <td className="result-column-spaced">{clientId || "--"}</td>
        <th>Total Requests:</th>
        <td className="result-column-spaced">
          {results[RESULT.requestCount] === undefined
            ? "--"
            : results[RESULT.requestCount]}
        </td>
        <th>LodeRunner Version:</th>
        <td>{clientVersion || "--"}</td>
      </tr>
      <tr>
        <th>Region:</th>
        <td className="result-column-spaced">{clientRegion || "--"}</td>
        <th>Successful Requests:</th>
        <td className="result-column-spaced">
          {results[RESULT.successfulRequestCount] === undefined
            ? "--"
            : results[RESULT.successfulRequestCount]}
        </td>
        <th>Log & App Insights Tag:</th>
        <td>{clientTag || "--"}</td>
      </tr>
      <tr>
        <th>Zone:</th>
        <td className="result-column-spaced">{clientZone || "--"}</td>
        <th className="result-column-spaced">Failed Requests:</th>
        <td>
          {results[RESULT.failedRequestCount] === undefined
            ? "--"
            : results[RESULT.failedRequestCount]}
        </td>
        <th>Prometheus Enabled:</th>
        <td>
          {clientPrometheus === undefined ? "--" : clientPrometheus.toString()}
        </td>
      </tr>
    </TableWrapper>
  </>
);

Client.propTypes = {
  client: {
    [LOAD_CLIENT.id]: PropTypes.string,
    [LOAD_CLIENT.name]: PropTypes.string,
    [LOAD_CLIENT.region]: PropTypes.string,
    [LOAD_CLIENT.zone]: PropTypes.string,
    [LOAD_CLIENT.prometheus]: PropTypes.bool,
    [LOAD_CLIENT.tag]: PropTypes.string,
    [LOAD_CLIENT.startTime]: PropTypes.string,
    [LOAD_CLIENT.version]: PropTypes.string,
  }.isRequired,
  results: PropTypes.shape({
    [RESULT.completionTime]: PropTypes.string,
    [RESULT.requestCount]: PropTypes.number,
    [RESULT.successfulRequestCount]: PropTypes.number,
    [RESULT.failedRequestCount]: PropTypes.number,
  }),
};

Client.defaultProps = {
  results: {},
};

import { useContext, useEffect, useRef, useState } from "react";
import {
  AppContext,
  ClientsContext,
  ConfigsContext,
  TestPageContext,
} from "../../contexts";
import postTestRun from "../../services/testRun";
import { CLIENT, CLIENT_STATUS_TYPES, CONFIG } from "../../models";
import { MODAL_CONTENT } from "../../utilities/constants";
import getMMMDYYYYhmma from "../../utilities/datetime";
import "./styles.css";

const TestSubmission = () => {
  const { clients, selectedClientIds, setSelectedClientIds } =
    useContext(ClientsContext);
  const { configs, setOpenedConfigId, testRunConfigId, setTestRunConfigId } =
    useContext(ConfigsContext);
  const { setIsPending } = useContext(AppContext);
  const { setModalContent } = useContext(TestPageContext);

  const testRunConfig = configs.find(
    ({ [CONFIG.id]: id }) => id === testRunConfigId
  );
  const selectedClients = clients.filter(
    ({ [CLIENT.loadClientId]: clientId }) => selectedClientIds[clientId]
  );
  const [testRunClientIds] = useState(
    selectedClients.reduce(
      (agg, { [CLIENT.loadClientId]: clientId }) => ({
        ...agg,
        [clientId]: useRef(),
      }),
      {}
    )
  );
  useEffect(() => {
    selectedClients.forEach(({ [CLIENT.loadClientId]: clientId }) => {
      testRunClientIds[clientId].current.checked = true;
    });
  }, []);

  const [isScheduledForNow, setIsScheduledForNow] = useState(true);
  const scheduledStartTimeRef = useRef();
  const testRunNameRef = useRef();

  const handleEditConfig = (configId) => () => {
    setOpenedConfigId(configId);
    setModalContent(MODAL_CONTENT.configForm);
    setTestRunConfigId(-1);
  };

  const handleCancel = () => {
    setModalContent(MODAL_CONTENT.closed);
  };

  const handleSubmit = () => {
    const testRunClients = selectedClients.filter(
      ({ [CLIENT.loadClientId]: clientId }) => {
        return testRunClientIds[clientId].current.checked;
      }
    );
    const testRunName = testRunNameRef.current.value;

    const hasScheduledStartTime =
      isScheduledForNow ||
      (scheduledStartTimeRef.current && scheduledStartTimeRef.current.value);

    const errors = [];
    if (testRunClients.length === 0) {
      errors.push("Need to select at least one load client.");
    }
    if (!hasScheduledStartTime) {
      errors.push("Need to schedule the test run start.");
    }

    if (errors.length > 0) {
      // eslint-disable-next-line no-alert
      alert(`Unable to submit load test run. ${errors.join(" ")}`);
    } else {
      setIsPending(true);

      const now = new Date();
      const scheduledStartTime = isScheduledForNow
        ? now.toISOString()
        : scheduledStartTimeRef.current.value;

      postTestRun(
        testRunConfig,
        testRunClients,
        scheduledStartTime,
        testRunName
      )
        .then(() => {
          // eslint-disable-next-line no-alert
          alert("Successfully submitted load test run request");
          setModalContent(MODAL_CONTENT.closed);
          setSelectedClientIds({});
        })
        .catch(() => {
          // eslint-disable-next-line no-alert
          alert("Unable to submit load test run request");
        })
        .finally(() => setIsPending(false));
    }
  };

  return (
    <div className="testsubmission">
      <h1>Load Test Submission</h1>
      <div className="testsubmission-config">
        <h2>Selected Load Test Config</h2>
        <div>
          <span
            className="testsubmission-config-label"
            title="User friendly name for config settings"
          >
            Name:&nbsp;
          </span>
          {`${testRunConfig[CONFIG.name] || "--"}`}
        </div>
        <div>
          <span className="testsubmission-config-label" title="Config ID">
            ID:&nbsp;
          </span>
          {testRunConfig[CONFIG.id]}
        </div>
        <br />
        <div>
          <span
            className="testsubmission-config-label"
            title="Validate settings with target clients without running load test"
          >
            Dry Run:&nbsp;
          </span>
          {testRunConfig[CONFIG.dryRun].toString()}
        </div>
        <div>
          <span className="testsubmission-config-label" title="Servers to test">
            Servers:
          </span>
          <ul>
            {testRunConfig[CONFIG.servers].map((s, index) => (
              // eslint-disable-next-line react/no-array-index-key
              <li key={`${testRunConfig[CONFIG.id]}-server-${index}`}>{s}</li>
            ))}
          </ul>
        </div>
        <div>
          <span
            className="testsubmission-config-label"
            title="Load test file to test"
          >
            Load Test Files:
          </span>
          <ul>
            {testRunConfig[CONFIG.files].map((f, index) => (
              // eslint-disable-next-line react/no-array-index-key
              <li key={`${testRunConfig[CONFIG.id]}-file-${index}`}>{f}</li>
            ))}
          </ul>
        </div>
        <div>
          <span
            className="testsubmission-config-label"
            title="Base URL for load test files"
          >
            Base URL:&nbsp;
          </span>
          {testRunConfig[CONFIG.baseUrl] || "--"}
        </div>
        <div>
          <span
            className="testsubmission-config-label"
            title=" Use strict RFC rules when parsing json. JSON property names are case sensitive. Exceptions will occur for trailing commas and comments in JSON."
          >
            Parse Load Test Files with Strict JSON:&nbsp;
          </span>
          {testRunConfig[CONFIG.strictJson].toString()}
        </div>
        <br />
        <div>
          <span
            className="testsubmission-config-label"
            title="Add a tag to the log"
          >
            Tag:&nbsp;
          </span>
          {testRunConfig[CONFIG.tag] || "--"}
        </div>
        <div>
          <span
            className="testsubmission-config-label"
            title="Display 200 and 300 results as well as errors"
          >
            Verbose:&nbsp;
          </span>
          {testRunConfig[CONFIG.verbose] === undefined
            ? "--"
            : testRunConfig[CONFIG.verbose].toString()}
        </div>
        <br />
        <div>
          <span
            className="testsubmission-config-label"
            title="Run test in an infinite loop"
          >
            Run Loop:&nbsp;
          </span>
          {testRunConfig[CONFIG.runLoop].toString()}
        </div>
        <div>
          <span className="testsubmission-config-label" title="Test duration">
            Duration:&nbsp;
          </span>
          {testRunConfig[CONFIG.duration]} second(s)
        </div>
        <div>
          <span
            className="testsubmission-config-label"
            title="Processes load file randomly instead of from top to bottom"
          >
            Randomize:&nbsp;
          </span>
          {testRunConfig[CONFIG.randomize].toString()}
        </div>
        <div>
          <span
            className="testsubmission-config-label"
            title="Sleep between each request"
          >
            Sleep:&nbsp;
          </span>
          {testRunConfig[CONFIG.sleep]} ms
        </div>
        <br />
        <div>
          <span className="testsubmission-config-label" title="Request timeout">
            Timeout:&nbsp;
          </span>
          {testRunConfig[CONFIG.timeout]} second(s)
        </div>
        <div>
          <span
            className="testsubmission-config-label"
            title="Display validation error messages"
          >
            Verbose Errors:&nbsp;
          </span>
          {testRunConfig[CONFIG.verboseErrors].toString()}
        </div>
        <div>
          <span
            className="testsubmission-config-label"
            title="Maximum validation errors"
          >
            Max Errors:&nbsp;
          </span>
          {testRunConfig[CONFIG.maxErrors]}
        </div>
        <br />
        <button
          className="unset testsubmission-button"
          type="button"
          onClick={handleEditConfig(testRunConfig[CONFIG.id])}
          onKeyDown={handleEditConfig(testRunConfig[CONFIG.id])}
        >
          EDIT CONFIG
        </button>
      </div>
      <div className="testsubmission-clients">
        <h2>Selected Load Client(s)</h2>
        {selectedClients.map(
          ({
            [CLIENT.loadClientId]: clientId,
            [CLIENT.clientStatusId]: statusId,
            [CLIENT.lastStatusChange]: lastStatusChange,
            [CLIENT.lastUpdated]: lastUpdated,
            [CLIENT.message]: message,
            [CLIENT.name]: name,
            [CLIENT.prometheus]: prometheus,
            [CLIENT.region]: region,
            [CLIENT.startTime]: startTime,
            [CLIENT.status]: status,
            [CLIENT.tag]: tag,
            [CLIENT.version]: version,
            [CLIENT.zone]: zone,
          }) => (
            <div key={clientId} className="testsubmission-clients-item">
              <input
                type="checkbox"
                ref={testRunClientIds[clientId]}
                aria-label="Load Client Selection for Load Test Run"
              />
              <div>
                <div>
                  <span className="testsubmission-clients-item-label">
                    Name:&nbsp;
                  </span>
                  {name || "--"}
                </div>
                <div className="testsubmission-clients-item-status">
                  <span className="testsubmission-clients-item-label">
                    Status:&nbsp;
                  </span>
                  {status}&nbsp;
                  <div
                    aria-label={`Load Client Status: ${status}`}
                    className={`testsubmission-clients-item-status-indicator status-${
                      status === CLIENT_STATUS_TYPES.ready ? "ready" : "pending"
                    }`}
                  />
                </div>
                <div>
                  <span className="testsubmission-clients-item-label">
                    Status Changed:&nbsp;
                  </span>
                  {getMMMDYYYYhmma(lastStatusChange)}
                </div>
                <div>
                  <span className="testsubmission-clients-item-label">
                    Updated:&nbsp;
                  </span>
                  {getMMMDYYYYhmma(lastUpdated)}
                </div>
                <div>
                  <span className="testsubmission-clients-item-label">
                    Message:&nbsp;
                  </span>
                  {message}
                </div>
                <div>
                  <span className="testsubmission-clients-item-label">
                    Status ID:&nbsp;
                  </span>
                  {statusId}
                </div>
                <div>
                  <span className="testsubmission-clients-item-label">
                    Region:&nbsp;
                  </span>
                  {region}
                </div>
                <div>
                  <span className="testsubmission-clients-item-label">
                    Zone:&nbsp;
                  </span>
                  {zone}
                </div>
                <div>
                  <span className="testsubmission-clients-item-label">
                    Prometheus Enabled:&nbsp;
                  </span>
                  {prometheus?.toString()}
                </div>
                <div>
                  <span className="testsubmission-clients-item-label">
                    Log & App Insights Tag:&nbsp;
                  </span>
                  {tag || "--"}
                </div>
                <div>
                  <span className="testsubmission-clients-item-label">
                    Deployed:&nbsp;
                  </span>
                  {getMMMDYYYYhmma(startTime)}
                </div>
                <div>
                  <span className="testsubmission-clients-item-label">
                    LodeRunner Version:&nbsp;
                  </span>
                  {version}
                </div>
                <div>
                  <span className="testsubmission-clients-item-label">
                    LodeRunner ID:&nbsp;
                  </span>
                  {clientId}
                </div>
              </div>
            </div>
          )
        )}
      </div>
      <div className="testsubmission-settings">
        <h2>Load Test Settings</h2>
        <label htmlFor="testRunName">
          <span className="testsubmission-settings-label">Name:</span>
          <input type="text" id="testRunName" ref={testRunNameRef} />
        </label>
        <label htmlFor="testRunSchedule">
          <span className="testsubmission-settings-schedule-text">
            <span className="testsubmission-settings-label">
              Scheduled Start Time:&nbsp;
            </span>
            <input
              type="checkbox"
              checked={isScheduledForNow}
              onChange={({ target }) => setIsScheduledForNow(target.checked)}
            />
            now
          </span>
          {isScheduledForNow || (
            <input type="datetime-local" ref={scheduledStartTimeRef} />
          )}
        </label>
      </div>
      <div className="testsubmission-controls">
        <button
          type="button"
          className="unset testsubmission-button"
          onClick={handleCancel}
          onKeyDown={handleCancel}
        >
          CANCEL
        </button>
        <button
          type="button"
          className="unset testsubmission-button"
          onClick={handleSubmit}
          onKeyDown={handleSubmit}
        >
          SUBMIT
        </button>
      </div>
    </div>
  );
};

export default TestSubmission;

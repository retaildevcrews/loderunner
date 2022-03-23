import React, { useContext, useEffect, useRef, useState } from "react";
import { navigate } from "hookrouter";
import {
  AppContext,
  ClientsContext,
  ConfigsContext,
  TestPageContext,
  TestRunsContext,
} from "../../contexts";
import { postTestRun } from "../../services/testRuns";
import {
  CLIENT,
  CLIENT_STATUS_TYPES,
  CONFIG,
  CONFIG_OPTIONS,
  LOAD_CLIENT,
  TEST_RUN,
  removeConfigDependencies,
  addDefaultsToConfig,
} from "../../models";
import { MODAL_CONTENT } from "../../utilities/constants";
import getMMMDYYYYhmma from "../../utilities/datetime";
import "./styles.css";

const TestSubmission = () => {
  const { setIsPending } = useContext(AppContext);
  const { clients, selectedClientIds, setSelectedClientIds } =
    useContext(ClientsContext);
  const { configs, testRunConfigId } = useContext(ConfigsContext);
  const { setModalContent } = useContext(TestPageContext);
  const { setFetchTestRunsTrigger } = useContext(TestRunsContext);

  const [formData, setFormData] = useState();
  const testRunNameRef = useRef();
  const [isScheduledForNow, setIsScheduledForNow] = useState(true);
  const scheduledStartTimeRef = useRef();

  const testRunConfig = configs.find(
    ({ [CONFIG.id]: id }) => id === testRunConfigId
  );
  const selectedClients = clients.filter(
    ({ [CLIENT.loadClientId]: clientId }) => selectedClientIds[clientId]
  );
  const [testRunClientRefs] = useState(
    selectedClients.reduce(
      (agg, { [CLIENT.loadClientId]: clientId }) => ({
        ...agg,
        [clientId]: React.createRef(),
      }),
      {}
    )
  );

  useEffect(() => {
    selectedClients.forEach(({ [CLIENT.loadClientId]: clientId }) => {
      testRunClientRefs[clientId].current.checked = true;
    });
  }, []);

  useEffect(() => {
    if (!formData) {
      return;
    }

    setIsPending(true);

    postTestRun(formData)
      .then(() => {
        // eslint-disable-next-line no-alert
        alert("Successfully submitted load test run request");
        setModalContent(MODAL_CONTENT.closed);
        setSelectedClientIds({});
      })
      .then(() => setFetchTestRunsTrigger(Date.now()))
      .catch((err) => {
        if (err.length > 0) {
          // eslint-disable-next-line no-alert
          alert(`Unable to submit load test run. ${err.join(" ")}`);
        } else if (err.message) {
          // eslint-disable-next-line no-alert
          alert(`Unable to submit load test run request. ${err.message}`);
        } else {
          // eslint-disable-next-line no-alert
          alert(`Unable to submit load test run request. ${err}`);
        }
      })
      .finally(() => setIsPending(false));
  }, [formData]);

  const handleEditConfig = (configId) => () => navigate(`/configs/${configId}`);

  const handleCancel = () => {
    setModalContent(MODAL_CONTENT.closed);
  };

  const handleSubmit = () => {
    const testRunClients = selectedClients
      .filter(
        ({ [CLIENT.loadClientId]: clientId }) =>
          testRunClientRefs[clientId].current.checked
      )
      .map( client => ({
        [LOAD_CLIENT.id]: client[CLIENT.loadClientId],
        [LOAD_CLIENT.name]: client[CLIENT.name],
        [LOAD_CLIENT.prometheus]: client[CLIENT.prometheus],
        [LOAD_CLIENT.startupArgs]: client[CLIENT.startupArgs],
        [LOAD_CLIENT.startTime]: client[CLIENT.startTime],
        [LOAD_CLIENT.region]: client[CLIENT.region],
        [LOAD_CLIENT.version]: client[CLIENT.version],
        [LOAD_CLIENT.zone]: client[CLIENT.zone],
      }));

    // TODO: When LR.API uses dictionary for TestRun.LoadClients
    // const testRunClients = selectedClients
    //   .filter(({ [CLIENT.loadClientId]: clientId }) => testRunClientRefs[clientId].current.checked)
    //   .reduce((agg, client) => ({
    //     ...agg,
    //     [client[CLIENT.loadClientId]]: client,
    //   }), {});

    addDefaultsToConfig(testRunConfig);

    removeConfigDependencies(testRunConfig);

    const now = new Date();

    setFormData({
      [TEST_RUN.name]: testRunNameRef.current.value,
      [TEST_RUN.config]: testRunConfig,
      [TEST_RUN.clients]: testRunClients,
      [TEST_RUN.createdTime]: now.toISOString(),
      [TEST_RUN.scheduledStartTime]: isScheduledForNow
        ? now.toISOString()
        : scheduledStartTimeRef.current.value,
    });
  };

  const showDependentConfig = (config) =>
    CONFIG_OPTIONS[config].dependencies.reduce(
      (isVisible, [dependentOn, shouldExist]) =>
        isVisible && testRunConfig[dependentOn] === shouldExist,
      true
    );

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
          {testRunConfig[CONFIG.dryRun]?.toString() ??
            CONFIG_OPTIONS[CONFIG.dryRun].default.toString()}
        </div>
        <div>
          <span className="testsubmission-config-label" title="Servers to test">
            Servers:
          </span>
          <ul>
            {testRunConfig[CONFIG.servers]?.map((s, index) => (
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
            {testRunConfig[CONFIG.files]?.map((f, index) => (
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
          {testRunConfig[CONFIG.strictJson]?.toString() ??
            CONFIG_OPTIONS[CONFIG.strictJson].default.toString()}
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
        <br />
        <div>
          <span
            className="testsubmission-config-label"
            title="Run test in an infinite loop"
          >
            Run Loop:&nbsp;
          </span>
          {testRunConfig[CONFIG.runLoop]?.toString() ??
            CONFIG_OPTIONS[CONFIG.runLoop].default.toString()}
        </div>
        {showDependentConfig([CONFIG.duration]) && (
          <div>
            <span className="testsubmission-config-label" title="Test duration">
              Duration:&nbsp;
            </span>
            {testRunConfig[CONFIG.duration] ??
              CONFIG_OPTIONS[CONFIG.duration].default}{" "}
            second(s)
          </div>
        )}
        {showDependentConfig([CONFIG.randomize]) && (
          <div>
            <span
              className="testsubmission-config-label"
              title="Processes load file randomly instead of from top to bottom"
            >
              Randomize:&nbsp;
            </span>
            {testRunConfig[CONFIG.randomize]?.toString() ??
              CONFIG_OPTIONS[CONFIG.randomize].default.toString()}
          </div>
        )}
        {showDependentConfig(CONFIG.maxErrors) && (
          <div>
            <span
              className="testsubmission-config-label"
              title="Maximum validation errors"
            >
              Max Errors:&nbsp;
            </span>
            {testRunConfig[CONFIG.maxErrors] ??
              CONFIG_OPTIONS[CONFIG.maxErrors].default}
          </div>
        )}
        <div>
          <span
            className="testsubmission-config-label"
            title="Sleep between each request"
          >
            Sleep:&nbsp;
          </span>
          {testRunConfig[CONFIG.sleep] ?? CONFIG_OPTIONS[CONFIG.sleep].default}{" "}
          ms
        </div>
        <br />
        <div>
          <span className="testsubmission-config-label" title="Request timeout">
            Timeout:&nbsp;
          </span>
          {testRunConfig[CONFIG.timeout] ??
            CONFIG_OPTIONS[CONFIG.timeout].default}{" "}
          second(s)
        </div>
        <div>
          <span
            className="testsubmission-config-label"
            title="Display validation error messages"
          >
            Verbose Errors:&nbsp;
          </span>
          {testRunConfig[CONFIG.verboseErrors]?.toString() ??
            CONFIG_OPTIONS[CONFIG.timeout].default.toString()}
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
                ref={testRunClientRefs[clientId]}
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
                  {prometheus?.toString() ?? "false"}
                </div>
                <div>
                  <span className="testsubmission-clients-item-label">
                    Log &amp; App Insights Tag:&nbsp;
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

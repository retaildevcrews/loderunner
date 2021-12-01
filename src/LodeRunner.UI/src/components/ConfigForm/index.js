import React, { useContext, useEffect, useRef, useState } from "react";
import ArrayOfStringInput from "./ArrayOfStringInput";
import IntegerInput from "./IntegerInput";
import StringInput from "./StringInput";
import BooleanInput from "./BooleanInput";
import { writeConfig } from "../../services/configs";
import { ConfigsContext, DisplayContext } from "../../contexts";
import { CONFIG } from "../../models";
import { MODAL_CONTENT } from "../../utilities/constants";
import spinner from "../../images/spinner.svg";
import "./styles.css";

const ConfigForm = () => {
  // context props
  const { setModalContent } = useContext(DisplayContext);
  const { configs, openedConfigIndex, setFetchConfigsTrigger } =
    useContext(ConfigsContext);

  const [isPending, setIsPending] = useState(false);
  const [errors, setErrors] = useState({});
  const [formData, setFormData] = useState();

  const openedConfig = configs[openedConfigIndex];

  // initial boolean form values
  const dryRunFlagRef = useRef(
    openedConfig ? openedConfig[CONFIG.dryRun] : false
  );
  const randomizeFlagRef = useRef(
    openedConfig ? openedConfig[CONFIG.randomize] : false
  );
  const runLoopFlagRef = useRef(
    openedConfig ? openedConfig[CONFIG.runLoop] : false
  );
  const [runLoopFlagState, setRunLoopFlagState] = useState(
    openedConfig ? openedConfig[CONFIG.runLoop] : false
  );
  const strictJsonFlagRef = useRef(
    openedConfig ? openedConfig[CONFIG.strictJson] : false
  );
  const verboseErrorsFlagRef = useRef(
    openedConfig ? openedConfig[CONFIG.verboseErrors] : false
  );

  // declare string array form refs
  const getArrayOfStringInputRefs = (values = [0]) =>
    values.map((_, index) => ({
      id: index,
      ref: React.createRef(),
    }));
  const [fileFlagRefs, setFileFlagRefs] = useState(
    openedConfig
      ? getArrayOfStringInputRefs(openedConfig[CONFIG.files])
      : getArrayOfStringInputRefs()
  );
  const [serverFlagRefs, setServerFlagRefs] = useState(
    openedConfig
      ? getArrayOfStringInputRefs(openedConfig[CONFIG.servers])
      : getArrayOfStringInputRefs()
  );

  // declare string form refs
  const baseUrlFlagRef = useRef();
  const configNameRef = useRef();
  const durationFlagRef = useRef();
  const maxErrorsFlagRef = useRef();
  const sleepFlagRef = useRef();
  const tagFlagRef = useRef();
  const timeoutFlagRef = useRef();

  useEffect(() => {
    // do not set initial values if new config
    if (!openedConfig) {
      return;
    }
    // initial string form values
    baseUrlFlagRef.current.value = openedConfig[CONFIG.baseUrl];
    configNameRef.current.value = openedConfig[CONFIG.name];
    if (durationFlagRef.current) {
      durationFlagRef.current.value = openedConfig[CONFIG.duration];
    }
    maxErrorsFlagRef.current.value = openedConfig[CONFIG.maxErrors];
    sleepFlagRef.current.value = openedConfig[CONFIG.sleep];
    tagFlagRef.current.value = openedConfig[CONFIG.tag];
    timeoutFlagRef.current.value = openedConfig[CONFIG.timeout];

    // initial string array form values
    fileFlagRefs.forEach(({ id: index, ref }) => {
      // eslint-disable-next-line no-param-reassign
      ref.current.value = openedConfig[CONFIG.files][index];
    });
    serverFlagRefs.forEach(({ id: index, ref }) => {
      // eslint-disable-next-line no-param-reassign
      ref.current.value = openedConfig[CONFIG.servers][index];
    });
  }, []);

  const onRefCurrentChange =
    (ref) =>
    ({ target }) => {
      // eslint-disable-next-line no-param-reassign
      ref.current = target.value;
    };

  const onRunLoopFlagRefChange = ({ target }) => {
    runLoopFlagRef.current = target.value === "true";
    if (target.value !== "true") {
      durationFlagRef.current.value = 0;
      randomizeFlagRef.current = false;
    }
    setRunLoopFlagState(target.value === "true");
  };

  // POST or PUT config
  useEffect(() => {
    if (!formData) {
      return;
    }

    const configWriteMethod = openedConfig ? "PUT" : "POST";

    // Create new or update existing config
    writeConfig(configWriteMethod, formData)
      .then(() => {
        setModalContent(MODAL_CONTENT.closed);
        setFetchConfigsTrigger(Date.now());
      })
      .catch((err) => {
        // UI display object error(s), otherwise, just log
        if (typeof err !== "object") {
          setErrors({ response: err });
        } else if (err.message) {
          // Handle thrown error
          setErrors({ response: err.message });
        } else {
          // Handle custom input error(s)
          setErrors(err);
        }
        setIsPending(false);
      });
  }, [formData]);

  const saveConfig = () => {
    setIsPending(true);

    const inputs = {
      [CONFIG.baseUrl]: baseUrlFlagRef.current.value,
      [CONFIG.dryRun]: dryRunFlagRef.current,
      [CONFIG.duration]:
        durationFlagRef.current && durationFlagRef.current.value,
      [CONFIG.files]: fileFlagRefs.map(({ ref }) => ref.current.value),
      [CONFIG.name]: configNameRef.current.value,
      [CONFIG.maxErrors]: maxErrorsFlagRef.current.value,
      [CONFIG.servers]: serverFlagRefs.map(({ ref }) => ref.current.value),
      [CONFIG.strictJson]: strictJsonFlagRef.current,
      [CONFIG.randomize]: randomizeFlagRef.current,
      [CONFIG.runLoop]: runLoopFlagRef.current,
      [CONFIG.sleep]: sleepFlagRef.current.value,
      [CONFIG.tag]: tagFlagRef.current.value,
      [CONFIG.verboseErrors]: verboseErrorsFlagRef.current,
      [CONFIG.timeout]: timeoutFlagRef.current.value,
    };

    // include config ID if updating existing config
    if (openedConfig) {
      inputs.id = openedConfig.id;
    }

    setFormData(inputs);
  };

  return (
    <div className="configform">
      {isPending && (
        <div className="configform-pending">
          <img src={spinner} alt="loading" />
        </div>
      )}
      <h2>
        {openedConfigIndex === -1 ? "New Config" : openedConfig[CONFIG.name]}
      </h2>
      <StringInput
        label="Name"
        description="User friendly name for config settings"
        elRef={configNameRef}
        inputName="configName"
      />
      <br />
      <ArrayOfStringInput
        label="Servers"
        description="Servers to test (required)"
        flagRefs={serverFlagRefs}
        setFlagRefs={setServerFlagRefs}
        inputName="serverFlag"
      />
      {errors[CONFIG.servers] && (
        <div className="configform-error">ERROR: {errors[CONFIG.servers]}</div>
      )}
      <br />
      <ArrayOfStringInput
        label="Load Test Files"
        description="Load test file to test (required)"
        flagRefs={fileFlagRefs}
        setFlagRefs={setFileFlagRefs}
        inputName="fileFlag"
      />
      {errors[CONFIG.files] && (
        <div className="configform-error">ERROR: {errors[CONFIG.files]}</div>
      )}
      <br />
      <StringInput
        label="Base URL"
        description="Base URL for load test files"
        elRef={baseUrlFlagRef}
        inputName="baseUrlFlag"
      />
      <br />
      <BooleanInput
        label="Strict JSON"
        description="Use strict RFC rules when parsing json. JSON property names are case sensitive. Exceptions will occur for trailing commas and comments in JSON."
        elRef={strictJsonFlagRef}
        inputName="strictJsonFlag"
        onChange={onRefCurrentChange(strictJsonFlagRef)}
      />
      <br />
      <BooleanInput
        label="Dry Run"
        description="Validate the settings with the target clients"
        elRef={dryRunFlagRef}
        inputName="dryRunFlag"
        onChange={onRefCurrentChange(dryRunFlagRef)}
      />
      <br />
      <StringInput
        label="Tag"
        description="Add a tag to the log"
        elRef={tagFlagRef}
        inputName="tagFlag"
      />
      <br />
      <div className="configform-runloop">
        <BooleanInput
          label="Run Loop"
          description="Run test in an infinite loop"
          elRef={runLoopFlagRef}
          inputName="runLoopFlag"
          onChange={onRunLoopFlagRefChange}
        />
        {runLoopFlagState && (
          <>
            <br />
            <div className="configform-runloop-dependent">
              <IntegerInput
                label="Duration"
                description="Test duration"
                elRef={durationFlagRef}
                inputName="durationFlag"
                units="second(s)"
              />
              {errors[CONFIG.duration] && (
                <div className="configform-error">
                  ERROR: {errors[CONFIG.duration]}
                </div>
              )}
              <br />
              <BooleanInput
                label="Randomize"
                description="Processes load file randomly instead of from top to bottom"
                elRef={randomizeFlagRef}
                inputName="randomizeFlag"
                onChange={onRefCurrentChange(randomizeFlagRef)}
              />
            </div>
          </>
        )}
      </div>
      <br />
      <IntegerInput
        label="Sleep"
        description="Sleep between each request"
        elRef={sleepFlagRef}
        inputName="sleepFlag"
        units="ms"
      />
      {errors[CONFIG.sleep] && (
        <div className="configform-error">ERROR: {errors[CONFIG.sleep]}</div>
      )}
      <br />
      <BooleanInput
        label="Verbose Errors"
        description="Display validation error messages"
        elRef={verboseErrorsFlagRef}
        inputName="verboseErrorsFlag"
        onChange={onRefCurrentChange(verboseErrorsFlagRef)}
      />
      <br />
      <IntegerInput
        label="Max Errors"
        description="Maximum validation errors"
        elRef={maxErrorsFlagRef}
        inputName="maxErrorsFlag"
        defaultValue={10}
      />
      {errors[CONFIG.maxErrors] && (
        <div className="configform-error">
          ERROR: {errors[CONFIG.maxErrors]}
        </div>
      )}
      <br />
      <IntegerInput
        label="Timeout"
        description="Request timeout"
        elRef={timeoutFlagRef}
        inputName="timeoutFlag"
        units="second(s)"
        defaultValue={30}
      />
      {errors[CONFIG.timeout] && (
        <div className="configform-error">ERROR: {errors[CONFIG.timeout]}</div>
      )}
      <div className="configform-save">
        {!errors.response && Object.keys(errors).length > 0 && (
          <div className="configform-save-error configform-error">
            Invalid config value(s)
          </div>
        )}
        {errors.response && (
          <div className="configform-save-error configform-error">
            Unable to save config:
            <br />
            {errors.response}
          </div>
        )}
        <button type="button" onClick={saveConfig}>
          SAVE
        </button>
      </div>
    </div>
  );
};

export default ConfigForm;

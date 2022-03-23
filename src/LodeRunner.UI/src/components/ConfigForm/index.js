import { useEffect, useState } from "react";
import PropTypes from "prop-types";
import ArrayOfStringInput from "./ArrayOfStringInput";
import BooleanInput from "./BooleanInput";
import IntegerInput from "./IntegerInput";
import StringInput from "./StringInput";
import { CONFIG, CONFIG_OPTIONS, removeConfigDependencies } from "../../models";
import "./styles.css";

const ConfigForm = ({
  children,
  writeConfig,
  baseUrlFlag,
  configName,
  dryRunFlag,
  durationFlag,
  fileFlags,
  maxErrorsFlag,
  randomizeFlag,
  runLoopFlag,
  serverFlags,
  setFileFlags,
  setServerFlags,
  sleepFlag,
  strictJsonFlag,
  tagFlag,
  timeoutFlag,
  verboseErrorsFlag,
}) => {
  const [runLoopFlagState, setRunLoopFlagState] = useState(runLoopFlag.current);
  const [formData, setFormData] = useState();
  const [errors, setErrors] = useState({});

  const onRunLoopFlagChange = ({ target }) => {
    // eslint-disable-next-line no-param-reassign
    runLoopFlag.current = target.value === "true";
    if (target.value === "false") {
      // eslint-disable-next-line no-param-reassign
      randomizeFlag.current = false;
    }
    setRunLoopFlagState(target.value === "true");
  };

  // Sets reference value on boolean toogle
  const onBooleanInputChange =
    (ref) =>
    ({ target }) => {
      // eslint-disable-next-line no-param-reassign
      ref.current = target.value === "true";
    };

  useEffect(() => {
    // Handle Potentially Unmounted DOM Elements (Dependent on Run Loop State)
    if (durationFlag.current) {
      // eslint-disable-next-line no-param-reassign
      durationFlag.current.value = CONFIG_OPTIONS[CONFIG.duration].default;
    }
    if (maxErrorsFlag.current) {
      // eslint-disable-next-line no-param-reassign
      maxErrorsFlag.current.value = CONFIG_OPTIONS[CONFIG.maxErrors].default;
    }
  }, [runLoopFlagState]);

  useEffect(() => {
    if (!formData) {
      return;
    }

    writeConfig(formData)
      .then((callback = () => {}) => {
        setErrors({});
        callback();
      })
      .catch((err) => {
        if (typeof err !== "object") {
          // Display non-object errors
          setErrors({ response: err });
        } else if (err.message) {
          // Display thrown error
          setErrors({ response: err.message });
        } else {
          // Display custom input error(s)
          setErrors(err);
        }
      });
  }, [formData]);

  const handleSave = () => {
    // send default value if referencing unmounted element (dependent on run loop)
    const duration =
      durationFlag.current?.value ?? CONFIG_OPTIONS[CONFIG.duration].default;
    const maxErrors =
      maxErrorsFlag.current?.value ?? CONFIG_OPTIONS[CONFIG.maxErrors].default;

    const inputs = {
      [CONFIG.baseUrl]: baseUrlFlag.current.value,
      [CONFIG.dryRun]: dryRunFlag.current,
      [CONFIG.duration]: duration,
      [CONFIG.files]: fileFlags
        .map(({ ref }) => ref.current.value)
        .filter((file) => file),
      [CONFIG.maxErrors]: maxErrors,
      [CONFIG.name]: configName.current.value,
      [CONFIG.randomize]: randomizeFlag.current,
      [CONFIG.runLoop]: runLoopFlag.current,
      [CONFIG.servers]: serverFlags
        .map(({ ref }) => ref.current.value)
        .filter((server) => server),
      [CONFIG.sleep]: sleepFlag.current.value,
      [CONFIG.strictJson]: strictJsonFlag.current,
      [CONFIG.tag]: tagFlag.current.value,
      [CONFIG.timeout]: timeoutFlag.current.value,
      [CONFIG.verboseErrors]: verboseErrorsFlag.current,
    };

    removeConfigDependencies(inputs);

    setFormData(inputs);
  };

  return (
    <div>
      {children}
      <BooleanInput
        label="Dry Run"
        description="Validate settings with target clients without running load test"
        elRef={dryRunFlag}
        inputName="dryRunFlag"
        onChange={onBooleanInputChange(dryRunFlag)}
      />
      <br />
      <StringInput
        label="Name"
        description="User friendly name for config settings"
        elRef={configName}
        inputName="configName"
      />
      <br />
      <ArrayOfStringInput
        label="Servers"
        description="Servers to test (required)"
        flagRefs={serverFlags}
        setFlagRefs={setServerFlags}
        inputName="serverFlags"
      />
      {errors[CONFIG.servers] && (
        <div className="configform-error">ERROR: {errors[CONFIG.servers]}</div>
      )}
      <br />
      <ArrayOfStringInput
        label="Load Test Files"
        description="Load test file to test (required)"
        flagRefs={fileFlags}
        setFlagRefs={setFileFlags}
        inputName="fileFlag"
      />
      {errors[CONFIG.files] && (
        <div className="configform-error">ERROR: {errors[CONFIG.files]}</div>
      )}
      <br />
      <StringInput
        label="Base URL"
        description="Base URL for load test files"
        elRef={baseUrlFlag}
        inputName="baseUrlFlag"
      />
      <br />
      <BooleanInput
        label="Parse Load Test Files with Strict JSON"
        description="Use strict RFC rules when parsing json. JSON property names are case sensitive. Exceptions will occur for trailing commas and comments in JSON."
        elRef={strictJsonFlag}
        inputName="strictJsonFlag"
        onChange={onBooleanInputChange(strictJsonFlag)}
      />
      <br />
      <StringInput
        label="Tag"
        description="Add a tag to the log"
        elRef={tagFlag}
        inputName="tagFlag"
      />
      <br />
      <div className="configform-runloop">
        <BooleanInput
          label="Run Loop"
          description="Run test in an infinite loop"
          elRef={runLoopFlag}
          inputName="runLoopFlag"
          onChange={onRunLoopFlagChange}
        />
        {runLoopFlagState ? (
          <>
            <div className="configform-runloop-dependent">
              <IntegerInput
                label="Duration"
                description="Test duration"
                elRef={durationFlag}
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
                elRef={randomizeFlag}
                inputName="randomizeFlag"
                onChange={onBooleanInputChange(randomizeFlag)}
              />
            </div>
          </>
        ) : (
          <>
            <div className="configform-runloop-dependent">
              <IntegerInput
                label="Max Errors"
                description="Maximum validation errors"
                elRef={maxErrorsFlag}
                inputName="maxErrorsFlag"
              />
              {errors[CONFIG.maxErrors] && (
                <div className="configform-error">
                  ERROR: {errors[CONFIG.maxErrors]}
                </div>
              )}
            </div>
          </>
        )}
      </div>
      <br />
      <IntegerInput
        label="Sleep"
        description="Sleep between each request"
        elRef={sleepFlag}
        inputName="sleepFlag"
        units="ms"
      />
      {errors[CONFIG.sleep] && (
        <div className="configform-error">ERROR: {errors[CONFIG.sleep]}</div>
      )}
      <br />
      <IntegerInput
        label="Timeout"
        description="Request timeout"
        elRef={timeoutFlag}
        inputName="timeoutFlag"
        units="second(s)"
      />
      {errors[CONFIG.timeout] && (
        <div className="configform-error">ERROR: {errors[CONFIG.timeout]}</div>
      )}
      <br />
      <BooleanInput
        label="Verbose Errors"
        description="Display validation error messages"
        elRef={verboseErrorsFlag}
        inputName="verboseErrorsFlag"
        onChange={onBooleanInputChange(verboseErrorsFlag)}
      />
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
        <button type="button" onClick={handleSave}>
          SAVE
        </button>
      </div>
    </div>
  );
};

ConfigForm.propTypes = {
  children: PropTypes.oneOfType([
    PropTypes.arrayOf(PropTypes.node),
    PropTypes.node,
  ]).isRequired,
  writeConfig: PropTypes.func.isRequired,
  baseUrlFlag: PropTypes.shape({
    current: PropTypes.instanceOf(Element),
  }).isRequired,
  configName: PropTypes.shape({
    current: PropTypes.instanceOf(Element),
  }).isRequired,
  dryRunFlag: PropTypes.shape({ current: PropTypes.bool }).isRequired,
  durationFlag: PropTypes.shape({
    current: PropTypes.instanceOf(Element),
  }).isRequired,
  fileFlags: PropTypes.arrayOf(
    PropTypes.shape({
      id: PropTypes.number,
      ref: PropTypes.shape({ current: PropTypes.instanceOf(Element) }),
    })
  ).isRequired,
  maxErrorsFlag: PropTypes.shape({
    current: PropTypes.instanceOf(Element),
  }).isRequired,
  randomizeFlag: PropTypes.shape({ current: PropTypes.bool }).isRequired,
  runLoopFlag: PropTypes.shape({ current: PropTypes.bool }).isRequired,
  serverFlags: PropTypes.arrayOf(
    PropTypes.shape({
      id: PropTypes.number,
      ref: PropTypes.shape({ current: PropTypes.instanceOf(Element) }),
    })
  ).isRequired,
  setFileFlags: PropTypes.func.isRequired,
  setServerFlags: PropTypes.func.isRequired,
  sleepFlag: PropTypes.shape({
    current: PropTypes.instanceOf(Element),
  }).isRequired,
  strictJsonFlag: PropTypes.shape({ current: PropTypes.bool }).isRequired,
  tagFlag: PropTypes.shape({
    current: PropTypes.instanceOf(Element),
  }).isRequired,
  timeoutFlag: PropTypes.shape({
    current: PropTypes.instanceOf(Element),
  }).isRequired,
  verboseErrorsFlag: PropTypes.shape({ current: PropTypes.bool }).isRequired,
};

export default ConfigForm;

import { useContext, useEffect, useRef, useState } from "react";
import ArrayOfStringInput from "./ArrayOfStringInput";
import IntegerInput from "./IntegerInput";
import StringInput from "./StringInput";
import RadioInput from "./RadioInput";
import { postConfig } from "../../services/configs";
import { ConfigsContext, DisplayContext } from "../../contexts";
import { CONFIG } from "../../models";
import spinner from "../../images/spinner.svg";
import "./styles.css";

const ConfigForm = () => {
  const { setModalContent } = useContext(DisplayContext);
  const { setFetchConfigsTrigger } = useContext(ConfigsContext);

  const [isPending, setIsPending] = useState(false);
  const [errors, setErrors] = useState({});
  const [postData, setPostData] = useState();

  const configNameRef = useRef();
  const [serverFlagRefs, setServerFlagRefs] = useState([]);
  const [fileFlagRefs, setFileFlagRefs] = useState([]);
  const baseUrlFlagRef = useRef();
  const strictJsonFlagRef = useRef(false);
  const dryRunFlagRef = useRef(false);
  const tagFlagRef = useRef();
  const runLoopFlagRef = useRef(false);
  const [runLoopFlagState, setRunLoopFlagState] = useState(false);
  const durationFlagRef = useRef();
  const randomizeFlagRef = useRef(false);
  const sleepFlagRef = useRef();
  const verboseErrorsFlagRef = useRef(false);
  const maxErrorsFlagRef = useRef();
  const timeoutFlagRef = useRef();

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

  const booleanOptions = [
    { label: "True", value: true },
    { label: "False", value: false },
  ];

  useEffect(() => {
    if (!postData) {
      return;
    }
    postConfig(postData).then((erred) => {
      if (Object.keys(erred).length > 0) {
        setErrors(erred);
        setIsPending(false);
      } else {
        setModalContent();
        setFetchConfigsTrigger(Date.now());
      }
    });
  }, [postData]);

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

    setPostData(inputs);
  };

  return (
    <div className="configform">
      {isPending && (
        <div className="configform-pending">
          <img src={spinner} alt="loading" />
        </div>
      )}
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
      <RadioInput
        label="Strict JSON"
        description="Use strict RFC rules when parsing json. JSON property names are case sensitive. Exceptions will occur for trailing commas and comments in JSON."
        elRef={strictJsonFlagRef}
        inputName="strictJsonFlag"
        options={booleanOptions}
        onChange={onRefCurrentChange(strictJsonFlagRef)}
      />
      <br />
      <RadioInput
        label="Dry Run"
        description="Validate the settings with the target clients"
        elRef={dryRunFlagRef}
        inputName="dryRunFlag"
        options={booleanOptions}
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
        <RadioInput
          label="Run Loop"
          description="Run test in an infinite loop"
          elRef={runLoopFlagRef}
          inputName="runLoopFlag"
          options={booleanOptions}
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
              <RadioInput
                label="Randomize"
                description="Processes load file randomly instead of from top to bottom"
                elRef={randomizeFlagRef}
                inputName="randomizeFlag"
                options={booleanOptions}
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
      <RadioInput
        label="Verbose Errors"
        description="Display validation error messages"
        elRef={verboseErrorsFlagRef}
        inputName="verboseErrorsFlag"
        options={booleanOptions}
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
      {!errors.postConfig && Object.keys(errors).length > 0 && (
        <div className="configform-save-error configform-error">
          Invalid config value(s)
        </div>
      )}
      {errors.postConfig && (
        <div className="configform-save-error configform-error">
          Unable to save config:
          <br />
          {errors.postConfig}
        </div>
      )}
      <div className="configform-save">
        <button type="button" onClick={saveConfig}>
          SAVE
        </button>
      </div>
    </div>
  );
};

export default ConfigForm;

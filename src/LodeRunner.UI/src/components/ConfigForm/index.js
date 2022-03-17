import { useEffect, useState } from "react";
import ArrayOfStringInput from "./ArrayOfStringInput";
import BooleanInput from "./BooleanInput";
import IntegerInput from "./IntegerInput";
import StringInput from "./StringInput";

const ConfigForm = ({
  children,
  executeSave,

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

  const onRunLoopFlagChange = ({ target }) => {
    runLoopFlag.current = target.value === "true";
    if (target.value === "false") {
      randomizeFlag.current = false;
    }
    setRunLoopFlagState(target.value === "true");
  };

  const onRefCurrentChange =
    (ref) =>
    ({ target }) => {
      // eslint-disable-next-line no-param-reassign
      ref.current = target.value;
    };

  const handleSave = () => {
    const inputs = {
      [CONFIG.baseUrl]: baseUrlFlag.current.value,
      [CONFIG.dryRun]: dryRunFlag.current,
      [CONFIG.duration]: durationFlag.current?.value, // TODO: send default value if not populated
      [CONFIG.files]: fileFlags.map(({ ref }) => ref.current.value),
      [CONFIG.maxErrors]: maxErrorsFlag.current.value,
      [CONFIG.name]: configName.current.value,
      [CONFIG.randomize]: randomizeFlag.current,
      [CONFIG.runLoop]: runLoopFlag.current,
      [CONFIG.servers]: serverFlags.map(({ ref }) => ref.current.value),
      [CONFIG.sleep]: sleepFlag.current.value,
      [CONFIG.strictJson]: strictJsonFlag.current,
      [CONFIG.tag]: tagFlag.current.value,
      [CONFIG.timeout]: timeoutFlag.current.value,
      [CONFIG.verboseErrors]: verboseErrorsFlag.current,
    };
    executeSave(inputs);
  };

  return (
    <div>
      {children}
      <BooleanInput
        label="Dry Run"
        description="Validate settings with target clients without running load test"
        elRef={dryRunFlag}
        inputName="dryRunFlag"
        onChange={onRefCurrentChange(dryRunFlag)}
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
      <br />
      <ArrayOfStringInput
        label="Load Test Files"
        description="Load test file to test (required)"
        flagRefs={fileFlags}
        setFlagRefs={setFileFlags}
        inputName="fileFlag"
      />
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
        onChange={onRefCurrentChange(strictJsonFlag)}
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
      </div>
      {runLoopFlagState ? (
        <>
          <br />
          <div className="configform-runloop-dependent">
            <IntegerInput
              label="Duration"
              description="Test duration"
              elRef={durationFlag}
              inputName="durationFlag"
              units="second(s)"
            />
            <br />
            <BooleanInput
              label="Randomize"
              description="Processes load file randomly instead of from top to bottom"
              elRef={randomizeFlag}
              inputName="randomizeFlag"
              onChange={onRefCurrentChange(randomizeFlag)}
            />
          </div>
        </>
      ) : (
        <>
          <br />
          <div className="configform-runloop-dependent">
            <IntegerInput
              label="Max Errors"
              description="Maximum validation errors"
              elRef={maxErrorsFlag}
              inputName="maxErrorsFlag"
            />
          </div>
        </>
      ) }
      <br />
      <IntegerInput
        label="Sleep"
        description="Sleep between each request"
        elRef={sleepFlag}
        inputName="sleepFlag"
        units="ms"
      />
      <br />
      <IntegerInput
        label="Timeout"
        description="Request timeout"
        elRef={timeoutFlag}
        inputName="timeoutFlag"
        units="second(s)"
      />
      <br />
      <BooleanInput
        label="Verbose Errors"
        description="Display validation error messages"
        elRef={verboseErrorsFlag}
        inputName="verboseErrorsFlag"
        onChange={onRefCurrentChange(verboseErrorsFlag)}
      />
      <button type="button" onClick={handleSave}>
        SAVE
      </button>
    </div>
  );
};

export default ConfigForm;

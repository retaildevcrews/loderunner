import PropTypes from "prop-types";
import { useState, useRef, useEffect } from "react";
import { A } from "hookrouter";
import ConfigForm from "../ConfigForm";
import { getArrayOfStringInputRefs } from "../ConfigForm/ArrayOfStringInput";
import { CONFIG } from "../../models";
import "./styles.css";

const ConfigPage = ({ configId }) => {

  const [openedConfig, setOpenedConfig] = useState({}); //TODO: handle when undefined
  const [serverFlagRefs, setServerFlagRefs] = useState(
    getArrayOfStringInputRefs(openedConfig[CONFIG.servers]) // TODO: Handle missing value with default
  );
  const [fileFlagRefs, setFileFlagRefs] = useState(
    getArrayOfStringInputRefs(openedConfig[CONFIG.files])
  );

  // initial boolean form values
  // TODO: Handle missing value with default
  const baseUrlFlagRef = useRef();
  const configNameRef = useRef();
  const dryRunFlagRef = useRef(openedConfig[CONFIG.dryRun]);
  const durationFlagRef = useRef();
  const maxErrorsFlagRef = useRef();
  const randomizeFlagRef = useRef(openedConfig[CONFIG.randomize]);
  const runLoopFlagRef = useRef(openedConfig[CONFIG.runLoop]);
  const sleepFlagRef = useRef();
  const strictJsonFlagRef = useRef(openedConfig[CONFIG.strictJson]);
  const tagFlagRef = useRef();
  const timeoutFlagRef = useRef();
  const verboseErrorsFlagRef = useRef(openedConfig[CONFIG.verboseErrors]);

  // TODO: Handle missing values with default
  useEffect(() => {
    // Handle array of references
    // TODO: Handle empty servers/files
    serverFlagRefs.forEach(({ id: index, ref }) => {
      // eslint-disable-next-line no-param-reassign
      ref.current.value =
        (openedConfig[CONFIG.servers] && openedConfig[CONFIG.servers][index]) ||
        "";
    });
    fileFlagRefs.forEach(({ id: index, ref }) => {
      // eslint-disable-next-line no-param-reassign
      ref.current.value =
        openedConfig[CONFIG.files] && openedConfig[CONFIG.files][index];
    });

    // Handle Potentially Unmounted DOM Elements (Dependent on Run Loop State)
    if (durationFlagRef.current) {
      durationFlagRef.current.value = openedConfig[CONFIG.duration];
    }
    if (maxErrorsFlagRef.current) {
      maxErrorsFlagRef.current.value = openedConfig[CONFIG.maxErrors];
    }

    // Handle Others
    baseUrlFlagRef.current.value = openedConfig[CONFIG.baseUrl];
    configNameRef.current.value = openedConfig[CONFIG.name]; 
    sleepFlagRef.current.value = openedConfig[CONFIG.sleep];
    tagFlagRef.current.value = openedConfig[CONFIG.tag];    
    timeoutFlagRef.current.value = openedConfig[CONFIG.timeout];
  }, [openedConfig]);

  const executeSave = (inputs) => {
    // TODO: make PUT call
    // eslint-disable-next-line no-param-reassign
    inputs[CONFIG.id] = openedConfig[CONFIG.id];
  };

  return (
    <div className="config">
      <div className="page-header">
        <h1>Load Test Config</h1>
        <A href="/" className="unset navigation">
          Load Test Submission
        </A>
      </div>
      <ConfigForm
        writeConfig={putConfig}
        baseUrlFlag={baseUrlFlagRef}
        configName={configNameRef}
        dryRunFlag={dryRunFlagRef}
        durationFlag={durationFlagRef}
        fileFlags={fileFlagRefs}
        maxErrorsFlag={maxErrorsFlagRef}
        randomizeFlag={randomizeFlagRef}
        runLoopFlag={runLoopFlagRef}
        serverFlags={serverFlagRefs}
        setFileFlags={setFileFlagRefs}
        setServerFlags={setServerFlagRefs}
        sleepFlag={sleepFlagRef}
        strictJsonFlag={strictJsonFlagRef}
        tagFlag={tagFlagRef}
        timeoutFlag={timeoutFlagRef}
        verboseErrorsFlag={verboseErrorsFlagRef}
      >
        <h2>{openedConfig[CONFIG.name]}</h2>
      </ConfigForm>
    </div>
  );
};

ConfigPage.propTypes = {
  configId: PropTypes.string.isRequired,
};

export default ConfigPage;

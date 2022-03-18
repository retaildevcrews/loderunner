import { useContext, useEffect, useRef, useState } from "react";
import PropTypes from "prop-types";
import { A } from "hookrouter";
import ConfigForm from "../ConfigForm";
import { getArrayOfStringInputRefs } from "../ConfigForm/ArrayOfStringInput";
import { AppContext } from "../../contexts";
import { writeConfig, getConfig } from "../../services/configs";
import { CONFIG } from "../../models";
import "./styles.css";

const ConfigPage = ({ configId }) => {
  const { setIsPending } = useContext(AppContext);
  const [openedConfig, setOpenedConfig] = useState(); //TODO: handle when undefined
  const [fetchConfigTrigger, setFetchConfigTrigger] = useState(0);

  const [fileFlagRefs, setFileFlagRefs] = useState(getArrayOfStringInputRefs());
  const [serverFlagRefs, setServerFlagRefs] = useState(
    getArrayOfStringInputRefs()
  );

  // declare form references
  // TODO: Handle missing value with default
  const baseUrlFlagRef = useRef();
  const configNameRef = useRef();
  const dryRunFlagRef = useRef();
  const durationFlagRef = useRef();
  const maxErrorsFlagRef = useRef();
  const randomizeFlagRef = useRef();
  const runLoopFlagRef = useRef();
  const sleepFlagRef = useRef();
  const strictJsonFlagRef = useRef();
  const tagFlagRef = useRef();
  const timeoutFlagRef = useRef();
  const verboseErrorsFlagRef = useRef();

  useEffect(() => {
    setIsPending(true);
    getConfig(configId)
      .then((res) => setOpenedConfig(res))
      .finally(() => setIsPending(false));
  }, [fetchConfigTrigger]);

  // Update inputs when openedConfig is set
  useEffect(() => {
    if (!openedConfig) {
      return;
    }
    // Handle array of references
    setFileFlagRefs(getArrayOfStringInputRefs(openedConfig[CONFIG.files]));
    setServerFlagRefs(getArrayOfStringInputRefs(openedConfig[CONFIG.servers]));

    // Handle Potentially Unmounted DOM Elements (Dependent on Run Loop State)
    if (durationFlagRef.current) {
      durationFlagRef.current.value = openedConfig[CONFIG.duration];
    }
    if (maxErrorsFlagRef.current) {
      maxErrorsFlagRef.current.value = openedConfig[CONFIG.maxErrors];
    }

    // Handle primitive references
    baseUrlFlagRef.current.value = openedConfig[CONFIG.baseUrl];
    configNameRef.current.value = openedConfig[CONFIG.name];
    dryRunFlagRef.current = openedConfig[CONFIG.dryRun];
    randomizeFlagRef.current = openedConfig[CONFIG.randomize];
    runLoopFlagRef.current = openedConfig[CONFIG.runLoop];
    sleepFlagRef.current.value = openedConfig[CONFIG.sleep];
    strictJsonFlagRef.current = openedConfig[CONFIG.strictJson];
    tagFlagRef.current.value = openedConfig[CONFIG.tag];
    timeoutFlagRef.current.value = openedConfig[CONFIG.timeout];
    verboseErrorsFlagRef.current = openedConfig[CONFIG.verboseErrors];
  }, [openedConfig]);

  useEffect(() => {
    if (!openedConfig) {
      return;
    }

    if (openedConfig[CONFIG.files]) {
      openedConfig[CONFIG.files].forEach((value, index) => {
        fileFlagRefs[index].ref.current.value = value;
      });
    }

    if (openedConfig[CONFIG.servers]) {
      openedConfig[CONFIG.servers].forEach((value, index) => {
        serverFlagRefs[index].ref.current.value = value;
      });
    }
  }, [fileFlagRefs, serverFlagRefs]);

  // Update existing config
  const putConfig = (inputs) => {
    // eslint-disable-next-line no-param-reassign
    inputs[CONFIG.id] = openedConfig[CONFIG.id];
    return writeConfig("PUT", inputs).then(() =>
      setFetchConfigTrigger(Date.now())
    );
  };

  return openedConfig ? (
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
  ) : (
    <div>Load Test Config Not Found</div>
  );
};

ConfigPage.propTypes = {
  configId: PropTypes.string.isRequired,
};

export default ConfigPage;

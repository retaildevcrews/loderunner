import { useContext, useEffect, useRef, useState } from "react";
import PropTypes from "prop-types";
import { A } from "hookrouter";
import ConfigForm from "../ConfigForm";
import { AppContext } from "../../contexts";
import { writeConfig, getConfig } from "../../services/configs";
import { CONFIG, CONFIG_OPTIONS } from "../../models";
import "./styles.css";

const ConfigPage = ({ configId }) => {
  const { setIsPending } = useContext(AppContext);
  const [openedConfig, setOpenedConfig] = useState();
  const [fetchConfigTrigger, setFetchConfigTrigger] = useState(0);

  const [fileFlagRefs, setFileFlagRefs] = useState([]);
  const [serverFlagRefs, setServerFlagRefs] = useState([]);

  // declare string/integer references
  const baseUrlFlagRef = useRef();
  const configNameRef = useRef();
  const durationFlagRef = useRef();
  const sleepFlagRef = useRef();
  const maxErrorsFlagRef = useRef();
  const tagFlagRef = useRef();
  const timeoutFlagRef = useRef();

  // handle boolean input references
  const dryRunFlagRef = useRef(openedConfig?.[CONFIG.dryRun] ?? CONFIG_OPTIONS[CONFIG.dryRun].default);
  const randomizeFlagRef = useRef(openedConfig?.[CONFIG.randomize] ?? CONFIG_OPTIONS[CONFIG.randomize].default);
  const runLoopFlagRef = useRef(openedConfig?.[CONFIG.runLoop] ?? CONFIG_OPTIONS[CONFIG.runLoop].default);
  const strictJsonFlagRef = useRef(openedConfig?.[CONFIG.strictJson] ?? CONFIG_OPTIONS[CONFIG.strictJson].default);
  const verboseErrorsFlagRef = useRef(openedConfig?.[CONFIG.verboseErrors] ?? CONFIG_OPTIONS[CONFIG.verboseErrors].default);

  useEffect(() => {
    setIsPending(true);
    getConfig(configId)
      .then((res) => setOpenedConfig(res))
      .finally(() => setIsPending(false));
  }, [fetchConfigTrigger]);

  // Update string/integer inputs when openedConfig is set
  useEffect(() => {
    if (!openedConfig) {
      return;
    }

    // Handle array of references
    if (openedConfig[CONFIG.files]?.length > 0) {
      setFileFlagRefs(openedConfig[CONFIG.files].map((value, index) => ({
        id: index,
        ref: null,
        initialValue: value,
      })));
    } else {
      setFileFlagRefs([{
        id: 0,
        ref: null,
        initialValue: CONFIG_OPTIONS[CONFIG.files].default,
      }]);
    }

    if (openedConfig[CONFIG.servers]?.length > 0) {
      setServerFlagRefs(openedConfig[CONFIG.servers].map((value, index) => ({
        id: index,
        ref: null,
        initialValue: value,
      })));
    } else {
      setServerFlagRefs([{
        id: 0,
        ref: null,
        initialValue: CONFIG_OPTIONS[CONFIG.servers].default,
      }]);
    }

    // Handle Potentially Unmounted DOM Elements (Dependent on Run Loop State)
    if (durationFlagRef.current) {
      durationFlagRef.current.value = openedConfig[CONFIG.duration] ?? CONFIG_OPTIONS[CONFIG.duration].default;
    }
    if (maxErrorsFlagRef.current) {
      maxErrorsFlagRef.current.value = openedConfig[CONFIG.maxErrors] ?? CONFIG_OPTIONS[CONFIG.maxErrors].default;
    }

    // Handle string/integer references
    baseUrlFlagRef.current.value = openedConfig[CONFIG.baseUrl] ?? CONFIG_OPTIONS[CONFIG.baseUrl].default;
    configNameRef.current.value = openedConfig[CONFIG.name] ?? CONFIG_OPTIONS[CONFIG.name].default;
    sleepFlagRef.current.value = openedConfig[CONFIG.sleep] ?? CONFIG_OPTIONS[CONFIG.sleep].default;
    tagFlagRef.current.value = openedConfig[CONFIG.tag] ?? CONFIG_OPTIONS[CONFIG.tag].default;
    timeoutFlagRef.current.value = openedConfig[CONFIG.timeout] ?? CONFIG_OPTIONS[CONFIG.timeout].default;
  }, [openedConfig]);

  // Update existing config
  const putConfig = (inputs) => {
    setIsPending(true);
    // eslint-disable-next-line no-param-reassign
    inputs[CONFIG.id] = openedConfig[CONFIG.id];
    return writeConfig("PUT", inputs)
      .then(() => setFetchConfigTrigger(Date.now()))
      .finally(() => setIsPending(false));
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

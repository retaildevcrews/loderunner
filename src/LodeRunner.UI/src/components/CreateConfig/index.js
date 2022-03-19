import { useContext, useEffect, useRef, useState } from "react";
import ConfigForm from "../ConfigForm";
import { getArrayOfStringInputRefs } from "../ConfigForm/ArrayOfStringInput";
import { writeConfig } from "../../services/configs";
import { AppContext, ConfigsContext, TestPageContext } from "../../contexts";
import { CONFIG, CONFIG_OPTIONS } from "../../models";
import { MODAL_CONTENT } from "../../utilities/constants";
import "./styles.css";

const CreateConfig = () => {
  // context props
  const { setIsPending } = useContext(AppContext);
  const { setFetchConfigsTrigger } = useContext(ConfigsContext);
  const { setModalContent } = useContext(TestPageContext);

  // initial boolean form values
  const dryRunFlagRef = useRef(CONFIG_OPTIONS[CONFIG.dryRun].default);
  const randomizeFlagRef = useRef(CONFIG_OPTIONS[CONFIG.randomize].default);
  const runLoopFlagRef = useRef(CONFIG_OPTIONS[CONFIG.runLoop].default);
  const strictJsonFlagRef = useRef(CONFIG_OPTIONS[CONFIG.strictJson].default);
  const verboseErrorsFlagRef = useRef(CONFIG_OPTIONS[CONFIG.verboseErrors].default);

  // initialize array of string refs
  const [fileFlagRefs, setFileFlagRefs] = useState(getArrayOfStringInputRefs(CONFIG_OPTIONS[CONFIG.files].default));
  const [serverFlagRefs, setServerFlagRefs] = useState(getArrayOfStringInputRefs(CONFIG_OPTIONS[CONFIG.servers].default));

  // declare refs without initial values
  const baseUrlFlagRef = useRef();
  const configNameRef = useRef();
  const durationFlagRef = useRef();
  const maxErrorsFlagRef = useRef();
  const sleepFlagRef = useRef();
  const tagFlagRef = useRef();
  const timeoutFlagRef = useRef();

  useEffect(() => {
    // Handle Potentially Unmounted DOM Elements (Dependent on Run Loop State)
    if (durationFlagRef.current) {
      durationFlagRef.current.value = CONFIG_OPTIONS[CONFIG.duration].default;
    }
    if (maxErrorsFlagRef.current) {
      maxErrorsFlagRef.current.value = CONFIG_OPTIONS[CONFIG.maxErrors].default;
    }

    // set initial values after ref elements mount
    baseUrlFlagRef.current.value = CONFIG_OPTIONS[CONFIG.baseUrl].default;
    configNameRef.current.value = CONFIG_OPTIONS[CONFIG.name].default;
    sleepFlagRef.current.value = CONFIG_OPTIONS[CONFIG.sleep].default;
    tagFlagRef.current.value = CONFIG_OPTIONS[CONFIG.tag].default;
    timeoutFlagRef.current.value = CONFIG_OPTIONS[CONFIG.timeout].default;
  }, []);

  // Create new config
  const postConfig = (inputs) => {
    setIsPending(true);
    return writeConfig("POST", inputs)
      .then(() => {
        setFetchConfigsTrigger(Date.now());
        return () => setModalContent(MODAL_CONTENT.closed);
      })
      .finally(() => {
        setIsPending(false);
      });
  };

  return (
    <div className="configform">
      <ConfigForm
        writeConfig={postConfig}
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
        <h2>New Config</h2>
      </ConfigForm>
    </div>
  );
};

export default CreateConfig;

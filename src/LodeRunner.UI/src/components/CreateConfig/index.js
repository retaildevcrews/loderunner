import { useContext, useEffect, useRef, useState } from "react";
import ConfigForm from "../ConfigForm";
import { getArrayOfStringInputRefs } from "../ConfigForm/ArrayOfStringInput";
import { writeConfig } from "../../services/configs";
import { ConfigsContext, TestPageContext } from "../../contexts";
import { MODAL_CONTENT } from "../../utilities/constants";
import "./styles.css";

const CreateConfig = () => {
  // context props
  const { setFetchConfigsTrigger } = useContext(ConfigsContext);
  const { setModalContent } = useContext(TestPageContext);

  // initial boolean form values
  //TODO: Set via default value function in model
  const dryRunFlagRef = useRef(false);
  const randomizeFlagRef = useRef(false);
  const runLoopFlagRef = useRef(false);
  const strictJsonFlagRef = useRef(false);
  const verboseErrorsFlagRef = useRef(false);

  // initialize array of string refs
  const [serverFlagRefs, setServerFlagRefs] = useState(
    getArrayOfStringInputRefs()
  );
  const [fileFlagRefs, setFileFlagRefs] = useState(getArrayOfStringInputRefs());

  // declare string refs
  const baseUrlFlagRef = useRef();
  const configNameRef = useRef();
  const durationFlagRef = useRef();
  const maxErrorsFlagRef = useRef();
  const sleepFlagRef = useRef();
  const tagFlagRef = useRef();
  const timeoutFlagRef = useRef();

  useEffect(() => {
    // TODO: use default values to set
    // Handle Potentially Unmounted DOM Elements (Dependent on Run Loop State)
    if (durationFlagRef.current) {
      durationFlagRef.current.value = "";
    }
    if (maxErrorsFlagRef.current) {
      maxErrorsFlagRef.current.value = 10;
    }

    // Handle Others
    configNameRef.current.value = "";
    sleepFlagRef.current.value = 0;
    tagFlagRef.current.value = "";
    timeoutFlagRef.current.value = 30;
  }, []);

  // Create new config
  const postConfig = (inputs) =>
    writeConfig("POST", inputs).then(() => {
      setModalContent(MODAL_CONTENT.closed);
      setFetchConfigsTrigger(Date.now());
    });

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

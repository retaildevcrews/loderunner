import PropTypes from "prop-types";
import { useContext, useEffect, useRef, useState } from "react";
import ConfigForm from "../ConfigForm";
import { getArrayOfStringInputRefs } from "../ConfigForm/ArrayOfStringInput";
import { writeConfig, getConfig } from "../../services/configs";
import { AppContext, ConfigsContext, TestPageContext } from "../../contexts";
import { CONFIG } from "../../models";
import { MODAL_CONTENT } from "../../utilities/constants";
import "./styles.css";

const CreateConfig = () => {
  // context props
  const { setIsPending } = useContext(AppContext);
  const configsContext = useContext(ConfigsContext);
  const testPageContext = useContext(TestPageContext);

  const [openedConfig, setOpenedConfig] = useState(); //moved to config page
  const [fetchConfigTrigger, setFetchConfigTrigger] = useState(0);
  const [errors, setErrors] = useState({});
  const [formData, setFormData] = useState();

  // TODO: Move to ConfigPage
  useEffect(() => {
    if (!isNewConfig) {
      getConfig(openedConfigId).then((res) => setOpenedConfig(res));
    }
  }, [fetchConfigTrigger]);

  // initial boolean form values
  //TODO: Set via default value function in model
  const dryRunFlagRef = useRef(false);
  const randomizeFlagRef = useRef(false);
  const runLoopFlagRef = useRef(false);
  const strictJsonFlagRef = useRef(false);
  const verboseErrorsFlagRef = useRef(false);

  // initialize array of string refs
  const [serverFlagRefs, setServerFlagRefs] = useState(getArrayOfStringInputRefs());
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

  // POST or PUT config
  useEffect(() => {
    if (!formData) {
      return;
    }

    const configWriteMethod = openedConfig ? "PUT" : "POST";

    // Create new or update existing config
    writeConfig(configWriteMethod, formData)
      .then(() => {
        if (!isNewConfig) {
          setFetchConfigTrigger(Date.now());
        }
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
      })
      .finally(() => {
        setIsPending(false);
        if (isNewConfig) {
          testPageContext.setModalContent(MODAL_CONTENT.closed);
          configsContext.setFetchConfigsTrigger(Date.now());
        }
      });
  }, [formData]);

  const executeSave = (inputs) => {
    setIsPending(true);
    setFormData(inputs);
  };

  return (
    <div className="configform">
      <ConfigForm
        executeSave={executeSave}

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

      {errors[CONFIG.servers] && (
        <div className="configform-error">ERROR: {errors[CONFIG.servers]}</div>
      )}
      {errors[CONFIG.files] && (
        <div className="configform-error">ERROR: {errors[CONFIG.files]}</div>
      )}

      
      {false ? (
        <>
            {errors[CONFIG.duration] && (
              <div className="configform-error">
                ERROR: {errors[CONFIG.duration]}
              </div>
            )}
        </>
      ) : (
        <>
          {errors[CONFIG.maxErrors] && (
            <div className="configform-error">
              ERROR: {errors[CONFIG.maxErrors]}
            </div>
          )}
        </>
      )}
      
      {errors[CONFIG.sleep] && (
        <div className="configform-error">ERROR: {errors[CONFIG.sleep]}</div>
      )}
      
      {errors[CONFIG.timeout] && (
        <div className="configform-error">ERROR: {errors[CONFIG.timeout]}</div>
      )}
      
      <br />
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
      </div>
    </div>
  );
};

export default CreateConfig;

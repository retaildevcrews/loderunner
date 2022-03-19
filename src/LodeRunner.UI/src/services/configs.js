import { getApi, writeApi, deleteApi } from "./utilities";
import { CONFIG } from "../models";

const getConfigs = async () => {
  const content = await getApi("LoadTestConfigs");

  return content || [];
};

const checkConfigInputs = (inputs) => {
  const checkedInputs = [
    CONFIG.servers,
    CONFIG.files,
    CONFIG.duration,
    CONFIG.sleep,
    CONFIG.maxErrors,
    CONFIG.timeout,
  ];

  const errors = checkedInputs.reduce((errs, config) => {
    switch (config) {
      case CONFIG.servers:
      case CONFIG.files:
        // REQUIRED: flags
        return inputs[config].some((i) => i && i.length > 0)
          ? errs
          : { ...errs, [config]: `Missing ${config} flag` };
      case CONFIG.sleep:
      case CONFIG.timeout:
        // Positive integer value or zero
        return Number.parseInt(inputs[config], 10) >= 0
          ? errs
          : {
              ...errs,
              [config]: "Must be a positive integer or zero",
            };
      case CONFIG.duration:
        // Dependent (runLoop == True) positive integer value or zero
        if (
          inputs[CONFIG.runLoop] &&
          !(Number.parseInt(inputs[config], 10) >= 0)
        ) {
          return {
            ...errs,
            [config]: "Must be a positive integer or zero",
          };
        }
        return errs;
      case CONFIG.maxErrors:
        // Dependent (runLoop == False) positive integer value or zero
        if (
          !inputs[CONFIG.runLoop] &&
          !(Number.parseInt(inputs[config], 10) >= 0)
        ) {
          return {
            ...errs,
            [config]: "Must be a positive integer or zero",
          };
        }
        return errs;
      default:
        return errs;
    }
  }, {});

  if (Object.keys(errors).length > 0) {
    throw errors;
  }
};

const writeConfig = async (method, inputs) => {
  checkConfigInputs(inputs);
  const endpoint =
    method === "PUT"
      ? `LoadTestConfigs/${inputs[CONFIG.id]}`
      : "LoadTestConfigs";
  return writeApi(method, endpoint)(inputs);
};

const deleteConfig = deleteApi("LoadTestConfigs");

const getConfig = async (configId) => {
  return (await getApi(`LoadTestConfigs/${configId}`)) || {};
};

export { checkConfigInputs, deleteConfig, getConfig, getConfigs, writeConfig };

import { getApi, postApi } from "./utilities";
import { CONFIG } from "../models";

const getConfigs = getApi("LoadTestConfig");

const checkPostConfigInputs = (inputs) => {
  const checkedConfigs = [
    CONFIG.servers,
    CONFIG.files,
    CONFIG.duration,
    CONFIG.sleep,
    CONFIG.maxErrors,
    CONFIG.timeout,
  ];

  return checkedConfigs.reduce((errors, config) => {
    switch (config) {
      case CONFIG.servers:
      case CONFIG.files:
        // REQUIRED: flags
        return inputs[config].some((i) => i && i.length > 0)
          ? errors
          : { ...errors, [config]: `Missing ${config} flag` };
      case CONFIG.sleep:
      case CONFIG.maxErrors:
      case CONFIG.timeout:
        // Positive integer value or zero
        return Number.parseInt(inputs[config], 10) >= 0
          ? errors
          : {
              ...errors,
              [config]: "Must be a positive integer or zero",
            };
      case CONFIG.duration:
        // Dependent positive integer value or zero
        if (
          inputs[CONFIG.runLoop] &&
          !(Number.parseInt(inputs[config], 10) >= 0)
        ) {
          return {
            ...errors,
            [config]: "Must be a positive integer or zero",
          };
        }
        return errors;
      default:
        return errors;
    }
  }, {});
};

const getPostConfigBody = (inputs) =>
  Object.values(CONFIG).reduce((body, config) => {
    switch (config) {
      case CONFIG.baseUrl:
      case CONFIG.name:
      case CONFIG.tag:
        // don't send if no input
        return inputs[config].length > 0
          ? { ...body, [config]: inputs[config] }
          : body;

      // case CONFIG.delayStart:
      //   // set constant value
      //   return { ...body, [config]: -1 };

      case CONFIG.duration:
      case CONFIG.randomize:
        // send only if runLoop is set
        if (!inputs[CONFIG.runLoop]) {
          return body;
        }

        return {
          ...body,
          [CONFIG.duration]: Number.parseInt(inputs[CONFIG.duration], 10),
          [CONFIG.randomize]: inputs[CONFIG.randomize],
        };

      case CONFIG.files:
      case CONFIG.servers:
        // format input
        return {
          ...body,
          [config]: inputs[config].filter((c) => c),
        };

      case CONFIG.maxErrors:
      case CONFIG.sleep:
      case CONFIG.timeout:
        // convert input to type integer
        return { ...body, [config]: Number.parseInt(inputs[config], 10) };

      case CONFIG.dryRun:
      case CONFIG.strictJson:
      case CONFIG.runLoop:
      case CONFIG.verboseErrors:
        // always send boolean input
        return { ...body, [config]: inputs[config] };
      default:
        return body;
    }
  }, {});

const postConfig = async (inputs) => {
  const errors = checkPostConfigInputs(inputs);
  if (Object.keys(errors).length > 0) {
    return errors;
  }

  const body = getPostConfigBody(inputs);

  return postApi("LoadTestConfig")(body).catch((err) => ({
    postConfig: err.message,
  }));
};

export { getConfigs, checkPostConfigInputs, getPostConfigBody, postConfig };

import { getApi, writeApi, deleteApi } from "./utilities";
import getAsBoolean from "../utilities/types";
import { CONFIG } from "../models";

const getConfigs = async () => {
  const content = await getApi("LoadTestConfigs");

  return content || [];
};

const checkConfigInputs = (inputs) => {
  const checkedConfigs = [
    CONFIG.servers,
    CONFIG.files,
    CONFIG.duration,
    CONFIG.sleep,
    CONFIG.maxErrors,
    CONFIG.timeout,
  ];

  const errors = checkedConfigs.reduce((errs, config) => {
    switch (config) {
      case CONFIG.servers:
      case CONFIG.files:
        // REQUIRED: flags
        return inputs[config].some((i) => i && i.length > 0)
          ? errs
          : { ...errs, [config]: `Missing ${config} flag` };
      case CONFIG.sleep:
      case CONFIG.maxErrors:
      case CONFIG.timeout:
        // Positive integer value or zero
        return Number.parseInt(inputs[config], 10) >= 0
          ? errs
          : {
              ...errs,
              [config]: "Must be a positive integer or zero",
            };
      case CONFIG.duration:
        // Dependent positive integer value or zero
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
      default:
        return errs;
    }
  }, {});

  if (Object.keys(errors).length > 0) {
    throw errors;
  }
};

const getConfigPayload = (inputs) =>
  Object.values(CONFIG).reduce((data, config) => {
    switch (config) {
      case CONFIG.name:
      case CONFIG.tag:
        // don't send if no input
        return inputs[config].length > 0
          ? { ...data, [config]: inputs[config] }
          : data;
      case CONFIG.baseUrl:
        // send even if no input
        return { ...data, [config]: inputs[config] };
      case CONFIG.duration:
      case CONFIG.randomize:
        // send only if runLoop is set
        if (!inputs[CONFIG.runLoop]) {
          return data;
        }

        return {
          ...data,
          [CONFIG.duration]: Number.parseInt(inputs[CONFIG.duration], 10),
          [CONFIG.randomize]: inputs[CONFIG.randomize],
        };

      case CONFIG.files:
      case CONFIG.servers:
        // array of non-empty strings
        return {
          ...data,
          [config]: inputs[config].filter((c) => c),
        };

      case CONFIG.maxErrors:
      case CONFIG.sleep:
      case CONFIG.timeout:
        // convert input to type integer
        return { ...data, [config]: Number.parseInt(inputs[config], 10) };

      case CONFIG.dryRun:
      case CONFIG.strictJson:
      case CONFIG.runLoop:
      case CONFIG.verbose:
      case CONFIG.verboseErrors:
        // always send boolean input
        return { ...data, [config]: getAsBoolean(inputs[config]) };
      default:
        return data;
    }
  }, {});

const writeConfig = async (method, inputs) => {
  checkConfigInputs(inputs);
  const payload = getConfigPayload(inputs);
  const endpoint =
    method === "PUT"
      ? `LoadTestConfigs/${inputs[CONFIG.id]}`
      : "LoadTestConfigs";
  return writeApi(method, endpoint)(payload);
};

const deleteConfig = deleteApi("LoadTestConfigs");

export {
  getConfigs,
  checkConfigInputs,
  getConfigPayload,
  writeConfig,
  deleteConfig,
};

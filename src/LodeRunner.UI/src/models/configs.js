export const CONFIG = {
  baseUrl: "baseURL",
  delayStart: "delayStart",
  dryRun: "dryRun",
  duration: "duration",
  entityType: "entityType",
  files: "files",
  id: "id",
  name: "name",
  maxErrors: "maxErrors",
  partitionKey: "partitionKey",
  randomize: "randomize",
  runLoop: "runLoop",
  servers: "server",
  sleep: "sleep",
  strictJson: "strictJson",
  tag: "tag",
  timeout: "timeout",
  verboseErrors: "verboseErrors",
};

export const CONFIG_OPTIONS = {
  [CONFIG.baseUrl]: {
    default: "",
    dependencies: [],
    required: false,
  },
  [CONFIG.dryRun]: {
    default: false,
    dependencies: [],
    required: false,
  },
  [CONFIG.duration]: {
    default: 0, // infinite
    dependencies: [[[CONFIG.runLoop], true]],
    required: false,
  },
  [CONFIG.files]: {
    default: "",
    dependencies: [],
    required: true,
  },
  [CONFIG.id]: {
    default: null,
    dependencies: [],
    required: false,
  },
  [CONFIG.name]: {
    default: "",
    dependencies: [],
    required: false,
  },
  [CONFIG.maxErrors]: {
    default: 10,
    dependencies: [[[CONFIG.runLoop], false]],
    required: false,
  },
  [CONFIG.randomize]: {
    default: false,
    dependencies: [[[CONFIG.runLoop], true]],
    required: false,
  },
  [CONFIG.runLoop]: {
    default: false,
    dependencies: [],
    required: false,
  },
  [CONFIG.servers]: {
    default: "",
    dependencies: [],
    required: true,
  },
  [CONFIG.sleep]: {
    default: 0,
    dependencies: [],
    required: false,
  },
  [CONFIG.strictJson]: {
    default: false,
    dependencies: [],
    required: false,
  },
  [CONFIG.tag]: {
    default: "",
    dependencies: [],
    required: false,
  },
  [CONFIG.timeout]: {
    default: 30,
    dependencies: [],
    required: false,
  },
  [CONFIG.verboseErrors]: {
    default: false,
    dependencies: [],
    required: false,
  },
};

export const removeConfigDependencies = (config) => {
  Object.entries(CONFIG_OPTIONS).forEach(([configKey, { dependencies }]) => {
    dependencies.forEach(([dependentOn, shouldExist]) => {
      if (config[dependentOn] !== shouldExist) {
        // eslint-disable-next-line no-param-reassign
        delete config[configKey];
      }
    });
  });
}

export const addDefaultsToConfig = (config) => {
  Object.entries(CONFIG_OPTIONS).forEach(([configKey, configOptions]) => {
    // eslint-disable-next-line no-param-reassign
    config[configKey] ??= configOptions.default;
  })
}

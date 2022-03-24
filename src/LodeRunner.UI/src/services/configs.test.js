import { checkConfigInputs } from "./configs";
import { CONFIG, addDefaultsToConfig, removeConfigDependencies } from "../models";

describe("checkConfigInputs", () => {
  it("should return no errors", () => {
    const inputs = {
      [CONFIG.servers]: ["", undefined, "server-1"],
      [CONFIG.files]: ["file-1", undefined, ""],
      [CONFIG.runLoop]: false,
      [CONFIG.sleep]: 0,
      [CONFIG.maxErrors]: 10,
      [CONFIG.timeout]: 30,
    };
    expect(() => checkConfigInputs(inputs)).not.toThrow();
  });

  it("should err on all checked inputs", () => {
    const inputs = {
      [CONFIG.servers]: ["", undefined],
      [CONFIG.files]: [undefined, ""],
      [CONFIG.runLoop]: true,
      [CONFIG.sleep]: -10,
      [CONFIG.maxErrors]: "-",
      [CONFIG.timeout]: -30,
    };

    const errors = {
      [CONFIG.servers]: `Missing ${CONFIG.servers} flag`,
      [CONFIG.files]: `Missing ${CONFIG.files} flag`,
      [CONFIG.duration]: "Must be a positive integer or zero",
      [CONFIG.sleep]: "Must be a positive integer or zero",
      [CONFIG.timeout]: "Must be a positive integer or zero",
    };

    try {
      checkConfigInputs(inputs);
      expect("Did not throw").toEqual();
    } catch (err) {
      expect(err).toEqual(errors);
    }
  });
});

describe("addDefaultsToConfig", () => {
  it("should replace all nulls with values", () => {
    const config = { [CONFIG.id]: "123" };
    addDefaultsToConfig(config);
    Object.values(config).forEach(configValue => {
      expect(configValue).not.toBeNull();
    })
  })
});

describe("removeConfigDependencies", () => {
  it("should remove max errors", () => {
    const config = { [CONFIG.id]: "123" };
    addDefaultsToConfig(config);
    config[CONFIG.runLoop] = true;
    removeConfigDependencies(config);
    expect(config[CONFIG.duration]).not.toBeUndefined();
    expect(config[CONFIG.randomize]).not.toBeUndefined();
    expect(config[CONFIG.maxErrors]).toBeUndefined();
  })
  it("should remove duration and randomize", () => {
    const config = { [CONFIG.id]: "123" };
    addDefaultsToConfig(config);
    config[CONFIG.runLoop] = false;
    removeConfigDependencies(config);
    expect(config[CONFIG.duration]).toBeUndefined();
    expect(config[CONFIG.randomize]).toBeUndefined();
    expect(config[CONFIG.maxErrors]).not.toBeUndefined();
  })
});

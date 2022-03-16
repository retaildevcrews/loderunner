import { checkConfigInputs, getConfigPayload } from "./configs";
import { CONFIG } from "../models";

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
      [CONFIG.maxErrors]: "Must be a positive integer or zero",
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

describe("getConfigPayload", () => {
  it("should set all inputs", () => {
    const inputs = {
      [CONFIG.baseUrl]: "test",
      [CONFIG.name]: "test",
      [CONFIG.tag]: "test",
      [CONFIG.runLoop]: true,
      [CONFIG.duration]: 0,
      [CONFIG.randomize]: false,
      [CONFIG.files]: ["file-01", "", undefined, "file-02"],
      [CONFIG.servers]: ["server-01", "", undefined, "server-02"],
      [CONFIG.maxErrors]: "10",
      [CONFIG.sleep]: "10",
      [CONFIG.timeout]: "10",
      [CONFIG.dryRun]: false,
      [CONFIG.strictJson]: true,
      [CONFIG.verbose]: true,
      [CONFIG.verboseErrors]: true,
    };

    const expectedPayload = {
      [CONFIG.baseUrl]: "test",
      [CONFIG.name]: "test",
      [CONFIG.tag]: "test",
      [CONFIG.runLoop]: true,
      [CONFIG.duration]: 0,
      [CONFIG.randomize]: false,
      [CONFIG.files]: ["file-01", "file-02"],
      [CONFIG.servers]: ["server-01", "server-02"],
      [CONFIG.maxErrors]: 10,
      [CONFIG.sleep]: 10,
      [CONFIG.timeout]: 10,
      [CONFIG.dryRun]: false,
      [CONFIG.strictJson]: true,
      [CONFIG.verbose]: true,
      [CONFIG.verboseErrors]: true,
    };

    expect(getConfigPayload(inputs)).toEqual(expectedPayload);
  });

  it("should set default inputs", () => {
    const inputs = {
      [CONFIG.baseUrl]: "",
      [CONFIG.name]: "",
      [CONFIG.tag]: "",
      [CONFIG.runLoop]: false,
      [CONFIG.duration]: 0,
      [CONFIG.randomize]: false,
      [CONFIG.files]: ["", undefined],
      [CONFIG.servers]: ["", undefined],
      [CONFIG.maxErrors]: "10",
      [CONFIG.sleep]: "10",
      [CONFIG.timeout]: "10",
      [CONFIG.dryRun]: false,
      [CONFIG.strictJson]: true,
      [CONFIG.verbose]: true,
      [CONFIG.verboseErrors]: true,
    };

    const expectedPayload = {
      [CONFIG.baseUrl]: "",
      [CONFIG.runLoop]: false,
      [CONFIG.files]: [],
      [CONFIG.servers]: [],
      [CONFIG.maxErrors]: 10,
      [CONFIG.sleep]: 10,
      [CONFIG.timeout]: 10,
      [CONFIG.dryRun]: false,
      [CONFIG.strictJson]: true,
      [CONFIG.verbose]: true,
      [CONFIG.verboseErrors]: true,
    };

    expect(getConfigPayload(inputs)).toEqual(expectedPayload);
  });
});

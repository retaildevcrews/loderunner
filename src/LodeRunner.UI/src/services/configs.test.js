import { checkConfigInputs } from "./configs";
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

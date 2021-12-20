import getAsBoolean from "./types";

describe("getAsBoolean", () => {
  it("should return input if of type boolean", () => {
    expect(getAsBoolean(true)).toEqual(true);
    expect(getAsBoolean(false)).toEqual(false);
  });

  it("should return true of type boolean", () => {
    expect(getAsBoolean("true")).toEqual(true);
    expect(getAsBoolean(1)).toEqual(true);
  });

  it("should return false of type boolean", () => {
    expect(getAsBoolean("false")).toEqual(false);
    expect(getAsBoolean("")).toEqual(false);
    expect(getAsBoolean(0)).toEqual(false);
    expect(getAsBoolean(undefined)).toEqual(false);
  });
});

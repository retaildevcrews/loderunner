import getMMMDYYYYhmma from "./datetime";

describe("Datetime Formatter: MMM D, YYYY @ h:mm a", () => {
  it("should be formatted", () => {
    const initialDateTime = "2021-01-01T13:02:00";
    const expectedDateTime = "Jan 1, 2021 @ 1:02 pm";
    expect(getMMMDYYYYhmma(initialDateTime)).toEqual(expectedDateTime);
  });

  it("should return undefined", () => {
    expect(getMMMDYYYYhmma(undefined)).toBeUndefined();
  });
});

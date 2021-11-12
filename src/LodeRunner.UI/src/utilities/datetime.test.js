import getMMMDYYYYhmma from "./datetime";

test("Datetime Formatter: MMM D, YYYY @ h:mm a", () => {
  const initialDateTime = "2021-01-01T13:02:00";
  const expectedDateTime = "Jan 1, 2021 @ 1:02 pm";

  expect(getMMMDYYYYhmma(initialDateTime)).toEqual(expectedDateTime);
});

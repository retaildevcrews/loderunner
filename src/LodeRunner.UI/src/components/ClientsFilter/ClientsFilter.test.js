import { sortByProperty, filterByStatus } from "./index";
import { SORT_TYPES } from "./constants";
import { CLIENT, CLIENT_STATUSES } from "../../models";

test("sortingByProperty", () => {
  const [type, { key }] = Object.entries(SORT_TYPES)[0];
  const initialItems = [{ [key]: 9 }, { [key]: 1 }, { [key]: 5 }];
  const expectedItems = [{ [key]: 1 }, { [key]: 5 }, { [key]: 9 }];

  expect(initialItems.sort(sortByProperty(type))).toStrictEqual(expectedItems);
});

describe("filterByStatus", () => {
  const initialItems = Object.entries(CLIENT_STATUSES).map((status) => ({
    [CLIENT.status]: status[1],
  }));
  it("should not filter anything", () => {
    const noFilterStatus = "";
    expect(initialItems.filter(filterByStatus(noFilterStatus)).length).toBe(
      initialItems.length
    );
  });
  it("should filter different statuses", () => {
    const filterStatus = initialItems[0][CLIENT.status];
    expect(initialItems.filter(filterByStatus(filterStatus)).length).toBe(1);
  });
});

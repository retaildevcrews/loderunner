import { sortByProperty } from ".";

const testLoadClients = [
  {
    id: 1,
    dateCreated: 2018,
  },
  {
    id: 2,
    dateCreated: 2020,
  },
  {
    id: 3,
    dateCreated: 2016,
  },
];

const sortedByDateClients = [
  {
    id: 3,
    dateCreated: 2016,
  },
  {
    id: 1,
    dateCreated: 2018,
  },
  {
    id: 2,
    dateCreated: 2020,
  },
];

test("sorting function sorts properly", () => {
  const sortedById = testLoadClients.sort(sortByProperty("id"));
  const sortedByDate = testLoadClients.sort(sortByProperty("dateCreated"));
  expect(sortedById).toStrictEqual(testLoadClients);
  expect(sortedByDate).toStrictEqual(sortedByDateClients);
});

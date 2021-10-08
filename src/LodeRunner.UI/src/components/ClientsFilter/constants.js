import { CLIENT, CLIENT_STATUSES } from "../../models";

const SORT_TYPES = {
  name: {
    key: CLIENT.name,
    label: "Name",
  },
  dateCreated: {
    key: CLIENT.startTime,
    label: "Date Created",
  },
};

const STATUSES = [
  {
    key: CLIENT_STATUSES.starting,
    label: "Starting",
  },
  {
    key: CLIENT_STATUSES.ready,
    label: "Ready",
  },
  {
    key: CLIENT_STATUSES.testing,
    label: "Busy",
  },
  {
    key: CLIENT_STATUSES.terminating,
    label: "Shutting Down",
  },
];

export { SORT_TYPES, STATUSES };

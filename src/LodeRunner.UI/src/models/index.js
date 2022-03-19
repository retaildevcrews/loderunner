import { CONFIG, CONFIG_OPTIONS } from "./configs";

export const CLIENT = {
  clientStatusId: "clientStatusId",
  lastStatusChange: "lastStatusChange",
  lastUpdated: "lastUpdated",
  loadClientId: "loadClientId",
  message: "message",
  name: "name",
  prometheus: "prometheus",
  region: "region",
  startTime: "startTime",
  startupArgs: "startupArgs",
  status: "status",
  tag: "tag",
  version: "version",
  zone: "zone",
};

export const CLIENT_STATUS_TYPES = {
  starting: "Starting",
  ready: "Ready",
  testing: "Testing",
  terminating: "Terminating",
};

export const CLIENT_STATUS = {
  id: "id",
  lastStatusChange: "lastStatusChange",
  lastUpdated: "lastUpdated",
  loadClient: "loadClient",
  message: "message",
  status: "status",
};

export const LOAD_CLIENT = {
  id: "id",
  name: "name",
  prometheus: "prometheus",
  region: "region",
  startupArgs: "startupArgs",
  startTime: "startTime",
  version: "version",
  zone: "zone",
};

export const TEST_RUN = {
  id: "id",
  name: "name",
  config: "loadTestConfig",
  clients: "loadClients",
  createdTime: "createdTime",
  scheduledStartTime: "startTime",
  finalCompletionTime: "completedTime",
  results: "clientResults",
};

export const RESULT = {
  client: "loadClient",
  completionTime: "completedTime",
  requestCount: "totalRequests",
  successfulRequestCount: "successfulRequests",
  failedRequestCount: "failedRequests",
};

export { CONFIG, CONFIG_OPTIONS };

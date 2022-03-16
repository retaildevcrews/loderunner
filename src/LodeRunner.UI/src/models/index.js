export const CLIENT = {
  clientStatusId: "clientStatusId",
  entityType: "entityType",
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
  entityType: "entityType",
  id: "id",
  lastStatusChange: "lastStatusChange",
  lastUpdated: "lastUpdated",
  loadClient: "loadClient",
  message: "message",
  status: "status",
};

export const LOAD_CLIENT = {
  entityType: "entityType",
  id: "id",
  name: "name",
  prometheus: "prometheus",
  region: "region",
  startupArgs: "startupArgs",
  startTime: "startTime",
  version: "version",
  zone: "zone",
};

export const CONFIG = {
  baseUrl: "baseURL",
  delayStart: "delayStart",
  dryRun: "dryRun",
  duration: "duration",
  entityType: "entityType",
  files: "files",
  id: "id",
  name: "name",
  maxErrors: "maxErrors",
  partitionKey: "partitionKey",
  randomize: "randomize",
  runLoop: "runLoop",
  servers: "server",
  sleep: "sleep",
  strictJson: "strictJson",
  tag: "tag",
  timeout: "timeout",
  verbose: "verbose",
  verboseErrors: "verboseErrors",
};

export const TEST_RUN = {
  entityType: "entityType",
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

import { writeApi } from "./utilities";
import { CLIENT, CONFIG, TEST_RUN } from "../models";

const getPostPayload = ({
  [TEST_RUN.name]: testRunName,
  [TEST_RUN.createdTime]: testRunCreatedTime,
  [TEST_RUN.scheduledStartTime]: testRunStartTime,
  [TEST_RUN.config]: config,
  [TEST_RUN.clients]: clients,
}) => {
  const configPayload = JSON.parse(JSON.stringify(config));
  delete configPayload[CONFIG.partitionKey];
  delete configPayload[CONFIG.entityType];

  const clientsPayload = clients.map(
    ({
      [CLIENT.loadClientId]: id,
      [CLIENT.name]: name,
      [CLIENT.version]: version,
      [CLIENT.region]: region,
      [CLIENT.zone]: zone,
      [CLIENT.prometheus]: prometheus,
      [CLIENT.startupArgs]: startupArgs,
      [CLIENT.startTime]: startTime,
    }) => ({
      id,
      name,
      version,
      region,
      zone,
      prometheus,
      startupArgs,
      startTime,
    })
  );

  return {
    [TEST_RUN.name]: testRunName,
    [TEST_RUN.createdTime]: testRunCreatedTime,
    [TEST_RUN.scheduledStartTime]: testRunStartTime,
    [TEST_RUN.config]: configPayload,
    [TEST_RUN.clients]: clientsPayload,
  };
};

const postTestRun = async (input) => {
  return writeApi("POST", "TestRuns")(getPostPayload(input));
};

export { postTestRun, getPostPayload };

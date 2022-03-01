import { writeApi } from "./utilities";
import { TEST_RUN } from "../models";

const postTestRun = async (
  config,
  clients,
  scheduledStartTime,
  testRunName
) => {
  const inputs = {
    [TEST_RUN.config]: config,
    [TEST_RUN.clients]: clients,
    [TEST_RUN.scheduledStartTime]: scheduledStartTime,
    [TEST_RUN.createdTime]: new Date().toISOString(),
    [TEST_RUN.name]: testRunName,
  };
  return writeApi("POST", "TestRuns")(inputs);
};

export default postTestRun;

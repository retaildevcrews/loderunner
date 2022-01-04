import { writeApi } from "./utilities";
import { TEST_RUN } from "../models";

const postTestRun = async (config, clients, scheduledStartTime) => {
  const inputs = {
    [TEST_RUN.config]: config,
    [TEST_RUN.clients]: clients,
    [TEST_RUN.scheduledStartTime]: scheduledStartTime,
  };
  return writeApi("POST", "testruns")(inputs);
};

export default postTestRun;

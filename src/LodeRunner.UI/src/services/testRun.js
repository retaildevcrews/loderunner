import { writeApi } from "./utilities";

const postTestRun = async (config, clients) => {
  const inputs = {
    loadTestConfig: config,
    loadClients: clients,
  };
  return writeApi("POST", "testruns")(inputs);
};

export default postTestRun;

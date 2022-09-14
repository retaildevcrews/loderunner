import { TEST_RUN } from "../models";
import { deleteApi, getApi, writeApi } from "./utilities";

const getTestRuns = async () => {
  const content = await getApi("testruns");
  return content || [];
};

const getTestRunById = async (testRunId) => {
  const content = await getApi(`testruns/${testRunId}`);
  return content || {};
};

const deleteTestRun = deleteApi("testruns");

const checkTestRunInputs = (inputs) => {
  const checkedInputs = [
    TEST_RUN.clients,
    TEST_RUN.name,
    TEST_RUN.scheduledStartTime,
  ];

  const errors = checkedInputs.reduce((errs, config) => {
    switch (config) {
      case TEST_RUN.clients:
        return inputs[config].length > 0
          ? errs
          : [...errs, "Need to select at least one load client."];
      case TEST_RUN.name:
        return inputs[config]
          ? errs
          : [...errs, "Need to set a test run name."];
      case TEST_RUN.scheduledStartTime:
        return inputs[config]
          ? errs
          : [...errs, "Need to schedule the test run start."];
      default:
        return errs;
    }
  }, []);

  if (errors.length > 0) {
    throw errors;
  }
};

const postTestRun = async (inputs) => {
  checkTestRunInputs(inputs);
  return writeApi("POST", "TestRuns")(inputs);
};

const stopTestRun = async (id) => {
  const inputs = await getTestRunById(id);
  inputs.hardStop = true;
  return writeApi("PUT", `TestRuns/${id}`)(inputs);
};

export { deleteTestRun, getTestRunById, getTestRuns, postTestRun, stopTestRun };

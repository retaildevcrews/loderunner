import { TEST_RUN } from "../models";
import { writeApi } from "./utilities";

const checkTestRunInputs = (inputs) => {
  const checkedInputs = [
    TEST_RUN.clients,
    TEST_RUN.scheduledStartTime,
  ];

  const errors = checkedInputs.reduce((errs, config) => {
    switch (config) {
      case TEST_RUN.clients:
        return inputs[config].length > 0 ? errs : [...errs, "Need to select at least one load client."];
      case TEST_RUN.scheduledStartTime:
        return inputs[config] || [...errs, "Need to schedule the test run start."];
      default:
        return errs;
    };
  }, []);

  if (errors.length > 0) {
    throw errors;
  }
};

const postTestRun = async (inputs) => {
  checkTestRunInputs(inputs);
  return writeApi("POST", "TestRuns")(inputs);
};

export {
  checkTestRunInputs,
  postTestRun,
};

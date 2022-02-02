import { getApi, deleteApi } from "./utilities";

const getResults = async () => {
  const content = await getApi("testruns");
  return content || [];
};

const getResultById = async (testRunId) => {
  const content = await getApi(`testruns/${testRunId}`);
  return content || {};
};

const deleteTestRun = deleteApi("testruns");

export { getResults, getResultById, deleteTestRun };

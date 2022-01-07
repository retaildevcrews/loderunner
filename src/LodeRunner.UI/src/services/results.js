import { getApi } from "./utilities";

const getResults = async () => {
  const content = await getApi("testruns");
  return content || [];
};

const getResultById = async (testRunId) => {
  const content = await getApi(`testruns/${testRunId}`);
  return content || {};
};

export { getResults, getResultById };

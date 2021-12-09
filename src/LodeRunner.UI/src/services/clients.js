import { getApi } from "./utilities";

const getClients = async () => {
  const content = await getApi("clients");

  return content || [];
};

// eslint-disable-next-line import/prefer-default-export
export { getClients };

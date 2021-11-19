const fetchApi = (endpoint) => async () => {
  try {
    const res = await fetch(`${process.env.REACT_APP_SERVER}/api/${endpoint}`);
    const body = await res.json();

    if (Array.isArray(body)) {
      return body.filter((c) => c);
    }
    return body;
  } catch (err) {
    // eslint-disable-next-line
    console.error(`Issue fetching ${endpoint}`, err);
    return [];
  }
};

const fetchClients = fetchApi("clients");
const fetchConfigs = fetchApi("configs");

export { fetchClients, fetchConfigs };

const STATUS_CODE_NO_CONTENT = 204;

let { REACT_APP_SERVER } = process.env;
if (REACT_APP_SERVER && REACT_APP_SERVER.endsWith("/")) {
  REACT_APP_SERVER = REACT_APP_SERVER.slice(0, -1);
}

const getResponseBody = async (res) => {
  const contentType = res.headers.get("content-type");
  let body;

  // check and resolve for status codes
  if (res.status === STATUS_CODE_NO_CONTENT) {
    return undefined;
  }

  // check and resolve for content-type
  if (!contentType) {
    throw new Error("No content in response");
  } else if (contentType.includes("text")) {
    body = await res.text();
  } else if (contentType.includes("json")) {
    body = await res.json();
  } else {
    throw new Error("Unhandled response type");
  }

  if (!res.ok) {
    if (typeof body !== "string") {
      throw new Error(JSON.stringify(body));
    }
    throw new Error(body);
  }

  return body;
};

const getApi = async (endpoint) => {
  try {
    const res = await fetch(`${REACT_APP_SERVER}/api/${endpoint}`);
    const body = await getResponseBody(res);
    return body;
  } catch (err) {
    // eslint-disable-next-line no-console
    console.error(`Issue with GET request at ${endpoint}`, err);
    throw err;
  }
};

const writeApi = (method, endpoint) => async (payload) => {
  try {
    const res = await fetch(`${REACT_APP_SERVER}/api/${endpoint}`, {
      method,
      headers: {
        accept: "application/json",
        "Content-Type": "application/json",
      },
      body: JSON.stringify(payload),
    });
    return await getResponseBody(res);
  } catch (err) {
    // eslint-disable-next-line no-console
    console.error(`Issue with ${method} request at ${endpoint}`, err);
    throw err;
  }
};

const deleteApi = (endpoint) => async (id) => {
  try {
    if (!id) {
      throw new Error("Missing ID");
    }

    const res = await fetch(`${REACT_APP_SERVER}/api/${endpoint}/${id}`, {
      method: "DELETE",
    });
    const body = await getResponseBody(res);
    return body;
  } catch (err) {
    // eslint-disable-next-line no-console
    console.error(`Issue with DELETE request at ${endpoint}`, err);
    throw err;
  }
};

export { getApi, writeApi, deleteApi, getResponseBody };

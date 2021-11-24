const getApi = (endpoint) => async () => {
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

const postApi = (endpoint) => async (data) => {
  try {
    const res = await fetch(`${process.env.REACT_APP_SERVER}/api/${endpoint}`, {
      method: "POST",
      headers: {
        accept: "application/json",
        "Content-Type": "application/json",
      },
      body: JSON.stringify(data),
    });

    const contentType = res.headers.get("content-type");
    let body;

    if (contentType.includes("text")) {
      body = await res.text();
    } else if (contentType.includes("json")) {
      body = await res.json();
    } else {
      throw new Error("Unhandled response type");
    }

    if (!res.ok) {
      throw new Error(JSON.stringify(body));
    }

    return {};
  } catch (err) {
    // eslint-disable-next-line
    console.error(`Issue fetching ${endpoint}`, err);
    throw err;
  }
};

export { getApi, postApi };

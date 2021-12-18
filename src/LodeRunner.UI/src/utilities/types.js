const getAsBoolean = (value) => {
  if (typeof value === "boolean") {
    return value;
  }

  return !(value === "false" || !value);
};

export default getAsBoolean;

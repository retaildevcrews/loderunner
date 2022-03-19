const months = [
  "Jan",
  "Feb",
  "Mar",
  "Apr",
  "May",
  "Jun",
  "Jul",
  "Aug",
  "Sep",
  "Oct",
  "Nov",
  "Dec",
];

function getMMMDYYYYhmma(datetime) {
  if (!datetime) {
    return undefined;
  }

  // Example format: Nov 1, 2021 @ 3:04pm
  const date = new Date(datetime);
  const hours = date.getHours();
  const minutes = date.getMinutes();

  const formattedDate = `${
    months[date.getMonth()]
  } ${date.getDate()}, ${date.getFullYear()}`;

  const formattedHours = hours % 12 || 12;
  const formattedMinutes = minutes < 10 ? `0${minutes}` : minutes;
  const formattedTime = `${formattedHours}:${formattedMinutes} ${
    hours > 11 ? "pm" : "am"
  }`;

  return `${formattedDate} - ${formattedTime}`;
}

export default getMMMDYYYYhmma;

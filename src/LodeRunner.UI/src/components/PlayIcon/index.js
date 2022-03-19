import { useState } from "react";
import PropTypes from "prop-types";

const PlayIcon = ({ fillColor, hoverColor, width }) => {
  const [currentColor, setCurrentColor] = useState(fillColor);

  return (
    <svg
      xmlns="http://www.w3.org/2000/svg"
      x="0px"
      y="0px"
      width={width}
      height={width}
      viewBox="0 0 60 60"
      onPointerEnter={() => setCurrentColor(hoverColor)}
      onPointerLeave={() => setCurrentColor(fillColor)}
    >
      <g>
        <path
          fill={currentColor}
          d="M45.563,29.174l-22-15c-0.307-0.208-0.703-0.231-1.031-0.058C22.205,14.289,22,14.629,22,15v30 c0,0.371,0.205,0.711,0.533,0.884C22.679,45.962,22.84,46,23,46c0.197,0,0.394-0.059,0.563-0.174l22-15 C45.836,30.64,46,30.331,46,30S45.836,29.36,45.563,29.174z M24,43.107V16.893L43.225,30L24,43.107z"
        />
        <path
          fill={currentColor}
          d="M30,0C13.458,0,0,13.458,0,30s13.458,30,30,30s30-13.458,30-30S46.542,0,30,0z M30,58C14.561,58,2,45.439,2,30 S14.561,2,30,2s28,12.561,28,28S45.439,58,30,58z"
        />
      </g>
    </svg>
  );
};

PlayIcon.defaultProps = {
  fillColor: "var(--c-neutral-darkest)",
  hoverColor: "var(--c-neutral)",
  width: "1em",
};

PlayIcon.propTypes = {
  fillColor: PropTypes.string,
  hoverColor: PropTypes.string,
  width: PropTypes.string,
};

export default PlayIcon;

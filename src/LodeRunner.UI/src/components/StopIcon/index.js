import { useState } from "react";
import PropTypes from "prop-types";

const StopIcon = ({ fillColor, hoverColor, width }) => {
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
          d="M 30 0 C 13.458 0 0 13.458 0 30 s 13.458 30 30 30 s 30 -13.458 30 -30 S 46.542 0 30 0 z M 30 58 C 14.561 58 2 45.439 2 30 S 14.561 2 30 2 s 28 12.561 28 28 S 45.439 58 30 58 z M 18 16 C 17 16 16 17 16 18 V 42 C 16 43 17 44 18 44 l 24 0 C 43 44 44 43 44 42 l 0 -24 C 44 17 43 16 42 16 z M 18 42 V 18 L 42 18 L 42 42 z"
        />
        <path
          fill={currentColor}
          d="M30,0C13.458,0,0,13.458,0,30s13.458,30,30,30s30-13.458,30-30S46.542,0,30,0z M30,58C14.561,58,2,45.439,2,30 S14.561,2,30,2s28,12.561,28,28S45.439,58,30,58z"
        />
      </g>
    </svg>
  );
};

StopIcon.defaultProps = {
  fillColor: "var(--c-neutral-darkest)",
  hoverColor: "var(--c-neutral)",
  width: "1em",
};

StopIcon.propTypes = {
  fillColor: PropTypes.string,
  hoverColor: PropTypes.string,
  width: PropTypes.string,
};

export default StopIcon;

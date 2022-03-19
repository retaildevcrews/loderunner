import { useState } from "react";
import PropTypes from "prop-types";

const TrashIcon = ({ fillColor, hoverColor, width }) => {
  const [currentColor, setCurrentColor] = useState(fillColor);

  return (
    <svg
      xmlns="http://www.w3.org/2000/svg"
      viewBox="0 0 487.116 487.116"
      x="0px"
      y="0px"
      width={width}
      height={width}
      onPointerEnter={() => setCurrentColor(hoverColor)}
      onPointerLeave={() => setCurrentColor(fillColor)}
    >
      <path
        fill={currentColor}
        d="M440.446,65.222H327.617C325.365,28.948,288.518,0,243.558,0c-44.97,0-81.817,28.948-84.069,65.222H46.669
        c-5.728,0-10.364,4.635-10.364,10.364v51.547c0,5.728,4.636,10.364,10.364,10.364h18.537l30.935,340.195
        c0.486,5.334,4.96,9.423,10.324,9.423H380.64c5.365,0,9.839-4.089,10.324-9.423L421.9,137.497h18.547
        c5.729,0,10.364-4.636,10.364-10.364V75.586C450.811,69.856,446.176,65.222,440.446,65.222z M371.177,466.387H115.929
        L86.021,137.518h315.065L371.177,466.387z M243.558,20.728c33.42,0,60.879,19.656,63.268,44.493H180.28
        C182.669,40.384,210.138,20.728,243.558,20.728z M430.082,116.769H57.033V85.95h373.049V116.769z"
      />
    </svg>
  );
};

TrashIcon.defaultProps = {
  fillColor: "var(--c-neutral-darkest)",
  hoverColor: "var(--c-neutral)",
  width: "1em",
};

TrashIcon.propTypes = {
  fillColor: PropTypes.string,
  hoverColor: PropTypes.string,
  width: PropTypes.string,
};

export default TrashIcon;

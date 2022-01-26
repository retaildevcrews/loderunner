import PropTypes from "prop-types";
import { useEffect } from "react";

const IntegerInput = ({
  label,
  description,
  elRef,
  inputName,
  units,
  defaultValue,
}) => {
  console.log({ inputName, defaultValue });
  useEffect(() => {
    // eslint-disable-next-line no-param-reassign
    elRef.current.value = defaultValue;
  }, []);
  return (
    <div className="configform-input">
      <label htmlFor={inputName}>
        <span className="configform-input-label">{label}: </span>
        {description}
        <br />
        <input
          key={inputName}
          ref={elRef}
          type="number"
          step="1"
          name={inputName}
          defaultValue={defaultValue}
        />
        &nbsp;
        {units}
      </label>
    </div>
  );
};

IntegerInput.propTypes = {
  label: PropTypes.string.isRequired,
  description: PropTypes.string.isRequired,
  elRef: PropTypes.oneOfType([
    PropTypes.func,
    PropTypes.shape({
      // eslint-disable-next-line react/forbid-prop-types
      current: PropTypes.any,
    }),
  ]).isRequired,
  inputName: PropTypes.string.isRequired,
  units: PropTypes.string,
  defaultValue: PropTypes.number,
};

IntegerInput.defaultProps = {
  units: "",
  defaultValue: 0,
};

export default IntegerInput;

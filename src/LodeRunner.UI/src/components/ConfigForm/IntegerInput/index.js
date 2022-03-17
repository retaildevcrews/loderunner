import PropTypes from "prop-types";

const IntegerInput = ({
  label,
  description,
  elRef,
  inputName,
  units,
}) => (
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
      />
      &nbsp;
      {units}
    </label>
  </div>
);

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
};

IntegerInput.defaultProps = {
  units: "",
};

export default IntegerInput;

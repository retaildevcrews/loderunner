import PropTypes from "prop-types";

const RadioInput = ({
  label,
  description,
  elRef,
  inputName,
  options,
  onChange,
}) => (
  <div className="configform-input" onChange={onChange}>
    <span className="configform-input-label"> {label}: </span>
    {description}
    <br />
    {options.map(({ label: optionLabel, value }) => (
      <div key={`${inputName}-${value}`}>
        <input
          type="radio"
          name={inputName}
          value={value}
          defaultChecked={elRef.current === value}
        />
        {optionLabel}
        <br />
      </div>
    ))}
  </div>
);

RadioInput.propTypes = {
  label: PropTypes.string.isRequired,
  description: PropTypes.string.isRequired,
  elRef: PropTypes.oneOfType([
    PropTypes.func,
    // eslint-disable-next-line react/forbid-prop-types
    PropTypes.shape({ current: PropTypes.any }),
  ]).isRequired,
  inputName: PropTypes.string.isRequired,
  options: PropTypes.arrayOf(
    PropTypes.shape({ label: PropTypes.string, value: PropTypes.bool })
  ).isRequired,
  onChange: PropTypes.func.isRequired,
};

export default RadioInput;

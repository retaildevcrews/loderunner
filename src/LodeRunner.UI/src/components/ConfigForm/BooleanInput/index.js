import PropTypes from "prop-types";

const BooleanInput = ({ label, description, elRef, inputName, onChange }) => (
  <div className="configform-input" onChange={onChange}>
    <label htmlFor={inputName}>
      <span className="configform-input-label"> {label}: </span>
      {description}
      <br />
      <div key={`${inputName}-true`}>
        <input
          type="radio"
          name={inputName}
          value
          defaultChecked={elRef.current === true}
        />
        True
        <br />
      </div>
      <div key={`${inputName}-false`}>
        <input
          type="radio"
          name={inputName}
          value={false}
          defaultChecked={elRef.current === false}
        />
        False
        <br />
      </div>
    </label>
  </div>
);

BooleanInput.propTypes = {
  label: PropTypes.string.isRequired,
  description: PropTypes.string.isRequired,
  elRef: PropTypes.oneOfType([
    PropTypes.func,
    // eslint-disable-next-line react/forbid-prop-types
    PropTypes.shape({ current: PropTypes.any }),
  ]).isRequired,
  inputName: PropTypes.string.isRequired,
  onChange: PropTypes.func.isRequired,
};

export default BooleanInput;

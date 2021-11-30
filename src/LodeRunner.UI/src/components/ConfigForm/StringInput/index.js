import PropTypes from "prop-types";

const StringInput = ({ label, description, elRef, inputName }) => (
  <div className="configform-input">
    <span className="configform-input-label">{label}: </span>
    {description}
    <br />
    <input ref={elRef} type="string" name={inputName} defaultValue="" />
  </div>
);

StringInput.propTypes = {
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
};

export default StringInput;

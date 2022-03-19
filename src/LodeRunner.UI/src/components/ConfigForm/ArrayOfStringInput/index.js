import React, { useEffect, useState } from "react";
import PropTypes from "prop-types";
import "./styles.css";

const ArrayOfStringInput = ({
  label,
  description,
  flagRefs,
  setFlagRefs,
  inputName,
}) => {
  const [editedFlag, setEditedFlag] = useState({
    id: flagRefs.length,
    index: undefined,
  });

  useEffect(() => {
    if (editedFlag.index === undefined) {
      return;
    }

    const newFlags =
      // Add new input or remove input
      editedFlag.index === -1
        ? [...flagRefs, { id: editedFlag.id, ref: React.createRef("") }]
        : [
            ...flagRefs.slice(0, editedFlag.index),
            ...flagRefs.slice(editedFlag.index + 1),
          ];

    setFlagRefs(newFlags);
  }, [editedFlag]);

  return (
    <div className="configform-input arrayofstringinput">
      <label htmlFor={inputName}>
        <span className="configform-input-label">{label}: </span>
        {description}
        <br />
        {flagRefs.map(({ id, initialValue }, index) => (
          <div className="arrayofstringinput-input" key={`${inputName}-${id}`}>
            <input ref={element => flagRefs[index].ref = element } type="string" name={inputName} defaultValue={initialValue} />
            &nbsp;
            {flagRefs.length - 1 !== index ? (
              <>
                <button
                  type="button"
                  onClick={() => setEditedFlag({ id: editedFlag.id, index })}
                >
                  -
                </button>
                <br />
              </>
            ) : (
              <button
                type="button"
                onClick={() => setEditedFlag({ id: id + 1, index: -1 })
                }
              >
                +
              </button>
            )}
            <br />
          </div>
        ))}
      </label>
    </div>
  );
};

ArrayOfStringInput.propTypes = {
  label: PropTypes.string.isRequired,
  description: PropTypes.string.isRequired,
  flagRefs: PropTypes.arrayOf(
    PropTypes.oneOfType([
      PropTypes.func,
      // eslint-disable-next-line react/forbid-prop-types
      PropTypes.shape({ current: PropTypes.any }),
    ])
  ).isRequired,
  setFlagRefs: PropTypes.func.isRequired,
  inputName: PropTypes.string.isRequired,
};

export default ArrayOfStringInput;

import PropTypes from "prop-types";
import { MODAL_CONTENT } from "../../utilities/constants";
import "./styles.css";

const Modal = ({ children, content, setContent }) => {
  const closeModal = () => setContent(MODAL_CONTENT.closed);

  return (
    <>
      <div role="presentation" className="modal-wrapper" onClick={closeModal} />
      <div
        role="presentation"
        className={`modal modal-${content.toLowerCase()}`}
        onClick={(e) => e.stopPropagation()}
      >
        <div className="modal-header">
          <button
            className="modal-exit"
            type="button"
            onClick={closeModal}
            onKeyDown={closeModal}
            aria-label="Close"
          >
            x
          </button>
        </div>
        {children}
      </div>
    </>
  );
};

Modal.propTypes = {
  children: PropTypes.node.isRequired,
  content: PropTypes.string,
  setContent: PropTypes.func.isRequired,
};

Modal.defaultProps = {
  content: MODAL_CONTENT.closed,
};

export default Modal;

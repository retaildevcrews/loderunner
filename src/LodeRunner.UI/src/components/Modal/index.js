import { useContext } from "react";
import PropTypes from "prop-types";
import { DisplayContext } from "../../contexts";
import "./styles.css";

const Modal = ({ children }) => {
  const { modalContent, setModalContent } = useContext(DisplayContext);
  const closeModal = () => setModalContent(undefined);

  return (
    <div role="presentation" className="modal-wrapper" onClick={closeModal}>
      <div
        role="presentation"
        className={`modal ${modalContent}`}
        onClick={(e) => e.stopPropagation()}
      >
        <div className="modal-header">
          <button
            className="modal-exit"
            type="button"
            onClick={closeModal}
            onKeyDown={closeModal}
          >
            x
          </button>
        </div>
        {children}
      </div>
    </div>
  );
};

Modal.propTypes = {
  children: PropTypes.node.isRequired,
};

export default Modal;

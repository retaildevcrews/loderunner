import { useContext } from "react";
import { PendingFeatureContext } from "../../contexts";
import magicWord from "../../images/the-magic-word.gif";
import "./styles.css";

const PendingFeature = () => {
  const { setIsPendingFeatureOpen } = useContext(PendingFeatureContext);

  return (
    <div
      role="presentation"
      className="modal-wrapper"
      onClick={() => setIsPendingFeatureOpen(false)}
    >
      <div
        role="presentation"
        className="modal pendingfeature"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="pendingfeature-header">
          <span>Uh oh. You&apos;ve stumbled upon a pending feature!</span>
          <button
            className="pendingfeature-exit"
            type="button"
            onClick={() => setIsPendingFeatureOpen(false)}
            onKeyDown={() => setIsPendingFeatureOpen(false)}
          >
            x
          </button>
        </div>
        <img alt="Pending Feature" src={magicWord} />
      </div>
    </div>
  );
};

export default PendingFeature;

import magicWord from "../../images/the-magic-word.gif";
import "./styles.css";

const PendingFeature = () => {
  return (
    <>
      <div className="pendingfeature-header">
        <span>Uh oh. You&apos;ve stumbled upon a pending feature!</span>
      </div>
      <img alt="Pending Feature" src={magicWord} />
    </>
  );
};

export default PendingFeature;

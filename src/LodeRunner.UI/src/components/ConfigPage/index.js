import PropTypes from "prop-types";
import { A } from "hookrouter";
import ConfigForm from "../ConfigForm";
import "./styles.css";

const ConfigPage = ({ configId }) => {
  return (
    <div className="config">
      <div className="page-header">
        <h1>
          Load Test Config
        </h1>
        <A href="/" className="unset navigation">
          Load Test Submission
        </A>
      </div>
      <ConfigForm openedConfigId={configId}/>
    </div>
  );
};

ConfigPage.propTypes = {
    configId: PropTypes.string.isRequired,
  };
  
export default ConfigPage;
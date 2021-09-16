import React, { useContext } from "react";
import PropTypes from "prop-types";
import { LoadClientContext } from "../../contexts";

import "./styles.css";

const LoadClientDetails = (props) => {
  const { currClientDetails, handleClose } = props;
  const { loadClients } = useContext(LoadClientContext);
  const details = loadClients[currClientDetails];

  return (
    <div className="main">
      <div className="popup-box">
        <div className="box">
          <button
            type="button"
            className="close-icon"
            onClick={handleClose}
            onKeyDown={handleClose}
          >
            x
          </button>
          <h1>{details.name}</h1>
          <p>version: {details.version}</p>
          <p>ID: {details.id}</p>
          <p>Region: {details.region}</p>
          <p>Zone: {details.zone}</p>
          <p>Scheduler: {details.scheduler}</p>
          <p>Status: {details.currstatus}</p>
          <p>Metrics: {details.metrics}</p>
        </div>
      </div>
    </div>
  );
};

LoadClientDetails.propTypes = {
  currClientDetails: PropTypes.number.isRequired,
  handleClose: PropTypes.func.isRequired,
};

export default LoadClientDetails;

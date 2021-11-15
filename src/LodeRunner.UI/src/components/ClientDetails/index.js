import React, { useContext } from "react";
import PropTypes from "prop-types";
import Pencil from "../Pencil";
import { ClientContext, PendingFeatureContext } from "../../contexts";
import { CLIENT } from "../../models";
import getMMMDYYYYhmma from "../../utilities/datetime";
import "./styles.css";

const ClientDetails = (props) => {
  const { clientDetailsIndex, closeModal } = props;
  const { clients } = useContext(ClientContext);
  const { setIsPendingFeatureOpen } = useContext(PendingFeatureContext);

  const {
    [CLIENT.clientStatusId]: statusId,
    [CLIENT.lastStatusChange]: lastStatusChange,
    [CLIENT.lastUpdated]: lastUpdated,
    [CLIENT.loadClientId]: clientId,
    [CLIENT.message]: message,
    [CLIENT.name]: name,
    [CLIENT.prometheus]: prometheus,
    [CLIENT.region]: region,
    [CLIENT.startTime]: startTime,
    [CLIENT.startupArgs]: startupArgs,
    [CLIENT.status]: status,
    [CLIENT.version]: version,
    [CLIENT.zone]: zone,
  } = clients[clientDetailsIndex];

  return (
    <div className="clientdetails">
      <div className="clientdetails-header">
        <div>
          <h1>
            <span className="clientdetails-key">Name:&nbsp;</span>
            {name || "Unknown"}
            <button
              className="unset"
              type="button"
              onClick={() => setIsPendingFeatureOpen(true)}
              onKeyDown={() => setIsPendingFeatureOpen(true)}
            >
              <Pencil fillColor="#2c7f84" hoverColor="#24b2b9" width="1em" />
            </button>
          </h1>
          <div>
            <span className="clientdetails-key">Updated: </span>
            {getMMMDYYYYhmma(lastUpdated)}
          </div>
        </div>
        <button
          className="clientdetails-header-exit"
          type="button"
          onClick={closeModal}
          onKeyDown={closeModal}
        >
          x
        </button>
      </div>
      <h2 className="clientdetails-sectiontitle">
        <span className="clientdetails-key">Status: </span>
        {status}
      </h2>
      <div>
        <span className="clientdetails-key">Status Changed: </span>
        {getMMMDYYYYhmma(lastStatusChange)}
      </div>
      <div>
        <span className="clientdetails-key">Message: </span>
        {message}
      </div>
      <div>
        <span className="clientdetails-key">Status ID: </span>
        {statusId}
      </div>
      <h2 className="clientdetails-sectiontitle">
        <span className="clientdetails-key">Startup Arguments: </span>
        {startupArgs}
      </h2>
      <div>
        <span className="clientdetails-key">Region: </span>
        {region}
      </div>
      <div>
        <span className="clientdetails-key">Zone: </span>
        {zone}
      </div>
      <div>
        <span className="clientdetails-key">Prometheus Enabled: </span>
        {prometheus.toString()}
      </div>
      <div>
        <span className="clientdetails-key">Deployed: </span>
        {getMMMDYYYYhmma(startTime)}
      </div>
      <div>
        <span className="clientdetails-key">LodeRunner Version: </span>
        {version}
      </div>
      <div>
        <span className="clientdetails-key">LodeRunner ID: </span>
        {clientId}
      </div>
    </div>
  );
};

ClientDetails.propTypes = {
  clientDetailsIndex: PropTypes.number.isRequired,
  closeModal: PropTypes.func.isRequired,
};

export default ClientDetails;

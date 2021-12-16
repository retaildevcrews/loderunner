import React, { useContext } from "react";
import Pencil from "../Pencil";
import { ClientsContext, DisplayContext } from "../../contexts";
import { CLIENT } from "../../models";
import { MAIN_CONTENT, MODAL_CONTENT } from "../../utilities/constants";
import getMMMDYYYYhmma from "../../utilities/datetime";
import "./styles.css";

const ClientDetails = () => {
  const { setMainContent, setModalContent } = useContext(DisplayContext);
  const { clients, setOpenedClientDetailsIndex, openedClientDetailsIndex } =
    useContext(ClientsContext);

  const openPendingFeatureModal = () =>
    setModalContent(MODAL_CONTENT.pendingFeature);

  const closeClientDetails = () => {
    setMainContent(MAIN_CONTENT.configs);

    setOpenedClientDetailsIndex(-1);
  };

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
    [CLIENT.status]: status,
    [CLIENT.tag]: tag,
    [CLIENT.version]: version,
    [CLIENT.zone]: zone,
  } = clients[openedClientDetailsIndex];

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
              onClick={openPendingFeatureModal}
              onKeyDown={openPendingFeatureModal}
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
          onClick={closeClientDetails}
          onKeyDown={closeClientDetails}
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
        <span className="clientdetails-key">Startup Arguments </span>
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
        <span className="clientdetails-key">Log & App Insights Tag: </span>
        {tag || "--"}
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

export default ClientDetails;

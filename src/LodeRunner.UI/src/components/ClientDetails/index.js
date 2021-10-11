import React, { useContext } from "react";
import PropTypes from "prop-types";
import { ClientContext } from "../../contexts";
import { CLIENT } from "../../models";
import "./styles.css";

const ClientDetails = (props) => {
  const { clientDetailsIndex, closeModal } = props;
  const { clients } = useContext(ClientContext);

  const {
    [CLIENT.name]: name,
    [CLIENT.loadClientId]: id,
    [CLIENT.status]: status,
    [CLIENT.version]: version,
    [CLIENT.region]: region,
    [CLIENT.zone]: zone,
    [CLIENT.prometheus]: prometheus,
    [CLIENT.startupArgs]: startupArgs,
    [CLIENT.startTime]: startTime,
    [CLIENT.message]: message,
  } = clients[clientDetailsIndex];

  return (
    <>
      <button type="button" onClick={closeModal} onKeyDown={closeModal}>
        x
      </button>
      <h1>Name: {name}</h1>
      <p>ID: {id}</p>
      <p>Status: {status}</p>
      <p>version: {version}</p>
      <p>Region: {region}</p>
      <p>Zone: {zone}</p>
      <p>Prometheus: {prometheus}</p>
      <p>Startup Arguments: {startupArgs}</p>
      <p>Start Date: {startTime}</p>
      <p>Message: {message}</p>
    </>
  );
};

ClientDetails.propTypes = {
  clientDetailsIndex: PropTypes.number.isRequired,
  closeModal: PropTypes.func.isRequired,
};

export default ClientDetails;

import { useContext, useState } from "react";
import PropTypes from "prop-types";
import CheckMark from "../CheckMark";
import { ClientContext } from "../../contexts";
import { CLIENT, CLIENT_STATUSES } from "../../models";
import "./styles.css";

const Clients = ({
  setFetchClientsInterval,
  openClientDetails,
  openedClientDetailsIndex,
  closeClientDetails,
}) => {
  const { clients } = useContext(ClientContext);
  const [selectedClients, setSelectedClients] = useState({});

  function toggleClient(loadClientId) {
    setSelectedClients({
      ...selectedClients,
      [loadClientId]: !selectedClients[loadClientId],
    });
  }

  const toggleClientDetails = (index) => {
    if (index === openedClientDetailsIndex) {
      closeClientDetails();
    } else {
      openClientDetails(index);
    }
  };

  return (
    <div className="clients">
      <h1>LodeRunner</h1>
      <h3>Client Mode</h3>
      <select
        defaultValue="0"
        onChange={({ target }) => setFetchClientsInterval(target.value)}
      >
        <option value="0">Auto Refresh</option>
        <option value="5000">5 seconds</option>
        <option value="15000">15 seconds</option>
        <option value="30000">30 seconds</option>
        <option value="60000">1 minute</option>
      </select>
      <div>
        {clients.map((c, index) => {
          const {
            [CLIENT.loadClientId]: loadClientId,
            [CLIENT.status]: status,
            [CLIENT.name]: name,
          } = c;
          return (
            <div key={loadClientId} className="clients-item">
              <button
                className="clients-item-select"
                type="button"
                onClick={() => toggleClient(loadClientId)}
                onKeyDown={() => toggleClient(loadClientId)}
              >
                {name || "Unknown"}
                {selectedClients[loadClientId] && (
                  <CheckMark fillColor="white" width="1em" />
                )}
              </button>
              <button
                className={`clients-item-status ${
                  status === CLIENT_STATUSES.ready ? "ready" : "pending"
                } ${openedClientDetailsIndex === index && "selected"}`}
                type="button"
                title={status}
                aria-label={status}
                onClick={() => toggleClientDetails(index)}
                onKeyDown={() => toggleClientDetails(index)}
              >
                {openedClientDetailsIndex === index && ">"}
              </button>
            </div>
          );
        })}
      </div>
    </div>
  );
};

Clients.propTypes = {
  setFetchClientsInterval: PropTypes.func.isRequired,
  openClientDetails: PropTypes.func.isRequired,
  openedClientDetailsIndex: PropTypes.number.isRequired,
  closeClientDetails: PropTypes.func.isRequired,
};

export default Clients;

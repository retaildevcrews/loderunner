import { useContext, useState } from "react";
import PropTypes from "prop-types";
import CheckMark from "../CheckMark";
import { ClientsContext, DisplayContext } from "../../contexts";
import { CLIENT, CLIENT_STATUSES } from "../../models";
import { MAIN_CONTENT } from "../../utilities/constants";
import "./styles.css";

const Clients = ({ setFetchClientsInterval }) => {
  const [selectedClients, setSelectedClients] = useState({});
  const { setMainContent } = useContext(DisplayContext);
  const { clients, setOpenedClientDetailsIndex, openedClientDetailsIndex } =
    useContext(ClientsContext);

  function toggleClient(loadClientId) {
    setSelectedClients({
      ...selectedClients,
      [loadClientId]: !selectedClients[loadClientId],
    });
  }

  const toggleClientDetails = (index) => {
    if (index === openedClientDetailsIndex) {
      setMainContent(MAIN_CONTENT.configs);
      setOpenedClientDetailsIndex(-1);
    } else {
      setMainContent(MAIN_CONTENT.clientDetails);
      setOpenedClientDetailsIndex(index);
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
        <option value="0">Auto Refresh: OFF</option>
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
};

export default Clients;

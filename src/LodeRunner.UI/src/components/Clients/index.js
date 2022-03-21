import { useContext } from "react";
import PropTypes from "prop-types";
import CheckMarkIcon from "../CheckMarkIcon";
import { ClientsContext, TestPageContext } from "../../contexts";
import { CLIENT, CLIENT_STATUS_TYPES } from "../../models";
import { MAIN_CONTENT } from "../../utilities/constants";
import "./styles.css";

const Clients = ({ setFetchClientsInterval }) => {
  const { setMainContent } = useContext(TestPageContext);
  const {
    clients,
    setSelectedClientIds,
    selectedClientIds,
    setOpenedClientDetailsId,
    openedClientDetailsId,
  } = useContext(ClientsContext);

  function toggleClient(loadClientId) {
    setSelectedClientIds({
      ...selectedClientIds,
      [loadClientId]: !selectedClientIds[loadClientId],
    });
  }

  const toggleClientDetails = (clientId) => {
    if (clientId === openedClientDetailsId) {
      setMainContent(MAIN_CONTENT.configs);
      setOpenedClientDetailsId(-1);
    } else {
      setMainContent(MAIN_CONTENT.clientDetails);
      setOpenedClientDetailsId(clientId);
    }
  };

  return (
    <div className="clients">
      <h1>LodeRunner</h1>
      <h3>Client Mode</h3>
      <select
        defaultValue="0"
        onChange={({ target }) => setFetchClientsInterval(target.value)}
        aria-label="Auto Refresh Load Client List"
      >
        <option value="0">Auto Refresh: OFF</option>
        <option value="5000">5 seconds</option>
        <option value="15000">15 seconds</option>
        <option value="30000">30 seconds</option>
        <option value="60000">1 minute</option>
      </select>
      <div>
        {clients.length > 0 ? (
          clients.map(
            ({
              [CLIENT.loadClientId]: loadClientId,
              [CLIENT.status]: status,
              [CLIENT.name]: name,
            }) => (
              <div
                key={loadClientId}
                className={`clients-item status-${
                  status === CLIENT_STATUS_TYPES.ready ? "ready" : "pending"
                } ${openedClientDetailsId === loadClientId && "selected"}`}
              >
                <button
                  className="clients-item-select"
                  type="button"
                  onClick={() => toggleClient(loadClientId)}
                  onKeyDown={() => toggleClient(loadClientId)}
                  title="Select Load Client for Test Run"
                  label-aria="Select Load Client for Test Run"
                >
                  <CheckMarkIcon
                    fillColor="var(--c-neutral-lightest)"
                    width="1.25em"
                    isChecked={selectedClientIds[loadClientId]}
                  />
                </button>
                <button
                  className="clients-item-status"
                  type="button"
                  title="Open Load Client Details"
                  aria-label="Open Load Client Details"
                  onClick={() => toggleClientDetails(loadClientId)}
                  onKeyDown={() => toggleClientDetails(loadClientId)}
                >
                  {name || "--"}
                </button>
              </div>
            )
          )
        ) : (
          <div className="clients-notification">
            <p>No Load Clients Available</p>
          </div>
        )}
      </div>
    </div>
  );
};

Clients.propTypes = {
  setFetchClientsInterval: PropTypes.func.isRequired,
};

export default Clients;

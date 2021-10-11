import { useContext, useState } from "react";
import PropTypes from "prop-types";
import { ClientContext } from "../../contexts";
import { CLIENT } from "../../models";
import ClientsFilter, {
  sortByProperty,
  filterByStatus,
} from "../ClientsFilter";
import { SORT_TYPES } from "../ClientsFilter/constants";
import "./styles.css";

const Clients = ({ openClientDetails }) => {
  const { clients } = useContext(ClientContext);
  const [filterBy, setFilterBy] = useState("");
  const [sortBy, setSortBy] = useState(SORT_TYPES.name.key);
  const [selectedClients, setSelectedClients] = useState({});

  function toggleClient(loadClientId) {
    setSelectedClients({
      ...selectedClients,
      [loadClientId]: !selectedClients[loadClientId],
    });
  }

  return (
    <>
      <h1>Clients</h1>
      <ClientsFilter {...{ setFilterBy, setSortBy }} />
      <ul>
        {clients
          .filter(filterByStatus(filterBy))
          .sort(sortByProperty(sortBy))
          .map((c, index) => {
            const {
              [CLIENT.loadClientId]: loadClientId,
              [CLIENT.status]: status,
              [CLIENT.name]: name,
            } = c;
            return (
              <li key={loadClientId}>
                <button
                  type="button"
                  onClick={() => toggleClient(loadClientId)}
                >
                  {name}
                </button>
                <button
                  type="button"
                  title={status}
                  aria-label={status}
                  onClick={() => openClientDetails(index)}
                >
                  Open Details
                </button>
              </li>
            );
          })}
      </ul>
    </>
  );
};

Clients.propTypes = {
  openClientDetails: PropTypes.func.isRequired,
};

export { Clients as default, sortByProperty, filterByStatus };

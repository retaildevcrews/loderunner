import { useContext, useState } from "react";
import PropTypes from "prop-types";
import { LoadClientContext } from "../../contexts";

import "./styles.css";

const SORT_TYPES = {
  id: "id",
  dateCreated: "dateCreated",
};
const sortByProperty = (type) => (a, b) => {
  const sortProperty = SORT_TYPES[type];
  return a[sortProperty] - b[sortProperty];
};

const LoadClients = ({ handleOpen }) => {
  const { loadClients } = useContext(LoadClientContext);
  const [excuteClients, setExecuteClients] = useState({});
  const [sortType, setSortType] = useState(SORT_TYPES.id);

  function handleToggleSelected(id) {
    setExecuteClients({
      ...excuteClients,
      [id]: !excuteClients[id],
    });
  }

  return (
    <>
      <div className="sidenav">
        <div className="header">
          <div>
            <h1>Load Clients</h1>
          </div>
          <div>
            <select onChange={(e) => setSortType(e.target.value)}>
              <option value="sort">Sort By:</option>
              <option value={SORT_TYPES.id}>Name</option>
              <option value={SORT_TYPES.dateCreated}>Date Created</option>
            </select>
          </div>
          <div id="filter">
            <select>
              <option value="0">Filter By:</option>
              <option value="1">Ready</option>
              <option value="2">Unresponsive</option>
              <option value="3">Busy</option>
            </select>
          </div>
        </div>
        <hr />
        <div>
          <ul>
            {loadClients.sort(sortByProperty(sortType)).map((lc, index) => (
              <li key={lc.id}>
                <button
                  type="button"
                  className={`loadclient ${
                    excuteClients[lc.id] ? "selected" : ""
                  }`}
                  onClick={() => handleToggleSelected(lc.id)}
                >
                  {lc.name}
                </button>
                <div className="divider" />
                <button
                  type="button"
                  className={`load-client-status ${lc.currstatus}`}
                  title={lc.currstatus}
                  aria-label={lc.currstatus}
                  onClick={() => handleOpen(index)}
                />
              </li>
            ))}
          </ul>
        </div>
      </div>
    </>
  );
};

LoadClients.propTypes = {
  handleOpen: PropTypes.func.isRequired,
};

export { LoadClients as default, sortByProperty };

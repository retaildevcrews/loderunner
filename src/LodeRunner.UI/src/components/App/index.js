import { useEffect, useState, useRef } from "react";
import Clients from "../Clients";
import ClientDetails from "../ClientDetails";
import PendingFeature from "../PendingFeature";
import { ClientContext, PendingFeatureContext } from "../../contexts";
import "./styles.css";

function App() {
  const [isPendingFeatureOpen, setIsPendingFeatureOpen] = useState(false);
  const [fetchClientsCount, setFetchClientsCount] = useState(0);
  const [clients, setClients] = useState([]);
  const [clientDetailsIndex, setClientDetailsIndex] = useState(-1);
  const [isClientDetailsOpen, setIsClientDetailsOpen] = useState(false);

  const fetchClientsIntervalId = useRef();

  useEffect(() => {
    fetch(`${process.env.REACT_APP_SERVER}/api/clients`)
      .then((res) => res.json())
      .then((body) => {
        if (Array.isArray(body)) {
          setClients(body.filter((c) => c));
        }
      })
      .catch((err) => {
        // eslint-disable-next-line
        console.error("Issue fetching Clients", err);
      });
  }, [fetchClientsCount]);

  const setFetchClientsInterval = (interval) => {
    // Clear old interval
    if (fetchClientsIntervalId.current) {
      clearInterval(fetchClientsIntervalId.current);
      fetchClientsIntervalId.current = undefined;
    }

    // Set new interval
    if (interval > 0) {
      fetchClientsIntervalId.current = setInterval(() => {
        setFetchClientsCount(Date.now());
      }, interval);
    }
  };

  const resetClientDetailsIndex = () => {
    setClientDetailsIndex(-1);
  };

  const handleClientDetailsClose = () => {
    setIsClientDetailsOpen(false);
    resetClientDetailsIndex();
  };

  const handleClientDetailsOpen = (index) => {
    setIsClientDetailsOpen(true);
    setClientDetailsIndex(index);
  };

  return (
    <div className="app">
      <PendingFeatureContext.Provider value={{ setIsPendingFeatureOpen }}>
        {isPendingFeatureOpen && <PendingFeature />}
        <div className="app-staticposition">
          <ClientContext.Provider value={{ clients }}>
            <Clients
              setFetchClientsInterval={setFetchClientsInterval}
              openClientDetails={handleClientDetailsOpen}
              openedClientDetailsIndex={clientDetailsIndex}
              closeClientDetails={handleClientDetailsClose}
            />
            {isClientDetailsOpen && (
              <ClientDetails
                closeModal={handleClientDetailsClose}
                clientDetailsIndex={clientDetailsIndex}
              />
            )}
          </ClientContext.Provider>
        </div>
      </PendingFeatureContext.Provider>
    </div>
  );
}

export default App;

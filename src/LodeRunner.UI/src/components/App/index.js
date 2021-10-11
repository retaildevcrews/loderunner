import { useEffect, useState } from "react";
import Clients from "../Clients";
import ClientDetails from "../ClientDetails";
import { ClientContext } from "../../contexts";
import "./styles.css";

function App() {
  const [clients, setClients] = useState([]);
  const [clientDetailsIndex, setClientDetailsIndex] = useState(-1);
  const [isClientDetailsOpen, setIsClientDetailsOpen] = useState(false);

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
  }, []);

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
    <>
      <ClientContext.Provider value={{ clients }}>
        <Clients openClientDetails={handleClientDetailsOpen} />
        {isClientDetailsOpen && (
          <ClientDetails
            closeModal={handleClientDetailsClose}
            clientDetailsIndex={clientDetailsIndex}
          />
        )}
      </ClientContext.Provider>
    </>
  );
}

export default App;

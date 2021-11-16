import { useEffect, useState, useRef } from "react";
import Clients from "../Clients";
import ContentPage from "../ContentPage";
import PendingFeature from "../PendingFeature";
import Modal from "../Modal";
import { ClientsContext, ConfigsContext, DisplayContext } from "../../contexts";
import "./styles.css";

function App() {
  const [modalContent, setModalContent] = useState(undefined);
  const [mainContent, setMainContent] = useState("configs");
  const [fetchClientsCount, setFetchClientsCount] = useState(0);
  const [clients, setClients] = useState([]);
  const [openedClientDetailsIndex, setOpenedClientDetailsIndex] = useState(-1);
  const [configs, setConfigs] = useState([]);

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

  useEffect(() => {
    // fetch(`${process.env.REACT_APP_SERVER}/api/configs`)
    //   .then((res) => res.json())
    //   .then((body) => {
    //     if (Array.isArray(body)) {
    //       setConfigs(body.filter((c) => c));
    //     }
    //   })
    //   .catch((err) => {
    //     // eslint-disable-next-line
    //     console.error("Issue fetching Configs", err);
    //   });
    setConfigs([
      {
        entityType: "LoadTestConfig",
        id: "abc123",
        name: "Configurations 1",
        files: ["baseline.json"],
        strictJson: false,
        baseUrl: "",
        verboseErrors: true,
        randomize: false,
        timeout: 30,
        server: ["https://ngsa-memory.com"],
        tag: "",
        sleep: 0,
        runLoop: false,
        duration: 0,
        maxErrors: 10,
        delayStart: -1,
        dryRun: false,
      },
      {
        entityType: "LoadTestConfig",
        id: "def456",
        name: "Configurations 2",
        files: ["baseline.json", "benchmark.json"],
        strictJson: false,
        baseUrl: "",
        verboseErrors: true,
        randomize: false,
        timeout: 30,
        server: ["https://ngsa-memory.com", "https://ngsa-cosmos.com"],
        tag: "pre-deploy",
        sleep: 0,
        runLoop: false,
        duration: 0,
        maxErrors: 10,
        delayStart: -1,
        dryRun: false,
      },
      {
        entityType: "LoadTestConfig",
        id: "ghi789",
        name: "Configurations 3",
        files: ["benchmark.json"],
        strictJson: false,
        baseUrl: "",
        verboseErrors: true,
        randomize: false,
        timeout: 30,
        server: ["https://ngsa-cosmos.com"],
        tag: "",
        sleep: 0,
        runLoop: false,
        duration: 0,
        maxErrors: 10,
        delayStart: -1,
        dryRun: false,
      },
    ]);
  }, []);

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

  return (
    <div className="app">
      <DisplayContext.Provider
        value={{ modalContent, setModalContent, mainContent, setMainContent }}
      >
        <ConfigsContext.Provider value={{ configs }}>
          {modalContent && (
            <Modal>
              {modalContent === "pendingFeature" && <PendingFeature />}
            </Modal>
          )}
          <div className="app-content">
            <ClientsContext.Provider
              value={{
                clients,
                setOpenedClientDetailsIndex,
                openedClientDetailsIndex,
              }}
            >
              <Clients setFetchClientsInterval={setFetchClientsInterval} />
              <ContentPage />
            </ClientsContext.Provider>
          </div>
        </ConfigsContext.Provider>
      </DisplayContext.Provider>
    </div>
  );
}

export default App;

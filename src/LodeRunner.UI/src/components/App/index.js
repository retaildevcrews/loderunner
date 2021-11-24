import { useEffect, useState, useRef } from "react";
import Clients from "../Clients";
import ConfigForm from "../ConfigForm";
import ContentPage from "../ContentPage";
import PendingFeature from "../PendingFeature";
import Modal from "../Modal";
import { ClientsContext, ConfigsContext, DisplayContext } from "../../contexts";
import { getClients } from "../../services/clients";
import "./styles.css";

function App() {
  const [modalContent, setModalContent] = useState(undefined);
  const [mainContent, setMainContent] = useState("configs");
  const [fetchClientsTrigger, setFetchClientsTrigger] = useState(0);
  const [clients, setClients] = useState([]);
  const [openedClientDetailsIndex, setOpenedClientDetailsIndex] = useState(-1);
  const [fetchConfigsTrigger, setFetchConfigsTrigger] = useState(0);
  const [configs, setConfigs] = useState([]);

  const fetchClientsIntervalId = useRef();

  useEffect(() => {
    getClients().then((c) => setClients(c));
  }, [fetchClientsTrigger]);

  useEffect(() => {
    // getConfigs().then((c) => setConfigs(c));
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
  }, [fetchConfigsTrigger]);

  const setFetchClientsInterval = (interval) => {
    // Clear old interval
    if (fetchClientsIntervalId.current) {
      clearInterval(fetchClientsIntervalId.current);
      fetchClientsIntervalId.current = undefined;
    }

    // Set new interval
    if (interval > 0) {
      fetchClientsIntervalId.current = setInterval(() => {
        setFetchClientsTrigger(Date.now());
      }, interval);
    }
  };

  return (
    <div className="app">
      <DisplayContext.Provider
        value={{ modalContent, setModalContent, mainContent, setMainContent }}
      >
        <ConfigsContext.Provider value={{ configs, setFetchConfigsTrigger }}>
          {modalContent && (
            <Modal>
              {modalContent === "pendingFeature" && <PendingFeature />}
              {modalContent === "configForm" && <ConfigForm />}
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

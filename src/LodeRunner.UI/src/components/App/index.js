import { useEffect, useState, useRef } from "react";
import Clients from "../Clients";
import ConfigForm from "../ConfigForm";
import ContentPage from "../ContentPage";
import PendingFeature from "../PendingFeature";
import Modal from "../Modal";
import TestSubmission from "../TestSubmission";
import { ClientsContext, ConfigsContext, DisplayContext } from "../../contexts";
import { getClients } from "../../services/clients";
import { getConfigs } from "../../services/configs";
import { MAIN_CONTENT, MODAL_CONTENT } from "../../utilities/constants";
import { ReactComponent as Spinner } from "../../images/spinner.svg";
import "./styles.css";

function App() {
  const [mainContent, setMainContent] = useState(MAIN_CONTENT.configs);
  const [modalContent, setModalContent] = useState(MODAL_CONTENT.closed);
  const [isPending, setIsPending] = useState(false);

  const fetchClientsIntervalId = useRef();
  const [fetchClientsTrigger, setFetchClientsTrigger] = useState(0);
  const [clients, setClients] = useState([]);
  const [openedClientDetailsId, setOpenedClientDetailsId] = useState(-1);

  const [fetchConfigsTrigger, setFetchConfigsTrigger] = useState(0);
  const [configs, setConfigs] = useState([]);
  const [openedConfigId, setOpenedConfigId] = useState(-1);

  const [selectedClientIds, setSelectedClientIds] = useState({});
  const [testRunConfigId, setTestRunConfigId] = useState(-1);

  useEffect(() => {
    getClients()
      .then((c) => setClients(c))
      .catch(() => setClients([]));
  }, [fetchClientsTrigger]);

  useEffect(() => {
    getConfigs()
      .then((c) => setConfigs(c))
      .catch(() => setConfigs([]));
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

  useEffect(() => {
    // Handle cleanup when modal closes
    if (modalContent === MODAL_CONTENT.closed && testRunConfigId !== -1) {
      setTestRunConfigId(-1);
    }
  }, [modalContent]);

  return (
    <div className="app">
      <DisplayContext.Provider
        value={{
          mainContent,
          setMainContent,
          modalContent,
          setModalContent,
          setIsPending,
        }}
      >
        <ConfigsContext.Provider
          value={{
            configs,
            setFetchConfigsTrigger,
            openedConfigId,
            setOpenedConfigId,
            testRunConfigId,
            setTestRunConfigId,
          }}
        >
          <ClientsContext.Provider
            value={{
              clients,
              selectedClientIds,
              setSelectedClientIds,
              openedClientDetailsId,
              setOpenedClientDetailsId,
            }}
          >
            {isPending && (
              <div className="pending-overlay">
                <Spinner />
              </div>
            )}
            {modalContent && (
              <Modal>
                {modalContent === MODAL_CONTENT.pendingFeature && (
                  <PendingFeature />
                )}
                {modalContent === MODAL_CONTENT.configForm && <ConfigForm />}
                {modalContent === MODAL_CONTENT.testSubmission && (
                  <TestSubmission />
                )}
              </Modal>
            )}
            <div className="app-content">
              <Clients setFetchClientsInterval={setFetchClientsInterval} />
              <ContentPage />
            </div>
          </ClientsContext.Provider>
        </ConfigsContext.Provider>
      </DisplayContext.Provider>
    </div>
  );
}

export default App;

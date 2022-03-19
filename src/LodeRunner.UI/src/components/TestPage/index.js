import { useContext, useEffect, useRef, useState } from "react";
import ClientDetails from "../ClientDetails";
import Clients from "../Clients";
import CreateConfig from "../CreateConfig";
import Configs from "../Configs";
import IncompleteTestRuns from "../IncompleteTestRuns";
import Modal from "../Modal";
import PendingFeature from "../PendingFeature";
import TestSubmission from "../TestSubmission";
import {
  AppContext,
  ClientsContext,
  ConfigsContext,
  TestPageContext,
  TestRunsContext,
} from "../../contexts";
import { getClients } from "../../services/clients";
import { getConfigs } from "../../services/configs";
import { getTestRuns } from "../../services/testRuns";
import { MAIN_CONTENT, MODAL_CONTENT } from "../../utilities/constants";
import "./styles.css";

const TestPage = () => {
  const [mainContent, setMainContent] = useState(MAIN_CONTENT.configs);
  const [modalContent, setModalContent] = useState(MODAL_CONTENT.closed);

  const [configs, setConfigs] = useState([]);
  const [testRunConfigId, setTestRunConfigId] = useState(-1);

  const [clients, setClients] = useState([]);
  const [selectedClientIds, setSelectedClientIds] = useState({});
  const [openedClientDetailsId, setOpenedClientDetailsId] = useState(-1);

  const [testRuns, setTestRuns] = useState([]);

  const [fetchConfigsTrigger, setFetchConfigsTrigger] = useState(0);
  const [fetchClientsTrigger, setFetchClientsTrigger] = useState(0);
  const fetchClientsIntervalId = useRef();
  const [fetchTestRunsTrigger, setFetchTestRunsTrigger] = useState(0);

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

  const { setIsPending } = useContext(AppContext);

  useEffect(() => {
    getClients()
      .then((c) => setClients(c))
      .catch(() => setClients([]));
    return () => clearInterval(fetchClientsIntervalId.current);
  }, [fetchClientsTrigger]);

  useEffect(() => {
    setIsPending(true);
    getConfigs()
      .then((c) => setConfigs(c))
      .catch(() => setConfigs([]))
      .finally(() => setIsPending(false));
  }, [fetchConfigsTrigger]);

  useEffect(() => {
    getTestRuns()
      .then((t) => setTestRuns(t))
      .catch(() => setTestRuns([]));
  }, [fetchTestRunsTrigger]);

  useEffect(() => {
    // Handle cleanup when modal closes
    if (modalContent === MODAL_CONTENT.closed && testRunConfigId !== -1) {
      setTestRunConfigId(-1);
    }
  }, [modalContent]);

  return (
    <TestPageContext.Provider
      value={{ mainContent, setMainContent, modalContent, setModalContent }}
    >
      <ConfigsContext.Provider
        value={{
          setFetchConfigsTrigger,
          configs,
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
          <TestRunsContext.Provider
            value={{ testRuns, setFetchTestRunsTrigger }}
          >
            {modalContent && (
              <Modal content={modalContent} setContent={setModalContent}>
                {modalContent === MODAL_CONTENT.pendingFeature && (
                  <PendingFeature />
                )}
                {modalContent === MODAL_CONTENT.configForm && <CreateConfig />}
                {modalContent === MODAL_CONTENT.testSubmission && (
                  <TestSubmission />
                )}
              </Modal>
            )}
            <div className="testpage-content">
              <div className="testpage-content-activepanel">
                <Clients setFetchClientsInterval={setFetchClientsInterval} />
                <IncompleteTestRuns />
              </div>
              <div className="testpage-content-column">
                {mainContent === MAIN_CONTENT.clientDetails && (
                  <ClientDetails />
                )}
                {mainContent === MAIN_CONTENT.configs && <Configs />}
              </div>
            </div>
          </TestRunsContext.Provider>
        </ClientsContext.Provider>
      </ConfigsContext.Provider>
    </TestPageContext.Provider>
  );
};

export default TestPage;

import { useContext } from "react";
import { A } from "hookrouter";
import PlayIcon from "../PlayIcon";
import PencilIcon from "../PencilIcon";
import RefreshIcon from "../RefreshIcon";
import TrashIcon from "../TrashIcon";
import {
  AppContext,
  ClientsContext,
  ConfigsContext,
  TestPageContext,
} from "../../contexts";
import { deleteConfig } from "../../services/configs";
import { CONFIG } from "../../models";
import { MODAL_CONTENT } from "../../utilities/constants";
import "./styles.css";

const Configs = () => {
  const { setIsPending } = useContext(AppContext);
  const { setModalContent } = useContext(TestPageContext);
  const { selectedClientIds } = useContext(ClientsContext);
  const {
    setFetchConfigsTrigger,
    configs,
    setTestRunConfigId,
  } = useContext(ConfigsContext);

  const openConfigFormModal = (e) => {
    e.stopPropagation();
    setModalContent(MODAL_CONTENT.configForm);
  };

  const handleDeleteConfig = (id, name) => (e) => {
    e.stopPropagation();

    // eslint-disable-next-line no-alert
    const isDeleteConfig = window.confirm(
      `Delete load test config, ${name} (${id})?`
    );

    if (isDeleteConfig) {
      setIsPending(true);

      deleteConfig(id)
        .catch(() => {
          // eslint-disable-next-line no-alert
          alert(`Unable to delete load test config, ${name} (${id})`);
        })
        .finally(() => {
          setFetchConfigsTrigger(Date.now());
          setIsPending(false);
        });
    }
  };

  const handleRunTest = (id, name) => (e) => {
    e.stopPropagation();

    const isClientSelected = Object.entries(selectedClientIds).some(
      // eslint-disable-next-line no-unused-vars
      ([_, isSelected]) => isSelected
    );

    if (!isClientSelected) {
      // eslint-disable-next-line no-alert
      alert(
        `No load clients selected for test run with load test config, ${
          name || "--"
        } (${id})`
      );
    } else {
      setTestRunConfigId(id);
      setModalContent(MODAL_CONTENT.testSubmission);
    }
  };

  return (
    <div className="configs">
      <div className="page-header">
        <h1>
          <button
            type="button"
            aria-label="Refresh Load Test Result"
            className="unset refresh"
            onClick={() => setFetchConfigsTrigger(Date.now())}
          >
            <RefreshIcon height="0.8em" />
          </button>
          Load Test Configs
          <button
            className="unset configs-newitem"
            type="button"
            onClick={openConfigFormModal}
            onKeyDown={openConfigFormModal}
            aria-label="New Load Test Config"
            title="New Load Test Config"
          >
            +
          </button>
        </h1>
        <A href="/results" className="unset navigation">
          Test Run Overview
        </A>
      </div>
      <div>
        {configs.map(
          ({
            [CONFIG.id]: configId,
            [CONFIG.name]: name,
            [CONFIG.servers]: servers,
            [CONFIG.files]: files,
          }) => (
            <div role="presentation" key={configId} className="card">
              <div>
                <div>
                  <span className="card-key">Name:</span> {name || "--"}
                </div>
                <div>
                  <span className="card-key">ID:</span> {configId}
                </div>
                <br />
                <div>
                  <span className="card-key">Servers: </span>
                  {servers.join(", ")}
                </div>
                <div>
                  <span className="card-key">Files: </span>
                  {files.join(", ")}
                </div>
              </div>
              <div className="configs-item-options">
                <A href={`/configs/${configId}`}>
                  <PencilIcon
                    width="3em"
                    fillColor="var(--c-neutral-light)"
                    hoverColor="var(--c-neutral-lightest)"
                  />
                </A>
                <button
                  className="unset"
                  type="button"
                  onClick={handleDeleteConfig(configId, name)}
                  onKeyDown={handleDeleteConfig(configId, name)}
                  aria-label="Delete Load Test Config"
                >
                  <TrashIcon
                    width="2em"
                    fillColor="var(--c-neutral-light)"
                    hoverColor="var(--c-neutral-lightest)"
                  />
                </button>
                <button
                  className="unset runtest"
                  type="button"
                  onClick={handleRunTest(configId, name)}
                  onKeyDown={handleRunTest(configId, name)}
                  aria-label="Run Load Test"
                >
                  <PlayIcon
                    width="2.4em"
                    fillColor="var(--c-neutral-light)"
                    hoverColor="var(--c-neutral-lightest)"
                  />
                </button>
              </div>
            </div>
          )
        )}
      </div>
    </div>
  );
};

export default Configs;

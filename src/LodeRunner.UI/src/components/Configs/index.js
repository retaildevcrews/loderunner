import { useContext } from "react";
import Pencil from "../Pencil";
import Trash from "../Trash";
import { ConfigsContext, DisplayContext } from "../../contexts";
import { CONFIG } from "../../models";
import { MODAL_CONTENT } from "../../utilities/constants";
import "./styles.css";

const Configs = () => {
  const { setModalContent } = useContext(DisplayContext);
  const { configs, setOpenedConfigIndex } = useContext(ConfigsContext);

  const openConfigFormModal = (index) => (e) => {
    e.stopPropagation();
    setOpenedConfigIndex(index);
    setModalContent(MODAL_CONTENT.configForm);
  };

  const openPendingFeatureModal = () =>
    setModalContent(MODAL_CONTENT.pendingFeature);

  return (
    <div className="configs">
      <div className="configs-header">
        <h1>
          Configs
          <button
            className="unset"
            type="button"
            onClick={openConfigFormModal(-1)}
            onKeyDown={openConfigFormModal(-1)}
          >
            <Pencil fillColor="#2c7f84" hoverColor="#24b2b9" width="1em" />
          </button>
        </h1>
      </div>
      <div>
        {configs.map((c, index) => {
          const {
            [CONFIG.id]: configId,
            [CONFIG.name]: name,
            [CONFIG.servers]: servers,
            [CONFIG.files]: files,
          } = c;
          return (
            <div
              role="presentation"
              key={configId}
              className="configs-item"
              type="button"
              onClick={openPendingFeatureModal}
              onKeyDown={openPendingFeatureModal}
            >
              <div>
                <div>
                  <span className="configs-key">Name:</span> {name || "Unknown"}
                </div>
                <div>
                  <span className="configs-key">ID:</span> {configId}
                </div>
                <br />
                <div>
                  <span className="configs-key">Servers:</span>
                  {servers.join(", ")}
                </div>
                <div>
                  <span className="configs-key">Files:</span>
                  {files.join(", ")}
                </div>
              </div>
              <div className="configs-item-options">
                <button
                  className="unset"
                  type="button"
                  onClick={openConfigFormModal(index)}
                  onKeyDown={openConfigFormModal(index)}
                >
                  <Pencil
                    width="3em"
                    fillColor="lightgrey"
                    hoverColor="whitesmoke"
                  />
                </button>
                <button
                  className="unset"
                  type="button"
                  onClick={openPendingFeatureModal}
                  onKeyDown={openPendingFeatureModal}
                >
                  <Trash
                    width="2em"
                    fillColor="lightgrey"
                    hoverColor="whitesmoke"
                  />
                </button>
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
};

export default Configs;

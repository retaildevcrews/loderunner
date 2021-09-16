import { useState } from "react";
import LoadClients from "../LoadClients";
import LoadClientDetails from "../LoadClientDetails";
import Configs from "../Configs";
import { ConfigsContext, LoadClientContext } from "../../contexts";
import { loadClients, configs, loadTests } from "../../data";

import "./styles.css";

function App() {
  const [isOpen, setIsOpen] = useState(false);
  const [currClientDetails, setCurrClientDetails] = useState(-1);

  const handleOpen = (index) => {
    setIsOpen(true);
    setCurrClientDetails(index);
  };

  const resetCurrClientDetails = () => {
    setCurrClientDetails(-1);
  };

  const handleClose = () => {
    setIsOpen(false);
    resetCurrClientDetails();
  };

  return (
    <div className="App">
      <LoadClientContext.Provider value={{ loadClients }}>
        <LoadClients handleOpen={handleOpen} />
        {isOpen && (
          <LoadClientDetails
            handleClose={handleClose}
            currClientDetails={currClientDetails}
          />
        )}
      </LoadClientContext.Provider>
      <ConfigsContext.Provider value={{ configs, loadTests }}>
        <Configs />
      </ConfigsContext.Provider>
    </div>
  );
}

export default App;

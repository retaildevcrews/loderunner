import { useContext } from "react";
import { DisplayContext } from "../../contexts";
import ClientDetails from "../ClientDetails";
import Configs from "../Configs";
import { MAIN_CONTENT } from "../../utilities/constants";

const ContentPage = () => {
  const { mainContent } = useContext(DisplayContext);

  return (
    <div className="app-content">
      {mainContent === MAIN_CONTENT.clientDetails && <ClientDetails />}
      {mainContent === MAIN_CONTENT.configs && <Configs />}
    </div>
  );
};

export default ContentPage;

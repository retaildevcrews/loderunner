import { useContext } from "react";
import { DisplayContext } from "../../contexts";
import ClientDetails from "../ClientDetails";
import Configs from "../Configs";

const ContentPage = () => {
  const { mainContent } = useContext(DisplayContext);

  return (
    <div className="app-content">
      {mainContent === "clientDetails" && <ClientDetails />}
      {mainContent === "configs" && <Configs />}
    </div>
  );
};

export default ContentPage;

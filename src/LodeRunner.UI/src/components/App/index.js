import { useState } from "react";
import { useRoutes } from "hookrouter";
import routes from "./routes";
import NotFoundPage from "../NotFoundPage";
import { AppContext } from "../../contexts";
import { ReactComponent as Spinner } from "../../images/spinner.svg";
import "./styles.css";

function App() {
  const routeResult = useRoutes(routes);
  const [isPending, setIsPending] = useState(false);

  return (
    <div className={`app ${isPending ? "pending-overlay-enabled" : ""}`}>
      {isPending && (
        <div className="pending-overlay">
          <Spinner />
        </div>
      )}
      <AppContext.Provider value={{ setIsPending }}>
        {routeResult || <NotFoundPage />}
      </AppContext.Provider>
    </div>
  );
}

export default App;

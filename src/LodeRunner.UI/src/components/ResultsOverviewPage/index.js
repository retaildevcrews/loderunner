import { A } from "hookrouter";
import "./styles.css";

const ResultsOverviewPage = () => (
  <div className="resultsoverview">
    <div className="resultsoverview-header">
      <h1>Load Test Results</h1>
      <A href="/" className="unset navigation-test">
        Load Test Submission
      </A>
    </div>
  </div>
);

export default ResultsOverviewPage;

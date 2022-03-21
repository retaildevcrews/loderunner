import { A, navigate } from "hookrouter";
import { useMemo, useContext } from "react";
import { TestRunsContext } from "../../contexts";
import getMMMDYYYYhmma from "../../utilities/datetime";
import { TEST_RUN } from "../../models";
import "./styles.css";

const IncompleteTestRuns = () => {
  const { testRuns } = useContext(TestRunsContext);
  const incompleteTestRuns = useMemo(
    () =>
      testRuns
        .filter(
          ({ [TEST_RUN.finalCompletionTime]: completionTime }) =>
            !completionTime
        )
        .slice(0, 20),
    [testRuns]
  );

  return (
    <div className="incompletetestruns">
      <A href="/results" className="unset navigation">
        See All Test Runs
      </A>
      {incompleteTestRuns.map(
        ({
          [TEST_RUN.createdTime]: createdTime,
          [TEST_RUN.id]: id,
          [TEST_RUN.name]: name,
          [TEST_RUN.scheduledStartTime]: startTime,
        }) => (
          <button
            key={id}
            className="incompletetestruns-item"
            type="button"
            onClick={() => navigate(`/results/${id}`)}
            onKeyDown={() => navigate(`/results/${id}`)}
            title="Incomplete Test Run"
            label-aria="Incomplete Test Run"
          >
            <div>{name || "--"}</div>
            <div>Scheduled: {getMMMDYYYYhmma(startTime)}</div>
            <div>Created: {getMMMDYYYYhmma(createdTime)}</div>
          </button>
        )
      )}
    </div>
  );
};

export default IncompleteTestRuns;

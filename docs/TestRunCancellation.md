# Test Run Cancellation

The cancellation request refers to the implementation of the ability to stop a test run, so that user may stop long running test runs.

The following documentation describes workflow, logging and data maintenance tasks associated with the implementation of Test Run Execution cancellation.

## Execute TestRun Workflow overview

- Adds TestRun to pending TestRuns list and update `Client Status`
- Starts Interval Checker to watch for an incoming `Cancellation Request`
  - Creates LoadRunner command mode instance
    - Upon completion, an execution exception or cancellation request will trigger `TestRun Complete` event to Post the TestRun current state back into CosmosDB.
  - Ends LodeRunner command mode instance

### Logging

- If a `Cancellation Request` is received during TestRun execution, a `TestRun Cancellation request received` message will be logged.
- Then, if a `Cancellation Request` was received, the UpdateTestRun method after a successful Post operation will logging `TestRun Hard Stop completed`.

For information about how to view logs please refer to section `View Test Run logs from LodeRunner in client mode` under [Running an Example Load Test](../README.md#running-an-example-load-test)

### Data Maintenance

In case of any unexpected issue or exception may occur while `TestRun is waiting for Cancellation to complete`, the `UpdateTestRun` method migth not be able to set the HardStopTime field to finalize the cancellation request. If so, we will have TestRuns in an incomplete state because HardStop will be set, but HardStopTime will not.

Eventually we need to do some data maintenance to take care of any TestRun documents that meet the above condition.

import { CLIENT, CONFIG, LOAD_CLIENT, TEST_RUN } from "../models";
import { getPostPayload as getPostTestRunPayload } from "./testRun";

describe("getPostPayload", () => {
  it("should return payload with expected structure", () => {
    const input = {
      [TEST_RUN.name]: TEST_RUN.name,
      [TEST_RUN.createdTime]: TEST_RUN.createdTime,
      [TEST_RUN.scheduledStartTime]: TEST_RUN.scheduledStartTime,
      [TEST_RUN.config]: {
        ...CONFIG,
        [CONFIG.baseUrl]: CONFIG.baseUrl,
        [CONFIG.servers]: CONFIG.servers,
      },
      [TEST_RUN.clients]: [CLIENT, CLIENT],
    };

    delete input[TEST_RUN.config].baseUrl;
    delete input[TEST_RUN.config].servers;
    // TODO: Undetermined if utilized
    delete input[TEST_RUN.config][CONFIG.verbose];

    const expectedPayload = {
      name: TEST_RUN.name,
      createdTime: TEST_RUN.createdTime,
      startTime: TEST_RUN.scheduledStartTime,
      loadTestConfig: {
        id: CONFIG.id,
        name: CONFIG.name,
        files: CONFIG.files,
        strictJson: CONFIG.strictJson,
        baseURL: CONFIG.baseUrl,
        verboseErrors: CONFIG.verboseErrors,
        randomize: CONFIG.randomize,
        timeout: CONFIG.timeout,
        server: CONFIG.servers,
        tag: CONFIG.tag,
        sleep: CONFIG.sleep,
        runLoop: CONFIG.runLoop,
        duration: CONFIG.duration,
        maxErrors: CONFIG.maxErrors,
        delayStart: CONFIG.delayStart,
        dryRun: CONFIG.dryRun,
      },
      loadClients: [
        {
          id: CLIENT.loadClientId,
          name: CLIENT.name,
          version: LOAD_CLIENT.version,
          region: LOAD_CLIENT.region,
          zone: LOAD_CLIENT.zone,
          prometheus: LOAD_CLIENT.prometheus,
          startupArgs: LOAD_CLIENT.startupArgs,
          startTime: LOAD_CLIENT.startTime,
        },
        {
          id: CLIENT.loadClientId,
          name: CLIENT.name,
          version: LOAD_CLIENT.version,
          region: LOAD_CLIENT.region,
          zone: LOAD_CLIENT.zone,
          prometheus: LOAD_CLIENT.prometheus,
          startupArgs: LOAD_CLIENT.startupArgs,
          startTime: LOAD_CLIENT.startTime,
        },
      ],
    };

    expect(getPostTestRunPayload(input)).toEqual(expectedPayload);
  });
});

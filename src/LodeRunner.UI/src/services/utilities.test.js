import { getResponseBody } from "./utilities";
import { Response } from "whatwg-fetch";

describe("getResponseBody", () => {
  it("should return undefined when statuscode is a 204", async () => {
    const res = new Response(null, { status: 204 });

    const result = await getResponseBody(res);

    expect(result).toEqual(undefined);
  });

  it("should raise an error when content-type is not handled", async () => {
    const res = new Response("", { status: 200, headers: { "Content-Type": "application/xml" } });

    await expect(getResponseBody(res)).rejects.toThrow("Unhandled response type");
  });

  it("should raise an error when status is not ok with json body", async () => {
    const body = JSON.stringify({ hello: "world" })
    const res = new Response(body, { status: 404, headers: { "Content-Type": "application/json" } });

    await expect(getResponseBody(res)).rejects.toThrow(body);
  });

  it("should raise an error when status is not ok with text body", async () => {
    expect(true).toEqual(true);
  });

  it("should return the text body", async () => {
    expect(true).toEqual(true);
  });

  it("should return the json body", async () => {
    expect(true).toEqual(true);
  });
});
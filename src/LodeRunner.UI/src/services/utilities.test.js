import { getResponseBody } from "./utilities";
import { Response } from "whatwg-fetch";

describe("getResponseBody", () => {
  it("should return undefined when statuscode is a 204", async () => {
    const res = new Response(null, { status: 204 });

    const result = await getResponseBody(res);

    expect(result).toEqual(undefined);
  });

  it("should return the text body", async () => {
    const res = new Response("Hello, World!", { status: 200, headers: { "Content-Type": "text/plain" } });

    const result = await getResponseBody(res);

    expect(result).toEqual("Hello, World!");
  });

  it("should return the parsed json body", async () => {
    const res = new Response('{"hello":"world"}', { status: 200, headers: { "Content-Type": "application/json" } });

    const result = await getResponseBody(res);

    expect(result).toEqual({ hello: "world" });
  });

  it("should raise an error when content-type is not handled", async () => {
    const res = new Response("", { status: 200, headers: { "Content-Type": "application/xml" } });

    await expect(getResponseBody(res)).rejects.toThrow("Unhandled response type");
  });

  it("should raise an error when status is not ok with json body as text", async () => {
    const res = new Response('{"hello":"world"}', { status: 404, headers: { "Content-Type": "application/json" } });

    await expect(getResponseBody(res)).rejects.toThrow('{"hello":"world"}');
  });

  it("should raise an error when status is not ok with text body", async () => {
    const res = new Response("Hello, World!", { status: 404, headers: { "Content-Type": "text/plain" } });

    await expect(getResponseBody(res)).rejects.toThrow("Hello, World!");
  });
});

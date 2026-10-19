import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";
import vm from "node:vm";

const html = await readFile(new URL("../public/admin-login.html", import.meta.url), "utf8");
const script = html.match(/<script>([\s\S]*?)<\/script>/)[1];

function page(fetchResult, search = "") {
  const elements = new Map();
  const redirects = [];
  function element(id) {
    if (!elements.has(id)) elements.set(id, {
      hidden: true, disabled: false, value: "", type: "password", textContent: "",
      action: "https://gameserver.rene007.nl/auth/login",
      handlers: {}, attributes: {}, classes: new Set(),
      classList: { add(name) { element(id).classes.add(name); } },
      setAttribute(name, value) { this.attributes[name] = value; },
      removeAttribute(name) { delete this.attributes[name]; },
      addEventListener(name, handler) { this.handlers[name] = handler; },
      focus() { this.focused = true; }
    });
    return elements.get(id);
  }
  vm.runInNewContext(script, {
    URL, URLSearchParams,
    FormData: class { *[Symbol.iterator]() { yield ["httpd_username", "rene"]; yield ["httpd_password", "test-password-only"]; } },
    document: { getElementById: element },
    window: {
      location: { search, origin: "https://gameserver.rene007.nl", replace: (url) => redirects.push(url) },
      addEventListener() {}
    },
    fetch: async (...args) => typeof fetchResult === "function" ? fetchResult(...args) : fetchResult
  });
  return { element, redirects, submit: () => element("login-form").handlers.submit({ preventDefault() {} }) };
}

test("incorrect credentials display an alert and allow another attempt", async () => {
  const p = page({ ok: true, status: 200, url: "https://gameserver.rene007.nl/login?error=1" });
  p.element("password").value = "test-password-only";
  await p.submit();
  assert.equal(p.element("login-message").hidden, false);
  assert.equal(p.element("login-message").attributes.role, "alert");
  assert.match(p.element("login-message").textContent, /wachtwoord is onjuist/);
  assert.equal(p.element("password").value, "");
  assert.equal(p.element("password").focused, true);
  assert.equal(p.element("login-submit").disabled, false);
  assert.deepEqual(p.redirects, []);
});

test("successful login opens dashboard", async () => {
  const p = page({ ok: true, status: 200, url: "https://gameserver.rene007.nl/" });
  await p.submit();
  assert.deepEqual(p.redirects, ["/"]);
});

test("network failure displays a retryable connection error", async () => {
  const p = page(() => { throw new Error("Network failed"); });
  await p.submit();
  assert.match(p.element("login-message").textContent, /Geen verbinding/);
  assert.equal(p.element("login-message").hidden, false);
  assert.equal(p.element("login-submit").disabled, false);
});

test("server rejection and missing cookies get distinct messages", async () => {
  const blocked = page({ ok: false, status: 403, url: "https://gameserver.rene007.nl/auth/login" });
  await blocked.submit();
  assert.match(blocked.element("login-message").textContent, /geblokkeerd/);
  const cookies = page({ ok: true, status: 200, url: "https://gameserver.rene007.nl/login" });
  await cookies.submit();
  assert.match(cookies.element("login-message").textContent, /cookies/);
});

test("native login error query also displays an alert", () => {
  const p = page(null, "?error=1");
  assert.equal(p.element("login-message").hidden, false);
  assert.match(p.element("login-message").textContent, /wachtwoord is onjuist/);
});

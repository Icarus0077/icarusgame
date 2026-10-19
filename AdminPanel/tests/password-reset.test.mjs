import assert from "node:assert/strict";
import { readFile, readdir } from "node:fs/promises";
import { registerHooks } from "node:module";
import { DatabaseSync } from "node:sqlite";
import test from "node:test";
import nodemailer from "nodemailer";
import net from "node:net";
import { EventEmitter } from "node:events";

const workerEnvironment = { SEED_DEMO_DATA: "false" };
const backgroundTasks = [];
globalThis.__icarusResetEnvForTests = workerEnvironment;
globalThis.__icarusResetTasksForTests = backgroundTasks;
// Node's type stripping runs the production TypeScript directly; resolve the
// extensionless imports used by the application's bundler.
registerHooks({ resolve(specifier, context, nextResolve) {
  if (specifier === "cloudflare:workers") {
    const source = "export const env = globalThis.__icarusResetEnvForTests; export const waitUntil = task => globalThis.__icarusResetTasksForTests.push(task);";
    return { url: `data:text/javascript,${encodeURIComponent(source)}`, shortCircuit: true };
  }
  if (specifier.startsWith(".") && context.parentURL?.endsWith(".ts") && !/\.[a-z]+$/i.test(specifier)) {
    return nextResolve(`${specifier}.ts`, context);
  }
  return nextResolve(specifier, context);
} });

const { requestPasswordReset, confirmPasswordReset, ensureResetSchema, resetEmail } = await import("../app/api/_shared/password-reset.ts");
const { createCredential, verifyCredential } = await import("../app/api/_shared/credentials.ts");
const { createResetSender, openMailSocket } = await import("../app/api/_shared/reset-mail.ts");
const { resetBody, resetClientIp, resetOriginAllowed } = await import("../app/api/_shared/reset-http.ts");
const { POST: requestResetApi } = await import("../app/api/game/v1/password-reset/request/route.ts");
const { POST: confirmResetApi } = await import("../app/api/game/v1/password-reset/confirm/route.ts");
const { POST: checkGameAccess } = await import("../app/api/game/v1/access/check/route.ts");
const oldPassword = "example-old-password";
const newPassword = "example-new-password";

async function fixture(t, role = "Player", status = "active") {
  const sqlite = new DatabaseSync(":memory:");
  t.after(() => sqlite.close());
  const migrations = new URL("../drizzle/", import.meta.url);
  for (const name of (await readdir(migrations)).filter((name) => name.endsWith(".sql")).sort()) {
    sqlite.exec(await readFile(new URL(name, migrations), "utf8"));
  }
  const wrap = (sql, args = []) => ({
    bind: (...values) => wrap(sql, values),
    first: async () => sqlite.prepare(sql).get(...args) ?? null,
    run: async () => ({ meta: { changes: Number(sqlite.prepare(sql).run(...args).changes) } }),
    all: async () => ({ results: sqlite.prepare(sql).all(...args) }),
  });
  let previousBatch = Promise.resolve();
  const db = {
    prepare: (sql) => wrap(sql),
    batch: (statements) => {
      // D1 serializes transactions on a database. Match that behavior when
      // multiple public requests share this in-memory SQLite connection.
      const result = previousBatch.then(async () => {
        sqlite.exec("BEGIN");
        try {
          const results = [];
          for (const statement of statements) results.push(await statement.run());
          sqlite.exec("COMMIT");
          return results;
        } catch (error) { sqlite.exec("ROLLBACK"); throw error; }
      });
      previousBatch = result.catch(() => {});
      return result;
    },
  };
  await ensureResetSchema(db);
  sqlite.prepare("INSERT INTO game_users (id, display_name, email, role, status) VALUES ('player-1', 'Testspeler', 'player@example.test', ?, ?)").run(role, status);
  const credential = await createCredential(oldPassword);
  sqlite.prepare("INSERT INTO game_user_credentials (id, user_id, salt, hash, iterations) VALUES ('cred-1', 'player-1', ?, ?, ?)").run(credential.salt, credential.hash, credential.iterations);
  const mails = [];
  const request = async (now = Date.now(), email = "player@example.test", sender = async (mail) => { mails.push(mail); }) => {
    await requestPasswordReset(db, email, "192.0.2.1", sender, "https://gameserver.rene007.nl", now);
  };
  const token = () => new URLSearchParams(new URL(mails.at(-1).resetUrl).hash.slice(1)).get("token");
  const currentCredential = () => sqlite.prepare("SELECT salt, hash, iterations FROM game_user_credentials WHERE user_id = 'player-1'").get();
  const addAdmin = () => sqlite.prepare("INSERT INTO admin_principals (id, user_id, email, display_name, role) VALUES ('admin-1', 'other-id', 'PLAYER@EXAMPLE.TEST', 'Beheerder', 'Viewer')").run();
  return { db, sqlite, mails, request, token, currentCredential, addAdmin };
}

test("ordinary player receives a single-use link only at the registered address", async (t) => {
  const f = await fixture(t);
  await f.request();
  assert.equal(f.mails.length, 1);
  assert.equal(f.mails[0].recipient, "player@example.test");
  assert.match(f.token(), /^[a-f0-9]{64}$/);
  assert.notEqual(f.sqlite.prepare("SELECT token_hash FROM game_password_resets").get().token_hash, f.token());
  assert.equal(await verifyCredential(oldPassword, f.currentCredential()), true);
  assert.equal(await confirmPasswordReset(f.db, f.token(), newPassword, newPassword), true);
  assert.equal(await verifyCredential(newPassword, f.currentCredential()), true);
  assert.equal(await verifyCredential(oldPassword, f.currentCredential()), false);
  assert.equal(await confirmPasswordReset(f.db, f.token(), oldPassword, oldPassword), false);
  assert.equal(f.sqlite.prepare("SELECT COUNT(*) AS n FROM audit_events").get().n, 2);
});

test("a player without credentials chooses a first persistent password through email", async (t) => {
  const f = await fixture(t);
  f.sqlite.exec("DELETE FROM game_user_credentials");
  await f.request();
  assert.equal(f.mails.length, 1);
  assert.equal(f.currentCredential(), undefined);
  assert.equal(await confirmPasswordReset(f.db, f.token(), newPassword, newPassword), true);
  assert.equal(await verifyCredential(newPassword, f.currentCredential()), true);
  assert.equal(await verifyCredential(newPassword, f.currentCredential()), true);
  assert.equal(await confirmPasswordReset(f.db, f.token(), oldPassword, oldPassword), false);
});

test("self-chosen password repeatedly authenticates the game and keeps legacy clients compatible", async (t) => {
  const f = await fixture(t);
  f.sqlite.exec("DELETE FROM game_user_credentials");
  f.sqlite.exec("INSERT INTO game_user_settings (id, user_id, game_id, can_play) VALUES ('setting-1', 'player-1', 'master-duel', 1)");
  await f.request();
  assert.equal(await confirmPasswordReset(f.db, f.token(), newPassword, newPassword), true);
  workerEnvironment.DB = f.db;
  const call = (credentials) => checkGameAccess(new Request("https://gameserver.rene007.nl/api/game/v1/access/check", {
    method: "POST", headers: { "content-type": "application/json" },
    body: JSON.stringify({ user: "player@example.test", gameId: "master-duel", ...credentials }),
  }));
  for (const credentials of [{ password: newPassword }, { password: newPassword }, { accessCode: newPassword }]) {
    const response = await call(credentials);
    assert.equal(response.status, 200);
    assert.equal((await response.json()).decision.allowed, true);
  }
  assert.equal((await call({ password: oldPassword })).status, 401);
});

test("a first-password link is invalidated if a credential is created before redemption", async (t) => {
  const f = await fixture(t);
  const credential = f.currentCredential();
  f.sqlite.exec("DELETE FROM game_user_credentials");
  await f.request();
  f.sqlite.prepare("INSERT INTO game_user_credentials (id, user_id, salt, hash, iterations) VALUES ('new-credential', 'player-1', ?, ?, ?)")
    .run(credential.salt, credential.hash, credential.iterations);
  assert.equal(await confirmPasswordReset(f.db, f.token(), newPassword, newPassword), false);
  assert.equal(await verifyCredential(oldPassword, f.currentCredential()), true);
});

test("a protected account without credentials cannot use first-password setup", async (t) => {
  const f = await fixture(t);
  f.sqlite.exec("DELETE FROM game_user_credentials");
  f.addAdmin();
  await f.request();
  assert.equal(f.mails.length, 0);
});

for (const role of ["Admin", "Super Admin", "Administrator", "Moderator", "Owner", "unknown"]) {
  test(`public recovery excludes privileged or unknown role: ${role}`, async (t) => {
    const f = await fixture(t, role);
    await f.request();
    assert.equal(f.mails.length, 0);
    assert.equal(f.sqlite.prepare("SELECT COUNT(*) AS n FROM game_password_resets").get().n, 0);
    assert.equal(await verifyCredential(oldPassword, f.currentCredential()), true);
  });
}

test("admin membership protects a Player account with the same email, even for a disabled admin", async (t) => {
  const f = await fixture(t);
  f.addAdmin();
  f.sqlite.exec("UPDATE admin_principals SET status = 'disabled'");
  await f.request();
  assert.equal(f.mails.length, 0);
});

test("Tester accounts may recover, blocked accounts may not", async (t) => {
  const tester = await fixture(t, "Tester");
  await tester.request();
  assert.equal(tester.mails.length, 1);
  const blocked = await fixture(t, "Player", "blocked");
  await blocked.request();
  assert.equal(blocked.mails.length, 0);
});

for (const change of ["role", "membership", "email", "status", "password"]) {
  test(`redemption rechecks account protection after a ${change} change`, async (t) => {
    const f = await fixture(t);
    await f.request();
    if (change === "role") f.sqlite.exec("UPDATE game_users SET role = 'Admin'");
    if (change === "membership") f.addAdmin();
    if (change === "email") f.sqlite.exec("UPDATE game_users SET email = 'different@example.test'");
    if (change === "status") f.sqlite.exec("UPDATE game_users SET status = 'blocked'");
    if (change === "password") {
      const changed = await createCredential("admin-selected-password");
      f.sqlite.prepare("UPDATE game_user_credentials SET hash = ?, salt = ?").run(changed.hash, changed.salt);
    }
    const before = f.currentCredential().hash;
    assert.equal(await confirmPasswordReset(f.db, f.token(), newPassword, newPassword), false);
    assert.equal(f.currentCredential().hash, before);
  });
}

test("expired and malformed links never change a password", async (t) => {
  const f = await fixture(t);
  const now = Date.now();
  await f.request(now);
  assert.equal(await confirmPasswordReset(f.db, f.token(), newPassword, newPassword, now + 15 * 60 * 1000), false);
  assert.equal(await confirmPasswordReset(f.db, "not-a-token", newPassword, newPassword), false);
  assert.equal(await verifyCredential(oldPassword, f.currentCredential()), true);
});

test("only one concurrent redemption can succeed; all older links are invalidated", async (t) => {
  const f = await fixture(t);
  await f.request();
  const earlier = f.token();
  await f.request();
  const outcomes = await Promise.all([
    confirmPasswordReset(f.db, f.token(), newPassword, newPassword),
    confirmPasswordReset(f.db, f.token(), "other-new-password", "other-new-password"),
  ]);
  assert.equal(outcomes.filter(Boolean).length, 1);
  assert.equal(await confirmPasswordReset(f.db, earlier, oldPassword, oldPassword), false);
});

test("request limits, unknown addresses and delivery failures do not change credentials", async (t) => {
  const f = await fixture(t);
  for (let i = 0; i < 5; i++) await f.request();
  assert.equal(f.mails.length, 3);
  await f.request(Date.now(), "unknown@example.test");
  assert.equal(f.mails.length, 3);
  assert.equal(await verifyCredential(oldPassword, f.currentCredential()), true);
  const failure = await fixture(t);
  await assert.rejects(failure.request(Date.now(), "player@example.test", async () => { throw new Error("mail unavailable"); }));
  assert.equal(failure.sqlite.prepare("SELECT COUNT(*) AS n FROM game_password_resets").get().n, 0);
  assert.equal(await verifyCredential(oldPassword, failure.currentCredential()), true);
});

test("password confirmation and length are checked before changing credentials", async (t) => {
  const f = await fixture(t);
  await f.request();
  await assert.rejects(confirmPasswordReset(f.db, f.token(), newPassword, "different-password"), /overeen/);
  await assert.rejects(confirmPasswordReset(f.db, f.token(), "short", "short"), /wachtwoord/);
  assert.equal(await verifyCredential(oldPassword, f.currentCredential()), true);
});

test("OVH submission uses authenticated STARTTLS, validates certificates and logs no secrets", async (t) => {
  let options;
  let sent;
  let closed = false;
  t.mock.method(nodemailer, "createTransport", (value) => {
    options = value;
    return { sendMail: async (message) => { sent = message; }, close: () => { closed = true; } };
  });
  const send = createResetSender({ SMTP_HOST: "pro2.mail.ovh.net", SMTP_PORT: "587", SMTP_USER: "info@example.test", SMTP_PASSWORD: "example-mail-secret" });
  await send({ recipient: "player@example.test", resetUrl: "https://gameserver.rene007.nl/password-reset.html#token=example" });
  assert.equal(options.host, "pro2.mail.ovh.net");
  assert.equal(typeof options.getSocket, "function");
  assert.equal(options.tls.servername, "pro2.mail.ovh.net");
  assert.equal(options.secure, false);
  assert.equal(options.requireTLS, true);
  assert.equal(options.tls.rejectUnauthorized, true);
  assert.equal(options.debug, false);
  assert.equal(sent.to, "player@example.test");
  assert.equal(sent.from.address, "info@example.test");
  assert.equal(closed, true);
  assert.throws(() => createResetSender({}), /ingesteld/);
});

test("mail sockets preserve the TLS hostname and hand over only a connected socket", (t) => {
  const socket = new EventEmitter();
  socket.destroy = () => {};
  let address;
  let result;
  let calls = 0;
  t.mock.method(net, "createConnection", options => { address = options; return socket; });
  openMailSocket("pro2.mail.ovh.net", 587, (error, value) => { calls++; assert.equal(error, null); result = value; });
  assert.deepEqual(address, {host: "pro2.mail.ovh.net", port: 587});
  assert.equal(calls, 0);
  socket.emit("connect");
  assert.equal(result.connection, socket);
  assert.equal(calls, 1);
  assert.equal(socket.listenerCount("error"), 0);
});

test("failed mail sockets close and never hand over a broken connection", (t) => {
  const socket = new EventEmitter();
  let destroyed = false;
  let calls = 0;
  socket.destroy = () => { destroyed = true; };
  t.mock.method(net, "createConnection", () => socket);
  const failure = new Error("example failure");
  openMailSocket("pro2.mail.ovh.net", 587, (error, value) => { calls++; assert.equal(error, failure); assert.equal(value, false); });
  socket.emit("error", failure);
  socket.emit("connect");
  assert.equal(calls, 1);
  assert.equal(destroyed, true);
});

test("public input rejects injection and oversized bodies and uses the proxy's final address", async () => {
  assert.equal(resetEmail(" Player@Example.test "), "player@example.test");
  assert.equal(resetEmail("player@example.test\r\nBcc:other@example.test"), null);
  const request = (body) => new Request("https://gameserver.rene007.nl/api/game/v1/password-reset/request", {
    method: "POST", body, headers: { "content-type": "application/json", "x-forwarded-for": "spoofed, 192.0.2.1" },
  });
  assert.deepEqual(await resetBody(request('{"email":"player@example.test"}')), { email: "player@example.test" });
  await assert.rejects(resetBody(request("[1,2]")));
  await assert.rejects(resetBody(request(JSON.stringify({ email: "a".repeat(5000) }))));
  assert.equal(resetClientIp(request("{}")), "192.0.2.1");
  assert.equal(resetOriginAllowed(new Request("https://gameserver.rene007.nl", { headers: { origin: "https://attacker.test" } })), false);
});

test("public APIs do not trust caller roles and never reveal protected account existence", async (t) => {
  const f = await fixture(t);
  f.addAdmin();
  Object.assign(workerEnvironment, { DB: f.db, SMTP_HOST: "pro2.mail.ovh.net", SMTP_PORT: "587", SMTP_USER: "info@example.test", SMTP_PASSWORD: "example-mail-secret" });
  let deliveries = 0;
  const errors = t.mock.method(console, "error", () => {});
  t.mock.method(nodemailer, "createTransport", () => ({ sendMail: async () => { deliveries++; }, close() {} }));
  const call = (email) => requestResetApi(new Request("https://gameserver.rene007.nl/api/game/v1/password-reset/request", {
    method: "POST", headers: { "content-type": "application/json", "x-forwarded-for": "192.0.2.1" },
    body: JSON.stringify({ email, role: "Player", isAdmin: false, recipient: "attacker@example.test" }),
  }));
  const protectedResponse = await call("player@example.test");
  const unknownResponse = await call("unknown@example.test");
  assert.equal(protectedResponse.status, 202);
  assert.equal(unknownResponse.status, 202);
  assert.deepEqual(await protectedResponse.json(), await unknownResponse.json());
  await Promise.all(backgroundTasks.splice(0));
  assert.equal(errors.mock.callCount(), 0);
  assert.equal(deliveries, 0);
  assert.equal(await verifyCredential(oldPassword, f.currentCredential()), true);
});

test("public recovery mails only the stored account address, never a caller-supplied recipient", async (t) => {
  const f = await fixture(t);
  Object.assign(workerEnvironment, { DB: f.db, SMTP_HOST: "pro2.mail.ovh.net", SMTP_PORT: "587", SMTP_USER: "info@example.test", SMTP_PASSWORD: "example-mail-secret" });
  const errors = t.mock.method(console, "error", () => {});
  const sent = [];
  t.mock.method(nodemailer, "createTransport", () => ({ sendMail: async (mail) => { sent.push(mail); }, close() {} }));
  const response = await requestResetApi(new Request("https://gameserver.rene007.nl/api/game/v1/password-reset/request", {
    method: "POST", headers: { "content-type": "application/json", "x-forwarded-for": "192.0.2.1" },
    body: JSON.stringify({ email: "player@example.test", recipient: "attacker@example.test" }),
  }));
  assert.equal(response.status, 202);
  await Promise.all(backgroundTasks.splice(0));
  assert.equal(errors.mock.callCount(), 0);
  assert.equal(sent.length, 1);
  assert.equal(sent[0].to, "player@example.test");
});

test("the public confirmation API refuses a token issued before admin promotion", async (t) => {
  const f = await fixture(t);
  await f.request();
  f.addAdmin();
  workerEnvironment.DB = f.db;
  const response = await confirmResetApi(new Request("https://gameserver.rene007.nl/api/game/v1/password-reset/confirm", {
    method: "POST", headers: { "content-type": "application/json", "x-forwarded-for": "192.0.2.1" },
    body: JSON.stringify({ token: f.token(), password: newPassword, confirmation: newPassword, role: "Player" }),
  }));
  assert.equal(response.status, 400);
  assert.equal(await verifyCredential(oldPassword, f.currentCredential()), true);
});

test("an unconfigured relay fails uniformly without claiming a reset mail was sent", async () => {
  const previousPassword = workerEnvironment.SMTP_PASSWORD;
  delete workerEnvironment.SMTP_PASSWORD;
  try {
    const response = await requestResetApi(new Request("https://gameserver.rene007.nl/api/game/v1/password-reset/request", {
      method: "POST", headers: { "content-type": "application/json" },
      body: JSON.stringify({ email: "player@example.test" }),
    }));
    assert.equal(response.status, 503);
    assert.match((await response.json()).error, /tijdelijk niet beschikbaar/);
    assert.equal(backgroundTasks.length, 0);
  } finally { workerEnvironment.SMTP_PASSWORD = previousPassword; }
});

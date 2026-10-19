import assert from "node:assert/strict";
import { access, readFile } from "node:fs/promises";
import test from "node:test";

const root = new URL("../", import.meta.url);

async function render(pathname = "/") {
  const workerUrl = new URL("../dist/server/index.js", import.meta.url);
  workerUrl.searchParams.set("test", `${process.pid}-${Date.now()}`);
  const { default: worker } = await import(workerUrl.href);

  return worker.fetch(
    new Request(`http://localhost${pathname}`, { headers: { accept: "text/html" } }),
    { ASSETS: { fetch: async () => new Response("Not found", { status: 404 }) } },
    { waitUntil() {}, passThroughOnException() {} },
  );
}

test("server-renders the Icarus admin console", async () => {
  const response = await render();
  assert.equal(response.status, 200);
  assert.match(response.headers.get("content-type") ?? "", /^text\/html\b/i);

  const html = await response.text();
  assert.match(html, /<title>Icarus Control \| Game Operations<\/title>/i);
  assert.match(html, /ICARUS/);
  assert.match(html, /Overzicht/);
  assert.match(html, /IP-toegang/);
  assert.match(html, /Kaarttoegang/);
  assert.doesNotMatch(html, /codex-preview|Your site is taking shape|react-loading-skeleton/i);
});

test("ships database-backed admin capabilities without starter assets", async () => {
  const [hosting, schema, api, adminConsole, credentials, packageJson] = await Promise.all([
    readFile(new URL(".openai/hosting.json", root), "utf8"),
    readFile(new URL("db/schema.ts", root), "utf8"),
    readFile(new URL("app/api/admin/route.ts", root), "utf8"),
    readFile(new URL("app/AdminConsole.tsx", root), "utf8"),
    readFile(new URL("app/api/_shared/credentials.ts", root), "utf8"),
    readFile(new URL("package.json", root), "utf8"),
  ]);

  assert.match(hosting, /"d1":\s*"DB"/);
  assert.match(schema, /game_users/);
  assert.match(schema, /card_policies/);
  assert.match(schema, /audit_events/);
  assert.match(api, /authorize\(request/);
  assert.match(api, /update-card-policy/);
  assert.match(api, /reset-user-password/);
  assert.match(api, /Wachtwoord opnieuw ingesteld/);
  assert.match(adminConsole, /Wachtwoord resetten/);
  assert.match(adminConsole, /accessCodeConfirmation/);
  assert.match(credentials, /PBKDF2/);
  assert.doesNotMatch(packageJson, /react-loading-skeleton/);
  await assert.rejects(access(new URL("app/_sites-preview/SkeletonPreview.tsx", root)));
});

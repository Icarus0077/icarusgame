import assert from "node:assert/strict";
import { readFile, readdir } from "node:fs/promises";
import { registerHooks } from "node:module";
import { DatabaseSync } from "node:sqlite";
import test from "node:test";

const workerEnvironment = { SEED_DEMO_DATA: "false" };
globalThis.__icarusRankedEnvironment = workerEnvironment;
globalThis.__icarusRankedActor = null;
registerHooks({ resolve(specifier, context, nextResolve) {
  if (specifier === "cloudflare:workers") return { url: `data:text/javascript,${encodeURIComponent("export const env = globalThis.__icarusRankedEnvironment;")}`, shortCircuit: true };
  if (specifier.endsWith("chatgpt-auth")) return { url: `data:text/javascript,${encodeURIComponent("export async function getChatGPTUser() { return globalThis.__icarusRankedActor; }")}`, shortCircuit: true };
  if (specifier.startsWith(".") && context.parentURL?.endsWith(".ts") && !/\.[a-z]+$/i.test(specifier)) return nextResolve(`${specifier}.ts`, context);
  return nextResolve(specifier, context);
} });

const {
  ensureRankedSchema, eloChange, rankForStep, getRankedSnapshot, rankedOverview, rankedRanks,
  recordRankedMatch, simulateRanked, createRankedDemoAccounts, RankedError,
} = await import("../app/api/_shared/ranked.ts");
const { GET, POST } = await import("../app/api/admin/ranked/route.ts");
const { POST: checkAccess } = await import("../app/api/game/v1/access/check/route.ts");
const { createCredential } = await import("../app/api/_shared/credentials.ts");

async function fixture(t, { migrationLimit = Infinity, skipEnsure = false } = {}) {
  const sqlite = new DatabaseSync(":memory:");
  t.after(() => sqlite.close());
  sqlite.exec("PRAGMA foreign_keys = ON");
  const migrations = new URL("../drizzle/", import.meta.url);
  for (const name of (await readdir(migrations)).filter((name) => name.endsWith(".sql") && Number.parseInt(name, 10) <= migrationLimit).sort()) sqlite.exec(await readFile(new URL(name, migrations), "utf8"));
  const controls = { beforeWrite: null, failSql: null };
  const wrap = (sql, args = []) => ({
    sql,
    bind: (...values) => wrap(sql, values),
    first: async () => sqlite.prepare(sql).get(...args) ?? null,
    all: async () => ({ results: sqlite.prepare(sql).all(...args) }),
    run: async () => {
      if (controls.failSql?.test(sql)) { controls.failSql = null; throw new Error("Injected storage failure"); }
      return { meta: { changes: Number(sqlite.prepare(sql).run(...args).changes) } };
    },
  });
  let previousBatch = Promise.resolve();
  const db = {
    prepare: (sql) => wrap(sql),
    batch: (statements) => {
      const result = previousBatch.then(async () => {
        if (controls.beforeWrite && statements.some((statement) => /UPDATE ranked_profiles SET rating/.test(statement.sql))) {
          const callback = controls.beforeWrite; controls.beforeWrite = null; callback();
        }
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
  if (!skipEnsure) await ensureRankedSchema(db);
  for (const [id, name] of [["alice", "Alice"], ["bob", "Bob"], ["celia", "Celia"]]) {
    sqlite.prepare("INSERT INTO game_users (id, display_name, email, role, status) VALUES (?, ?, ?, 'Player', 'active')").run(id, name, `${id}@example.invalid`);
    sqlite.prepare("INSERT INTO game_user_settings (id, user_id, game_id, can_play, ranked_enabled) VALUES (?, ?, 'master-duel', 1, 1)").run(`settings_${id}`, id);
  }
  const profiles = () => sqlite.prepare("SELECT user_id, season_id, rating, games, wins, losses, draws FROM ranked_profiles ORDER BY season_id, user_id").all().map((row) => ({ ...row }));
  const count = (table) => sqlite.prepare(`SELECT COUNT(*) AS n FROM ${table}`).get().n;
  return { db, sqlite, controls, profiles, count };
}

const match = (matchId = "duel_000001", outcome = "a") => ({ matchId, playerAId: "alice", playerBId: "bob", outcome });
const simulation = (batchId = "batch_000001", seed = 42, count = 20) => ({ batchId, seed, count, accountIds: ["alice", "bob", "celia"] });

test("equal Elo produces +16/-16, an upset earns more and a draw conserves points", () => {
  assert.deepEqual(eloChange(1000, 1000, "a"), { deltaA: 16, deltaB: -16, ratingAAfter: 1016, ratingBAfter: 984 });
  assert.ok(eloChange(1000, 1400, "a").deltaA > eloChange(1400, 1000, "a").deltaA);
  assert.equal(eloChange(1000, 1000, "draw").deltaA, 0);
  assert.ok(eloChange(1000, 1400, "draw").deltaA > 0);
  for (const outcome of ["a", "b", "draw"]) {
    const delta = eloChange(1013, 1278, outcome);
    assert.equal(delta.deltaA + delta.deltaB, 0);
    assert.equal(delta.ratingAAfter + delta.ratingBAfter, 2291);
  }
});

test("Elo floor and safe integer boundary limit the transfer without minting points", () => {
  assert.deepEqual(eloChange(0, 0, "a"), { deltaA: 0, deltaB: 0, ratingAAfter: 0, ratingBAfter: 0 });
  const atFloor = eloChange(2, 2, "a");
  assert.equal(atFloor.ratingAAfter, 4); assert.equal(atFloor.ratingBAfter, 0);
  const atCeiling = eloChange(Number.MAX_SAFE_INTEGER, Number.MAX_SAFE_INTEGER, "a");
  assert.equal(atCeiling.ratingAAfter, Number.MAX_SAFE_INTEGER); assert.equal(atCeiling.deltaA, 0);
  for (const invalid of [-1, NaN, Infinity, 1.5, Number.MAX_SAFE_INTEGER + 1]) assert.throws(() => eloChange(invalid, 1000, "a"), RankedError);
});

test("the 39-step visible ladder retains Greek IDs and starts at Bronze 4 regardless of Elo", async (t) => {
  const f = await fixture(t);
  assert.deepEqual(rankedRanks.map(({ id, name, patronName, material, minRating, badgePath }) => ({ id, name, patronName, material, minRating, badgePath })), [
    { id: "icarus", name: "Bronze", patronName: "Icarus", material: "bronze", minRating: 0, badgePath: "/ranked/icarus.png" },
    { id: "hermes", name: "Silver", patronName: "Hermes", material: "silver", minRating: 1100, badgePath: "/ranked/hermes.png" },
    { id: "athena", name: "Gold", patronName: "Athena", material: "gold", minRating: 1250, badgePath: "/ranked/athena.png" },
    { id: "apollo", name: "Platinum", patronName: "Apollo", material: "platinum", minRating: 1400, badgePath: "/ranked/apollo.png" },
    { id: "poseidon", name: "Diamond", patronName: "Poseidon", material: "diamond", minRating: 1550, badgePath: "/ranked/poseidon.png" },
    { id: "zeus", name: "Master", patronName: "Zeus", material: "master", minRating: 1750, badgePath: "/ranked/zeus.png" },
  ]);
  assert.equal(rankedRanks.reduce((total, rank) => total + rank.tierLevels, 0), 39);
  for (const [step, rankId, level] of [[0, "icarus", 4], [3, "icarus", 1], [4, "hermes", 5], [8, "hermes", 1],
    [9, "athena", 6], [14, "athena", 1], [15, "apollo", 7], [21, "apollo", 1],
    [22, "poseidon", 8], [29, "poseidon", 1], [30, "zeus", 9], [38, "zeus", 1]]) {
    assert.equal(rankForStep(step).id, rankId); assert.equal(rankForStep(step).level, level);
  }
  const fresh = await getRankedSnapshot(f.db, "alice");
  assert.equal(fresh.rating, 1000); assert.equal(fresh.rankId, "icarus");
  assert.equal(fresh.rankName, "Bronze"); assert.equal(fresh.rankPatronName, "Icarus");
  assert.deepEqual([fresh.progressionVersion, fresh.ladderStep, fresh.level, fresh.tierLevels], [1, 0, 4, 4]);
  assert.deepEqual([fresh.levelWins, fresh.levelLosses, fresh.diamonds, fresh.highestStep], [0, 0, 0, 0]);
  assert.equal(fresh.lastRankedAt, null); assert.equal(fresh.nextDecayAt, null);
  assert.equal(fresh.seasonId, "ranked-v1"); assert.equal(fresh.placementGamesRemaining, 5);
  assert.equal(fresh.badgePath, "/ranked/icarus.png");
  assert.equal(f.count("ranked_profiles"), 0, "login snapshot is read-only");
});

test("an authoritative duel stores each account's result and full rating history", async (t) => {
  const f = await fixture(t);
  const result = await recordRankedMatch(f.db, match());
  assert.equal(result.ratingAAfter, 1016); assert.equal(result.ratingBAfter, 984);
  assert.equal(result.isSimulation, false);
  const a = await getRankedSnapshot(f.db, "alice");
  const b = await getRankedSnapshot(f.db, "bob");
  assert.deepEqual([a.wins, a.losses, a.draws, a.games], [1, 0, 0, 1]);
  assert.deepEqual([b.wins, b.losses, b.draws, b.games], [0, 1, 0, 1]);
  const overview = await rankedOverview(f.db, "ranked-v1", "alice");
  assert.equal(overview.history.length, 1); assert.equal(overview.history[0].playerAName, "Alice");
  assert.equal(overview.leaderboard[0].position, 1); assert.equal(overview.leaderboard[0].userId, "alice");
});

test("draws are persisted for both players", async (t) => {
  const f = await fixture(t);
  await recordRankedMatch(f.db, match("duel_draw_1", "draw"));
  const profiles = f.profiles();
  assert.equal(profiles[0].draws, 1); assert.equal(profiles[1].draws, 1);
  assert.equal(profiles[0].rating + profiles[1].rating, 2000);
  const overview = await rankedOverview(f.db, "ranked-v1");
  assert.deepEqual(overview.leaderboard.map((player) => player.position), [1, 1], "equal ladder position shares a leaderboard position");
});

test("same match ID is idempotent; a conflicting retry is rejected", async (t) => {
  const f = await fixture(t);
  const first = await recordRankedMatch(f.db, match());
  assert.deepEqual(await recordRankedMatch(f.db, match()), first);
  await assert.rejects(recordRankedMatch(f.db, match("duel_000001", "b")), { status: 409 });
  assert.equal(f.count("ranked_matches"), 1); assert.equal(f.count("audit_events"), 1);
  assert.equal((await getRankedSnapshot(f.db, "alice")).games, 1);
});

test("concurrent duplicate delivery only records the result once", async (t) => {
  const f = await fixture(t);
  const results = await Promise.all([recordRankedMatch(f.db, match()), recordRankedMatch(f.db, match())]);
  assert.deepEqual(results[0], results[1]);
  assert.equal(f.count("ranked_matches"), 1); assert.equal((await getRankedSnapshot(f.db, "alice")).games, 1);
});

test("simultaneous duplicate simulation batches are applied only once", async (t) => {
  const f = await fixture(t);
  const results = await Promise.all([simulateRanked(f.db, simulation(), "admin"), simulateRanked(f.db, simulation(), "admin")]);
  assert.deepEqual(results[0], results[1]);
  assert.equal(f.count("ranked_matches"), 20); assert.equal(f.count("ranked_simulation_batches"), 1);
  assert.equal(f.profiles().reduce((total, profile) => total + profile.games, 0), 40);
});

test("concurrent distinct matches retry stale revisions without losing results", async (t) => {
  const f = await fixture(t);
  await Promise.all(Array.from({ length: 5 }, (_, index) => recordRankedMatch(f.db, match(`duel_multi_${index}`, index % 2 ? "b" : "a"))));
  assert.equal(f.count("ranked_matches"), 5);
  assert.equal((await getRankedSnapshot(f.db, "alice")).games, 5);
  assert.equal((await getRankedSnapshot(f.db, "bob")).games, 5);
  const ledger = f.sqlite.prepare("SELECT * FROM ranked_matches ORDER BY rowid").all();
  let ratingA = 1000; let ratingB = 1000;
  for (const row of ledger) {
    assert.equal(row.rating_a_before, ratingA); assert.equal(row.rating_b_before, ratingB);
    ratingA = row.rating_a_after; ratingB = row.rating_b_after;
  }
  assert.equal(ratingA, (await getRankedSnapshot(f.db, "alice")).rating);
  assert.equal(ratingA + ratingB, 2000);
});

for (const setting of ["blocked", "disabled-ranked", "disabled-play", "missing-settings"]) {
  test(`ineligible accounts cannot record or simulate: ${setting}`, async (t) => {
    const f = await fixture(t);
    if (setting === "blocked") f.sqlite.exec("UPDATE game_users SET status = 'blocked' WHERE id = 'alice'");
    if (setting === "disabled-ranked") f.sqlite.exec("UPDATE game_user_settings SET ranked_enabled = 0 WHERE user_id = 'alice'");
    if (setting === "disabled-play") f.sqlite.exec("UPDATE game_user_settings SET can_play = 0 WHERE user_id = 'alice'");
    if (setting === "missing-settings") f.sqlite.exec("DELETE FROM game_user_settings WHERE user_id = 'alice'");
    await assert.rejects(recordRankedMatch(f.db, match()), { status: 403 });
    await assert.rejects(simulateRanked(f.db, simulation(), "admin"), { status: 403 });
    assert.equal(f.count("ranked_matches"), 0);
  });
}

test("a ban applied between read and write is rechecked inside the transaction", async (t) => {
  const f = await fixture(t);
  f.controls.beforeWrite = () => f.sqlite.exec("UPDATE game_users SET status = 'blocked' WHERE id = 'alice'");
  await assert.rejects(simulateRanked(f.db, simulation(), "admin"), { status: 403 });
  assert.equal(f.count("ranked_matches"), 0); assert.equal(f.count("ranked_simulation_batches"), 0);
  assert.ok(f.profiles().every((profile) => profile.games === 0 && profile.rating === 1000));
});

test("a storage error rolls back every rating, match, batch and audit", async (t) => {
  const f = await fixture(t);
  f.controls.failSql = /INSERT INTO audit_events/;
  await assert.rejects(simulateRanked(f.db, simulation(), "admin"), /Injected storage failure/);
  assert.equal(f.count("ranked_matches"), 0); assert.equal(f.count("ranked_simulation_batches"), 0); assert.equal(f.count("audit_events"), 0);
  assert.ok(f.profiles().every((profile) => profile.games === 0 && profile.rating === 1000));
});

test("simulations persist account results in a completely separate season", async (t) => {
  const f = await fixture(t);
  await recordRankedMatch(f.db, match());
  const realBefore = f.profiles();
  const report = await simulateRanked(f.db, simulation("batch_100duels", 123, 100), "admin");
  assert.equal(report.engine, "statistical-elo-v1"); assert.equal(report.isSimulation, true);
  assert.equal(report.matches.length, 100); assert.equal(report.players.reduce((total, player) => total + player.games, 0), 200);
  assert.equal(report.players.reduce((total, player) => total + player.rating, 0), 3000);
  assert.deepEqual(f.profiles().filter((profile) => profile.season_id === "ranked-v1"), realBefore);
  const overview = await rankedOverview(f.db);
  assert.equal(overview.history.length, 100); assert.ok(overview.history.every((entry) => entry.isSimulation));
  const history = await rankedOverview(f.db, "simulation-v1", "alice");
  assert.ok(history.history.every((entry) => entry.playerAId === "alice" || entry.playerBId === "alice"));
});

test("same seed and initial ratings produce reproducible statistical outcomes", async (t) => {
  const a = await fixture(t); const b = await fixture(t);
  const first = await simulateRanked(a.db, simulation("batch_seed_a", 2026, 100), "admin");
  const second = await simulateRanked(b.db, simulation("batch_seed_b", 2026, 100), "admin");
  const outcomes = (report) => report.matches.map(({ playerAId, playerBId, outcome, deltaA, deltaB }) => ({ playerAId, playerBId, outcome, deltaA, deltaB }));
  assert.deepEqual(outcomes(first), outcomes(second));
  assert.ok(first.matches.some((entry) => entry.outcome === "draw"));
  assert.ok(first.matches.every((entry) => entry.playerAId !== entry.playerBId));
});

test("retrying a simulation batch is idempotent and different settings conflict", async (t) => {
  const f = await fixture(t);
  const first = await simulateRanked(f.db, simulation(), "admin");
  assert.deepEqual(await simulateRanked(f.db, { ...simulation(), accountIds: ["celia", "bob", "alice"] }, "admin"), first);
  await assert.rejects(simulateRanked(f.db, { ...simulation(), seed: 43 }, "admin"), { status: 409 });
  assert.equal(f.count("ranked_matches"), 20); assert.equal(f.count("audit_events"), 1);
});

test("simultaneous batches are atomic and preserve all games", async (t) => {
  const f = await fixture(t);
  await Promise.all([simulateRanked(f.db, simulation("batch_parallel_a"), "admin"), simulateRanked(f.db, simulation("batch_parallel_b", 43), "admin")]);
  assert.equal(f.count("ranked_matches"), 40); assert.equal(f.count("ranked_simulation_batches"), 2);
  assert.equal(f.profiles().reduce((total, profile) => total + profile.games, 0), 80);
  assert.equal(f.profiles().reduce((total, profile) => total + profile.rating, 0), 3000);
});

test("demo accounts have no password, cannot log into game, and never alter existing users", async (t) => {
  const f = await fixture(t);
  const original = f.sqlite.prepare("SELECT * FROM game_users WHERE id = 'alice'").get();
  const ids = await createRankedDemoAccounts(f.db, "admin");
  assert.equal(ids.length, 6);
  await createRankedDemoAccounts(f.db, "admin");
  assert.equal(f.count("game_users"), 9); assert.equal(f.count("game_user_credentials"), 0);
  const settings = f.sqlite.prepare("SELECT can_play, ranked_enabled FROM game_user_settings WHERE user_id = ?").get(ids[0]);
  assert.equal(settings.can_play, 0); assert.equal(settings.ranked_enabled, 1);
  assert.deepEqual(f.sqlite.prepare("SELECT * FROM game_users WHERE id = 'alice'").get(), original);
  await simulateRanked(f.db, { ...simulation(), accountIds: ids }, "admin");
  await assert.rejects(recordRankedMatch(f.db, { ...match(), playerAId: ids[0], playerBId: ids[1] }), { status: 403 });
});

test("input bounds reject self-duels, duplicate accounts, invalid seasons and unbounded workloads", async (t) => {
  const f = await fixture(t);
  await assert.rejects(recordRankedMatch(f.db, { ...match(), playerBId: "alice" }), RankedError);
  const invalid = [
    { count: 0 }, { count: 101 }, { count: 1.2 }, { seed: -1 }, { seed: 2 ** 32 },
    { seed: "42" }, { seasonId: "ranked-v1" }, { accountIds: ["alice", "alice"] },
    { accountIds: ["alice"] }, { batchId: "x" }, { accountIds: Array.from({ length: 33 }, (_, index) => `user${index}`) },
  ];
  for (const value of invalid) await assert.rejects(simulateRanked(f.db, { ...simulation(), ...value }, "admin"), RankedError);
  assert.equal(f.count("ranked_matches"), 0);
});

function request(body, { origin = "https://gameserver.rene007.nl", raw = false } = {}) {
  return new Request("https://gameserver.rene007.nl/api/admin/ranked", { method: "POST", headers: { "content-type": "application/json", origin }, body: raw ? body : JSON.stringify(body) });
}

test("ranked API requires administrator authentication, protects writes and disables caching", async (t) => {
  const f = await fixture(t); workerEnvironment.DB = f.db;
  globalThis.__icarusRankedActor = null;
  assert.equal((await GET(new Request("https://gameserver.rene007.nl/api/admin/ranked"))).status, 401);
  assert.equal((await POST(request({ action: "simulate", ...simulation() }))).status, 401);
  globalThis.__icarusRankedActor = { userId: "admin-id", email: "admin@example.invalid", displayName: "Admin" };
  f.sqlite.exec("INSERT INTO admin_principals (id, user_id, email, display_name, role) VALUES ('principal', 'admin-id', 'admin@example.invalid', 'Admin', 'Viewer')");
  assert.equal((await POST(request({ action: "simulate", ...simulation() }))).status, 403);
  const read = await GET(new Request("https://gameserver.rene007.nl/api/admin/ranked"));
  assert.equal(read.status, 200); assert.equal(read.headers.get("cache-control"), "no-store");
  f.sqlite.exec("UPDATE admin_principals SET role = 'Super Admin'");
  assert.equal((await POST(request({ action: "simulate", ...simulation() }, { origin: "https://evil.invalid" }))).status, 403);
  assert.equal((await POST(request("not-json", { raw: true }))).status, 400);
  assert.equal((await POST(request("x".repeat(5000), { raw: true }))).status, 400);
  const success = await POST(request({ action: "simulate", ...simulation() }));
  assert.equal(success.status, 200); assert.equal((await success.json()).report.matches.length, 20);
  assert.equal((await POST(request({ action: "record-match", ...match() }))).status, 400, "no client result-writing operation exists");
  globalThis.__icarusRankedActor = null;
});

async function enableLogin(f) {
  const credential = await createCredential("ranked-example-password");
  f.sqlite.prepare("INSERT INTO game_user_credentials (id, user_id, salt, hash, iterations) VALUES ('login-alice', 'alice', ?, ?, ?)")
    .run(credential.salt, credential.hash, credential.iterations);
  workerEnvironment.DB = f.db;
  return async (overrides = {}) => {
    const result = await checkAccess(new Request("https://gameserver.rene007.nl/api/game/v1/access/check", {
      method: "POST", headers: { "content-type": "application/json" },
      body: JSON.stringify({ user: "alice@example.invalid", password: "ranked-example-password", gameId: "master-duel", ...overrides }),
    }));
    return { status: result.status, body: await result.json() };
  };
}

test("login exposes only the authenticated account's real rating, never simulated results", async (t) => {
  const f = await fixture(t);
  const login = await enableLogin(f);
  await simulateRanked(f.db, simulation("batch_login_sim", 456, 100), "admin");
  const first = await login();
  assert.equal(first.status, 200); assert.equal(first.body.decision.allowed, true);
  assert.equal(first.body.ranked.userId, "alice"); assert.equal(first.body.ranked.seasonId, "ranked-v1");
  assert.equal(first.body.ranked.isSimulation, false); assert.equal(first.body.ranked.rating, 1000); assert.equal(first.body.ranked.games, 0);
  assert.ok((await getRankedSnapshot(f.db, "alice", "simulation-v1")).games > 0);
  await recordRankedMatch(f.db, match("duel_login_real"));
  const second = await login();
  assert.equal(second.body.ranked.rating, 1016); assert.equal(second.body.ranked.games, 1);
  const wrongPassword = await login({ password: "wrong-password-value" });
  assert.equal(wrongPassword.status, 401); assert.equal(wrongPassword.body.ranked, undefined);
});

test("ranked-disabled and unrelated games get no ranked login snapshot; denied users get none", async (t) => {
  const f = await fixture(t);
  const login = await enableLogin(f);
  f.sqlite.exec("UPDATE game_user_settings SET ranked_enabled = 0 WHERE user_id = 'alice'");
  const disabled = await login();
  assert.equal(disabled.body.decision.allowed, true); assert.equal(disabled.body.ranked, null);
  f.sqlite.exec("INSERT INTO game_user_settings (id, user_id, game_id, can_play, ranked_enabled) VALUES ('other-alice', 'alice', 'other-game', 1, 1)");
  assert.equal((await login({ gameId: "other-game" })).body.ranked, null);
  for (const change of ["UPDATE game_user_settings SET can_play = 0 WHERE user_id = 'alice' AND game_id = 'master-duel'", "DELETE FROM game_user_settings WHERE user_id = 'alice' AND game_id = 'master-duel'"]) {
    f.sqlite.exec(change);
    const denied = await login();
    assert.equal(denied.body.decision.allowed, false); assert.equal(denied.body.ranked, undefined);
  }
  f.sqlite.exec("UPDATE game_users SET status = 'blocked' WHERE id = 'alice'");
  assert.equal((await login()).body.ranked, undefined);
});

const day = 24 * 60 * 60 * 1000;
const testNow = Date.parse("2026-10-19T12:00:00.000Z");

function seedProgress(f, { userId = "alice", seasonId = "ranked-v1", step = 0, levelWins = 0, levelLosses = 0,
  lastRankedAt = null, decayApplied = 0, highestStep = step, diamonds = 0, rating = 1000 } = {}) {
  f.sqlite.prepare(`INSERT OR REPLACE INTO ranked_profiles
    (game_id, season_id, user_id, rating, ladder_step, level_wins, level_losses, last_ranked_at, decay_applied)
    VALUES ('master-duel', ?, ?, ?, ?, ?, ?, ?, ?)`)
    .run(seasonId, userId, rating, step, levelWins, levelLosses, lastRankedAt, decayApplied);
  f.sqlite.prepare("INSERT OR REPLACE INTO ranked_wallets (game_id, user_id, scope, diamonds, highest_step) VALUES ('master-duel', ?, ?, ?, ?)")
    .run(userId, seasonId === "simulation-v1" ? seasonId : "real", diamonds, highestStep);
}

async function play(f, outcomes, { prefix = "progress", seasonId = "ranked-v1", now = testNow } = {}) {
  const matches = [];
  for (let index = 0; index < outcomes.length; index++) {
    const outcome = outcomes[index] === "W" ? "a" : outcomes[index] === "L" ? "b" : "draw";
    matches.push(await recordRankedMatch(f.db, { ...match(`${prefix}_${index.toString().padStart(4, "0")}`, outcome), seasonId }, "test-server", now));
  }
  return matches;
}

test("visible ranks and leaderboard positions never derive from hidden Elo", async (t) => {
  const f = await fixture(t);
  seedProgress(f, { userId: "alice", rating: 3000, step: 0 });
  seedProgress(f, { userId: "bob", rating: 200, step: 30 });
  const a = await getRankedSnapshot(f.db, "alice", "ranked-v1", testNow);
  const b = await getRankedSnapshot(f.db, "bob", "ranked-v1", testNow);
  assert.equal(a.rankName, "Bronze"); assert.equal(a.level, 4);
  assert.equal(b.rankName, "Master"); assert.equal(b.level, 9);
  const overview = await rankedOverview(f.db, "ranked-v1", undefined, testNow);
  assert.deepEqual(overview.leaderboard.map(({ userId }) => userId), ["bob", "alice"]);
});

test("four accumulated wins promote without requiring a streak; draws preserve counters", async (t) => {
  const f = await fixture(t);
  const results = await play(f, "WLWDWW", { prefix: "accumulate_wins" });
  const beforeDraw = results[2].rankAAfter;
  const afterDraw = results[3].rankAAfter;
  assert.deepEqual([beforeDraw.levelWins, beforeDraw.levelLosses], [2, 1]);
  assert.deepEqual([afterDraw.levelWins, afterDraw.levelLosses], [2, 1]);
  const last = results.at(-1);
  assert.equal(last.rankABefore.ladderStep, 0); assert.equal(last.rankAAfter.ladderStep, 1);
  assert.equal(last.rankAAfter.level, 3); assert.equal(last.rewardA, 100);
  assert.deepEqual([last.rankAAfter.levelWins, last.rankAAfter.levelLosses], [0, 0]);
  assert.equal(last.rankAAfter.diamonds, 100);
});

test("three accumulated losses demote, reset both counters and never remove diamonds", async (t) => {
  const f = await fixture(t);
  seedProgress(f, { step: 5, diamonds: 600 });
  const results = await play(f, "LWLWL", { prefix: "accumulate_losses" });
  const last = results.at(-1);
  assert.deepEqual([last.rankABefore.levelWins, last.rankABefore.levelLosses], [2, 2]);
  assert.equal(last.rankAAfter.ladderStep, 4); assert.equal(last.rewardA, 0);
  assert.equal(last.rankAAfter.diamonds, 600);
  assert.deepEqual([last.rankAAfter.levelWins, last.rankAAfter.levelLosses], [0, 0]);
});

test("every tier boundary pays 200 total rather than 100 plus 200", async (t) => {
  const f = await fixture(t);
  for (const step of [3, 8, 14, 21, 29]) {
    seedProgress(f, { step, levelWins: 3 });
    const [result] = await play(f, "W", { prefix: `boundary_${step}` });
    assert.equal(result.rankAAfter.ladderStep, step + 1);
    assert.notEqual(result.rankABefore.rankId, result.rankAAfter.rankId);
    assert.equal(result.rewardA, 200); assert.equal(result.rankAAfter.diamonds, 200);
  }
});

test("Bronze 4 floor and Master 1 ceiling reset bounded counters without rewards", async (t) => {
  const f = await fixture(t);
  seedProgress(f, { step: 0, levelWins: 2, levelLosses: 2 });
  const [floor] = await play(f, "L", { prefix: "bronze_floor" });
  assert.deepEqual([floor.rankAAfter.ladderStep, floor.rankAAfter.levelWins, floor.rankAAfter.levelLosses], [0, 0, 0]);
  assert.equal(floor.rewardA, 0);
  seedProgress(f, { step: 38, levelWins: 3, levelLosses: 1, diamonds: 1000 });
  const [ceiling] = await play(f, "W", { prefix: "master_ceiling" });
  assert.deepEqual([ceiling.rankAAfter.ladderStep, ceiling.rankAAfter.levelWins, ceiling.rankAAfter.levelLosses], [38, 0, 0]);
  assert.equal(ceiling.rewardA, 0); assert.equal(ceiling.rankAAfter.diamonds, 1000);
});

for (const [step, reward] of [[0, 100], [3, 200]]) {
  test(`promotion rewards repeat after demotion (${reward} diamonds), while duplicate delivery never pays twice`, async (t) => {
    const f = await fixture(t);
    seedProgress(f, { step, highestStep: 38 });
    const first = await play(f, "WWWW", { prefix: `first_promote_${step}` });
    assert.equal(first.at(-1).rewardA, reward, "having reached this level earlier does not suppress the reward");
    await play(f, "LLL", { prefix: `demote_${step}` });
    assert.equal((await getRankedSnapshot(f.db, "alice", "ranked-v1", testNow)).ladderStep, step);
    const second = await play(f, "WWWW", { prefix: `again_promote_${step}` });
    assert.equal(second.at(-1).rewardA, reward);
    assert.equal(second.at(-1).rankAAfter.diamonds, reward * 2);
    await recordRankedMatch(f.db, match(`again_promote_${step}_0003`), "test-server", testNow + day);
    const final = await getRankedSnapshot(f.db, "alice", "ranked-v1", testNow + day);
    assert.equal(final.diamonds, reward * 2); assert.equal(final.highestStep, 38);
    assert.equal(f.sqlite.prepare("SELECT COUNT(*) AS n FROM ranked_rewards WHERE user_id = 'alice'").get().n, 2);
  });
}

test("inactivity has an exact full-30-day boundary and repeated checks are idempotent", async (t) => {
  const f = await fixture(t);
  seedProgress(f, { step: 10, levelWins: 2, levelLosses: 1, lastRankedAt: testNow, diamonds: 700 });
  const before = await getRankedSnapshot(f.db, "alice", "ranked-v1", testNow + 30 * day - 1);
  assert.equal(before.ladderStep, 10); assert.equal(before.levelWins, 2);
  const after = await getRankedSnapshot(f.db, "alice", "ranked-v1", testNow + 30 * day);
  assert.equal(after.ladderStep, 9); assert.deepEqual([after.levelWins, after.levelLosses], [0, 0]);
  assert.equal(after.lastRankedAt, new Date(testNow).toISOString());
  assert.equal(after.nextDecayAt, new Date(testNow + 60 * day).toISOString());
  assert.equal(after.diamonds, 700); assert.equal(f.count("ranked_rewards"), 0);
  assert.deepEqual(await getRankedSnapshot(f.db, "alice", "ranked-v1", testNow + 30 * day), after);
});

test("90-day inactivity loses three positions across tiers, floors safely and grants no reward", async (t) => {
  const f = await fixture(t);
  seedProgress(f, { step: 4, lastRankedAt: testNow, diamonds: 500 });
  const after = await getRankedSnapshot(f.db, "alice", "ranked-v1", testNow + 90 * day);
  assert.equal(after.ladderStep, 1); assert.equal(after.rankName, "Bronze"); assert.equal(after.level, 3);
  assert.equal(after.diamonds, 500); assert.equal(after.highestStep, 4);
  assert.equal(after.nextDecayAt, new Date(testNow + 120 * day).toISOString());
  const floor = await getRankedSnapshot(f.db, "alice", "ranked-v1", testNow + 1000 * day);
  assert.equal(floor.ladderStep, 0); assert.equal(floor.diamonds, 500);
  assert.deepEqual(await getRankedSnapshot(f.db, "alice", "ranked-v1", testNow + 1000 * day), floor);
});

test("lookup and leaderboard do not refresh ranked activity; only a finalized duel restarts decay", async (t) => {
  const f = await fixture(t);
  seedProgress(f, { step: 5, lastRankedAt: testNow });
  const overview = await rankedOverview(f.db, "ranked-v1", undefined, testNow + 40 * day);
  assert.equal(overview.leaderboard[0].lastRankedAt, new Date(testNow).toISOString());
  assert.equal(overview.leaderboard[0].ladderStep, 4);
  const [draw] = await play(f, "D", { prefix: "activity_draw", now: testNow + 50 * day });
  assert.equal(draw.rankABefore.ladderStep, 4);
  assert.equal(draw.rankAAfter.lastRankedAt, new Date(testNow + 50 * day).toISOString());
  assert.equal(draw.rankAAfter.nextDecayAt, new Date(testNow + 80 * day).toISOString());
  const beforeThreshold = await getRankedSnapshot(f.db, "alice", "ranked-v1", testNow + 80 * day - 1);
  assert.equal(beforeThreshold.ladderStep, 4);
});

test("concurrent inactivity lookups and a duel cannot apply the same decay twice", async (t) => {
  const f = await fixture(t);
  seedProgress(f, { step: 10, lastRankedAt: testNow });
  const now = testNow + 90 * day;
  await Promise.all([
    getRankedSnapshot(f.db, "alice", "ranked-v1", now),
    getRankedSnapshot(f.db, "alice", "ranked-v1", now),
    recordRankedMatch(f.db, match("concurrent_decay_duel", "draw"), "server", now),
  ]);
  const final = await getRankedSnapshot(f.db, "alice", "ranked-v1", now);
  assert.equal(final.ladderStep, 7); assert.equal(final.games, 1);
  assert.equal(final.lastRankedAt, new Date(now).toISOString());
});

test("promotions and diamonds remain atomic under concurrent distinct matches", async (t) => {
  const f = await fixture(t);
  seedProgress(f, { levelWins: 2 });
  await Promise.all([
    recordRankedMatch(f.db, match("concurrent_promotion_a"), "server", testNow),
    recordRankedMatch(f.db, match("concurrent_promotion_b"), "server", testNow),
  ]);
  const final = await getRankedSnapshot(f.db, "alice", "ranked-v1", testNow);
  assert.deepEqual([final.ladderStep, final.levelWins, final.diamonds], [1, 0, 100]);
  assert.equal(f.sqlite.prepare("SELECT COUNT(*) AS n FROM ranked_rewards WHERE user_id = 'alice'").get().n, 1);
});

test("a failed reward-ledger write rolls back match, progression and wallet together", async (t) => {
  const f = await fixture(t);
  seedProgress(f, { levelWins: 3 });
  f.controls.failSql = /INSERT INTO ranked_rewards/;
  await assert.rejects(recordRankedMatch(f.db, match("failed_reward_ledger"), "server", testNow), /Injected storage failure/);
  const final = await getRankedSnapshot(f.db, "alice", "ranked-v1", testNow);
  assert.deepEqual([final.ladderStep, final.levelWins, final.diamonds, final.games], [0, 3, 0, 0]);
  assert.equal(f.count("ranked_matches"), 0); assert.equal(f.count("ranked_rewards"), 0);
});

test("real diamond wallets span seasons while simulation rewards are isolated projections", async (t) => {
  const f = await fixture(t);
  seedProgress(f, { levelWins: 3 });
  const [first] = await play(f, "W", { prefix: "wallet_season1" });
  assert.equal(first.rewardA, 100);
  const newSeason = await getRankedSnapshot(f.db, "alice", "ranked-v2", testNow);
  assert.deepEqual([newSeason.ladderStep, newSeason.games, newSeason.diamonds], [0, 0, 100]);
  await play(f, "WWWW", { prefix: "wallet_season2", seasonId: "ranked-v2" });
  const shared = await getRankedSnapshot(f.db, "alice", "ranked-v1", testNow);
  assert.equal(shared.diamonds, 200);
  seedProgress(f, { seasonId: "simulation-v1", levelWins: 3 });
  const report = await simulateRanked(f.db, { ...simulation("simulation_wallet", 42, 100), accountIds: ["alice", "bob"] }, "admin", testNow);
  const projected = report.players.find((player) => player.userId === "alice");
  assert.ok(projected.diamonds > 0); assert.equal(projected.isSimulation, true);
  assert.equal((await getRankedSnapshot(f.db, "alice", "ranked-v1", testNow)).diamonds, 200);
  assert.equal(f.sqlite.prepare("SELECT SUM(amount) AS total FROM ranked_rewards WHERE user_id = 'alice' AND scope = 'real'").get().total, 200);
});

test("deployed pre-progression databases upgrade additively without retroactive rewards", async (t) => {
  const f = await fixture(t, { migrationLimit: 5, skipEnsure: true });
  f.sqlite.exec("INSERT INTO ranked_profiles (game_id, season_id, user_id, rating, games, wins, losses, draws) VALUES ('master-duel', 'ranked-v1', 'alice', 1850, 10, 7, 2, 1)");
  f.sqlite.exec(`INSERT INTO ranked_matches (id, game_id, season_id, player_a_id, player_b_id, outcome,
    rating_a_before, rating_a_after, rating_b_before, rating_b_after, delta_a, delta_b, is_simulation)
    VALUES ('legacy_match_01', 'master-duel', 'ranked-v1', 'alice', 'bob', 'a', 1834, 1850, 1000, 984, 16, -16, 0)`);
  await Promise.all([ensureRankedSchema(f.db), ensureRankedSchema(f.db)]);
  await ensureRankedSchema(f.db);
  const current = await getRankedSnapshot(f.db, "alice", "ranked-v1", testNow);
  assert.deepEqual([current.rating, current.games, current.wins, current.losses, current.draws], [1850, 10, 7, 2, 1]);
  assert.deepEqual([current.progressionVersion, current.ladderStep, current.level, current.diamonds], [1, 0, 4, 0]);
  assert.equal(current.lastRankedAt, null); assert.equal(f.count("ranked_rewards"), 0);
  const history = (await rankedOverview(f.db, "ranked-v1", undefined, testNow)).history;
  assert.equal(history[0].rankABefore, null); assert.equal(history[0].rankAAfter, null); assert.equal(history[0].rewardA, 0);
});

for (const table of ["ranked_wallets", "ranked_profiles"]) {
  test(`a participant ${table} row deleted after the read forces transaction rollback`, async (t) => {
    const f = await fixture(t);
    seedProgress(f, { levelWins: 3, diamonds: 500 });
    f.controls.beforeWrite = () => f.sqlite.prepare(`DELETE FROM ${table} WHERE user_id = 'alice'`).run();
    await assert.rejects(recordRankedMatch(f.db, match(`deleted_${table}`), "server", testNow), { status: 409 });
    assert.equal(f.count("ranked_matches"), 0); assert.equal(f.count("ranked_rewards"), 0);
    const untouched = await getRankedSnapshot(f.db, "bob", "ranked-v1", testNow);
    assert.equal(untouched.games, 0); assert.equal(untouched.rating, 1000);
    if (table === "ranked_wallets") {
      assert.equal(f.sqlite.prepare("SELECT ladder_step FROM ranked_profiles WHERE user_id = 'alice'").get().ladder_step, 0);
    } else {
      assert.equal(f.sqlite.prepare("SELECT diamonds FROM ranked_wallets WHERE user_id = 'alice'").get().diamonds, 500);
    }
  });
}

test("wallet revision conflicts preserve external balance changes and retry the complete duel", async (t) => {
  const f = await fixture(t);
  seedProgress(f, { levelWins: 3, diamonds: 500 });
  f.controls.beforeWrite = () => f.sqlite.exec("UPDATE ranked_wallets SET diamonds = diamonds + 50, revision = revision + 1 WHERE user_id = 'alice'");
  const [result] = await play(f, "W", { prefix: "wallet_revision" });
  assert.equal(result.rankABefore.diamonds, 550); assert.equal(result.rankAAfter.diamonds, 650);
  assert.equal(f.count("ranked_matches"), 1); assert.equal(f.count("ranked_rewards"), 1);
});

test("diamond wallets support values beyond 32-bit but reject unsafe numeric overflow atomically", async (t) => {
  const f = await fixture(t);
  seedProgress(f, { levelWins: 3, diamonds: 4_500_000_000 });
  const [wide] = await play(f, "W", { prefix: "wallet_long" });
  assert.equal(wide.rankAAfter.diamonds, 4_500_000_100);
  seedProgress(f, { levelWins: 3, diamonds: Number.MAX_SAFE_INTEGER - 10 });
  await assert.rejects(recordRankedMatch(f.db, match("wallet_overflow"), "server", testNow), { status: 409 });
  assert.equal((await getRankedSnapshot(f.db, "alice", "ranked-v1", testNow)).diamonds, Number.MAX_SAFE_INTEGER - 10);
  assert.equal(f.count("ranked_matches"), 1);
});

test("full climb visits all 39 positions, pays exactly 4300 and rewards a later Master 1 re-promotion", async (t) => {
  const f = await fixture(t);
  const climb = await play(f, "W".repeat(152), { prefix: "full_climb" });
  const promotions = climb.filter((result) => result.rewardA > 0);
  assert.equal(promotions.length, 38);
  assert.deepEqual(promotions.map((result) => result.rankAAfter.ladderStep), Array.from({ length: 38 }, (_, index) => index + 1));
  assert.equal(promotions.filter((result) => result.rewardA === 100).length, 33);
  assert.equal(promotions.filter((result) => result.rewardA === 200).length, 5);
  const summit = climb.at(-1).rankAAfter;
  assert.deepEqual([summit.rankName, summit.level, summit.ladderStep, summit.highestStep, summit.diamonds], ["Master", 1, 38, 38, 4300]);
  const capped = await play(f, "WWWW", { prefix: "full_climb_cap" });
  assert.equal(capped.reduce((total, result) => total + result.rewardA, 0), 0);
  assert.equal(capped.at(-1).rankAAfter.diamonds, 4300);
  const demotion = await play(f, "LLL", { prefix: "full_climb_demote" });
  assert.deepEqual([demotion.at(-1).rankAAfter.rankName, demotion.at(-1).rankAAfter.level], ["Master", 2]);
  const again = await play(f, "WWWW", { prefix: "full_climb_return" });
  assert.equal(again.at(-1).rewardA, 100);
  assert.equal(again.at(-1).rankAAfter.diamonds, 4400);
  assert.equal(again.at(-1).rankAAfter.level, 1);
});

for (const [step, reward] of [[0, 100], [3, 200]]) {
  test(`inactivity demotion permits a genuine re-promotion for ${reward} diamonds, but replayed duels never pay again`, async (t) => {
    const f = await fixture(t);
    seedProgress(f, { step });
    const firstPrefix = `idle_first_${step}`;
    const first = await play(f, "WWWW", { prefix: firstPrefix });
    assert.equal(first.at(-1).rewardA, reward);
    const later = testNow + 30 * day;
    const demoted = await getRankedSnapshot(f.db, "alice", "ranked-v1", later);
    assert.equal(demoted.ladderStep, step); assert.equal(demoted.highestStep, step + 1);
    assert.equal(demoted.diamonds, reward);
    // A delayed technical retry is not a new fourth win or a fresh activity.
    await recordRankedMatch(f.db, match(`${firstPrefix}_0003`), "server", later);
    assert.deepEqual(await getRankedSnapshot(f.db, "alice", "ranked-v1", later), demoted);
    const secondPrefix = `idle_return_${step}`;
    const second = await play(f, "WWWW", { prefix: secondPrefix, now: later + 1 });
    assert.equal(second.at(-1).rankAAfter.ladderStep, step + 1);
    assert.equal(second.at(-1).rewardA, reward);
    const earned = await getRankedSnapshot(f.db, "alice", "ranked-v1", later + 1);
    assert.equal(earned.diamonds, reward * 2); assert.equal(earned.wins, 8);
    await recordRankedMatch(f.db, match(`${secondPrefix}_0003`), "server", later + day);
    assert.deepEqual(await getRankedSnapshot(f.db, "alice", "ranked-v1", later + day), earned);
    assert.equal(f.sqlite.prepare("SELECT COUNT(*) AS n FROM ranked_rewards WHERE user_id = 'alice'").get().n, 2);
  });

  test(`simulation re-promotion after inactivity pays ${reward} projected diamonds; batch replay changes nothing`, async (t) => {
    const f = await fixture(t);
    seedProgress(f, { seasonId: "simulation-v1", step });
    // Seed 8 yields four Alice wins from these equal starting ratings. Her
    // higher rating on the second run still yields those four genuine wins.
    const input = { batchId: `idle_sim_first_${step}`, seed: 8, count: 4, accountIds: ["alice", "bob"] };
    const first = await simulateRanked(f.db, input, "admin", testNow);
    assert.ok(first.matches.every((result) => (result.outcome === "a" ? result.playerAId : result.outcome === "b" ? result.playerBId : null) === "alice"));
    assert.equal(first.players.find((player) => player.userId === "alice").diamonds, reward);
    const later = testNow + 30 * day;
    const demoted = await getRankedSnapshot(f.db, "alice", "simulation-v1", later);
    assert.equal(demoted.ladderStep, step); assert.equal(demoted.diamonds, reward);
    assert.deepEqual(await simulateRanked(f.db, input, "admin", later), first);
    assert.deepEqual(await getRankedSnapshot(f.db, "alice", "simulation-v1", later), demoted);
    const returningInput = { ...input, batchId: `idle_sim_return_${step}` };
    const returning = await simulateRanked(f.db, returningInput, "admin", later + 1);
    const earned = await getRankedSnapshot(f.db, "alice", "simulation-v1", later + 1);
    assert.equal(earned.ladderStep, step + 1); assert.equal(earned.diamonds, reward * 2); assert.equal(earned.wins, 8);
    assert.deepEqual(await simulateRanked(f.db, returningInput, "admin", later + day), returning);
    assert.deepEqual(await getRankedSnapshot(f.db, "alice", "simulation-v1", later + day), earned);
    assert.equal(f.sqlite.prepare("SELECT COUNT(*) AS n FROM ranked_rewards WHERE user_id = 'alice'").get().n, 2);
    assert.equal((await getRankedSnapshot(f.db, "alice", "ranked-v1", later + day)).diamonds, 0);
  });

  test(`concurrent delivery of the reward-bearing fourth win pays ${reward} diamonds exactly once`, async (t) => {
    const f = await fixture(t);
    seedProgress(f, { step });
    await play(f, "WWW", { prefix: `fourth_preparation_${step}` });
    const fourth = match(`fourth_reward_duplicate_${step}`);
    const results = await Promise.all([
      recordRankedMatch(f.db, fourth, "server", testNow),
      recordRankedMatch(f.db, fourth, "server", testNow),
    ]);
    assert.deepEqual(results[0], results[1]); assert.equal(results[0].rewardA, reward);
    const final = await getRankedSnapshot(f.db, "alice", "ranked-v1", testNow);
    assert.deepEqual([final.games, final.wins, final.ladderStep, final.levelWins, final.diamonds], [4, 4, step + 1, 0, reward]);
    assert.equal(f.count("ranked_matches"), 4);
    assert.equal(f.sqlite.prepare("SELECT COUNT(*) AS n FROM ranked_rewards WHERE user_id = 'alice'").get().n, 1);
  });
}

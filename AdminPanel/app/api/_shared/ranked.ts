// Rankings are owned by the server. No client-facing result-writing route may
// call recordRankedMatch without an authoritative, completed duel to verify.
export const rankedGameId = "master-duel";
export const rankedSeasonId = "ranked-v1";
export const simulationSeasonId = "simulation-v1";
export const rankedSeasons = [
  { id: simulationSeasonId, name: "Icarus simulatie", kind: "simulation" },
  { id: rankedSeasonId, name: "Icarus ranked", kind: "ranked" },
] as const;
export const rankedRanks = [
  { id: "icarus", name: "Bronze", patronName: "Icarus", material: "bronze", minRating: 0, startStep: 0, tierLevels: 4 },
  { id: "hermes", name: "Silver", patronName: "Hermes", material: "silver", minRating: 1100, startStep: 4, tierLevels: 5 },
  { id: "athena", name: "Gold", patronName: "Athena", material: "gold", minRating: 1250, startStep: 9, tierLevels: 6 },
  { id: "apollo", name: "Platinum", patronName: "Apollo", material: "platinum", minRating: 1400, startStep: 15, tierLevels: 7 },
  { id: "poseidon", name: "Diamond", patronName: "Poseidon", material: "diamond", minRating: 1550, startStep: 22, tierLevels: 8 },
  { id: "zeus", name: "Master", patronName: "Zeus", material: "master", minRating: 1750, startStep: 30, tierLevels: 9 },
].map((rank) => ({ ...rank, badgePath: `/ranked/${rank.id}.png` }));

export const rankedSchema = [
  `CREATE TABLE IF NOT EXISTS ranked_profiles (
    game_id TEXT NOT NULL, season_id TEXT NOT NULL,
    user_id TEXT NOT NULL REFERENCES game_users(id) ON DELETE CASCADE,
    rating INTEGER NOT NULL DEFAULT 1000, games INTEGER NOT NULL DEFAULT 0,
    wins INTEGER NOT NULL DEFAULT 0, losses INTEGER NOT NULL DEFAULT 0,
    draws INTEGER NOT NULL DEFAULT 0, updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    progression_version INTEGER NOT NULL DEFAULT 1, ladder_step INTEGER NOT NULL DEFAULT 0,
    level_wins INTEGER NOT NULL DEFAULT 0, level_losses INTEGER NOT NULL DEFAULT 0,
    last_ranked_at INTEGER, decay_applied INTEGER NOT NULL DEFAULT 0, revision INTEGER NOT NULL DEFAULT 0,
    PRIMARY KEY(game_id, season_id, user_id),
    CHECK(games = wins + losses + draws), CHECK(games >= 0)
  )`,
  `CREATE TABLE IF NOT EXISTS ranked_matches (
    id TEXT PRIMARY KEY, game_id TEXT NOT NULL, season_id TEXT NOT NULL,
    player_a_id TEXT NOT NULL REFERENCES game_users(id) ON DELETE CASCADE,
    player_b_id TEXT NOT NULL REFERENCES game_users(id) ON DELETE CASCADE,
    outcome TEXT NOT NULL CHECK(outcome IN ('a','b','draw')),
    rating_a_before INTEGER NOT NULL, rating_a_after INTEGER NOT NULL,
    rating_b_before INTEGER NOT NULL, rating_b_after INTEGER NOT NULL,
    delta_a INTEGER NOT NULL, delta_b INTEGER NOT NULL,
    is_simulation INTEGER NOT NULL, batch_id TEXT,
    rank_a_before TEXT, rank_a_after TEXT, rank_b_before TEXT, rank_b_after TEXT,
    reward_a INTEGER NOT NULL DEFAULT 0, reward_b INTEGER NOT NULL DEFAULT 0,
    created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CHECK(player_a_id <> player_b_id), CHECK(delta_a + delta_b = 0)
  )`,
  `CREATE INDEX IF NOT EXISTS idx_ranked_matches_season ON ranked_matches(game_id, season_id, created_at)`,
  `CREATE TABLE IF NOT EXISTS ranked_simulation_batches (
    id TEXT PRIMARY KEY, request_json TEXT NOT NULL, report_json TEXT NOT NULL,
    created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
  )`,
  `CREATE TABLE IF NOT EXISTS ranked_wallets (
    game_id TEXT NOT NULL, user_id TEXT NOT NULL REFERENCES game_users(id) ON DELETE CASCADE,
    scope TEXT NOT NULL, diamonds INTEGER NOT NULL DEFAULT 0, highest_step INTEGER NOT NULL DEFAULT 0,
    revision INTEGER NOT NULL DEFAULT 0, PRIMARY KEY(game_id, user_id, scope)
  )`,
  `CREATE TABLE IF NOT EXISTS ranked_rewards (
    game_id TEXT NOT NULL, user_id TEXT NOT NULL REFERENCES game_users(id) ON DELETE CASCADE,
    scope TEXT NOT NULL, highest_step INTEGER NOT NULL, amount INTEGER NOT NULL,
    match_id TEXT NOT NULL REFERENCES ranked_matches(id) ON DELETE CASCADE,
    created_at TEXT NOT NULL, PRIMARY KEY(game_id, user_id, scope, match_id)
  )`,
];

type RankedDb = Pick<D1Database, "prepare" | "batch">;
type Profile = {
  user_id: string; rating: number; games: number; wins: number; losses: number; draws: number;
  progression_version: number; ladder_step: number; level_wins: number; level_losses: number;
  last_ranked_at: number | null; decay_applied: number; revision: number;
  wallet_diamonds: number; wallet_highest: number; wallet_revision: number;
};
type Account = { id: string; display_name: string; email: string; status: string; access_group: string; can_play: number | null; ranked_enabled: number | null };
export type RankedOutcome = "a" | "b" | "draw";
export type RankedMatch = {
  id: string; seasonId: string; playerAId: string; playerBId: string;
  playerAName: string; playerBName: string; outcome: RankedOutcome;
  ratingABefore: number; ratingAAfter: number; ratingBBefore: number; ratingBAfter: number;
  deltaA: number; deltaB: number; isSimulation: boolean; createdAt: string;
  rankABefore: RankedSnapshot | null; rankAAfter: RankedSnapshot | null;
  rankBBefore: RankedSnapshot | null; rankBAfter: RankedSnapshot | null;
  rewardA: number; rewardB: number;
};
type MatchRow = {
  id: string; season_id: string; player_a_id: string; player_b_id: string;
  player_a_name: string; player_b_name: string; outcome: RankedOutcome;
  rating_a_before: number; rating_a_after: number; rating_b_before: number; rating_b_after: number;
  delta_a: number; delta_b: number; is_simulation: number; created_at: string;
  rank_a_before: string | null; rank_a_after: string | null; rank_b_before: string | null; rank_b_after: string | null;
  reward_a: number; reward_b: number;
};
export class RankedError extends Error {
  status: number;
  constructor(message: string, status = 400) { super(message); this.status = status; }
}

export async function ensureRankedSchema(db: RankedDb) {
  await db.batch(rankedSchema.map((sql) => db.prepare(sql)));
  // Deployed installations predate progression. Add columns without rebuilding
  // tables or touching historical MMR, outcomes, accounts or wallet balances.
  const additions: Record<string, Record<string, string>> = {
    ranked_profiles: { progression_version: "INTEGER NOT NULL DEFAULT 1", ladder_step: "INTEGER NOT NULL DEFAULT 0",
      level_wins: "INTEGER NOT NULL DEFAULT 0", level_losses: "INTEGER NOT NULL DEFAULT 0", last_ranked_at: "INTEGER",
      decay_applied: "INTEGER NOT NULL DEFAULT 0", revision: "INTEGER NOT NULL DEFAULT 0" },
    ranked_matches: { rank_a_before: "TEXT", rank_a_after: "TEXT", rank_b_before: "TEXT", rank_b_after: "TEXT",
      reward_a: "INTEGER NOT NULL DEFAULT 0", reward_b: "INTEGER NOT NULL DEFAULT 0" },
  };
  for (const [table, columns] of Object.entries(additions)) {
    const existing = (await db.prepare(`PRAGMA table_info(${table})`).all<{ name: string }>()).results;
    const missing = Object.entries(columns).filter(([name]) => !existing.some((column) => column.name === name));
    if (!missing.length) continue;
    try {
      await db.batch(missing.map(([name, definition]) => db.prepare(`ALTER TABLE ${table} ADD COLUMN ${name} ${definition}`)));
    } catch (error) {
      // Two workers can discover the same old schema concurrently. Only accept
      // the race if every required column now exists; all other failures remain.
      const actual = (await db.prepare(`PRAGMA table_info(${table})`).all<{ name: string }>()).results;
      if (!Object.keys(columns).every((name) => actual.some((column) => column.name === name))) throw error;
    }
  }
}

// Legacy descriptor lookup retained for older administrative tooling only.
// Visible progression MUST use rankForStep; Elo is hidden matchmaking strength.
export function rankForRating(rating: number) {
  return [...rankedRanks].reverse().find((rank) => rating >= rank.minRating) ?? rankedRanks[0];
}

export function rankForStep(step: number) {
  if (!Number.isInteger(step) || step < 0 || step > 38) throw new RankedError("Ongeldige ladderpositie.");
  const rank = [...rankedRanks].reverse().find((entry) => step >= entry.startStep)!;
  return { ...rank, level: rank.tierLevels - (step - rank.startStep) };
}

const decayPeriod = 30 * 24 * 60 * 60 * 1000;
const profileColumns = `p.user_id, p.rating, p.games, p.wins, p.losses, p.draws, p.progression_version,
  p.ladder_step, p.level_wins, p.level_losses, p.last_ranked_at, p.decay_applied, p.revision,
  COALESCE(w.diamonds, 0) AS wallet_diamonds, COALESCE(w.highest_step, 0) AS wallet_highest, COALESCE(w.revision, 0) AS wallet_revision`;
function walletScope(seasonId: string) { return seasonId === simulationSeasonId ? simulationSeasonId : "real"; }
function timestamp(now: number) {
  if (!Number.isSafeInteger(now) || now < 0 || !Number.isFinite(new Date(now).getTime())) throw new RankedError("Ongeldige ranked-datum.");
  return new Date(now).toISOString();
}

// Use one rounded delta and its negation, including for draws: every duel
// conserves the two players' total Elo. At the zero/safe-integer boundaries,
// limit the transfer instead of creating or destroying rating points.
export function eloChange(ratingA: number, ratingB: number, outcome: RankedOutcome) {
  if (![ratingA, ratingB].every((rating) => Number.isSafeInteger(rating) && rating >= 0) || !["a", "b", "draw"].includes(outcome)) {
    throw new RankedError("Ongeldige Elo-invoer.");
  }
  const expected = 1 / (1 + 10 ** ((ratingB - ratingA) / 400));
  const score = outcome === "a" ? 1 : outcome === "b" ? 0 : 0.5;
  const proposed = Math.round(32 * (score - expected)) || 0;
  const deltaA = Math.max(-Math.min(ratingA, Number.MAX_SAFE_INTEGER - ratingB),
    Math.min(proposed, ratingB, Number.MAX_SAFE_INTEGER - ratingA));
  return { deltaA, deltaB: -deltaA || 0, ratingAAfter: ratingA + deltaA, ratingBAfter: ratingB - deltaA };
}

function snapshot(profile: Profile, seasonId: string) {
  const rank = rankForStep(profile.ladder_step);
  return {
    userId: profile.user_id, seasonId, rating: profile.rating, games: profile.games,
    wins: profile.wins, losses: profile.losses, draws: profile.draws,
    rankId: rank.id, rankName: rank.name, rankPatronName: rank.patronName, badgePath: rank.badgePath,
    progressionVersion: 1, ladderStep: profile.ladder_step, level: rank.level, tierLevels: rank.tierLevels,
    levelWins: profile.level_wins, levelLosses: profile.level_losses, winsRequired: 4, lossesRequired: 3,
    diamonds: profile.wallet_diamonds, highestStep: profile.wallet_highest,
    lastRankedAt: profile.last_ranked_at === null ? null : timestamp(profile.last_ranked_at),
    nextDecayAt: profile.last_ranked_at === null ? null : timestamp(profile.last_ranked_at + (profile.decay_applied + 1) * decayPeriod),
    isSimulation: seasonId === simulationSeasonId,
    placementGamesRemaining: Math.max(0, 5 - profile.games),
  };
}
export type RankedSnapshot = ReturnType<typeof snapshot>;

async function readProfile(db: RankedDb, userId: string, seasonId: string) {
  return db.prepare(`SELECT ${profileColumns} FROM ranked_profiles p LEFT JOIN ranked_wallets w
    ON w.game_id = p.game_id AND w.user_id = p.user_id AND w.scope = ?
    WHERE p.game_id = ? AND p.season_id = ? AND p.user_id = ?`)
    .bind(walletScope(seasonId), rankedGameId, seasonId, userId).first<Profile>();
}

async function profileAfterDecay(db: RankedDb, userId: string, seasonId: string, now: number) {
  for (let attempt = 0; attempt < 8; attempt++) {
    const profile = await readProfile(db, userId, seasonId);
    if (!profile || profile.last_ranked_at === null) return profile;
    const elapsed = Math.max(0, Math.floor((now - profile.last_ranked_at) / decayPeriod));
    const overdue = elapsed - profile.decay_applied;
    if (overdue <= 0) return profile;
    const step = Math.max(0, profile.ladder_step - overdue);
    const result = await db.prepare(`UPDATE ranked_profiles SET ladder_step = ?, decay_applied = ?,
      level_wins = ?, level_losses = ?, revision = revision + 1, updated_at = CURRENT_TIMESTAMP
      WHERE game_id = ? AND season_id = ? AND user_id = ? AND revision = ?`)
      .bind(step, elapsed, step !== profile.ladder_step ? 0 : profile.level_wins, step !== profile.ladder_step ? 0 : profile.level_losses,
        rankedGameId, seasonId, userId, profile.revision).run();
    if (result.meta.changes) continue; // Re-read the committed wallet/progression together.
  }
  throw new RankedError("De ranked-voortgang is gelijktijdig gewijzigd. Probeer opnieuw.", 409);
}

export async function getRankedSnapshot(db: RankedDb, userId: string, seasonId = rankedSeasonId, now = Date.now()) {
  checkSeason(seasonId);
  timestamp(now);
  const profile = await profileAfterDecay(db, userId, seasonId, now);
  if (profile) return snapshot(profile, seasonId);
  const wallet = await db.prepare("SELECT diamonds AS wallet_diamonds, highest_step AS wallet_highest, revision AS wallet_revision FROM ranked_wallets WHERE game_id = ? AND user_id = ? AND scope = ?")
    .bind(rankedGameId, userId, walletScope(seasonId)).first<Pick<Profile, "wallet_diamonds" | "wallet_highest" | "wallet_revision">>();
  return snapshot({ ...initialProfile(userId), ...wallet }, seasonId);
}

function initialProfile(userId: string): Profile {
  return { user_id: userId, rating: 1000, games: 0, wins: 0, losses: 0, draws: 0,
    progression_version: 1, ladder_step: 0, level_wins: 0, level_losses: 0, last_ranked_at: null, decay_applied: 0,
    revision: 0, wallet_diamonds: 0, wallet_highest: 0, wallet_revision: 0 };
}

function checkSeason(seasonId: string) {
  if (seasonId !== simulationSeasonId && !/^ranked-v[1-9][0-9]{0,5}$/.test(seasonId)) throw new RankedError("Onbekend ranked-seizoen.");
}

function isDemo(account: Account) {
  return account.id.startsWith("ranked_demo_") && account.access_group === "Ranked Simulation" && account.email.endsWith("@ranked.icarus.invalid");
}

function eligible(account: Account, seasonId: string) {
  return account.status === "active" && account.ranked_enabled === 1 &&
    (account.can_play === 1 || (seasonId === simulationSeasonId && isDemo(account)));
}

function eligibilitySql(seasonId: string) {
  // Rechecked inside the write transaction, not only when the UI loaded.
  return `EXISTS (SELECT 1 FROM game_users u JOIN game_user_settings s ON s.user_id = u.id AND s.game_id = 'master-duel'
    WHERE u.id = ranked_profiles.user_id AND u.status = 'active' AND s.ranked_enabled = 1
    AND (s.can_play = 1 ${seasonId === simulationSeasonId ? "OR (substr(u.id, 1, 12) = 'ranked_demo_' AND u.access_group = 'Ranked Simulation' AND u.email LIKE '%@ranked.icarus.invalid')" : ""}))`;
}

async function accounts(db: RankedDb) {
  return (await db.prepare(`SELECT u.id, u.display_name, u.email, u.status, u.access_group, s.can_play, s.ranked_enabled
    FROM game_users u LEFT JOIN game_user_settings s ON s.user_id = u.id AND s.game_id = ?
    ORDER BY u.display_name, u.id`).bind(rankedGameId).all<Account>()).results;
}

async function profilesFor(db: RankedDb, ids: string[], seasonId: string, now: number) {
  const allAccounts = await accounts(db);
  const selected = ids.map((id) => {
    const account = allAccounts.find((entry) => entry.id === id);
    if (!account || !eligible(account, seasonId)) throw new RankedError("Een geselecteerd account mag niet deelnemen aan dit ranked-seizoen.", 403);
    return account;
  });
  await db.batch(selected.flatMap((account) => [db.prepare(`INSERT OR IGNORE INTO ranked_profiles (game_id, season_id, user_id)
    SELECT ?, ?, u.id FROM game_users u WHERE u.id = ?`)
    .bind(rankedGameId, seasonId, account.id),
    db.prepare("INSERT OR IGNORE INTO ranked_wallets (game_id, user_id, scope) VALUES (?, ?, ?)")
      .bind(rankedGameId, account.id, walletScope(seasonId))]));
  const profiles = new Map<string, Profile>();
  for (const account of selected) {
    const row = await profileAfterDecay(db, account.id, seasonId, now);
    if (!row) throw new RankedError("Ranked-account is niet meer beschikbaar.", 409);
    profiles.set(account.id, row);
  }
  return { profiles, selected };
}

function advanceProgress(profile: Profile, outcome: "win" | "loss" | "draw", now: number) {
  const previousStep = profile.ladder_step;
  let reward = 0;
  if (outcome === "win") profile.level_wins++;
  else if (outcome === "loss") profile.level_losses++;
  if (profile.level_wins >= 4 || profile.level_losses >= 3) {
    profile.ladder_step = profile.level_wins >= 4 ? Math.min(38, previousStep + 1) : Math.max(0, previousStep - 1);
    profile.level_wins = 0; profile.level_losses = 0;
    if (profile.ladder_step > previousStep) {
      reward = rankForStep(profile.ladder_step).id !== rankForStep(previousStep).id ? 200 : 100;
      profile.wallet_diamonds += reward;
      if (!Number.isSafeInteger(profile.wallet_diamonds)) throw new RankedError("De diamantbalans kan niet veilig worden verhoogd.", 409);
      profile.wallet_highest = Math.max(profile.wallet_highest, profile.ladder_step);
    }
  }
  profile.last_ranked_at = Math.max(profile.last_ranked_at ?? now, now);
  profile.decay_applied = 0;
  return reward;
}

function outcomeMatch(id: string, seasonId: string, a: Profile, b: Profile, outcome: RankedOutcome, names: Map<string, string>, createdAt: string): RankedMatch {
  const delta = eloChange(a.rating, b.rating, outcome);
  const rankABefore = snapshot(a, seasonId); const rankBBefore = snapshot(b, seasonId);
  const match: RankedMatch = { id, seasonId, playerAId: a.user_id, playerBId: b.user_id,
    playerAName: names.get(a.user_id) ?? a.user_id, playerBName: names.get(b.user_id) ?? b.user_id,
    outcome, ratingABefore: a.rating, ratingBBefore: b.rating, ...delta,
    isSimulation: seasonId === simulationSeasonId, createdAt,
    rankABefore, rankBBefore, rankAAfter: null, rankBAfter: null, rewardA: 0, rewardB: 0 };
  a.rating = delta.ratingAAfter; b.rating = delta.ratingBAfter;
  a.games++; b.games++;
  if (outcome === "draw") { a.draws++; b.draws++; }
  else if (outcome === "a") { a.wins++; b.losses++; }
  else { a.losses++; b.wins++; }
  const now = Date.parse(createdAt);
  match.rewardA = advanceProgress(a, outcome === "a" ? "win" : outcome === "b" ? "loss" : "draw", now);
  match.rewardB = advanceProgress(b, outcome === "b" ? "win" : outcome === "a" ? "loss" : "draw", now);
  match.rankAAfter = snapshot(a, seasonId); match.rankBAfter = snapshot(b, seasonId);
  return match;
}

function matchFromRow(row: MatchRow): RankedMatch {
  return { id: row.id, seasonId: row.season_id, playerAId: row.player_a_id, playerBId: row.player_b_id,
    playerAName: row.player_a_name, playerBName: row.player_b_name, outcome: row.outcome,
    ratingABefore: row.rating_a_before, ratingAAfter: row.rating_a_after,
    ratingBBefore: row.rating_b_before, ratingBAfter: row.rating_b_after,
    deltaA: row.delta_a, deltaB: row.delta_b, isSimulation: row.is_simulation === 1, createdAt: row.created_at,
    rankABefore: row.rank_a_before ? JSON.parse(row.rank_a_before) as RankedSnapshot : null,
    rankAAfter: row.rank_a_after ? JSON.parse(row.rank_a_after) as RankedSnapshot : null,
    rankBBefore: row.rank_b_before ? JSON.parse(row.rank_b_before) as RankedSnapshot : null,
    rankBAfter: row.rank_b_after ? JSON.parse(row.rank_b_after) as RankedSnapshot : null,
    rewardA: row.reward_a, rewardB: row.reward_b };
}

const matchSelect = `SELECT m.*, a.display_name AS player_a_name, b.display_name AS player_b_name FROM ranked_matches m
  JOIN game_users a ON a.id = m.player_a_id JOIN game_users b ON b.id = m.player_b_id`;

// Optimistic reads are validated INSIDE a single atomic D1 batch. A stale
// revision deliberately violates NOT NULL, rolling back every result, rating,
// audit and batch record. This works across worker instances (no process lock).
function commitStatements(db: RankedDb, before: Map<string, Profile>, after: Map<string, Profile>, matches: RankedMatch[], batchId: string | null) {
  const seasonId = matches[0].seasonId;
  const statements = [...after].flatMap(([id, profile]) => {
    const previous = before.get(id)!;
    return [db.prepare(`UPDATE ranked_profiles SET rating = CASE WHEN rating = ? AND games = ? AND wins = ? AND losses = ? AND draws = ? AND revision = ?
      AND ${eligibilitySql(seasonId)} THEN ? ELSE NULL END, games = ?, wins = ?, losses = ?, draws = ?, updated_at = CURRENT_TIMESTAMP
      , progression_version = 1, ladder_step = ?, level_wins = ?, level_losses = ?, last_ranked_at = ?, decay_applied = ?, revision = revision + 1
      WHERE game_id = ? AND season_id = ? AND user_id = ?`)
      .bind(previous.rating, previous.games, previous.wins, previous.losses, previous.draws, previous.revision,
        profile.rating, profile.games, profile.wins, profile.losses, profile.draws,
        profile.ladder_step, profile.level_wins, profile.level_losses, profile.last_ranked_at, profile.decay_applied, rankedGameId, seasonId, id),
    db.prepare(`UPDATE ranked_wallets SET diamonds = CASE WHEN revision = ? AND diamonds = ? AND highest_step = ? THEN ? ELSE NULL END,
      highest_step = ?, revision = revision + 1 WHERE game_id = ? AND user_id = ? AND scope = ?`)
      .bind(previous.wallet_revision, previous.wallet_diamonds, previous.wallet_highest, profile.wallet_diamonds, profile.wallet_highest,
        rankedGameId, id, walletScope(seasonId))];
  });
  for (const match of matches) {
    statements.push(db.prepare(`INSERT INTO ranked_matches
      (id, game_id, season_id, player_a_id, player_b_id, outcome, rating_a_before, rating_a_after,
       rating_b_before, rating_b_after, delta_a, delta_b, is_simulation, batch_id, created_at,
       rank_a_before, rank_a_after, rank_b_before, rank_b_after, reward_a, reward_b)
      VALUES (?, ?, ?, ?, ?, ?, ?, CASE WHEN
        (SELECT COUNT(*) FROM ranked_profiles WHERE game_id = ? AND season_id = ? AND user_id IN (?, ?)) = 2
        AND (SELECT COUNT(*) FROM ranked_wallets WHERE game_id = ? AND scope = ? AND user_id IN (?, ?)) = 2
        THEN ? ELSE NULL END, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)`)
      .bind(match.id, rankedGameId, seasonId, match.playerAId, match.playerBId, match.outcome, match.ratingABefore,
        rankedGameId, seasonId, match.playerAId, match.playerBId, rankedGameId, walletScope(seasonId), match.playerAId, match.playerBId, match.ratingAAfter,
        match.ratingBBefore, match.ratingBAfter, match.deltaA, match.deltaB, match.isSimulation ? 1 : 0, batchId, match.createdAt,
        JSON.stringify(match.rankABefore), JSON.stringify(match.rankAAfter), JSON.stringify(match.rankBBefore), JSON.stringify(match.rankBAfter), match.rewardA, match.rewardB));
    for (const [userId, reward, rankAfter] of [[match.playerAId, match.rewardA, match.rankAAfter], [match.playerBId, match.rewardB, match.rankBAfter]] as const) {
      if (reward > 0 && rankAfter) statements.push(db.prepare(`INSERT INTO ranked_rewards (game_id, user_id, scope, highest_step, amount, match_id, created_at)
        VALUES (?, ?, ?, ?, ?, ?, ?)`)
        .bind(rankedGameId, userId, walletScope(seasonId), rankAfter.ladderStep, reward, match.id, match.createdAt));
    }
  }
  return statements;
}

function cloneProfiles(profiles: Map<string, Profile>) {
  return new Map([...profiles].map(([id, profile]) => [id, { ...profile }]));
}

async function commitParticipantsExist(db: RankedDb, ids: string[], seasonId: string) {
  const parameters = ids.map(() => "?").join(",");
  const profiles = await db.prepare(`SELECT COUNT(*) AS count FROM ranked_profiles WHERE game_id = ? AND season_id = ? AND user_id IN (${parameters})`)
    .bind(rankedGameId, seasonId, ...ids).first<{ count: number }>();
  const wallets = await db.prepare(`SELECT COUNT(*) AS count FROM ranked_wallets WHERE game_id = ? AND scope = ? AND user_id IN (${parameters})`)
    .bind(rankedGameId, walletScope(seasonId), ...ids).first<{ count: number }>();
  return profiles?.count === ids.length && wallets?.count === ids.length;
}

function validIdentifier(value: unknown, label: string) {
  if (typeof value !== "string" || !/^[a-zA-Z0-9_-]{8,80}$/.test(value)) throw new RankedError(`Ongeldig ${label}; gebruik 8 tot 80 letters, cijfers, streepjes of underscores.`);
  return value;
}

export async function recordRankedMatch(db: RankedDb, input: { matchId: string; playerAId: string; playerBId: string; outcome: RankedOutcome; seasonId?: string }, actor = "authoritative-duel-server", now = Date.now()) {
  const seasonId = input.seasonId ?? rankedSeasonId;
  checkSeason(seasonId);
  if (seasonId === simulationSeasonId) throw new RankedError("Echte duelresultaten mogen niet in het simulatieseizoen.");
  const createdAt = timestamp(now);
  validIdentifier(input.matchId, "duel-ID");
  if (input.playerAId === input.playerBId) throw new RankedError("Een account kan niet tegen zichzelf spelen.");
  if (!["a", "b", "draw"].includes(input.outcome)) throw new RankedError("Ongeldige dueluitslag.");
  const existing = async () => {
    const row = await db.prepare(`${matchSelect} WHERE m.id = ?`).bind(input.matchId).first<MatchRow>();
    if (!row) return null;
    if (row.season_id !== seasonId || row.player_a_id !== input.playerAId || row.player_b_id !== input.playerBId || row.outcome !== input.outcome) {
      throw new RankedError("Dit duel-ID hoort al bij een andere uitslag.", 409);
    }
    return matchFromRow(row);
  };
  for (let attempt = 0; attempt < 8; attempt++) {
    const prior = await existing();
    if (prior) return prior;
    const { profiles: before, selected } = await profilesFor(db, [input.playerAId, input.playerBId], seasonId, now);
    const after = cloneProfiles(before);
    const match = outcomeMatch(input.matchId, seasonId, after.get(input.playerAId)!, after.get(input.playerBId)!, input.outcome,
      new Map(selected.map((account) => [account.id, account.display_name])), createdAt);
    try {
      await db.batch([...commitStatements(db, before, after, [match], null),
        auditStatement(db, actor, "Ranked-duel opgeslagen", input.matchId, `Serverbevestigde uitslag verwerkt in ${seasonId}.`)]);
      return match;
    } catch (error) {
      const duplicate = await existing();
      if (duplicate) return duplicate;
      if (!staleWrite(error)) throw error;
      if (!await commitParticipantsExist(db, [...before.keys()], seasonId)) throw new RankedError("Ranked-gegevens zijn tijdens het opslaan verwijderd. De uitslag is niet verwerkt.", 409);
    }
  }
  throw new RankedError("Ranked-resultaat is gelijktijdig gewijzigd. Probeer opnieuw met hetzelfde duel-ID.", 409);
}

type SimulationInput = { accountIds: string[]; count: number; seed: number; batchId: string; seasonId?: string };
type SimulationReport = {
  batchId: string; seasonId: string; seed: number; count: number; isSimulation: true;
  engine: "statistical-elo-v1"; matches: RankedMatch[];
  players: (ReturnType<typeof snapshot> & { displayName: string })[];
};

export async function simulateRanked(db: RankedDb, input: SimulationInput, actor: string, now = Date.now()): Promise<SimulationReport> {
  const createdAt = timestamp(now);
  validIdentifier(input.batchId, "simulatie-ID");
  if (input.seasonId !== undefined && input.seasonId !== simulationSeasonId) throw new RankedError("Simulaties mogen uitsluitend in het simulatieseizoen.");
  if (!Number.isInteger(input.count) || input.count < 1 || input.count > 100) throw new RankedError("Kies 1 tot 100 simulatieduels.");
  if (!Number.isInteger(input.seed) || input.seed < 0 || input.seed > 0xffffffff) throw new RankedError("De seed moet een geheel getal van 0 tot 4294967295 zijn.");
  if (!Array.isArray(input.accountIds) || input.accountIds.length < 2 || input.accountIds.length > 32 ||
      input.accountIds.some((id) => typeof id !== "string" || !id || id.length > 100) || new Set(input.accountIds).size !== input.accountIds.length) {
    throw new RankedError("Selecteer 2 tot 32 verschillende accounts.");
  }
  const ids = [...input.accountIds].sort();
  const requestJson = JSON.stringify({ accountIds: ids, count: input.count, seed: input.seed, seasonId: simulationSeasonId });
  const existing = async () => {
    const row = await db.prepare("SELECT request_json, report_json FROM ranked_simulation_batches WHERE id = ?")
      .bind(input.batchId).first<{ request_json: string; report_json: string }>();
    if (!row) return null;
    if (row.request_json !== requestJson) throw new RankedError("Dit simulatie-ID hoort al bij andere instellingen.", 409);
    return JSON.parse(row.report_json) as SimulationReport;
  };
  for (let attempt = 0; attempt < 8; attempt++) {
    const prior = await existing();
    if (prior) return prior;
    const { profiles: before, selected } = await profilesFor(db, ids, simulationSeasonId, now);
    const after = cloneProfiles(before);
    const names = new Map(selected.map((account) => [account.id, account.display_name]));
    let state = input.seed >>> 0;
    const random = () => { state = (Math.imul(state, 1664525) + 1013904223) >>> 0; return state / 0x100000000; };
    const matches: RankedMatch[] = [];
    for (let index = 0; index < input.count; index++) {
      const aIndex = Math.floor(random() * ids.length);
      const bIndex = (aIndex + 1 + Math.floor(random() * (ids.length - 1))) % ids.length;
      const a = after.get(ids[aIndex])!; const b = after.get(ids[bIndex])!;
      const roll = random();
      const expected = 1 / (1 + 10 ** ((b.rating - a.rating) / 400));
      const outcome = roll < 0.05 ? "draw" : (roll - 0.05) / 0.95 < expected ? "a" : "b";
      matches.push(outcomeMatch(`sim_${input.batchId}_${index + 1}`, simulationSeasonId, a, b, outcome, names, createdAt));
    }
    const report: SimulationReport = { batchId: input.batchId, seasonId: simulationSeasonId, seed: input.seed,
      count: input.count, isSimulation: true, engine: "statistical-elo-v1", matches,
      players: [...after.values()].map((profile) => ({ ...snapshot(profile, simulationSeasonId), displayName: names.get(profile.user_id)! })) };
    try {
      await db.batch([
        db.prepare("INSERT INTO ranked_simulation_batches (id, request_json, report_json) VALUES (?, ?, ?)").bind(input.batchId, requestJson, JSON.stringify(report)),
        ...commitStatements(db, before, after, matches, input.batchId),
        auditStatement(db, actor, "Ranked-simulatie opgeslagen", input.batchId, `${input.count} statistische simulatieduels; geen kaartengine; echte ranked onaangetast.`),
      ]);
      return report;
    } catch (error) {
      const duplicate = await existing();
      if (duplicate) return duplicate;
      if (!staleWrite(error)) throw error;
      if (!await commitParticipantsExist(db, ids, simulationSeasonId)) throw new RankedError("Ranked-gegevens zijn tijdens het opslaan verwijderd. De simulatie is niet verwerkt.", 409);
    }
  }
  throw new RankedError("De ranking is gelijktijdig gewijzigd. Probeer opnieuw met hetzelfde simulatie-ID.", 409);
}

function staleWrite(error: unknown) {
  return /NOT NULL constraint failed: ranked_(profiles.rating|wallets.diamonds|matches.rating_a_after)/.test(String(error));
}

function auditStatement(db: RankedDb, actor: string, action: string, target: string, detail: string) {
  return db.prepare("INSERT INTO audit_events (id, actor, action, target, detail, severity) VALUES (?, ?, ?, ?, ?, 'info')")
    .bind(crypto.randomUUID(), actor, action, target, detail);
}

export async function createRankedDemoAccounts(db: RankedDb, actor: string) {
  const names = ["Icarus", "Hermes", "Athena", "Apollo", "Poseidon", "Zeus"];
  const statements: D1PreparedStatement[] = [];
  const ids = names.map((name) => `ranked_demo_${name.toLowerCase()}`);
  for (let index = 0; index < ids.length; index++) {
    const id = ids[index]; const email = `${names[index].toLowerCase()}@ranked.icarus.invalid`;
    statements.push(db.prepare(`INSERT OR IGNORE INTO game_users (id, display_name, email, role, status, access_group)
      VALUES (?, ?, ?, 'Tester', 'active', 'Ranked Simulation')`).bind(id, `Simulatie ${names[index]}`, email));
    statements.push(db.prepare(`INSERT OR IGNORE INTO game_user_settings (id, user_id, game_id, can_play, ranked_enabled)
      SELECT ?, id, 'master-duel', 0, 1 FROM game_users WHERE id = ? AND email = ? AND access_group = 'Ranked Simulation'`)
      .bind(`settings_${id}`, id, email));
  }
  await db.batch([...statements, auditStatement(db, actor, "Ranked-demoaccounts klaargezet", simulationSeasonId,
    "Zes testaccounts zonder wachtwoord; game-login uitgeschakeld. Bestaande accounts worden niet gewijzigd.")]);
  return (await accounts(db)).filter(isDemo).map((account) => account.id);
}

export async function rankedOverview(db: RankedDb, seasonId = simulationSeasonId, userId?: string, now = Date.now()) {
  checkSeason(seasonId);
  timestamp(now);
  const allAccounts = await accounts(db);
  const profileIds = (await db.prepare("SELECT user_id FROM ranked_profiles WHERE game_id = ? AND season_id = ?")
    .bind(rankedGameId, seasonId).all<{ user_id: string }>()).results;
  const profiles: (Profile & { display_name: string })[] = [];
  for (const { user_id: id } of profileIds) {
    const profile = await profileAfterDecay(db, id, seasonId, now);
    if (profile) profiles.push({ ...profile, display_name: allAccounts.find((account) => account.id === id)?.display_name ?? id });
  }
  profiles.sort((a, b) => b.ladder_step - a.ladder_step || a.user_id.localeCompare(b.user_id));
  const historySql = `${matchSelect} WHERE m.game_id = ? AND m.season_id = ? ${userId ? "AND (m.player_a_id = ? OR m.player_b_id = ?)" : ""}
    ORDER BY m.created_at DESC, m.rowid DESC LIMIT 100`;
  const historyStatement = db.prepare(historySql);
  const history = await (userId ? historyStatement.bind(rankedGameId, seasonId, userId, userId) : historyStatement.bind(rankedGameId, seasonId)).all<MatchRow>();
  let previousStep: number | undefined;
  let position = 0;
  const leaderboard = profiles.map((profile, index) => {
    if (profile.ladder_step !== previousStep) position = index + 1;
    previousStep = profile.ladder_step;
    return { ...snapshot(profile, seasonId), displayName: profile.display_name, position };
  });
  return {
    selectedSeasonId: seasonId, seasons: rankedSeasons, ranks: rankedRanks,
    accounts: allAccounts.map((account) => ({ id: account.id, displayName: account.display_name, email: account.email,
      eligible: eligible(account, seasonId), isDemo: isDemo(account),
      disabledReason: eligible(account, seasonId) ? undefined : "Account geblokkeerd, spelen uitgeschakeld of ranked niet toegestaan." })),
    leaderboard,
    history: history.results.map(matchFromRow),
  };
}

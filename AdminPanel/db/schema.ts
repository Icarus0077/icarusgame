import { sql } from "drizzle-orm";
import { check, index, integer, primaryKey, sqliteTable, text, uniqueIndex } from "drizzle-orm/sqlite-core";

export const adminPrincipals = sqliteTable("admin_principals", {
  id: text("id").primaryKey(),
  userId: text("user_id").notNull().unique(),
  email: text("email").notNull().unique(),
  displayName: text("display_name").notNull(),
  role: text("role").notNull().default("Viewer"),
  status: text("status").notNull().default("active"),
  createdAt: text("created_at").notNull().default(sql`CURRENT_TIMESTAMP`),
});

export const gameUsers = sqliteTable("game_users", {
  id: text("id").primaryKey(),
  displayName: text("display_name").notNull(),
  email: text("email").notNull().unique(),
  role: text("role").notNull().default("Player"),
  status: text("status").notNull().default("active"),
  accessGroup: text("access_group").notNull().default("Public"),
  lastSeen: text("last_seen").notNull().default(sql`CURRENT_TIMESTAMP`),
  createdAt: text("created_at").notNull().default(sql`CURRENT_TIMESTAMP`),
}, (table) => [index("idx_game_users_status").on(table.status)]);

export const gameUserSettings = sqliteTable("game_user_settings", {
  id: text("id").primaryKey(),
  userId: text("user_id").notNull().references(() => gameUsers.id, { onDelete: "cascade" }),
  gameId: text("game_id").notNull(),
  canPlay: integer("can_play", { mode: "boolean" }).notNull().default(true),
  matchmakingRegion: text("matchmaking_region").notNull().default("eu-west"),
  rankedEnabled: integer("ranked_enabled", { mode: "boolean" }).notNull().default(true),
  deckSlots: integer("deck_slots").notNull().default(20),
  settingsJson: text("settings_json").notNull().default("{}"),
  updatedAt: text("updated_at").notNull().default(sql`CURRENT_TIMESTAMP`),
}, (table) => [
  uniqueIndex("uq_game_user_settings_user_game").on(table.userId, table.gameId),
  index("idx_game_user_settings_user_game").on(table.userId, table.gameId),
]);

export const gameUserCredentials = sqliteTable("game_user_credentials", {
  id: text("id").primaryKey(),
  userId: text("user_id").notNull().unique().references(() => gameUsers.id, { onDelete: "cascade" }),
  salt: text("salt").notNull(),
  hash: text("hash").notNull(),
  iterations: integer("iterations").notNull().default(210000),
  updatedAt: text("updated_at").notNull().default(sql`CURRENT_TIMESTAMP`),
}, (table) => [index("idx_game_user_credentials_user").on(table.userId)]);

export const gamePasswordResets = sqliteTable("game_password_resets", {
  tokenHash: text("token_hash").primaryKey(),
  userId: text("user_id").notNull().references(() => gameUsers.id, { onDelete: "cascade" }),
  email: text("email").notNull(),
  credentialHash: text("credential_hash").notNull(),
  expiresAt: integer("expires_at").notNull(),
  createdAt: integer("created_at").notNull(),
  usedAt: integer("used_at"),
}, (table) => [index("idx_game_password_resets_user").on(table.userId)]);

export const passwordResetLimits = sqliteTable("password_reset_limits", {
  bucket: text("bucket").primaryKey(),
  attempts: integer("attempts").notNull(),
  expiresAt: integer("expires_at").notNull(),
});

export const ipRules = sqliteTable("ip_rules", {
  id: text("id").primaryKey(),
  cidr: text("cidr").notNull(),
  label: text("label").notNull(),
  scope: text("scope").notNull(),
  environment: text("environment").notNull(),
  status: text("status").notNull().default("active"),
  expiresAt: text("expires_at"),
  createdAt: text("created_at").notNull().default(sql`CURRENT_TIMESTAMP`),
}, (table) => [index("idx_ip_rules_status_environment").on(table.status, table.environment)]);

export const cardPolicies = sqliteTable("card_policies", {
  id: text("id").primaryKey(),
  cardId: text("card_id").notNull().unique(),
  cardName: text("card_name").notNull(),
  availability: text("availability").notNull().default("testers"),
  audience: text("audience").notNull().default("Internal QA"),
  regulation: text("regulation").notNull().default("Standard 2026.08"),
  effectiveFrom: text("effective_from").notNull().default(sql`CURRENT_TIMESTAMP`),
  effectiveTo: text("effective_to"),
  updatedAt: text("updated_at").notNull().default(sql`CURRENT_TIMESTAMP`),
}, (table) => [index("idx_card_policies_availability").on(table.availability)]);

export const releases = sqliteTable("releases", {
  id: text("id").primaryKey(),
  version: text("version").notNull(),
  kind: text("kind").notNull(),
  environment: text("environment").notNull(),
  status: text("status").notNull().default("draft"),
  notes: text("notes").notNull().default(""),
  createdAt: text("created_at").notNull().default(sql`CURRENT_TIMESTAMP`),
}, (table) => [index("idx_releases_status").on(table.status)]);

export const auditEvents = sqliteTable("audit_events", {
  id: text("id").primaryKey(),
  actor: text("actor").notNull(),
  action: text("action").notNull(),
  target: text("target").notNull(),
  detail: text("detail").notNull(),
  severity: text("severity").notNull().default("info"),
  createdAt: text("created_at").notNull().default(sql`CURRENT_TIMESTAMP`),
}, (table) => [index("idx_audit_events_created_at").on(table.createdAt)]);

export const systemMetrics = sqliteTable("system_metrics", {
  key: text("key").primaryKey(),
  value: integer("value").notNull(),
  updatedAt: text("updated_at").notNull().default(sql`CURRENT_TIMESTAMP`),
});

export const rankedProfiles = sqliteTable("ranked_profiles", {
  gameId: text("game_id").notNull(),
  seasonId: text("season_id").notNull(),
  userId: text("user_id").notNull().references(() => gameUsers.id, { onDelete: "cascade" }),
  rating: integer("rating").notNull().default(1000),
  games: integer("games").notNull().default(0),
  wins: integer("wins").notNull().default(0),
  losses: integer("losses").notNull().default(0),
  draws: integer("draws").notNull().default(0),
  progressionVersion: integer("progression_version").notNull().default(1),
  ladderStep: integer("ladder_step").notNull().default(0),
  levelWins: integer("level_wins").notNull().default(0),
  levelLosses: integer("level_losses").notNull().default(0),
  lastRankedAt: integer("last_ranked_at"),
  decayApplied: integer("decay_applied").notNull().default(0),
  revision: integer("revision").notNull().default(0),
  updatedAt: text("updated_at").notNull().default(sql`CURRENT_TIMESTAMP`),
}, (table) => [
  primaryKey({ columns: [table.gameId, table.seasonId, table.userId] }),
  check("ranked_profile_totals", sql`${table.games} = ${table.wins} + ${table.losses} + ${table.draws}`),
  check("ranked_profile_games_nonnegative", sql`${table.games} >= 0`),
]);

export const rankedMatches = sqliteTable("ranked_matches", {
  id: text("id").primaryKey(),
  gameId: text("game_id").notNull(),
  seasonId: text("season_id").notNull(),
  playerAId: text("player_a_id").notNull().references(() => gameUsers.id, { onDelete: "cascade" }),
  playerBId: text("player_b_id").notNull().references(() => gameUsers.id, { onDelete: "cascade" }),
  outcome: text("outcome").notNull(),
  ratingABefore: integer("rating_a_before").notNull(),
  ratingAAfter: integer("rating_a_after").notNull(),
  ratingBBefore: integer("rating_b_before").notNull(),
  ratingBAfter: integer("rating_b_after").notNull(),
  deltaA: integer("delta_a").notNull(),
  deltaB: integer("delta_b").notNull(),
  isSimulation: integer("is_simulation", { mode: "boolean" }).notNull(),
  batchId: text("batch_id"),
  rankABefore: text("rank_a_before"),
  rankAAfter: text("rank_a_after"),
  rankBBefore: text("rank_b_before"),
  rankBAfter: text("rank_b_after"),
  rewardA: integer("reward_a").notNull().default(0),
  rewardB: integer("reward_b").notNull().default(0),
  createdAt: text("created_at").notNull().default(sql`CURRENT_TIMESTAMP`),
}, (table) => [
  index("idx_ranked_matches_season").on(table.gameId, table.seasonId, table.createdAt),
  check("ranked_match_outcome", sql`${table.outcome} IN ('a','b','draw')`),
  check("ranked_match_distinct_players", sql`${table.playerAId} <> ${table.playerBId}`),
  check("ranked_match_conserved_rating", sql`${table.deltaA} + ${table.deltaB} = 0`),
]);

export const rankedSimulationBatches = sqliteTable("ranked_simulation_batches", {
  id: text("id").primaryKey(),
  requestJson: text("request_json").notNull(),
  reportJson: text("report_json").notNull(),
  createdAt: text("created_at").notNull().default(sql`CURRENT_TIMESTAMP`),
});

// One real wallet spans all real seasons; simulations use their own scope.
export const rankedWallets = sqliteTable("ranked_wallets", {
  gameId: text("game_id").notNull(),
  userId: text("user_id").notNull().references(() => gameUsers.id, { onDelete: "cascade" }),
  scope: text("scope").notNull(),
  diamonds: integer("diamonds").notNull().default(0),
  highestStep: integer("highest_step").notNull().default(0),
  revision: integer("revision").notNull().default(0),
}, (table) => [primaryKey({ columns: [table.gameId, table.userId, table.scope] })]);

// Every genuine promotion pays, including a re-promotion after demotion.
// The match/account key prevents duplicate delivery paying a second time.
export const rankedRewards = sqliteTable("ranked_rewards", {
  gameId: text("game_id").notNull(),
  userId: text("user_id").notNull().references(() => gameUsers.id, { onDelete: "cascade" }),
  scope: text("scope").notNull(),
  highestStep: integer("highest_step").notNull(),
  amount: integer("amount").notNull(),
  matchId: text("match_id").notNull().references(() => rankedMatches.id, { onDelete: "cascade" }),
  createdAt: text("created_at").notNull(),
}, (table) => [primaryKey({ columns: [table.gameId, table.userId, table.scope, table.matchId] })]);

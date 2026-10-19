import { env } from "cloudflare:workers";

const schemaStatements = [
  `CREATE TABLE IF NOT EXISTS admin_principals (
    id TEXT PRIMARY KEY,
    user_id TEXT NOT NULL UNIQUE,
    email TEXT NOT NULL UNIQUE,
    display_name TEXT NOT NULL,
    role TEXT NOT NULL DEFAULT 'Viewer',
    status TEXT NOT NULL DEFAULT 'active',
    created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
  )`,
  `CREATE TABLE IF NOT EXISTS game_users (
    id TEXT PRIMARY KEY,
    display_name TEXT NOT NULL,
    email TEXT NOT NULL UNIQUE,
    role TEXT NOT NULL DEFAULT 'Player',
    status TEXT NOT NULL DEFAULT 'active',
    access_group TEXT NOT NULL DEFAULT 'Public',
    last_seen TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
  )`,
  `CREATE TABLE IF NOT EXISTS game_user_settings (
    id TEXT PRIMARY KEY,
    user_id TEXT NOT NULL,
    game_id TEXT NOT NULL,
    can_play INTEGER NOT NULL DEFAULT 1,
    matchmaking_region TEXT NOT NULL DEFAULT 'eu-west',
    ranked_enabled INTEGER NOT NULL DEFAULT 1,
    deck_slots INTEGER NOT NULL DEFAULT 20,
    settings_json TEXT NOT NULL DEFAULT '{}',
    updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(user_id, game_id),
    FOREIGN KEY(user_id) REFERENCES game_users(id) ON DELETE CASCADE
  )`,
  `CREATE TABLE IF NOT EXISTS game_user_credentials (
    id TEXT PRIMARY KEY,
    user_id TEXT NOT NULL UNIQUE,
    salt TEXT NOT NULL,
    hash TEXT NOT NULL,
    iterations INTEGER NOT NULL DEFAULT 210000,
    updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY(user_id) REFERENCES game_users(id) ON DELETE CASCADE
  )`,
  `CREATE TABLE IF NOT EXISTS ip_rules (
    id TEXT PRIMARY KEY,
    cidr TEXT NOT NULL,
    label TEXT NOT NULL,
    scope TEXT NOT NULL,
    environment TEXT NOT NULL,
    status TEXT NOT NULL DEFAULT 'active',
    expires_at TEXT,
    created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
  )`,
  `CREATE TABLE IF NOT EXISTS card_policies (
    id TEXT PRIMARY KEY,
    card_id TEXT NOT NULL UNIQUE,
    card_name TEXT NOT NULL,
    availability TEXT NOT NULL DEFAULT 'testers',
    audience TEXT NOT NULL DEFAULT 'Internal QA',
    regulation TEXT NOT NULL DEFAULT 'Standard 2026.08',
    effective_from TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    effective_to TEXT,
    updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
  )`,
  `CREATE TABLE IF NOT EXISTS releases (
    id TEXT PRIMARY KEY,
    version TEXT NOT NULL,
    kind TEXT NOT NULL,
    environment TEXT NOT NULL,
    status TEXT NOT NULL DEFAULT 'draft',
    notes TEXT NOT NULL DEFAULT '',
    created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
  )`,
  `CREATE TABLE IF NOT EXISTS audit_events (
    id TEXT PRIMARY KEY,
    actor TEXT NOT NULL,
    action TEXT NOT NULL,
    target TEXT NOT NULL,
    detail TEXT NOT NULL,
    severity TEXT NOT NULL DEFAULT 'info',
    created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
  )`,
  `CREATE TABLE IF NOT EXISTS system_metrics (
    key TEXT PRIMARY KEY,
    value INTEGER NOT NULL,
    updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
  )`,
  "CREATE INDEX IF NOT EXISTS idx_game_users_status ON game_users(status)",
  "CREATE INDEX IF NOT EXISTS idx_game_user_settings_user_game ON game_user_settings(user_id, game_id)",
  "CREATE INDEX IF NOT EXISTS idx_game_user_credentials_user ON game_user_credentials(user_id)",
  "CREATE INDEX IF NOT EXISTS idx_ip_rules_status_environment ON ip_rules(status, environment)",
  "CREATE INDEX IF NOT EXISTS idx_card_policies_availability ON card_policies(availability)",
  "CREATE INDEX IF NOT EXISTS idx_releases_status ON releases(status)",
  "CREATE INDEX IF NOT EXISTS idx_audit_events_created_at ON audit_events(created_at)",
];

export function database() {
  if (!env.DB) throw new Error("De Icarus-beheerdatabase is niet beschikbaar.");
  return env.DB;
}

export async function ensureDatabase() {
  const db = database();
  await db.batch(schemaStatements.map((statement) => db.prepare(statement)));
  if (env.SEED_DEMO_DATA !== "false") await seedDatabase();
}

export async function writeAudit(actor: string, action: string, target: string, detail: string, severity = "info") {
  await database().prepare("INSERT INTO audit_events (id, actor, action, target, detail, severity) VALUES (?, ?, ?, ?, ?, ?)")
    .bind(crypto.randomUUID(), actor, action, target, detail, severity).run();
}

async function seedDatabase() {
  const db = database();
  const users = [
    ["usr_nova", "Nova Vermeer", "nova@icarus.test", "Tester", "active", "Closed Alpha", "2026-08-14T11:41:00Z"],
    ["usr_orbit", "Orion Bakker", "orion@icarus.test", "Player", "active", "Creator Program", "2026-08-14T11:28:00Z"],
    ["usr_luna", "Luna de Wit", "luna@icarus.test", "Tester", "active", "Internal QA", "2026-08-14T10:52:00Z"],
    ["usr_kael", "Kael Smit", "kael@icarus.test", "Player", "blocked", "Closed Alpha", "2026-08-12T18:20:00Z"],
    ["usr_mira", "Mira Vos", "mira@icarus.test", "Moderator", "active", "Internal QA", "2026-08-14T11:47:00Z"],
  ];
  const ips = [
    ["ip_ams", "203.0.113.24/32", "QA-kantoor Amsterdam", "Gameplay", "Production", "active", null],
    ["ip_vpn", "100.122.148.0/24", "Icarus private network", "Admin", "Production", "active", null],
    ["ip_home", "198.51.100.46/32", "Release manager", "Admin", "Staging", "active", "2026-09-01"],
    ["ip_old", "192.0.2.0/28", "Externe playtest", "Gameplay", "Staging", "disabled", "2026-08-20"],
  ];
  const cards = [
    ["card_blue_eyes", "89631139", "Blue-Eyes White Dragon", "public", "Everyone", "Standard 2026.08"],
    ["card_spirit", "89604813", "Blue-Eyes Ultimate Spirit Dragon", "testers", "Closed Alpha", "Standard 2026.08"],
    ["card_neo", "2129638", "Blue-Eyes Alternative White Dragon", "public", "Everyone", "Standard 2026.08"],
    ["card_abyss", "64202399", "Blue-Eyes Abyss Dragon", "testers", "Internal QA", "Standard 2026.08"],
    ["card_roar", "17725109", "Roar of the Blue-Eyed Dragons", "hidden", "Nobody", "Preview 2026.09"],
    ["card_wishes", "80326401", "Wishes for Eyes of Blue", "testers", "Creators", "Preview 2026.09"],
  ];
  const releaseRows = [
    ["rel_184", "v1.8.4", "Server", "Production", "live", "Stabiele serverbuild met ruleset 1.8.4."],
    ["rel_content", "content-2026.08.14", "Content", "Staging", "staging", "Nieuwe Blue-Eyes policies en catalogusmanifest."],
    ["rel_190", "v1.9.0-rc2", "Client", "Production", "draft", "Admin API-koppeling en verplichte versiecontrole."],
    ["rel_rules", "rules-1.9.0", "Rules", "Production", "draft", "Nieuwe regulation voor gesloten playtest."],
  ];
  const audits = [
    ["audit_1", "system@icarus", "Release gepubliceerd", "v1.8.4", "Serverversie v1.8.4 is naar productie gepromoveerd.", "info", "2026-08-14T10:14:00Z"],
    ["audit_2", "mira@icarus.test", "Kaartpolicy gewijzigd", "17725109", "Roar of the Blue-Eyed Dragons is verborgen voor publieke accounts.", "warning", "2026-08-14T09:47:00Z"],
    ["audit_3", "system@icarus", "Toegang geweigerd", "198.51.100.201", "Herhaalde loginpogingen vanaf een niet-toegestaan adres.", "critical", "2026-08-14T08:32:00Z"],
    ["audit_4", "nova@icarus.test", "Whitelistregel toegevoegd", "203.0.113.24/32", "Tijdelijke QA-toegang voor de productieomgeving.", "info", "2026-08-13T16:21:00Z"],
  ];

  await db.batch([
    ...users.map((row) => db.prepare("INSERT OR IGNORE INTO game_users (id, display_name, email, role, status, access_group, last_seen) VALUES (?, ?, ?, ?, ?, ?, ?)").bind(...row)),
    ...users.map((row) => db.prepare("INSERT OR IGNORE INTO game_user_settings (id, user_id, game_id, can_play, matchmaking_region, ranked_enabled, deck_slots) VALUES (?, ?, 'master-duel', 1, 'eu-west', 1, 20)").bind(`settings_${row[0]}`, row[0])),
    ...ips.map((row) => db.prepare("INSERT OR IGNORE INTO ip_rules (id, cidr, label, scope, environment, status, expires_at) VALUES (?, ?, ?, ?, ?, ?, ?)").bind(...row)),
    ...cards.map((row) => db.prepare("INSERT OR IGNORE INTO card_policies (id, card_id, card_name, availability, audience, regulation) VALUES (?, ?, ?, ?, ?, ?)").bind(...row)),
    ...releaseRows.map((row) => db.prepare("INSERT OR IGNORE INTO releases (id, version, kind, environment, status, notes) VALUES (?, ?, ?, ?, ?, ?)").bind(...row)),
    ...audits.map((row) => db.prepare("INSERT OR IGNORE INTO audit_events (id, actor, action, target, detail, severity, created_at) VALUES (?, ?, ?, ?, ?, ?, ?)").bind(...row)),
    db.prepare("INSERT OR IGNORE INTO system_metrics (key, value) VALUES ('online_players', 1842)"),
    db.prepare("INSERT OR IGNORE INTO system_metrics (key, value) VALUES ('live_duels', 629)"),
    db.prepare("INSERT OR IGNORE INTO system_metrics (key, value) VALUES ('catalog_cards', 13820)"),
    db.prepare("INSERT OR IGNORE INTO system_metrics (key, value) VALUES ('pending_changes', 7)"),
  ]);
}

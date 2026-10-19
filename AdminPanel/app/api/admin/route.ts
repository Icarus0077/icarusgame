import { env } from "cloudflare:workers";
import { getChatGPTUser } from "../../chatgpt-auth";
import { accessCode, createCredential } from "../_shared/credentials";
import { ensureResetSchema, playerCanChoosePassword, requestPasswordReset } from "../_shared/password-reset";
import { createResetSender } from "../_shared/reset-mail";
import { resetClientIp } from "../_shared/reset-http";

export const dynamic = "force-dynamic";

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

type Actor = { id: string; name: string; email: string; role: string };

function database() {
  if (!env.DB) throw new Error("De Icarus-beheerdatabase is niet beschikbaar.");
  return env.DB;
}

async function ensureDatabase() {
  const db = database();
  await db.batch(schemaStatements.map((statement) => db.prepare(statement)));
  if (env.SEED_DEMO_DATA !== "false") await seedDatabase();
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

export async function authorize(request: Request): Promise<Actor> {
  const db = database();
  const user = await getChatGPTUser();
  const hostname = new URL(request.url).hostname;

  if (!user && (hostname === "localhost" || hostname === "127.0.0.1")) {
    return { id: "local-preview", name: "Lokale beheerder", email: "preview@project-icarus.local", role: "Super Admin" };
  }
  if (!user) throw new Response("Aanmelden vereist", { status: 401 });

  const count = await db.prepare("SELECT COUNT(*) AS count FROM admin_principals").first<{ count: number }>();
  if (!count?.count) {
    await db.prepare("INSERT INTO admin_principals (id, user_id, email, display_name, role) VALUES (?, ?, ?, ?, 'Super Admin')")
      .bind(crypto.randomUUID(), user.userId, user.email, user.displayName).run();
  }

  const principal = await db.prepare("SELECT id, display_name, email, role, status FROM admin_principals WHERE user_id = ? OR email = ? LIMIT 1")
    .bind(user.userId, user.email).first<{ id: string; display_name: string; email: string; role: string; status: string }>();
  if (!principal || principal.status !== "active") throw new Response("Geen beheerrechten", { status: 403 });
  return { id: principal.id, name: principal.display_name, email: principal.email, role: principal.role };
}

export async function GET(request: Request) {
  try {
    await ensureDatabase();
    const actor = await authorize(request);
    const db = database();
    const [users, gameSettings, ipRules, cardPolicies, releases, auditEvents, metrics] = await Promise.all([
      db.prepare("SELECT * FROM game_users ORDER BY status ASC, display_name ASC").all(),
      db.prepare("SELECT * FROM game_user_settings ORDER BY game_id ASC, updated_at DESC").all(),
      db.prepare("SELECT * FROM ip_rules ORDER BY status ASC, created_at DESC").all(),
      db.prepare("SELECT * FROM card_policies ORDER BY updated_at DESC, card_name ASC").all(),
      db.prepare("SELECT * FROM releases ORDER BY created_at DESC").all(),
      db.prepare("SELECT * FROM audit_events ORDER BY created_at DESC LIMIT 50").all(),
      db.prepare("SELECT key, value FROM system_metrics").all<{ key: string; value: number }>(),
    ]);
    const metricMap = Object.fromEntries(metrics.results.map((row) => [row.key, row.value]));
    return Response.json({
      actor: { name: actor.name, email: actor.email, role: actor.role },
      users: users.results,
      gameSettings: gameSettings.results,
      ipRules: ipRules.results,
      cardPolicies: cardPolicies.results,
      releases: releases.results,
      auditEvents: auditEvents.results,
      metrics: {
        onlinePlayers: metricMap.online_players ?? 0,
        liveDuels: metricMap.live_duels ?? 0,
        catalogCards: metricMap.catalog_cards ?? 0,
        pendingChanges: metricMap.pending_changes ?? 0,
      },
    });
  } catch (error) {
    return routeError(error);
  }
}

export async function POST(request: Request) {
  try {
    await ensureDatabase();
    const actor = await authorize(request);
    const payload = await request.json() as Record<string, unknown>;
    const action = text(payload.action, "actie");
    const db = database();

    switch (action) {
      case "create-user": {
        const displayName = text(payload.displayName, "naam");
        const email = text(payload.email, "e-mailadres").toLowerCase();
        if (!/^[^\s<>@]+@[^\s<>@]+\.[^\s<>@]+$/.test(email)) throw new Error("Vul een geldig e-mailadres in.");
        const role = enumValue(payload.role, ["Player", "Tester", "Moderator"]);
        const accessGroup = text(payload.accessGroup, "toegangsgroep");
        const gameId = gameIdentifier(payload.gameId);
        const matchmakingRegion = enumValue(payload.matchmakingRegion ?? "eu-west", ["eu-west", "us-east", "asia-pacific"]);
        const deckSlots = boundedInteger(payload.deckSlots ?? "20", 1, 200, "deckslots");
        const sender = role === "Moderator" ? null : createResetSender(env);
        const credential = role === "Moderator" ? await createCredential(accessCode(payload.accessCode)) : null;
        const userId = crypto.randomUUID();
        await db.batch([
          db.prepare("INSERT INTO game_users (id, display_name, email, role, access_group) VALUES (?, ?, ?, ?, ?)")
            .bind(userId, displayName, email, role, accessGroup),
          db.prepare("INSERT INTO game_user_settings (id, user_id, game_id, can_play, matchmaking_region, ranked_enabled, deck_slots) VALUES (?, ?, ?, 1, ?, 1, ?)")
            .bind(crypto.randomUUID(), userId, gameId, matchmakingRegion, deckSlots),
          ...(credential ? [db.prepare("INSERT INTO game_user_credentials (id, user_id, salt, hash, iterations) VALUES (?, ?, ?, ?, ?)")
            .bind(crypto.randomUUID(), userId, credential.salt, credential.hash, credential.iterations)] : []),
        ]);
        await audit(actor, "Gebruiker toegevoegd", email, `${displayName} kreeg toegang via ${accessGroup}.`, "info");
        if (sender) {
          try {
            await ensureResetSchema(db);
            const outcome = await requestPasswordReset(db, email, resetClientIp(request), sender, "https://gameserver.rene007.nl", Date.now(), true);
            if (outcome !== "sent") throw new Error("De instelmail is nog niet verstuurd.");
            await audit(actor, "Wachtwoordlink verstuurd", userId, "Speler kiest zelf een blijvend wachtwoord via een eenmalige e-maillink.", "info");
            return Response.json({ ok: true, message: "Speler toegevoegd. De speler ontvangt een e-maillink om zelf een blijvend wachtwoord te kiezen." }, { status: 201 });
          } catch {
            await audit(actor, "Wachtwoordlink niet verstuurd", userId, "Account aangemaakt zonder inlogwachtwoord; verstuur de instelmail opnieuw na controle van de mailverbinding.", "warning");
            return Response.json({ ok: true, warning: "Speler toegevoegd, maar de instelmail kon niet worden verstuurd. Gebruik Wachtwoordlink versturen om het opnieuw te proberen." }, { status: 201 });
          }
        }
        break;
      }
      case "send-user-password-link": {
        const userId = text(payload.userId, "gebruikers-ID");
        if (!await playerCanChoosePassword(db, userId)) throw new Error("Deze gebruiker heeft geen actief spelersaccount voor e-mailherstel.");
        const user = await db.prepare("SELECT email FROM game_users WHERE id = ?").bind(userId).first<{ email: string }>();
        if (!user) throw new Error("Gebruiker niet gevonden.");
        const sender = createResetSender(env);
        await ensureResetSchema(db);
        const outcome = await requestPasswordReset(db, user.email, resetClientIp(request), sender, "https://gameserver.rene007.nl", Date.now(), true);
        if (outcome === "rate_limited") return Response.json({ error: "Er zijn recent meerdere links aangevraagd. Probeer het over een uur opnieuw." }, { status: 429 });
        if (outcome !== "sent") throw new Error("De gebruiker kan momenteel geen wachtwoordlink ontvangen.");
        await audit(actor, "Wachtwoordlink verstuurd", userId, "Instellen of herstellen van een zelfgekozen spelerswachtwoord via e-mail.", "info");
        return Response.json({ ok: true, message: "De wachtwoordlink is verstuurd naar het geregistreerde spelersadres." });
      }
      case "reset-user-password": {
        const userId = text(payload.userId, "gebruikers-ID");
        if (await playerCanChoosePassword(db, userId)) throw new Error("Spelers kiezen zelf hun wachtwoord. Gebruik Wachtwoordlink versturen.");
        const nextPassword = accessCode(payload.accessCode);
        const confirmation = accessCode(payload.accessCodeConfirmation);
        if (nextPassword !== confirmation) throw new Error("De wachtwoorden komen niet overeen.");

        const user = await db.prepare("SELECT display_name, email FROM game_users WHERE id = ?")
          .bind(userId)
          .first<{ display_name: string; email: string }>();
        if (!user) throw new Error("Gebruiker niet gevonden.");

        const credential = await createCredential(nextPassword);
        await db.prepare(`INSERT INTO game_user_credentials (id, user_id, salt, hash, iterations)
          VALUES (?, ?, ?, ?, ?)
          ON CONFLICT(user_id) DO UPDATE SET
            salt = excluded.salt,
            hash = excluded.hash,
            iterations = excluded.iterations,
            updated_at = CURRENT_TIMESTAMP`)
          .bind(crypto.randomUUID(), userId, credential.salt, credential.hash, credential.iterations)
          .run();
        await audit(
          actor,
          "Wachtwoord opnieuw ingesteld",
          user.email,
          `Er is een nieuw gamewachtwoord ingesteld voor ${user.display_name}; het oude wachtwoord blijft onleesbaar.`,
          "warning",
        );
        break;
      }
      case "toggle-user": {
        const id = text(payload.id, "gebruikers-ID");
        const status = enumValue(payload.status, ["active", "blocked"]);
        await db.prepare("UPDATE game_users SET status = ? WHERE id = ?").bind(status, id).run();
        await audit(actor, status === "blocked" ? "Gebruiker geblokkeerd" : "Gebruiker geactiveerd", id, `Accountstatus gewijzigd naar ${status}.`, status === "blocked" ? "warning" : "info");
        break;
      }
      case "update-game-settings": {
        const userId = text(payload.userId, "gebruikers-ID");
        const gameId = gameIdentifier(payload.gameId);
        const canPlay = booleanValue(payload.canPlay);
        const matchmakingRegion = enumValue(payload.matchmakingRegion, ["eu-west", "us-east", "asia-pacific"]);
        const rankedEnabled = booleanValue(payload.rankedEnabled);
        const deckSlots = boundedInteger(payload.deckSlots, 1, 200, "deckslots");
        if (typeof payload.accessCode === "string" && payload.accessCode.trim()) throw new Error("Wijzig het spelerswachtwoord via een e-maillink, los van de game-instellingen.");
        const statements = [db.prepare(`INSERT INTO game_user_settings
          (id, user_id, game_id, can_play, matchmaking_region, ranked_enabled, deck_slots)
          VALUES (?, ?, ?, ?, ?, ?, ?)
          ON CONFLICT(user_id, game_id) DO UPDATE SET
            can_play = excluded.can_play,
            matchmaking_region = excluded.matchmaking_region,
            ranked_enabled = excluded.ranked_enabled,
            deck_slots = excluded.deck_slots,
            updated_at = CURRENT_TIMESTAMP`)
          .bind(crypto.randomUUID(), userId, gameId, canPlay ? 1 : 0, matchmakingRegion, rankedEnabled ? 1 : 0, deckSlots)];
        await db.batch(statements);
        await audit(actor, "Game-instellingen gewijzigd", `${userId}:${gameId}`, `Spelen ${canPlay ? "toegestaan" : "geweigerd"}; regio ${matchmakingRegion}; ${deckSlots} deckslots.`, canPlay ? "info" : "warning");
        break;
      }
      case "create-ip-rule": {
        const cidr = text(payload.cidr, "IP-adres");
        if (!validNetwork(cidr)) throw new Error("Gebruik een geldig IPv4-, IPv6- of CIDR-adres.");
        const label = text(payload.label, "label");
        const scope = enumValue(payload.scope, ["Gameplay", "Admin", "API"]);
        const environment = enumValue(payload.environment, ["Production", "Staging", "Development"]);
        const expiresAt = optionalText(payload.expiresAt);
        await db.prepare("INSERT INTO ip_rules (id, cidr, label, scope, environment, expires_at) VALUES (?, ?, ?, ?, ?, ?)")
          .bind(crypto.randomUUID(), cidr, label, scope, environment, expiresAt).run();
        await audit(actor, "Whitelistregel toegevoegd", cidr, `${label} toegestaan voor ${scope} in ${environment}.`, "info");
        break;
      }
      case "toggle-ip-rule": {
        const id = text(payload.id, "regel-ID");
        const status = enumValue(payload.status, ["active", "disabled"]);
        await db.prepare("UPDATE ip_rules SET status = ? WHERE id = ?").bind(status, id).run();
        await audit(actor, "Whitelistregel gewijzigd", id, `Netwerkregel staat nu op ${status}.`, status === "disabled" ? "warning" : "info");
        break;
      }
      case "create-card-policy": {
        const cardId = text(payload.cardId, "kaart-ID");
        if (!/^\d{1,12}$/.test(cardId)) throw new Error("Het kaart-ID mag alleen cijfers bevatten.");
        const cardName = text(payload.cardName, "kaartnaam");
        const availability = enumValue(payload.availability, ["public", "testers", "hidden", "disabled"]);
        const audience = enumValue(payload.audience, ["Everyone", "Closed Alpha", "Internal QA", "Creators", "Nobody"]);
        await db.prepare("INSERT INTO card_policies (id, card_id, card_name, availability, audience) VALUES (?, ?, ?, ?, ?)")
          .bind(crypto.randomUUID(), cardId, cardName, availability, audience).run();
        await audit(actor, "Kaartpolicy toegevoegd", cardId, `${cardName} is ${availability} voor ${audience}.`, availability === "public" ? "info" : "warning");
        break;
      }
      case "update-card-policy": {
        const id = text(payload.id, "policy-ID");
        const availability = enumValue(payload.availability, ["public", "testers", "hidden", "disabled"]);
        const audience = enumValue(payload.audience, ["Everyone", "Closed Alpha", "Internal QA", "Creators", "Nobody"]);
        await db.prepare("UPDATE card_policies SET availability = ?, audience = ?, updated_at = CURRENT_TIMESTAMP WHERE id = ?")
          .bind(availability, audience, id).run();
        await audit(actor, "Kaartpolicy gewijzigd", id, `Beschikbaarheid ${availability}; doelgroep ${audience}.`, availability === "public" ? "info" : "warning");
        break;
      }
      case "create-release": {
        const version = text(payload.version, "versie");
        const kind = enumValue(payload.kind, ["Client", "Server", "Content", "Rules"]);
        const environment = enumValue(payload.environment, ["Production", "Staging"]);
        const notes = text(payload.notes, "release-opmerking");
        await db.prepare("INSERT INTO releases (id, version, kind, environment, notes) VALUES (?, ?, ?, ?, ?)")
          .bind(crypto.randomUUID(), version, kind, environment, notes).run();
        await audit(actor, "Releaseconcept gemaakt", version, `${kind}-release voorbereid voor ${environment}.`, "info");
        break;
      }
      case "promote-release": {
        const id = text(payload.id, "release-ID");
        const currentStatus = enumValue(payload.currentStatus, ["draft", "staging"]);
        const nextStatus = currentStatus === "draft" ? "staging" : "live";
        const release = await db.prepare("SELECT version, environment FROM releases WHERE id = ?").bind(id).first<{ version: string; environment: string }>();
        if (!release) throw new Error("Release niet gevonden.");
        if (nextStatus === "live") {
          await db.batch([
            db.prepare("UPDATE releases SET status = 'archived' WHERE environment = ? AND status = 'live'").bind(release.environment),
            db.prepare("UPDATE releases SET status = 'live' WHERE id = ?").bind(id),
          ]);
        } else {
          await db.prepare("UPDATE releases SET status = 'staging' WHERE id = ?").bind(id).run();
        }
        await audit(actor, nextStatus === "live" ? "Release gepubliceerd" : "Release naar staging", release.version, `Releasestatus gewijzigd naar ${nextStatus}.`, nextStatus === "live" ? "warning" : "info");
        break;
      }
      default:
        throw new Error("Onbekende beheeractie.");
    }

    return Response.json({ ok: true });
  } catch (error) {
    return routeError(error);
  }
}

async function audit(actor: Actor, action: string, target: string, detail: string, severity: string) {
  await database().prepare("INSERT INTO audit_events (id, actor, action, target, detail, severity) VALUES (?, ?, ?, ?, ?, ?)")
    .bind(crypto.randomUUID(), actor.email, action, target, detail, severity).run();
}

function text(value: unknown, label: string) {
  if (typeof value !== "string" || !value.trim()) throw new Error(`${label} is verplicht.`);
  return value.trim().slice(0, 300);
}

function optionalText(value: unknown) {
  return typeof value === "string" && value.trim() ? value.trim().slice(0, 100) : null;
}

function gameIdentifier(value: unknown) {
  const gameId = text(value ?? "master-duel", "game-ID").toLowerCase();
  if (!/^[a-z0-9][a-z0-9-]{1,49}$/.test(gameId)) throw new Error("Gebruik een geldig game-ID met letters, cijfers en koppeltekens.");
  return gameId;
}

function boundedInteger(value: unknown, minimum: number, maximum: number, label: string) {
  const number = typeof value === "number" ? value : Number(value);
  if (!Number.isInteger(number) || number < minimum || number > maximum) throw new Error(`${label} moet tussen ${minimum} en ${maximum} liggen.`);
  return number;
}

function booleanValue(value: unknown) {
  if (value === true || value === 1 || value === "1" || value === "true" || value === "on") return true;
  if (value === false || value === 0 || value === "0" || value === "false" || value === "off") return false;
  throw new Error("Ongeldige ja/nee-waarde ontvangen.");
}

function enumValue<T extends string>(value: unknown, allowed: readonly T[]): T {
  if (typeof value !== "string" || !allowed.includes(value as T)) throw new Error("Ongeldige waarde ontvangen.");
  return value as T;
}

function validNetwork(value: string) {
  if (value.includes(":")) return /^[0-9a-fA-F:]+(?:\/\d{1,3})?$/.test(value);
  const match = value.match(/^(\d{1,3})(?:\.(\d{1,3})){3}(?:\/(\d{1,2}))?$/);
  if (!match) return false;
  const [address, prefix] = value.split("/");
  return address.split(".").every((part) => Number(part) <= 255) && (!prefix || Number(prefix) <= 32);
}

function routeError(error: unknown) {
  if (error instanceof Response) return error;
  const message = error instanceof Error ? error.message : "Onverwachte serverfout";
  const status = /UNIQUE constraint/.test(message) ? 409 : /verplicht|Ongeldige|geldig|alleen cijfers|moet tussen|toegangscode|wachtwoord|gevonden/.test(message) ? 400 : 500;
  return Response.json({ error: status === 409 ? "Deze waarde bestaat al." : message }, { status });
}

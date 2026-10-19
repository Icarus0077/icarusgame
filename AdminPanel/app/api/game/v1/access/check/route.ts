import { env } from "cloudflare:workers";
import { cardPolicyAllows, ipInCidr, safeSettings, tokenMatches, type CardPolicyRow } from "../../../../_shared/access";
import { verifyCredential } from "../../../../_shared/credentials";
import { database, ensureDatabase, writeAudit } from "../../../../_shared/database";
import { ensureRankedSchema, getRankedSnapshot, rankedGameId } from "../../../../_shared/ranked";

export const dynamic = "force-dynamic";

type AccessRequest = {
  user?: string;
  password?: string;
  accessCode?: string;
  gameId?: string;
  environment?: string;
  playerIp?: string;
  clientVersion?: string;
};

type AccessRow = {
  id: string;
  display_name: string;
  email: string;
  role: string;
  status: string;
  access_group: string;
  can_play: number | null;
  matchmaking_region: string | null;
  ranked_enabled: number | null;
  deck_slots: number | null;
  settings_json: string | null;
  credential_salt: string | null;
  credential_hash: string | null;
  credential_iterations: number | null;
};

type IpRuleRow = { cidr: string; label: string };
type ReleaseRow = { version: string; kind: string };

const allowedOrigin = "https://gameserver.rene007.nl";

export async function OPTIONS() {
  return new Response(null, { status: 204, headers: apiHeaders() });
}

export async function POST(request: Request) {
  const requestId = request.headers.get("x-request-id")?.slice(0, 100) || crypto.randomUUID();
  try {
    await ensureDatabase();
    const payload = await readPayload(request);
    const userReference = requiredText(payload.user, "user").toLowerCase();
    const gameId = gameIdentifier(payload.gameId ?? "master-duel");
    const environment = environmentValue(payload.environment ?? "Production");
    const playerIp = optionalText(payload.playerIp) ?? forwardedClientIp(request);
    const clientVersion = optionalText(payload.clientVersion);
    const suppliedAccessCode = optionalText(payload.password ?? payload.accessCode);
    const trustedService = serviceAuthorized(request);
    const db = database();

    const user = await db.prepare(`SELECT
      u.id, u.display_name, u.email, u.role, u.status, u.access_group,
      s.can_play, s.matchmaking_region, s.ranked_enabled, s.deck_slots, s.settings_json,
      c.salt AS credential_salt, c.hash AS credential_hash, c.iterations AS credential_iterations
      FROM game_users u
      LEFT JOIN game_user_settings s ON s.user_id = u.id AND s.game_id = ?
      LEFT JOIN game_user_credentials c ON c.user_id = u.id
      WHERE lower(u.id) = ? OR lower(u.email) = ?
      LIMIT 1`)
      .bind(gameId, userReference, userReference)
      .first<AccessRow>();

    if (!trustedService) {
      const credentialValid = Boolean(
        user && suppliedAccessCode && user.credential_salt && user.credential_hash && user.credential_iterations &&
        await verifyCredential(suppliedAccessCode, {
          salt: user.credential_salt,
          hash: user.credential_hash,
          iterations: user.credential_iterations,
        }),
      );
      if (!credentialValid) return authenticationFailed(requestId);
    }

    if (!user) return decisionResponse(requestId, false, "USER_NOT_FOUND", "De gebruiker bestaat niet.", { gameId, environment });
    if (user.status !== "active") return denied(requestId, user, gameId, environment, "USER_BLOCKED", "Het gebruikersaccount is geblokkeerd.");
    if (user.can_play !== 1) return denied(requestId, user, gameId, environment, "GAME_NOT_ALLOWED", "De gebruiker heeft geen toegang tot deze game.");

    const ipRules = await db.prepare(`SELECT cidr, label FROM ip_rules
      WHERE status = 'active' AND scope = 'Gameplay' AND environment = ?
      AND (expires_at IS NULL OR date(expires_at) >= date('now'))`)
      .bind(environment).all<IpRuleRow>();
    const whitelistEnforced = ipRules.results.length > 0;
    const matchedRule = playerIp ? ipRules.results.find((rule) => ipInCidr(playerIp, rule.cidr)) : undefined;
    if (whitelistEnforced && !playerIp) return denied(requestId, user, gameId, environment, "PLAYER_IP_REQUIRED", "Een speler-IP is verplicht voor deze omgeving.");
    if (whitelistEnforced && !matchedRule) return denied(requestId, user, gameId, environment, "IP_NOT_ALLOWED", "Het speler-IP staat niet op de gameplay-whitelist.");

    const [policies, releases] = await Promise.all([
      db.prepare(`SELECT card_id, card_name, availability, audience, regulation, updated_at
        FROM card_policies
        WHERE datetime(effective_from) <= CURRENT_TIMESTAMP
        AND (effective_to IS NULL OR datetime(effective_to) >= CURRENT_TIMESTAMP)
        ORDER BY card_id ASC`).all<CardPolicyRow>(),
      db.prepare("SELECT version, kind FROM releases WHERE environment = ? AND status = 'live' ORDER BY created_at DESC")
        .bind(environment).all<ReleaseRow>(),
    ]);
    const releaseMap = Object.fromEntries(releases.results.map((release) => [release.kind.toLowerCase(), release.version]));
    const cardOverrides = policies.results.map((policy) => ({
      cardId: policy.card_id,
      cardName: policy.card_name,
      allowed: cardPolicyAllows(user.access_group, policy),
      availability: policy.availability,
      audience: policy.audience,
      regulation: policy.regulation,
    }));

    await db.prepare("UPDATE game_users SET last_seen = CURRENT_TIMESTAMP WHERE id = ?").bind(user.id).run();
    // Only the authenticated account's real season is exposed to the client.
    // Simulation profiles are never used as a player's live ranked standing.
    let ranked = null;
    if (gameId === rankedGameId && user.ranked_enabled === 1) {
      await ensureRankedSchema(db);
      ranked = await getRankedSnapshot(db, user.id);
    }
    return json({
      requestId,
      decision: { allowed: true, code: "ALLOWED", reason: "Toegang verleend." },
      user: { id: user.id, displayName: user.display_name, email: user.email, role: user.role, accessGroup: user.access_group },
      ranked,
      game: { id: gameId, environment, clientVersion },
      settings: {
        matchmakingRegion: user.matchmaking_region,
        rankedEnabled: user.ranked_enabled === 1,
        deckSlots: user.deck_slots,
        contentAudience: user.access_group,
        defaultCardAccess: "public",
        cardOverrides,
        releases: releaseMap,
        custom: safeSettings(user.settings_json ?? "{}"),
      },
      policy: {
        evaluatedAt: new Date().toISOString(),
        ipWhitelistEnforced: whitelistEnforced,
        matchedNetwork: matchedRule ? { cidr: matchedRule.cidr, label: matchedRule.label } : null,
        cardPolicyVersion: latestPolicyVersion(policies.results),
      },
    }, 200, requestId);
  } catch (error) {
    if (error instanceof Response) return withApiHeaders(error, requestId);
    const message = error instanceof Error ? error.message : "Onverwachte serverfout";
    const status = /verplicht|geldig|ongeldig/i.test(message) ? 400 : 500;
    return json({ requestId, error: status === 500 ? "Interne serverfout" : message }, status, requestId);
  }
}

async function denied(requestId: string, user: AccessRow, gameId: string, environment: string, code: string, reason: string) {
  await writeAudit("game-api", "Game-toegang geweigerd", `${user.id}:${gameId}`, `${code}: ${reason}`, "warning");
  return decisionResponse(requestId, false, code, reason, { gameId, environment });
}

function decisionResponse(requestId: string, allowed: boolean, code: string, reason: string, game: { gameId: string; environment: string }) {
  return json({
    requestId,
    decision: { allowed, code, reason },
    game: { id: game.gameId, environment: game.environment },
    settings: null,
    policy: { evaluatedAt: new Date().toISOString() },
  }, 200, requestId);
}

function serviceAuthorized(request: Request) {
  const configuredToken = env.GAME_API_TOKEN;
  if (!configuredToken) return false;
  const authorization = request.headers.get("authorization") ?? "";
  const suppliedToken = authorization.startsWith("Bearer ") ? authorization.slice(7).trim() : "";
  return tokenMatches(suppliedToken, configuredToken);
}

function authenticationFailed(requestId: string) {
  return json({ requestId, error: "Ongeldige gebruikersgegevens." }, 401, requestId);
}

async function readPayload(request: Request) {
  try {
    return await request.json() as AccessRequest;
  } catch {
    throw new Error("Gebruik een geldige JSON-body.");
  }
}

function requiredText(value: unknown, label: string) {
  if (typeof value !== "string" || !value.trim()) throw new Error(`${label} is verplicht.`);
  return value.trim().slice(0, 300);
}

function optionalText(value: unknown) {
  return typeof value === "string" && value.trim() ? value.trim().slice(0, 300) : null;
}

function forwardedClientIp(request: Request) {
  const forwarded = request.headers.get("x-forwarded-for");
  if (forwarded) {
    const addresses = forwarded.split(",").map((address) => address.trim()).filter(Boolean);
    if (addresses.length > 0) return addresses[addresses.length - 1].slice(0, 100);
  }
  return request.headers.get("x-real-ip")?.trim().slice(0, 100) || null;
}

function gameIdentifier(value: unknown) {
  const gameId = requiredText(value, "gameId").toLowerCase();
  if (!/^[a-z0-9][a-z0-9-]{1,49}$/.test(gameId)) throw new Error("gameId is ongeldig.");
  return gameId;
}

function environmentValue(value: unknown) {
  const environment = requiredText(value, "environment");
  if (!["Production", "Staging", "Development"].includes(environment)) throw new Error("environment is ongeldig.");
  return environment;
}

function latestPolicyVersion(policies: CardPolicyRow[]) {
  return policies.reduce((latest, policy) => policy.updated_at > latest ? policy.updated_at : latest, "none");
}

function apiHeaders() {
  return {
    "access-control-allow-origin": allowedOrigin,
    "access-control-allow-headers": "authorization, content-type, x-request-id",
    "access-control-allow-methods": "POST, OPTIONS",
    "cache-control": "no-store",
    "content-type": "application/json; charset=utf-8",
    "x-content-type-options": "nosniff",
    "x-icarus-api-version": "1",
  };
}

function json(payload: unknown, status: number, requestId: string) {
  return Response.json(payload, { status, headers: { ...apiHeaders(), "x-request-id": requestId } });
}

function withApiHeaders(response: Response, requestId: string) {
  const headers = new Headers(response.headers);
  Object.entries(apiHeaders()).forEach(([key, value]) => headers.set(key, value));
  headers.set("x-request-id", requestId);
  return new Response(response.body, { status: response.status, statusText: response.statusText, headers });
}

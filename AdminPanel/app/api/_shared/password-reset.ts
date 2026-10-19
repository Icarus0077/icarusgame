import { accessCode, createCredential } from "./credentials";

// Only ordinary game accounts may use public recovery. Membership of the
// admin console takes precedence, even when the game role is still Player.
const eligibleAccount = `u.status = 'active'
  AND lower(u.role) IN ('player', 'tester')
  AND NOT EXISTS (SELECT 1 FROM admin_principals a
    WHERE lower(a.user_id) = lower(u.id) OR lower(a.email) = lower(u.email))`;

export const resetSchema = [
  `CREATE TABLE IF NOT EXISTS game_password_resets (
    token_hash TEXT PRIMARY KEY,
    user_id TEXT NOT NULL REFERENCES game_users(id) ON DELETE CASCADE,
    email TEXT NOT NULL,
    credential_hash TEXT NOT NULL,
    expires_at INTEGER NOT NULL,
    created_at INTEGER NOT NULL,
    used_at INTEGER
  )`,
  `CREATE INDEX IF NOT EXISTS idx_game_password_resets_user ON game_password_resets(user_id)`,
  `CREATE TABLE IF NOT EXISTS password_reset_limits (
    bucket TEXT PRIMARY KEY,
    attempts INTEGER NOT NULL,
    expires_at INTEGER NOT NULL
  )`,
];

export const resetRequestMessage = "Als dit e-mailadres bij een account met wachtwoordherstel hoort, ontvang je een resetlink. Controleer ook je spammap.";
export const invalidResetMessage = "Deze resetlink is ongeldig of verlopen. Vraag een nieuwe link aan.";
const lifetime = 15 * 60 * 1000;
const rateWindow = 60 * 60 * 1000;

type ResetUser = { id: string; email: string; credential_hash: string };
type ResetDb = Pick<D1Database, "prepare" | "batch">;
export type ResetMail = { recipient: string; resetUrl: string };
export type ResetSender = (mail: ResetMail) => Promise<void>;

export async function playerCanChoosePassword(db: ResetDb, userId: string) {
  return Boolean(await db.prepare(`SELECT u.id FROM game_users u WHERE u.id = ? AND ${eligibleAccount}`)
    .bind(userId).first<{ id: string }>());
}

export async function ensureResetSchema(db: ResetDb) {
  await db.batch(resetSchema.map((sql) => db.prepare(sql)));
}

export function resetEmail(value: unknown): string | null {
  if (typeof value !== "string") return null;
  const email = value.trim().toLowerCase();
  return email.length <= 254 && /^[^\s<>@]+@[^\s<>@]+\.[^\s<>@]+$/.test(email) ? email : null;
}

export async function requestPasswordReset(
  db: ResetDb, email: string, clientIp: string, sender: ResetSender, publicOrigin: string, now = Date.now(), trustedAdmin = false,
) {
  // Hash limiter keys so this table does not store email addresses or IPs.
  const allowed = await Promise.all([
    // Only the server-authorized admin route may retry a player's link without
    // being blocked by the player's earlier public attempts. Still rate-limited.
    trustedAdmin ? Promise.resolve(true) : takeLimit(db, `email:${email}`, 3, now),
    takeLimit(db, `${trustedAdmin ? "admin-ip" : "ip"}:${clientIp}`, 10, now),
  ]);
  if (allowed.some((value) => !value)) return "rate_limited" as const;

  const user = await db.prepare(`SELECT u.id, u.email, COALESCE(c.hash, '') AS credential_hash
    FROM game_users u LEFT JOIN game_user_credentials c ON c.user_id = u.id
    WHERE lower(u.email) = ? AND ${eligibleAccount} LIMIT 1`)
    .bind(email).first<ResetUser>();
  if (!user || !resetEmail(user.email)) return "not_eligible" as const;

  const token = randomToken();
  const tokenHash = await hash(token);
  // Recheck privileges at insertion time as well as during redemption.
  const inserted = await db.prepare(`INSERT INTO game_password_resets
    (token_hash, user_id, email, credential_hash, expires_at, created_at)
    SELECT ?, u.id, u.email, COALESCE(c.hash, ''), ?, ?
    FROM game_users u LEFT JOIN game_user_credentials c ON c.user_id = u.id
    WHERE u.id = ? AND COALESCE(c.hash, '') = ? AND lower(u.email) = ? AND ${eligibleAccount}`)
    .bind(tokenHash, now + lifetime, now, user.id, user.credential_hash, email).run();
  if (inserted.meta.changes !== 1) return "not_eligible" as const;

  const url = new URL("/password-reset.html", publicOrigin);
  if (url.protocol !== "https:") throw new Error("Een HTTPS-resetadres is verplicht.");
  // A fragment keeps the bearer token out of normal HTTP/access logs.
  url.hash = `token=${token}`;
  try {
    await sender({ recipient: user.email, resetUrl: url.href });
  } catch {
    await db.prepare("DELETE FROM game_password_resets WHERE token_hash = ?").bind(tokenHash).run();
    throw new Error("De resetmail kon niet worden verstuurd.");
  }
  await db.prepare(`INSERT INTO audit_events (id, actor, action, target, detail, severity)
    VALUES (?, 'player-recovery', 'Wachtwoordlink aangeboden', ?, 'De mailserver heeft de instelmail voor het geregistreerde spelersadres geaccepteerd.', 'info')`)
    .bind(crypto.randomUUID(), user.id).run();
  return "sent" as const;
}

export async function confirmPasswordReset(
  db: ResetDb, token: unknown, password: unknown, confirmation: unknown, now = Date.now(),
) {
  if (typeof token !== "string" || !/^[a-f0-9]{64}$/.test(token)) return false;
  const nextPassword = accessCode(password);
  if (nextPassword !== accessCode(confirmation)) throw new Error("De wachtwoorden komen niet overeen.");
  const tokenHash = await hash(token);

  const validToken = `SELECT r.user_id FROM game_password_resets r
    JOIN game_users u ON u.id = r.user_id
    LEFT JOIN game_user_credentials c ON c.user_id = u.id
    WHERE r.token_hash = ? AND r.used_at IS NULL AND r.expires_at > ?
      AND r.credential_hash = COALESCE(c.hash, '') AND lower(r.email) = lower(u.email)
      AND ${eligibleAccount}`;
  const existing = await db.prepare(validToken).bind(tokenHash, now).first<{ user_id: string }>();
  if (!existing) return false;

  const credential = await createCredential(nextPassword);
  // D1 batch is transactional. Matching the old hash also prevents two
  // concurrent redemptions and invalidates links after any password change.
  const results = await db.batch([
    db.prepare(`INSERT INTO game_user_credentials (id, user_id, salt, hash, iterations)
      SELECT ?, u.id, ?, ?, ? FROM game_users u WHERE u.id IN (${validToken})
      ON CONFLICT(user_id) DO UPDATE SET
        salt = excluded.salt, hash = excluded.hash, iterations = excluded.iterations,
        updated_at = CURRENT_TIMESTAMP`)
      .bind(crypto.randomUUID(), credential.salt, credential.hash, credential.iterations, tokenHash, now),
    db.prepare(`UPDATE game_password_resets SET used_at = ? WHERE user_id = ? AND used_at IS NULL
      AND EXISTS (SELECT 1 FROM game_user_credentials WHERE user_id = ? AND hash = ?)`)
      .bind(now, existing.user_id, existing.user_id, credential.hash),
    db.prepare(`INSERT INTO audit_events (id, actor, action, target, detail, severity)
      SELECT ?, 'player-recovery', 'Spelerwachtwoord hersteld', ?, 'Hersteld via een eenmalige e-maillink.', 'warning'
      WHERE EXISTS (SELECT 1 FROM game_user_credentials WHERE user_id = ? AND hash = ?)`)
      .bind(crypto.randomUUID(), existing.user_id, existing.user_id, credential.hash),
  ]);
  return results[0].meta.changes === 1;
}

export async function allowResetConfirmation(db: ResetDb, clientIp: string) {
  return takeLimit(db, `confirm:${clientIp}`, 20, Date.now());
}

async function takeLimit(db: ResetDb, key: string, maximum: number, now: number) {
  const bucket = await hash(key);
  const result = await db.prepare(`INSERT INTO password_reset_limits (bucket, attempts, expires_at)
    VALUES (?, 1, ?) ON CONFLICT(bucket) DO UPDATE SET
      attempts = CASE WHEN expires_at <= ? THEN 1 ELSE attempts + 1 END,
      expires_at = CASE WHEN expires_at <= ? THEN excluded.expires_at ELSE expires_at END
    RETURNING attempts`).bind(bucket, now + rateWindow, now, now).first<{ attempts: number }>();
  return Boolean(result && result.attempts <= maximum);
}

function randomToken() {
  return [...crypto.getRandomValues(new Uint8Array(32))].map((byte) => byte.toString(16).padStart(2, "0")).join("");
}

async function hash(value: string) {
  const digest = await crypto.subtle.digest("SHA-256", new TextEncoder().encode(value));
  return [...new Uint8Array(digest)].map((byte) => byte.toString(16).padStart(2, "0")).join("");
}

import { env, waitUntil } from "cloudflare:workers";
import { database, ensureDatabase } from "../../../../_shared/database";
import { ensureResetSchema, requestPasswordReset, resetEmail, resetRequestMessage, type ResetSender } from "../../../../_shared/password-reset";
import { createResetSender } from "../../../../_shared/reset-mail";
import { resetBody, resetClientIp, resetJson, resetOriginAllowed } from "../../../../_shared/reset-http";

export const dynamic = "force-dynamic";

export async function POST(request: Request) {
  if (!resetOriginAllowed(request)) return resetJson({ error: "Ongeldige herkomst." }, 403);
  let email: string | null;
  try {
    email = resetEmail((await resetBody(request)).email);
    if (!email) return resetJson({ error: "Vul een geldig e-mailadres in." }, 400);
  } catch {
    return resetJson({ error: "Gebruik een geldige JSON-body van maximaal 4096 bytes." }, 400);
  }
  let sender: ResetSender;
  try { sender = createResetSender(env); }
  catch { return resetJson({ error: "Wachtwoordherstel is tijdelijk niet beschikbaar. Neem contact op met de beheerder." }, 503); }
  // Account lookup and mail delivery run after the identical response, so SMTP
  // timing does not reveal whether an address belongs to an eligible account.
  waitUntil(processReset(email, resetClientIp(request), sender));
  return resetJson({ message: resetRequestMessage }, 202);
}

async function processReset(email: string, clientIp: string, sender: ResetSender) {
  try {
    await ensureDatabase();
    const db = database();
    await ensureResetSchema(db);
    await requestPasswordReset(db, email, clientIp, sender, "https://gameserver.rene007.nl");
  } catch {
    // Never log passwords, reset tokens, SMTP credentials or addresses.
    console.error("Password recovery could not complete. Check database and OVH relay configuration.");
    // Same response for eligible, protected, unknown and undeliverable accounts.
  }
}

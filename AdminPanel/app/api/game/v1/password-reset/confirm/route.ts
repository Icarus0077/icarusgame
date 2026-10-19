import { database, ensureDatabase } from "../../../../_shared/database";
import { allowResetConfirmation, confirmPasswordReset, ensureResetSchema, invalidResetMessage } from "../../../../_shared/password-reset";
import { resetBody, resetClientIp, resetJson, resetOriginAllowed } from "../../../../_shared/reset-http";

export const dynamic = "force-dynamic";

export async function POST(request: Request) {
  if (!resetOriginAllowed(request)) return resetJson({ error: "Ongeldige herkomst." }, 403);
  let payload: Record<string, unknown>;
  try { payload = await resetBody(request); }
  catch { return resetJson({ error: "Gebruik een geldige JSON-body van maximaal 4096 bytes." }, 400); }
  try {
    await ensureDatabase();
    const db = database();
    await ensureResetSchema(db);
    if (!await allowResetConfirmation(db, resetClientIp(request))) {
      return resetJson({ error: "Te veel pogingen. Probeer het later opnieuw." }, 429);
    }
    const changed = await confirmPasswordReset(db, payload.token, payload.password, payload.confirmation);
    return changed
      ? resetJson({ message: "Je wachtwoord is gewijzigd. Je kunt nu opnieuw inloggen in de game." })
      : resetJson({ error: invalidResetMessage }, 400);
  } catch (error) {
    const message = error instanceof Error ? error.message : "";
    if (/wachtwoord|toegangscode/.test(message)) return resetJson({ error: message }, 400);
    return resetJson({ error: "Wachtwoordherstel is tijdelijk niet beschikbaar. Probeer het later opnieuw." }, 503);
  }
}

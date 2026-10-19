import { authorize } from "../route";
import { database, ensureDatabase } from "../../_shared/database";
import { resetBody, resetOriginAllowed } from "../../_shared/reset-http";
import { createRankedDemoAccounts, ensureRankedSchema, rankedOverview, RankedError, simulateRanked, simulationSeasonId } from "../../_shared/ranked";

export const dynamic = "force-dynamic";

function response(body: unknown, status = 200) {
  return Response.json(body, { status, headers: { "cache-control": "no-store" } });
}

function failure(error: unknown) {
  if (error instanceof Response) return error;
  if (error instanceof RankedError) return response({ error: error.message }, error.status);
  // Database/constraint details and account data never go to unauthenticated
  // callers or leak in a generic failure message.
  return response({ error: "De ranked-gegevens konden niet worden verwerkt." }, 500);
}

export async function GET(request: Request) {
  try {
    await ensureDatabase();
    await authorize(request);
    const db = database();
    await ensureRankedSchema(db);
    const query = new URL(request.url).searchParams;
    return response(await rankedOverview(db, query.get("seasonId") ?? simulationSeasonId, query.get("userId") ?? undefined));
  } catch (error) { return failure(error); }
}

export async function POST(request: Request) {
  try {
    if (!resetOriginAllowed(request) && request.headers.get("origin") !== new URL(request.url).origin) return response({ error: "Ongeldige aanvraagherkomst." }, 403);
    await ensureDatabase();
    const actor = await authorize(request);
    if (!["Super Admin", "Admin", "Moderator"].includes(actor.role)) return response({ error: "Geen schrijfrechten voor ranked-simulaties." }, 403);
    let payload: Record<string, unknown>;
    try { payload = await resetBody(request); }
    catch { return response({ error: "Ongeldige JSON-aanvraag (maximaal 4 kB)." }, 400); }
    const db = database();
    await ensureRankedSchema(db);
    if (payload.action === "create-demo-accounts") return response({ ok: true, accountIds: await createRankedDemoAccounts(db, actor.email) });
    if (payload.action !== "simulate") return response({ error: "Onbekende ranked-actie." }, 400);
    const report = await simulateRanked(db, {
      accountIds: payload.accountIds as string[], count: payload.count as number,
      seed: payload.seed as number, batchId: payload.batchId as string,
      seasonId: payload.seasonId as string | undefined,
    }, actor.email);
    return response({ ok: true, report });
  } catch (error) { return failure(error); }
}

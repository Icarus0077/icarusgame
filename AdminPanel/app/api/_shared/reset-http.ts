export function resetJson(payload: unknown, status = 200) {
  return Response.json(payload, { status, headers: {
    "cache-control": "no-store",
    "x-content-type-options": "nosniff",
    "referrer-policy": "no-referrer",
  } });
}

export async function resetBody(request: Request): Promise<Record<string, unknown>> {
  if (!request.headers.get("content-type")?.toLowerCase().startsWith("application/json")) {
    throw new Error("Gebruik een geldige JSON-body.");
  }
  const reader = request.body?.getReader();
  if (!reader) throw new Error("Gebruik een geldige JSON-body.");
  const chunks: Uint8Array[] = [];
  let length = 0;
  try {
    while (true) {
      const { value, done } = await reader.read();
      if (done) break;
      length += value.length;
      if (length > 4096) {
        await reader.cancel();
        throw new Error("Het verzoek is te groot.");
      }
      chunks.push(value);
    }
    const data = new Uint8Array(length);
    let offset = 0;
    for (const chunk of chunks) { data.set(chunk, offset); offset += chunk.length; }
    const parsed: unknown = JSON.parse(new TextDecoder().decode(data));
    if (!parsed || typeof parsed !== "object" || Array.isArray(parsed)) throw new Error();
    return parsed as Record<string, unknown>;
  } catch {
    throw new Error("Gebruik een geldige JSON-body van maximaal 4096 bytes.");
  } finally {
    reader.releaseLock();
  }
}

export function resetClientIp(request: Request) {
  // Apache appends the actual connecting address; ignore caller-supplied
  // addresses further left in X-Forwarded-For.
  const addresses = request.headers.get("x-forwarded-for")?.split(",");
  return addresses?.at(-1)?.trim().slice(0, 100) || "unknown";
}

export function resetOriginAllowed(request: Request) {
  const origin = request.headers.get("origin");
  return !origin || origin === "https://gameserver.rene007.nl";
}

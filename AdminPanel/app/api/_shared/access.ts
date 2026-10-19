export type CardPolicyRow = {
  card_id: string;
  card_name: string;
  availability: string;
  audience: string;
  regulation: string;
  updated_at: string;
};

type ParsedAddress = { bits: 32 | 128; value: bigint };

export function ipInCidr(input: string, cidr: string) {
  const [networkText, prefixText] = cidr.split("/");
  const address = parseAddress(normalizeClientIp(input));
  const network = parseAddress(networkText);
  if (!address || !network || address.bits !== network.bits) return false;

  const prefix = prefixText === undefined ? address.bits : Number(prefixText);
  if (!Number.isInteger(prefix) || prefix < 0 || prefix > address.bits) return false;
  if (prefix === 0) return true;

  const shift = BigInt(address.bits - prefix);
  return address.value >> shift === network.value >> shift;
}

export function cardPolicyAllows(accessGroup: string, policy: CardPolicyRow) {
  if (policy.availability === "hidden" || policy.availability === "disabled") return false;
  if (policy.availability !== "public" && policy.availability !== "testers") return false;
  return audienceMatches(accessGroup, policy.audience);
}

export function tokenMatches(actual: string, expected: string) {
  const length = Math.max(actual.length, expected.length);
  let difference = actual.length ^ expected.length;
  for (let index = 0; index < length; index += 1) {
    difference |= (actual.charCodeAt(index) || 0) ^ (expected.charCodeAt(index) || 0);
  }
  return difference === 0;
}

export function safeSettings(value: string) {
  try {
    const parsed = JSON.parse(value) as unknown;
    return parsed && typeof parsed === "object" && !Array.isArray(parsed) ? parsed as Record<string, unknown> : {};
  } catch {
    return {};
  }
}

function audienceMatches(accessGroup: string, audience: string) {
  if (audience === "Everyone") return true;
  if (audience === "Nobody") return false;
  if (audience === "Creators") return accessGroup === "Creator Program" || accessGroup === "Creators";
  return accessGroup === audience;
}

function normalizeClientIp(value: string) {
  let normalized = value.split(",")[0]?.trim() ?? "";
  if (normalized.startsWith("[") && normalized.includes("]")) normalized = normalized.slice(1, normalized.indexOf("]"));
  if (/^\d{1,3}(?:\.\d{1,3}){3}:\d+$/.test(normalized)) normalized = normalized.slice(0, normalized.lastIndexOf(":"));
  return normalized.split("%")[0];
}

function parseAddress(value: string): ParsedAddress | null {
  if (value.includes(":")) return parseIpv6(value);
  const parts = value.split(".");
  if (parts.length !== 4) return null;
  let result = BigInt(0);
  for (const part of parts) {
    if (!/^\d{1,3}$/.test(part)) return null;
    const number = Number(part);
    if (number < 0 || number > 255) return null;
    result = (result << BigInt(8)) | BigInt(number);
  }
  return { bits: 32, value: result };
}

function parseIpv6(value: string): ParsedAddress | null {
  if (!/^[0-9a-fA-F:.]+$/.test(value) || (value.match(/::/g)?.length ?? 0) > 1) return null;
  let normalized = value;
  const ipv4Tail = normalized.match(/(?:^|:)(\d{1,3}(?:\.\d{1,3}){3})$/)?.[1];
  if (ipv4Tail) {
    const parsedIpv4 = parseAddress(ipv4Tail);
    if (!parsedIpv4) return null;
    const high = Number((parsedIpv4.value >> BigInt(16)) & BigInt(0xffff)).toString(16);
    const low = Number(parsedIpv4.value & BigInt(0xffff)).toString(16);
    normalized = `${normalized.slice(0, -ipv4Tail.length)}${high}:${low}`;
  }

  const halves = normalized.split("::");
  const left = halves[0] ? halves[0].split(":") : [];
  const right = halves[1] ? halves[1].split(":") : [];
  const missing = 8 - left.length - right.length;
  if ((halves.length === 1 && missing !== 0) || (halves.length === 2 && missing < 1)) return null;
  const groups = [...left, ...Array.from({ length: Math.max(0, missing) }, () => "0"), ...right];
  if (groups.length !== 8) return null;

  let result = BigInt(0);
  for (const group of groups) {
    if (!/^[0-9a-fA-F]{1,4}$/.test(group)) return null;
    result = (result << BigInt(16)) | BigInt(`0x${group}`);
  }
  return { bits: 128, value: result };
}

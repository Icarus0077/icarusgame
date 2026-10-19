import { tokenMatches } from "./access";

const credentialIterations = 210_000;
const minimumCodeLength = 12;
const maximumCodeLength = 128;

export type StoredCredential = {
  salt: string;
  hash: string;
  iterations: number;
};

export function accessCode(value: unknown) {
  if (typeof value !== "string") throw new Error("Een wachtwoord is verplicht.");
  const code = value.trim();
  if (code.length < minimumCodeLength || code.length > maximumCodeLength) {
    throw new Error(`Het wachtwoord moet ${minimumCodeLength} tot ${maximumCodeLength} tekens bevatten.`);
  }
  return code;
}

export function optionalAccessCode(value: unknown) {
  if (typeof value !== "string" || !value.trim()) return null;
  return accessCode(value);
}

export async function createCredential(code: string): Promise<StoredCredential> {
  const salt = crypto.getRandomValues(new Uint8Array(16));
  return {
    salt: encodeBase64(salt),
    hash: await deriveHash(code, salt, credentialIterations),
    iterations: credentialIterations,
  };
}

export async function verifyCredential(code: string, credential: StoredCredential) {
  if (!Number.isInteger(credential.iterations) || credential.iterations < 100_000 || credential.iterations > 1_000_000) return false;
  try {
    const actual = await deriveHash(code, decodeBase64(credential.salt), credential.iterations);
    return tokenMatches(actual, credential.hash);
  } catch {
    return false;
  }
}

async function deriveHash(code: string, salt: Uint8Array<ArrayBuffer>, iterations: number) {
  const key = await crypto.subtle.importKey("raw", new TextEncoder().encode(code), "PBKDF2", false, ["deriveBits"]);
  const bits = await crypto.subtle.deriveBits({ name: "PBKDF2", hash: "SHA-256", salt, iterations }, key, 256);
  return encodeBase64(new Uint8Array(bits));
}

function encodeBase64(value: Uint8Array) {
  let binary = "";
  value.forEach((byte) => { binary += String.fromCharCode(byte); });
  return btoa(binary);
}

function decodeBase64(value: string) {
  const binary = atob(value);
  const bytes = new Uint8Array(binary.length);
  for (let index = 0; index < binary.length; index += 1) bytes[index] = binary.charCodeAt(index);
  return bytes;
}

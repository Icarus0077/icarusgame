/// <reference types="@cloudflare/workers-types" />

export {};

declare global {
  namespace Cloudflare {
    interface Env {
      DB: D1Database;
      ASSETS: Fetcher;
      GAME_API_TOKEN: string;
      SEED_DEMO_DATA: string;
      SMTP_HOST?: string;
      SMTP_PORT?: string;
      SMTP_USER?: string;
      SMTP_PASSWORD?: string;
      SMTP_FROM?: string;
    }
  }
}

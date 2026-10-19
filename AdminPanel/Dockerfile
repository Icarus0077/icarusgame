FROM node:22.19-bookworm-slim

RUN apt-get update \
    && apt-get install -y --no-install-recommends ca-certificates \
    && rm -rf /var/lib/apt/lists/*

ENV PNPM_HOME=/pnpm
ENV PATH=/pnpm:$PATH
ENV NODE_ENV=production

RUN corepack enable && corepack prepare pnpm@10.14.0 --activate

WORKDIR /app
COPY package.json pnpm-lock.yaml ./
RUN pnpm install --frozen-lockfile

COPY . .
RUN pnpm build

EXPOSE 3000

CMD ["pnpm", "exec", "wrangler", "dev", "--config=dist/server/wrangler.json", "--persist-to=/data", "--ip=0.0.0.0", "--port=3000", "--local", "--env-file=/run/secrets/icarus.env", "--show-interactive-dev-session=false"]

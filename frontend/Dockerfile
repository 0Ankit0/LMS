FROM node:20-alpine AS deps

WORKDIR /app

ENV HUSKY=0

COPY package.json package-lock.json ./
RUN --mount=type=cache,target=/root/.npm npm ci

FROM node:20-alpine AS builder

WORKDIR /app

ARG NEXT_PUBLIC_APP_NAME="FastAPI Template"
ARG NEXT_PUBLIC_API_URL="http://localhost:8000/api/v1"
ARG NEXT_PUBLIC_WS_URL="ws://localhost:8000"
ARG NEXT_PUBLIC_ANALYTICS_ENABLED="false"
ARG NEXT_PUBLIC_ANALYTICS_PROVIDER="posthog"
ARG NEXT_PUBLIC_POSTHOG_KEY=""
ARG NEXT_PUBLIC_POSTHOG_HOST="https://us.i.posthog.com"
ARG INTERNAL_API_BASE_URL="http://backend:8000"

ENV NODE_ENV=production \
    HUSKY=0 \
    NEXT_TELEMETRY_DISABLED=1 \
    NEXT_PUBLIC_APP_NAME=${NEXT_PUBLIC_APP_NAME} \
    NEXT_PUBLIC_API_URL=${NEXT_PUBLIC_API_URL} \
    NEXT_PUBLIC_WS_URL=${NEXT_PUBLIC_WS_URL} \
    NEXT_PUBLIC_ANALYTICS_ENABLED=${NEXT_PUBLIC_ANALYTICS_ENABLED} \
    NEXT_PUBLIC_ANALYTICS_PROVIDER=${NEXT_PUBLIC_ANALYTICS_PROVIDER} \
    NEXT_PUBLIC_POSTHOG_KEY=${NEXT_PUBLIC_POSTHOG_KEY} \
    NEXT_PUBLIC_POSTHOG_HOST=${NEXT_PUBLIC_POSTHOG_HOST} \
    INTERNAL_API_BASE_URL=${INTERNAL_API_BASE_URL}

COPY --from=deps /app/node_modules ./node_modules
COPY package.json package-lock.json ./
COPY vite.config.ts postcss.config.mjs tsconfig.json ./
COPY docker ./docker
COPY public ./public
COPY src ./src

RUN --mount=type=cache,target=/app/node_modules/.vite \
    npm run build

FROM node:20-alpine AS runner

WORKDIR /app

ENV NODE_ENV=production \
    HUSKY=0 \
    PORT=3000 \
    HOSTNAME=0.0.0.0

RUN addgroup --system --gid 1001 nodejs \
    && adduser --system --uid 1001 appuser

COPY --from=deps --chown=appuser:nodejs /app/node_modules ./node_modules
COPY --from=builder --chown=appuser:nodejs /app/package.json ./package.json
COPY --from=builder --chown=appuser:nodejs /app/package-lock.json ./package-lock.json
COPY --from=builder --chown=appuser:nodejs /app/docker ./docker
COPY --from=builder --chown=appuser:nodejs /app/dist ./dist

USER appuser

EXPOSE 3000

CMD ["npm", "run", "start"]

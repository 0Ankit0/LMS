# Deployment

Use this guide when you want to run the template in containers or hand it off as the starting point for another project.

## Deployment modes

| Mode | Command | Purpose |
|---|---|---|
| Development containers | `docker compose --env-file .env.docker.dev -f docker-compose.yml -f docker-compose.dev.yml up --build` | Local live-reload stack with bind-mounted source |
| Production-style containers | `docker compose --env-file .env.docker.prod -f docker-compose.yml up --build -d` | Immutable image deployment with production-oriented settings |

## Environment files

- `.env.docker.dev.example` -> copy to `.env.docker.dev` for local container development
- `.env.docker.prod.example` -> copy to `.env.docker.prod` for production-style deployments
- `backend/.env.example` -> reference for non-container backend settings
- `frontend/.env.local.example` -> reference for local web-only development
- `mobile/.env.example` -> reference for mobile startup-safe defaults

## What the Compose stack includes

- `db`: PostgreSQL 16
- `redis`: Redis 7
- `backend`: FastAPI API service
- `worker`: Celery worker
- `frontend`: React + Vite web app

## Pre-deploy checklist

1. Replace placeholder secrets in `.env.docker.prod`.
2. Set the correct public URLs for:
   - `SERVER_HOST`
   - `FRONTEND_URL`
   - `NEXT_PUBLIC_API_URL`
   - `NEXT_PUBLIC_WS_URL`
   - `SOCIAL_AUTH_REDIRECT_URL`
3. Confirm cookie settings match your ingress strategy:
   - `BACKEND_SECURE_COOKIES`
   - `BACKEND_COOKIE_SAMESITE`
4. Verify `TRUSTED_HOSTS` and `BACKEND_CORS_ORIGINS`.
5. Enable or disable feature flags explicitly for the target environment.

## Release flow

1. Build images with the target env file.
2. Run migrations before shifting traffic: the backend container already runs `alembic upgrade head` on startup, but you should still treat schema rollout as part of your release process.
3. Confirm health and readiness:
   - `/api/v1/system/health/`
   - `/api/v1/system/ready/`
4. Smoke test:
   - login and refresh-token rotation
   - notifications or websocket delivery if enabled
   - payment/provider discovery if enabled
   - social auth callback URLs if enabled

## Operational note

The template exposes runtime capability discovery through `/api/v1/system/*`. Use those responses to confirm that enabled backend modules match the web/mobile surfaces you intend to expose in the target environment.

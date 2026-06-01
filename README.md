# FastAPI Template

Reusable full-stack starter built around **FastAPI**, **React + Vite**, and **Flutter** with feature-gated modules, provider discovery, automatic token refresh, and deployment-ready Docker defaults.

## LMS migration status

This repository has been migrated from the previous .NET LMS solution to FastAPI using this template as the base.

- .NET projects were removed from `main`.
- LMS backend functionality is implemented in `backend/src/apps/lms/`.
- Original LMS requirement docs are preserved in `docs/requirements/lms/`.
- Migration implementation notes are in `docs/requirements/lms/19-FastAPI-Migration-Implementation.md`.

## What the template gives you

- Backend feature flags for auth, multitenancy, notifications, websockets, finance, analytics, maps, and social auth.
- Runtime capability discovery so web and mobile clients can adapt to enabled modules and active providers.
- Rotating access/refresh token sessions across backend, web, and mobile clients.
- Provider-driven integrations for email, push, SMS, analytics, and payments.
- Local-first developer tooling plus Docker workflows for both development and production-style deployments.
- A structured `docs/` system for onboarding, operations, architecture, and template customization.

## Stack at a glance

| Layer | Technology | Notes |
|---|---|---|
| API | FastAPI + SQLModel + Alembic | Feature-gated routers, runtime settings, Celery, Redis, websockets |
| Web | React 19 + Vite + React Query + Zustand | Adaptive dashboard UI, protected routes, proactive token refresh |
| Mobile | Flutter + Riverpod + Dio | Native auth/session handling, provider-driven notifications |
| Infra | Postgres + Redis + Docker Compose | Shared local/dev/prod container entrypoints |

## Quick start

1. Run `make setup`.
2. Review the generated env files:
   - `backend/.env`
   - `frontend/.env.local`
   - `mobile/.env`
   - `.env.docker.dev`
   - `.env.docker.prod`
3. Start infrastructure with `make infra-up`.
4. Run migrations with `make backend-migrate`.
5. Start the apps:
   - `make backend-dev`
   - `make frontend-dev`
   - `make mobile-dev`
6. Validate the template with `make ci`.

## Auth session flow

- The backend now issues a complete **access + refresh** token pair in every login/signup/OTP/social flow, including cookie mode.
- The web client refreshes expired access tokens automatically before protected API requests and reuses the rotated token pair across the app.
- The mobile client restores sessions from refresh tokens on startup and queues concurrent refresh attempts so expired sessions recover cleanly.

## Docker workflows

### Development

```bash
docker compose --env-file .env.docker.dev -f docker-compose.yml -f docker-compose.dev.yml up --build
```

### Production-style

```bash
docker compose --env-file .env.docker.prod -f docker-compose.yml up --build -d
```

### Convenience targets

- `make docker-dev-up`
- `make docker-dev-down`
- `make docker-prod-up`
- `make docker-prod-down`

Podman is also supported:

```bash
podman compose --env-file .env.docker.dev -f docker-compose.yml -f docker-compose.dev.yml config
```

## Validation commands

- Backend lint/tests: `make backend-lint` and `make backend-test`
- Frontend lint/typecheck/tests/build: `make frontend-lint` and `make frontend-test`
- Mobile analyze/tests: `make mobile-lint` and `make mobile-test`
- Docs validation: `make docs`
- Full local quality bar: `make ci`

## Repository layout

```text
backend/   FastAPI app, database models, feature modules, Alembic migrations
frontend/  React + Vite app, auth/session UX, dashboard pages, runtime-aware components
mobile/    Flutter app, native auth/session flows, notifications, profile/settings
docs/      Requirements, architecture, onboarding, deployment, and operations guides
scripts/   Bootstrap, env-copy, health, and docs validation helpers
```

## Start here in the docs

- [Documentation index](./docs/README.md)
- [Local setup](./docs/onboarding/local-setup.md)
- [Project orientation](./docs/onboarding/project-orientation.md)
- [Environment configuration](./docs/infrastructure/environment-configuration.md)
- [Deployment guide](./docs/onboarding/deployment.md)
- [Template finalization checklist](./docs/onboarding/template-finalization-checklist.md)

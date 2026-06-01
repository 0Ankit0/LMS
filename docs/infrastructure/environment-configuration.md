# Environment Configuration

This template uses a small set of env-file entrypoints instead of forcing every downstream team to rediscover where settings belong.

## Environment files by layer

| File | Scope | Intended use |
|---|---|---|
| `backend/.env` | FastAPI runtime | Local backend development and direct process execution |
| `frontend/.env.local` | Vite local runtime | Local web development outside Docker |
| `mobile/.env` | Flutter startup defaults | Mobile-safe non-secret defaults committed with the template |
| `.env.docker.dev` | Compose development stack | Live-reload container development |
| `.env.docker.prod` | Compose production-style stack | Immutable image deployment settings |

## Development vs production-style Docker defaults

| Setting area | Development | Production-style |
|---|---|---|
| `BACKEND_DEBUG` | `True` | `False` |
| `BACKEND_APP_ENV` | `development` | `production` |
| `BACKEND_SECURE_COOKIES` | `False` | `True` |
| `BACKEND_COOKIE_SAMESITE` | `lax` | `none` |
| `FRONTEND_NODE_ENV` | `development` | `production` |
| Public URLs | localhost defaults | real HTTPS / WSS URLs |

## Backend configuration model

The backend settings model reads environment values first, then allows selected keys to be overridden from the `generalsetting` table after startup. That split matters:

- **Bootstrap-only settings** must come from env or secret management:
  - `DATABASE_URL`
  - `SECRET_KEY`
  - provider secrets and credentials
- **Runtime-safe settings** can be published through the general settings system.
- **Public runtime settings** are exposed through `/api/v1/system/*` for client discovery.

## Core configuration areas

- App identity: `PROJECT_NAME`, `APP_ENV`, `APP_INSTANCE_NAME`, `APP_REGION`
- Security/session settings: `SECRET_KEY`, `PASSWORD_PEPPER`, cookie settings, auth expiry values
- Feature flags: `FEATURE_*`
- Provider selection: email, push, SMS, analytics, maps, payments, social auth
- Runtime connectivity: database, Redis, Celery, frontend/backend URLs, websocket origins
- Operational controls: rate limits, log outputs, retry/timeouts, storage, worker behavior

## Recommended workflow for downstream teams

1. Start from the provided example files.
2. Set public URLs and secrets first.
3. Decide which feature flags are truly enabled for the project.
4. Confirm the system discovery endpoints match those choices.
5. Only then begin deleting modules, routes, or UI surfaces from the template.

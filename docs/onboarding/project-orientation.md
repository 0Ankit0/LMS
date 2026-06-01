# Project Orientation

This document is the fastest way to understand how the template is put together, where the important decisions live, and how to safely adapt it for a real project.

## What This Template Is

This repository is a reusable full-stack starter made of three main application layers:

- Backend: FastAPI API, auth/session handling, admin APIs, runtime configuration, observability, providers, background jobs, and database access.
- Frontend: React + Vite web app that discovers what the backend has enabled and adapts its UI from runtime APIs instead of assuming every feature exists.
- Mobile: Flutter client with the same capability-driven approach for runtime features and public configuration.

The template is built to be configuration-first. Most project customization should start by changing settings, feature flags, and provider choices before writing new branching logic.

## How The System Fits Together

### Backend

The backend is the runtime source of truth.

- [config.py](../../backend/src/apps/core/config.py) defines the settings model, default values, parsing rules, and which values are public or runtime-editable.
- [main.py](../../backend/src/main.py) wires those settings into the running app by registering middleware, feature-gated routers, CORS, trusted hosts, rate limiting, and media serving.
- [settings_store.py](../../backend/src/apps/core/settings_store.py) syncs environment settings into the `generalsetting` table and applies database overrides for keys that are allowed to change at runtime.
- [system/api.py](../../backend/src/apps/system/api.py) exposes safe runtime discovery to clients.
- [implementation/casbin-rbac.md](../../docs/implementation/casbin-rbac.md) explains how SQL roles and permissions are mirrored into Casbin for runtime authorization.

### Frontend and Mobile

The frontend and mobile apps do not decide on their own which modules exist.

- They ask the backend which modules and providers are enabled.
- They use public config APIs for client-safe runtime values.
- They should hide, show, or adapt screens based on capability discovery instead of hard-coded assumptions.

That design lets one template power different projects without each client needing a separate code fork for every feature combination.

## The Configuration Flow

This is the most important mental model in the project:

1. A setting is defined in [config.py](../../backend/src/apps/core/config.py).
2. The backend loads bootstrap values from process environment and `backend/.env`.
3. During startup, the backend syncs the environment snapshot into the `generalsetting` table.
4. For runtime-safe keys, the database can override the environment value when `use_db_value=true`.
5. The effective settings are then used by the app, middleware, workers, services, and discovery APIs.
6. Only explicitly allowlisted keys are exposed publicly to frontend or mobile clients.

## What Each Config Group Controls

### Core identity and environment

- `PROJECT_NAME`, `APP_ENV`, `APP_INSTANCE_NAME`, `APP_REGION`
- These help identify the deployment and are useful for docs, operators, and clients.

### Feature flags

- `FEATURE_AUTH`, `FEATURE_FINANCE`, `FEATURE_ANALYTICS`, `FEATURE_WEBSOCKETS`, and similar flags
- These decide whether routers and runtime behavior are registered at all.
- In practice, feature flags are the first place to start when slimming the template down for a new project.

### Security, sessions, and cookies

- Auth token lifetime, cookie names, `SECURE_COOKIES`, `COOKIE_DOMAIN`, `COOKIE_SAMESITE`
- Trusted host and proxy behavior also live in config and affect deployment hardening.

### Providers

- Email, push, SMS, analytics, maps, and payments are selected through provider settings.
- Credentials stay private on the backend.
- Public provider state is exposed through system discovery APIs only when safe.

### Operations

- Logging outputs and retention
- Rate limits
- Suspicious activity thresholds
- Celery runtime behavior
- Database pool tuning
- Shared outbound HTTP timeout and retry policy
- Websocket origin and heartbeat controls

### Storage

- Media can use local filesystem or S3-style object storage through `STORAGE_BACKEND`.
- URL generation is normalized through the storage helper layer.

## What Happens When You Change Something

Different settings affect different parts of the system:

- Feature flags usually change router registration in [main.py](../../backend/src/main.py) and should also change frontend/mobile visibility.
- Provider settings usually affect service adapters and system discovery responses.
- Cookie, host, DB pool, middleware, and Celery settings are startup-sensitive and may require a process restart even if they are runtime-editable in the database.
- Public config changes may require frontend or mobile UI updates if the client should react to them.

## How To Modify The Template Safely

### If you want to remove a feature

1. Turn off the relevant `FEATURE_*` flag.
2. Verify the backend no longer registers the router.
3. Remove or hide the matching frontend/mobile navigation.
4. Update docs so the next team knows the feature was intentionally excluded.

### If you want to add a new provider

1. Add provider settings in [config.py](../../backend/src/apps/core/config.py).
2. Add the provider adapter in the backend service layer.
3. Register it in the appropriate provider registry.
4. Expose only safe public values through discovery APIs if clients need them.
5. Update provider docs and tests.

### If you want to add a new operational setting

1. Add it to [config.py](../../backend/src/apps/core/config.py).
2. Decide whether it is bootstrap-only, runtime-editable, secret, or public.
3. Wire it into the code path where behavior actually changes.
4. Add it to `backend/.env.example` and docs.
5. Add tests for parsing and behavior, not just for the setting existing.

### If you want to expose a setting to clients

Only do that if the value is genuinely safe and useful in the browser or mobile app.

1. Keep the source of truth on the backend.
2. Add the key to the public allowlist only if it is safe.
3. Fetch it through system APIs instead of duplicating it into multiple clients unless build-time behavior really requires it.

## Files That Usually Matter Most

- Backend settings: [config.py](../../backend/src/apps/core/config.py)
- Backend runtime wiring: [main.py](../../backend/src/main.py)
- Runtime settings store: [settings_store.py](../../backend/src/apps/core/settings_store.py)
- System discovery endpoints: [api.py](../../backend/src/apps/system/api.py)
- Communications service: [service.py](../../backend/src/apps/communications/service.py)
- Authorization model guide: [casbin-rbac.md](../../docs/implementation/casbin-rbac.md)
- Observability service: [service.py](../../backend/src/apps/observability/service.py)
- Frontend runtime hooks: [use-system.ts](../../frontend/src/hooks/use-system.ts)

## Recommended Reading Order

1. [docs/README.md](../../docs/README.md)
2. [project-orientation.md](../../docs/onboarding/project-orientation.md)
3. [configuration-management.md](../../docs/onboarding/configuration-management.md)
4. [modifying-the-template.md](../../docs/onboarding/modifying-the-template.md)
5. [template-finalization-checklist.md](../../docs/onboarding/template-finalization-checklist.md)
6. [provider-configuration.md](../../docs/onboarding/provider-configuration.md)

If you follow those in order, you should understand both how the template works today and how to change it without fighting the architecture.

# Configuration Management

This document explains where configuration lives, how the project chooses the effective value at runtime, and how to decide whether a new setting should stay private, become runtime-editable, or be exposed to clients.

## How Configuration Works

### Backend

- Backend runtime settings are defined in [backend/src/apps/core/config.py](../../backend/src/apps/core/config.py).
- Values bootstrap from process environment and `backend/.env` through `pydantic-settings`.
- `backend/.env.example` is the committed template reference, while `backend/.env` is the primary local working file.
- After the database is available, runtime setting reads can be overridden from the `generalsetting` table.
- On startup, the backend syncs the current environment snapshot into `generalsetting.env_value`.
- Feature flags such as `FEATURE_NOTIFICATIONS` control router registration and runtime behavior.
- Provider settings such as `EMAIL_PROVIDER`, `PUSH_PROVIDER`, `SMS_PROVIDER`, and `ANALYTICS_PROVIDER` select the active implementation.
- Bootstrap-only settings remain environment-driven so the app can connect to infrastructure before runtime overrides are loaded.
- Runtime-overridable operational groups now include:
  - environment identity (`APP_ENV`, `APP_INSTANCE_NAME`, `APP_REGION`)
  - host/proxy and cookie controls
  - rate limits and suspicious-activity thresholds
  - Celery task behavior
  - DB pool tuning
  - media/storage routing
  - shared HTTP timeout/retry policy
  - websocket origin/heartbeat controls

### Runtime Resolution Order

When the backend needs a setting, it resolves it in this order:

1. Default value in the `Settings` model.
2. Value from process environment or the configured `.env` file.
3. Database override from `generalsetting` if:
   - the key is runtime-editable
   - `use_db_value=true`
   - `db_value` is present

That means the environment still defines the baseline, while the database can adjust selected runtime-safe values later.

## Config Classification

| Class | Stored in | Typical examples | Restart needed? |
|---|---|---|---|
| Bootstrap-only | `backend/.env`, deployment env, secret manager | database URLs, broker URLs, secret keys | yes |
| Runtime-editable | env baseline plus `generalsetting` override | rate limits, observability thresholds, retry policy | sometimes |
| Public runtime | backend allowlist and discovery APIs | `PROJECT_NAME`, `APP_ENV`, enabled features, active providers | client fetch, no rebuild when runtime-driven |
| Secret-only | env / secret manager only | vendor credentials, signing keys, webhook secrets | yes |

Runtime-editable does not always mean hot-reloadable. Anything consumed during process startup, middleware registration, worker bootstrap, or DB engine creation still requires a restart to fully apply.

### General Settings Table

- Table name: `generalsetting`
- Purpose: persist the known configuration keys, the current environment value, and an optional database override.
- Core columns:
  - `key`: unique config key name
  - `env_value`: latest value discovered from environment/bootstrap config
  - `db_value`: optional value to apply at runtime
  - `use_db_value`: when `true`, runtime reads prefer `db_value`
  - `is_runtime_editable`: marks keys that are safe to override after boot
- Seed behavior:
  - The Alembic migration creates and seeds the table with all known settings.
  - Startup sync keeps `env_value` current for future comparisons.
  - Non-secret operational settings can opt into `db_value`.
  - Secrets and bootstrap-only infrastructure settings remain `is_runtime_editable = false`.
  - Some settings are still consumed during process startup (for example middleware, Celery bootstrap, and DB engine setup), so changing them may still require an application restart to fully apply.

### Frontend

- Web configuration uses `NEXT_PUBLIC_*` variables for browser-safe startup values and `frontend/.env.local` is the primary local file.
- The frontend also fetches runtime config from `/api/v1/system/capabilities/` and `/api/v1/notifications/push/config/`.
- The frontend should treat backend discovery APIs as the runtime source of truth for what is enabled.
- If a value is secret or vendor-sensitive, it must stay on the backend and never be moved into `NEXT_PUBLIC_*`.

### Mobile

- Flutter reads `mobile/.env` through `flutter_dotenv` for app startup values.
- Mobile capability and push settings are also fetched from backend system APIs so the app can react to deployment-specific configuration.
- Mobile now also reads `/api/v1/system/general-settings/` for a safe public subset of runtime configuration, including whether a value is currently coming from environment or database.

## How Config Affects Behavior

### Feature flags

- Feature flags primarily change router registration in [main.py](../../backend/src/main.py).
- Clients should read capability discovery APIs and adapt their navigation or flows accordingly.

### Providers

- Provider selection mainly affects the communications layer, analytics adapters, maps, and payments.
- The provider name is config-driven.
- The implementation switch should happen inside registries and adapters, not in route handlers.

### Operational settings

- Cookie, proxy, trusted-host, DB pool, rate-limit, storage, Celery, and websocket settings directly change backend runtime behavior.
- Many of these settings are read during startup, so changing them in the database may still require restarting backend or worker processes.

### Public settings

- Public settings affect frontend or mobile only if the key is intentionally allowlisted.
- Public settings should be treated as stable client contracts.

## How To Manage Configuration

1. Add or change the backend setting in `config.py`.
2. Add the new variable to the env template used by the repo.
   Use `backend/.env.example`, `frontend/.env.local.example`, or `mobile/.env.example` depending on the owning surface.
3. Decide whether the key is:
   - bootstrap-only
   - runtime-editable
   - secret
   - public
4. Wire the setting into the code path where behavior actually changes.
5. If it is browser-safe and genuinely useful at runtime, add it to the public allowlist for `/api/v1/system/general-settings/`.
6. Document it in onboarding and infrastructure docs.
7. Add tests for parsing, defaults, sync behavior, and any public API shape change.

## How To Decide Where A Setting Belongs

### Keep it env-only

Use env-only when the value is needed before the database is available or when it would be risky to change at runtime.

Examples:

- database URLs
- broker URLs
- signing secrets
- vendor credentials
- service-account files

### Make it runtime-editable

Use runtime-editable when the value is operational, non-secret, and useful to adjust without a deploy.

Examples:

- rate limits
- log outputs or thresholds
- observability thresholds
- timeout and retry policies
- feature flags for controlled environments

### Make it public

Only make a setting public when the client really needs it and the value is safe for the browser or mobile app.

Examples:

- project or environment identity
- enabled feature flags
- active provider names
- public push or maps config

## Public vs Private Configuration

- Private: secrets, API keys, signing keys, database credentials, broker URLs, service-account JSON, and file-path credentials.
- Runtime private: non-secret backend-only operational knobs such as pool sizes, proxy trust, rate limits, storage backend selection, and observability thresholds.
- Public: feature flags, active provider names, payment-provider enablement, project name, and any explicitly allowlisted runtime-safe client settings.

## Common Mistakes To Avoid

- Adding a setting to `config.py` but never wiring it into runtime behavior.
- Keeping docs pointed at a different env file than the one the code actually loads.
- Making a secret runtime-editable through `generalsetting`.
- Exposing an operational backend-only setting to public system APIs.
- Changing a feature flag without updating client visibility rules.
- Treating a startup-sensitive setting as if it will hot-reload safely.

## Safe Change Checklist

- Is the config secret or public?
- Is the config bootstrap-only or safe for runtime override?
- Does the web app need it at build time or can it fetch it at runtime?
- Does the mobile app need an env fallback?
- Does the new key need a `generalsetting` row exposed or kept private?
- Did the docs and validator get updated?

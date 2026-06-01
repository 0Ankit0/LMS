# API Design

## System Endpoints

| Endpoint | Purpose |
|---|---|
| `GET /api/v1/system/capabilities/` | Enabled modules, active providers, fallbacks |
| `GET /api/v1/system/providers/` | Provider readiness by channel |
| `GET /api/v1/system/general-settings/` | Public runtime settings subset with env-vs-database source metadata |
| `GET /api/v1/system/health/` | Liveness signal |
| `GET /api/v1/system/ready/` | Readiness signal |

## General Settings Response Shape

`GET /api/v1/system/general-settings/` returns a list of safe public settings. Each item includes:

- `key`: the config key name
- `env_value`: the current environment/bootstrap value
- `db_value`: the stored database override, if any
- `effective_value`: the value actually used at runtime
- `source`: `environment` or `database`
- `use_db_value`: whether the database override is currently active
- `is_runtime_editable`: whether the key is allowed to be overridden after boot

This endpoint is intended for runtime-aware clients such as the mobile settings screen. It must never expose secrets.

## Notification Endpoints

| Endpoint | Purpose |
|---|---|
| `GET /api/v1/notifications/preferences/` | Fetch current user preferences |
| `PATCH /api/v1/notifications/preferences/` | Update channel flags |
| `GET /api/v1/notifications/devices/` | List registered devices |
| `POST /api/v1/notifications/devices/` | Register device token or subscription |
| `DELETE /api/v1/notifications/devices/{id}/` | Remove a registered device |
| `GET /api/v1/notifications/push/config/` | Public runtime push config |
| `PUT /api/v1/notifications/preferences/push-subscription/` | Legacy web push compatibility |
| `DELETE /api/v1/notifications/preferences/push-subscription/` | Legacy cleanup path |

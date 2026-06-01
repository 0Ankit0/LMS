# Data Dictionary

| Entity | Description | Key Fields |
|---|---|---|
| User | Authenticated account holder | `id`, `email`, `username`, `is_superuser` |
| NotificationPreference | Per-user channel preferences | `websocket_enabled`, `email_enabled`, `push_enabled`, `sms_enabled` |
| NotificationDevice | Per-device push registration | `provider`, `platform`, `token`, `endpoint`, `subscription_id`, `is_active` |
| Notification | Persisted in-app alert | `title`, `body`, `type`, `is_read`, `extra_data` |
| GeneralSetting | Runtime configuration snapshot plus optional DB override | `key`, `env_value`, `db_value`, `use_db_value`, `is_runtime_editable` |
| Tenant | Multi-tenant workspace | `id`, `name`, `slug`, `owner_id` |
| Payment | Financial transaction record | `provider`, `status`, `amount`, `currency` |

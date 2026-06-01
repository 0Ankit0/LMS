# Notifications

- Duplicate device registrations should update the existing device instead of creating noise.
- Push preference enabled with no valid device should not break notification creation.
- `push_enabled` is now derived from active registered devices during sync operations, so removing the last active push device should disable push delivery automatically.
- `push_provider` should only resolve to a single provider when exactly one active provider is registered. When multiple active push providers exist, clients should use `push_providers` instead.
- Provider fallback should not create duplicate deliveries when the primary succeeds late.
- Legacy push-subscription endpoints must remain compatible while device registry is adopted.
- Legacy Web Push fields on the preference record are compatibility fields only. They should reflect the active Web Push device, not the entire push channel state.

# Modifying the Template

Use this document when you already understand the project structure and want a safe pattern for changing it. If you are new to the repo, read [project-orientation.md](../../docs/onboarding/project-orientation.md) first.

## Safe Modification Workflow

1. Identify the layer you are changing: backend, frontend, mobile, docs, or infrastructure.
2. Confirm the source of truth for that behavior.
   Usually this is backend config, provider registries, system discovery APIs, or data models.
3. Update the source-of-truth config first.
4. Extend interfaces and registries before editing route or UI logic.
5. Update clients to consume capability discovery instead of hard-coded assumptions.
6. Add or adjust tests.
7. Update docs and run the validator.

## Common Modification Patterns

### Add a New Provider

- Add settings in `config.py`.
- Create an adapter in the backend provider layer.
- Register it in the communications or analytics factory.
- Expose any client-safe values through discovery APIs.
- Add web/mobile registration or SDK wiring if the provider is client-facing.
- Update env templates and provider docs.

### Add a New Feature Flagged Module

- Add a `FEATURE_*` setting.
- Gate router registration in `main.py`.
- Hide or show related UI through capability discovery.
- Document what happens when the module is disabled.
- Add tests for both enabled and disabled states when practical.

### Add or Change Operational Behavior

- Start by checking whether the project already has a config group for that concern.
- If the behavior should be configurable, add or reuse a setting instead of hard-coding a new constant.
- Wire the setting into the actual runtime entrypoint:
  - `main.py` for app startup or middleware
  - service layers for business logic
  - provider adapters for external integrations
  - client runtime hooks for browser or mobile-visible behavior
- Decide whether the setting is env-only, runtime-editable, or public.

### Switch From Local Storage To Object Storage

- Set `STORAGE_BACKEND=s3`.
- Fill in the bucket, region, endpoint, and path-style settings in `backend/.env`.
- Verify the storage helper layer produces the URLs you expect before changing any client assumptions.
- Keep `MEDIA_BASE_URL` explicit if your public asset domain differs from the API host.

### Move From Local To Staging Or Production

- Start with the profile guidance in [environment-configuration.md](../../docs/infrastructure/environment-configuration.md).
- Review the hardening steps in [production-hardening-checklist.md](../../docs/infrastructure/production-hardening-checklist.md).
- Replace local-friendly proxy, cookie, and provider defaults with environment-specific values.
- Re-run `make ci` and `make health-check` after the environment is deployed.

### Change an Existing Public API

- Preserve compatibility when possible.
- Update web and mobile consumers in the same change.
- Add migration notes to the docs if downstream projects might already depend on the old contract.

## Files You Usually Touch

- Backend settings: [config.py](../../backend/src/apps/core/config.py)
- Backend app wiring: [main.py](../../backend/src/main.py)
- Runtime override flow: [settings_store.py](../../backend/src/apps/core/settings_store.py)
- System discovery APIs: [api.py](../../backend/src/apps/system/api.py)
- Web runtime hooks: [use-system.ts](../../frontend/src/hooks/use-system.ts)
- Mobile runtime bootstrap: [notification_bootstrapper.dart](../../mobile/lib/features/notifications/presentation/widgets/notification_bootstrapper.dart)

## Do Not Skip

- Update `backend/.env.example`
- Update `frontend/.env.local.example` or `mobile/.env.example` if the change affects those surfaces
- Update any env handling notes if the loading path changes
- Update `docs/`
- Run `make ci`

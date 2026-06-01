# Working Principles

## Core Principles

1. Configuration first: features and providers are selected through settings before code branches are added.
2. Capability driven clients: web and mobile should ask the backend what is enabled instead of assuming modules exist.
3. Provider isolation: third-party integrations belong behind adapters and registries, not inside route handlers.
4. Compatibility before removal: when a public contract changes, keep a migration path until downstream projects can move.
5. Public vs private config separation: only client-safe values are exposed through discovery APIs.
6. Documentation is part of the product: every structural change must update docs and validation rules.

## Runtime Rules

- Backend settings are the source of truth for feature flags and active providers.
- The system APIs expose enabled modules, active providers, fallbacks, and health status.
- Clients adapt navigation and registration flows based on those system APIs.
- Notification delivery respects both user preferences and provider/device readiness.
- Push preference responses should be derived from active registered devices rather than inferred from one provider-specific field.

## Modification Rules

- Add new configuration in one place first, then expose it intentionally to the layers that need it.
- Prefer extending an interface over branching business logic around a specific vendor.
- Keep tests near the contract you changed: backend endpoints, client hooks, docs validation, or mobile providers.

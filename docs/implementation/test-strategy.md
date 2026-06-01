# Test Strategy

- Backend: unit tests for config parsing, provider fallback, system APIs, device registration, and notification compatibility routes.
- Frontend: typecheck, Vitest unit coverage for runtime service behavior, build verification for capability-driven hooks.
- Mobile: widget smoke tests, provider bootstrap safety, and notification preference/runtime integration checks.
- Docs: manifest validation plus non-empty file checks.
- CI: gate merges on backend, frontend, mobile, and docs stages.

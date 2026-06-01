# Template Finalization Checklist

Use this checklist when you are turning the starter into a real downstream project. It helps you separate "rename the template" work from "shape the product" work.

## Before You Rename Anything

1. Read [project-orientation.md](../../docs/onboarding/project-orientation.md), [configuration-management.md](../../docs/onboarding/configuration-management.md), and [modifying-the-template.md](../../docs/onboarding/modifying-the-template.md).
2. Run `make setup` so every app surface has a local env file.
3. Run `make infra-up` and `make backend-migrate` once to confirm the baseline template boots cleanly before you customize it.
4. Capture the initial health of the starter with `make health-check` and `make ci`.

## Rename The Template Identity

- Change `PROJECT_NAME`, `APP_INSTANCE_NAME`, and any product-facing names in backend, frontend, mobile, and docs.
- Review package/app identifiers in `frontend/package.json`, Flutter metadata, and deployment manifests.
- Update branded copy in the top-level [README.md](../../README.md) and docs index.

## Module Review

- Decide which `FEATURE_*` modules stay enabled.
- Remove navigation, routes, and docs references for modules your project will not ship.
- Keep capability discovery in place for the modules that remain so web and mobile continue to adapt safely.

## Provider Review

- Choose primary providers for email, push, SMS, analytics, maps, and payments.
- Remove provider credentials you will never use from deployment secrets management.
- Test sandbox credentials first, then document the production cutover separately.

## Configuration Review

Use this quick classification rule:

| Setting type | Where it lives | What it controls |
|---|---|---|
| Bootstrap-only | env / deployment secrets | infra connection, secrets, startup wiring |
| Runtime-editable | `generalsetting` plus env baseline | safe operational tuning after boot |
| Public runtime | backend allowlist and discovery APIs | browser/mobile-safe values |
| Secret-only | env / secret manager only | credentials, signing keys, webhook secrets |

- Review [backend/.env.example](../../backend/.env.example) group by group.
- Confirm which settings are startup-sensitive and require restart even if they are runtime-editable.
- Trim any public general settings your project does not actually need clients to consume.

## Storage And Data Review

- Choose `STORAGE_BACKEND=local` or `s3` intentionally.
- Review database naming, media URLs, and retention policies before the first shared environment.
- Confirm local bootstrap still works against Postgres before teammates start building on top of the template.

## Production Readiness Review

- Review [production-hardening-checklist.md](../../docs/infrastructure/production-hardening-checklist.md).
- Revisit rate limits, suspicious-activity thresholds, and cookie settings for your real traffic profile.
- Verify host/proxy trust, webhook validation, and provider callback URLs in staging before production.

# Local Setup

Use the repository root as the single entrypoint for local development. The goal is that a fresh clone becomes runnable without hidden steps.

## Bootstrap Workflow

1. Run `make setup`.
2. Review the generated or versioned env files:
   - `backend/.env`
   - `frontend/.env.local`
   - `mobile/.env`
   - `.env.docker.dev`
   - `.env.docker.prod`
3. Adjust values for your machine or target services.

## Important env file notes

- `backend/.env` is the main editable backend settings file for local development.
- `frontend/.env.local` contains browser-safe values and local web defaults.
- `mobile/.env` is intentionally versioned because it only contains startup-safe defaults needed by Flutter tooling. Keep secrets out of it.
- `.env.docker.dev` and `.env.docker.prod` drive Compose-based development and deployment.

## Run The Applications

1. Start backing services with `make infra-up`.
2. Run migrations with `make backend-migrate`.
3. Start the apps in separate terminals:
   - `make backend-dev`
   - `make frontend-dev`
   - `make mobile-dev`

## Run the stack with Docker

### Development containers

```bash
make docker-dev-up
```

This uses:

- `docker-compose.yml` as the shared base
- `docker-compose.dev.yml` as the live-reload development override
- `.env.docker.dev` for development settings

### Production-style containers

```bash
make docker-prod-up
```

This uses:

- `docker-compose.yml` as the production base
- `.env.docker.prod` for production-style settings

## Validate The Starter

1. Run `make health-check` after the backend is up.
2. Confirm these endpoints respond:
   - `/api/v1/system/health/`
   - `/api/v1/system/ready/`
   - `/api/v1/system/capabilities/`
   - `/api/v1/system/providers/`
   - `/api/v1/system/general-settings/`
3. Run `make ci` before treating the repository as your downstream baseline.

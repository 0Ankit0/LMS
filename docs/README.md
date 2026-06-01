# FastAPI Template Documentation

This documentation set is the handoff surface for downstream teams adopting the template. It covers what is built, how the modules fit together, how configuration flows through the stack, and how to change or deploy the starter safely.

## Getting Started

1. [requirements/requirements.md](../docs/requirements/requirements.md)
2. [onboarding/project-orientation.md](../docs/onboarding/project-orientation.md)
3. [onboarding/local-setup.md](../docs/onboarding/local-setup.md)
4. [infrastructure/environment-configuration.md](../docs/infrastructure/environment-configuration.md)
5. [onboarding/deployment.md](../docs/onboarding/deployment.md)
6. [onboarding/modifying-the-template.md](../docs/onboarding/modifying-the-template.md)
7. [onboarding/template-finalization-checklist.md](../docs/onboarding/template-finalization-checklist.md)
8. [implementation/lms-qa-and-seeding.md](../docs/implementation/lms-qa-and-seeding.md)

## Documentation Structure

- `requirements/`: scope, personas, expectations, and success criteria.
- `analysis/`: workflows, actors, data/event modeling, and domain notes.
- `high-level-design/`: system boundaries and runtime architecture.
- `detailed-design/`: API contracts, models, and implementation detail.
- `implementation/`: operational playbooks such as RBAC and rollout notes.
- `infrastructure/`: environments, runtime settings, and production hardening.
- `edge-cases/`: failure modes and template-specific operational concerns.
- `onboarding/`: local setup, deployment, extension guidance, and template handoff.

## Key Features

- Feature-gated backend modules with runtime capability discovery for both clients.
- Automatic refresh-token session recovery across the web and mobile apps.
- Development and production-style Docker env templates with a shared Compose base.
- Versioned mobile startup defaults so Flutter tooling works from a fresh clone.
- Portable markdown links and a docs structure aimed at downstream project teams.

## What changed to make the template reusable

- Backend auth flows now issue a complete access/refresh token pair consistently in both JSON and cookie modes.
- Web and mobile clients both restore sessions automatically through refresh-token rotation.
- Docker now has explicit development and production-style environment templates.
- Mobile ships with safe versioned startup defaults so a fresh checkout can analyze and test without hidden manual setup.
- Documentation links are portable and no longer tied to a machine-specific path.

## Documentation Status

- Docs structure/content validation: `python3 scripts/validate_documentation.py`
- Full local quality bar from repo root: `make ci`

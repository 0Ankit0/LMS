# Environment Profiles

| Profile | Use |
|---|---|
| `local` | Local cloning, feature experimentation, and UI development |
| `staging` | Integrated provider and deployment rehearsal |
| `production` | Live traffic with hardened hosts, secrets, and monitoring |

Each profile should define feature flags, primary providers, fallback order, database targets, Redis settings, and public client config.

For local development, Postgres remains the default target so the template matches production behavior. If a team changes the logical database name, they should update `POSTGRES_DB` and `DATABASE_URL` together to avoid drift.

# Security and Compliance

- Authorization checks must use the correct Casbin domain. A tenant-scoped request evaluated in the `global` domain can accidentally bypass intended tenant isolation.
- Public push config must never expose secrets or server-side credentials.
- Provider webhooks and callbacks require signature verification in production projects.
- Secrets must be loaded from environment or secret managers, never committed.
- Health endpoints should avoid leaking sensitive configuration details.

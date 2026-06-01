# Multi-Tenancy

- Tenant-aware UI must disappear when `FEATURE_MULTITENANCY=false`.
- Cached tenant context must not leak between users.
- Analytics and notifications should include tenant metadata only when context exists.
- Invitations and membership changes must invalidate stale client state.

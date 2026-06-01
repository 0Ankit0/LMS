# Casbin RBAC Guide

This project uses Casbin as the runtime authorization engine, but Casbin is only one part of the RBAC stack. The full system has two layers:

- SQL tables define the admin-managed catalog: roles, permissions, user-role assignments, and role-permission assignments.
- Casbin stores the runtime authorization tuples that are used for permission checks during requests.

If you only change one layer and forget the other, the system will look correct in the database but behave incorrectly at runtime.

## Files That Matter

- [casbin_enforcer.py](../../backend/src/apps/iam/casbin_enforcer.py)
- [casbin_model.conf](../../backend/src/apps/iam/casbin_model.conf)
- [casbin_rule.py](../../backend/src/apps/iam/models/casbin_rule.py)
- [rbac.py](../../backend/src/apps/iam/utils/rbac.py)
- [rbac.py](../../backend/src/apps/iam/api/v1/rbac.py)
- [main.py](../../backend/src/main.py)

## The Authorization Model

The active Casbin request model is domain-aware RBAC:

- Request tuple: `(subject, domain, object, action)`
- Policy tuple: `(role, domain, object, action)`
- Grouping tuple: `(user, role, domain)`

In practical terms:

- `subject` is usually the authenticated user id converted to a string.
- `domain` is either the global namespace or an organization slug from the `tenant` table.
- `object` is the permission resource such as `users`, `posts`, or `settings`.
- `action` is the permission action such as `read`, `write`, or `delete`.

The matcher in [casbin_model.conf](../../backend/src/apps/iam/casbin_model.conf) is intentionally strict:

- The user must hold the role in the same domain.
- The requested resource must exactly match the policy resource.
- The requested action must exactly match the policy action.

There are no wildcards, regexes, path patterns, or ownership checks in the current model.

## What Gets Stored In Casbin

Casbin persists everything to the `casbin_rule` table through the SQLAlchemy adapter.

For this project, the meaning of the columns is:

- `ptype="p"`: `v0=role`, `v1=domain`, `v2=resource`, `v3=action`
- `ptype="g"`: `v0=user_id`, `v1=role`, `v2=domain`

Examples:

```text
p, admin, global, users, read
p, admin, global, users, write
g, 42, admin, global
g, 42, owner, acme-inc
```

Those rows mean:

- The `admin` role may `read` and `write` `users` in the `global` domain.
- User `42` is an `admin` globally.
- User `42` is also an `owner` inside the `acme-inc` organization domain.

## Request Flow

The normal permission-check path looks like this:

1. Startup loads the shared Casbin enforcer in [main.py](../../backend/src/main.py).
2. Admin APIs create roles and permissions in SQL tables.
3. RBAC utility helpers mirror those assignments into Casbin policy rows.
4. A protected code path calls `check_permission(...)` in [rbac.py](../../backend/src/apps/iam/utils/rbac.py).
5. `check_permission(...)` delegates to `CasbinEnforcer.enforce(...)`.
6. Casbin evaluates the request tuple against the loaded policies.

That means the SQL tables are useful for management and auditability, but the allow/deny decision is made by Casbin.

## Global vs Tenant Domains

The constant `GLOBAL_DOMAIN = "global"` is the fallback namespace for non-organization checks.

Use the global domain when:

- The permission is application-wide.
- The resource is not tenant-scoped.

Use an organization slug when:

- Membership and access belong to one organization only.
- The same user can have different roles in different organizations.

Avoid mixing global and organization policies for the same request unless you intentionally want both behaviors to exist.

## How To Modify It Safely

### Add a new permission

1. Create a `Permission` row through the admin API or seed code.
2. Assign it to one or more roles through the RBAC helpers.
3. Protect the route or service with a permission check that uses the same resource and action strings.

If the strings do not match exactly, Casbin will deny the request.

### Add a new role

1. Create the `Role` row.
2. Attach one or more permissions to the role.
3. Assign the role to users in the correct domain.

Do not insert only the SQL row and expect authorization to work. The Casbin tuple must also exist, which is why the helper functions should be preferred over manual inserts.

### Change the matcher or policy shape

Only change [casbin_model.conf](../../backend/src/apps/iam/casbin_model.conf) when the authorization model itself needs to change.

Examples:

- Adding wildcard resources
- Adding ownership or attribute-based checks
- Changing how domains behave

When you change the model:

1. Update this document.
2. Update [casbin_rule.py](../../backend/src/apps/iam/models/casbin_rule.py) comments if column meaning changes.
3. Review every helper that writes policies or grouping rows.
4. Add tests for both allow and deny cases.

### Rename a resource or action

Treat this as a data migration, not just a code rename.

You must update:

- the `Permission` rows
- any existing Casbin `p` rows
- route guards or service checks
- admin UI or API consumers that create or display permissions

### Work with tenant roles

Organization role assignments should always include the organization slug as the domain. Reusing the global domain for organization membership will make access bleed across organizations.

## Catalog vs Organization Membership

There are two role systems in the codebase, and they are intentionally different:

- The SQL `Role`, `Permission`, `UserRole`, and `RolePermission` tables are the global RBAC catalog.
- Organization membership lives in `TenantMember`, where `Tenant` is the organization record and `Tenant.slug` becomes the Casbin domain.

That means:

- Use the RBAC admin APIs for global roles and global permissions.
- Use the multitenancy APIs for organization membership and owner/admin/member changes.
- When a request needs an organization-scoped Casbin check, resolve the domain from the organization record instead of inventing an arbitrary string.

## Recommended Modification Rules

- Prefer changing RBAC through the helpers in [rbac.py](../../backend/src/apps/iam/utils/rbac.py), not through raw SQL.
- Prefer exact, stable `resource` and `action` names over human-readable phrases.
- Keep route-level authorization strings aligned with the permission catalog.
- Document any new domain conventions before multiple teams start relying on them.

## Current Limitations

- The model only supports allow policies.
- Matching is exact, not pattern-based.
- Ownership rules are not built into the matcher.
- Superuser bypass behavior, if desired, belongs in application logic or a future model change.

If you need richer authorization than role + domain + exact permission, extend the model intentionally rather than sneaking special cases into random route handlers.

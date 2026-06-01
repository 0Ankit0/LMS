# Production Hardening Checklist

Use this document before promoting a downstream project beyond local development. The template is production-capable, but these reviews still belong to the project team.

## Secrets

- Move `SECRET_KEY`, database credentials, provider credentials, webhook secrets, and service-account material into a proper secret manager or deployment secret store.
- Keep secret-only values env-managed; do not expose them through `generalsetting` or client discovery APIs.
- Rotate default placeholder values before the first shared environment.

## Network and Proxy Trust

- Replace permissive local defaults for `TRUSTED_HOSTS`, `PROXY_TRUSTED_HOSTS`, and `FORWARDED_ALLOW_IPS` with the exact values used by your ingress and proxy chain.
- Review `BACKEND_CORS_ORIGINS`, `FRONTEND_URL`, and `SERVER_HOST` for each environment.
- Confirm `SECURE_COOKIES=True`, `COOKIE_DOMAIN`, and `COOKIE_SAMESITE` for any non-local deployment.

## Storage and Data Retention

- Choose the storage backend intentionally and verify bucket policies, object ACL strategy, and media base URLs if you use S3-compatible storage.
- Review log retention, audit data retention, and backup expectations before production rollout.
- Confirm DB pool settings match the deployed database size and connection limits.

## Providers and Callbacks

- Verify each enabled provider with environment-appropriate credentials.
- Validate webhook signatures and callback URLs for payment, auth, and notification providers.
- Keep sandbox and production credentials separate and documented in your deployment system.

## Worker and Background Processing

- Run Celery with explicit broker/result backend settings and confirm queue names, time limits, and retries suit your workload.
- Make sure worker startup uses the same env source-of-truth as the API process.
- Add platform-level monitoring for failed tasks, queue backlog, and retry storms.

## Observability and Response

- Tune log outputs, suspicious-activity thresholds, and incident review ownership for your team.
- Verify health and readiness endpoints through your deployment platform.
- Decide who owns admin review of security incidents before going live.

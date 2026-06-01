# Websockets

- Websocket routes must be fully disabled when the feature flag is off.
- Notification fan-out should degrade to stored delivery when websocket connection is unavailable.
- Tenant changes should re-scope realtime subscriptions.
- Redis-backed pub/sub must fail safely in local development.

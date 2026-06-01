# Use Case Descriptions

| Use Case | Primary Actor | Trigger | Result |
|---|---|---|---|
| Authenticate | User | Login, refresh, social auth, OTP | Session is created and tracked. |
| Manage Preferences | User | Settings update | Channel preferences are persisted. |
| Receive Notifications | User | Event occurs | Websocket, push, email, or SMS delivery is attempted. |
| Switch Tenant | User | Tenant selector change | Active context changes across the app. |
| Inspect Provider Status | Operator | System API request | Runtime capability and provider readiness are returned. |
| Bootstrap New Project | Template Team | Clone template | Docs, env profiles, and modules guide setup. |

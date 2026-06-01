# Event Catalog

| Event | Producer | Consumer | Notes |
|---|---|---|---|
| `user_logged_in` | IAM | Analytics, audit trail | Updates active session state. |
| `notification_created` | Notification service | Websocket, push, email, SMS | Fan-out respects channel preferences. |
| `device_registered` | Client app | Notification registry | Stores push reachability for a device. |
| `payment_initiated` | Finance module | Provider adapter, analytics | Tracks checkout lifecycle. |
| `system_capabilities_requested` | Web or mobile client | System API | Drives capability-based UI. |

# Data Flow Diagrams

```mermaid
flowchart LR
  Client -->|REST/WebSocket| API
  API -->|ORM| DB[(SQL Database)]
  API -->|Cache / broker| Redis[(Redis)]
  API -->|Provider adapters| Comms[Communications Layer]
  Comms --> Email[Email Service]
  Comms --> Push[Push Service]
  Comms --> SMS[SMS Service]
  API --> Analytics[Analytics Service]
```

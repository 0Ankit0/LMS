# C4 Component Diagram

```mermaid
flowchart TB
  API[FastAPI Application]
  System[System Endpoints]
  Notify[Notification Endpoints]
  Comms[Communications Service]
  Email[Email Providers]
  Push[Push Providers]
  SMS[SMS Providers]
  Analytics[Analytics Adapters]

  API --> System
  API --> Notify
  Notify --> Comms
  Comms --> Email
  Comms --> Push
  Comms --> SMS
  API --> Analytics
```

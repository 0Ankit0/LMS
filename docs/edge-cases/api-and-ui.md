# API and UI

- Clients must treat capability discovery as the source of truth for module visibility.
- Mobile and web should degrade gracefully when a provider is configured but not ready.
- System APIs must remain lightweight so bootstrapping does not slow first render.
- Admin-only actions should stay hidden and protected even if a route is guessed.

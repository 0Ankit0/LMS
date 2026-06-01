# Business Rules

1. Module flags determine whether routers, UI navigation, and runtime behavior are available.
2. Provider selection is environment-driven and does not require database changes.
3. Notification delivery must respect both user preferences and provider readiness.
4. A user may hold multiple active notification devices across providers and platforms.
5. Public push configuration must expose only client-safe values.
6. System discovery endpoints must remain available even when optional modules are disabled.

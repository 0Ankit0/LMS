# Implementation Guidelines

- Add new providers behind interfaces and registries before wiring them into business logic.
- Prefer feature flags and capability discovery over hard-coded client assumptions.
- Keep compatibility routes until downstream projects have a migration path.
- Add tests when introducing new provider payload shapes or public runtime APIs.

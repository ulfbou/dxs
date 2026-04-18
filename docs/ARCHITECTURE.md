# Architecture

```mermaid
graph TD
Contracts --> Planning
Planning --> EngineAbstractions
EngineAbstractions --> Engine
Engine --> InfraAbstractions
InfraAbstractions --> InfraSqlite

Executors --> Contracts
CLI --> Contracts
CLI --> Planning
CLI --> EngineAbstractions
```

## Forbidden References

| From           | To             | Error             |
| -------------- | -------------- | ----------------- |
| Executors      | Infrastructure | SOP 4.2 VIOLATION |
| Executors      | Engine         | SOP 4.2 VIOLATION |
| CLI            | Engine Impl    | SOP 4.2 VIOLATION |
| Planning       | Infrastructure | SOP 4.2 VIOLATION |
| Infrastructure | Engine         | SOP 4.2 VIOLATION |

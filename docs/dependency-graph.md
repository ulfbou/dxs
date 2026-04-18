```mermaid
graph TD
Contracts --> Planning
Planning --> EngineAbstractions
EngineAbstractions --> Engine
Engine --> InfrastructureAbstractions
InfrastructureAbstractions --> InfrastructureSqlite

Executors --> Contracts
CLI --> Contracts
CLI --> Planning
CLI --> EngineAbstractions

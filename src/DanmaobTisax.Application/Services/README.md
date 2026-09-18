# Services

Each functional domain gets its own subfolder here (e.g. `Organizations/`)
containing one `IXxxService` interface and one `XxxService` implementation,
with one method per use case.

Do NOT introduce CQRS commands/queries or a mediator library (e.g. MediatR)
here — see `docs/adr/ADR-0001-clean-architecture-backend.md`.

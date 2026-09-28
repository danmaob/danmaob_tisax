# ADR-0001: Clean Architecture Backend Skeleton for DanmaobTisax

**Status:** Accepted
**Date:** 2026-09-11

## Context

DanmaobTisax Compliance Manager is a multi-domain B2B platform (20 functional
domains planned across several releases: organizations, scope, catalog, gap
analysis, risks, evidence, CAPA, audits, etc.) that helps automotive-industry
organizations prepare for and maintain TISAX compliance using the VDA ISA
catalog. The backend will be built through many small, independent,
sequential implementation tasks executed by a coding agent (Cline) with a
limited context window, so the foundation must be simple, consistent, and
extensible without redesign.

The backend must also support two deployment topologies — On-Premise/Private
Cloud (a single organization) and SaaS (multiple organizations) — from the
same codebase and the same database schema, without structural differences
between the two.

## Decision

- The backend solution is named **DanmaobTisax** and lives entirely inside
  the `Net/` folder of the workspace.
- Target framework: **.NET 10.0** (LTS, supported until November 2028).
- Repository layout:
  ```
  Net/
    DanmaobTisax.slnx
    global.json
    Directory.Build.props
    .editorconfig
    .gitignore
    docs/
      adr/
        ADR-0001-clean-architecture-backend.md
    src/
      DanmaobTisax.Domain/
      DanmaobTisax.Application/
      DanmaobTisax.Infrastructure/
      DanmaobTisax.Api/
    tests/
      DanmaobTisax.ArchitectureTests/
    README.md
  ```
- Four Clean Architecture projects with strict reference rules:
  - **`DanmaobTisax.Domain`** — enterprise business rules/entities. Zero
    project references and zero external NuGet packages (BCL only).
  - **`DanmaobTisax.Application`** — application/use-case logic. References
    Domain only.
  - **`DanmaobTisax.Infrastructure`** — persistence, identity, external
    services. References Application only (and transitively Domain).
  - **`DanmaobTisax.Api`** — ASP.NET Core Web API (controllers), composition
    root. References Application and Infrastructure.
  - No project may reference `DanmaobTisax.Api`.
- Application layer pattern: **plain services**, explicitly not CQRS and not
  a mediator library (no MediatR). Convention: one interface `IXxxService`
  plus one implementation `XxxService` per functional domain, one method per
  use case.
- Folder convention so future functional domains (Jira epics E01–E20) are
  added by creating new subfolders inside the existing four projects — never
  by creating new projects and never by changing the base layer structure:
  - New domain entities go under `src/DanmaobTisax.Domain/<DomainName>/`.
  - New domain services go under
    `src/DanmaobTisax.Application/Services/<DomainName>/`.
  - New persistence/config code goes under
    `src/DanmaobTisax.Infrastructure/Persistence/<DomainName>/`.
- A dedicated test project, `DanmaobTisax.ArchitectureTests`, enforces these
  reference rules automatically on every build.

## Alternatives Considered

- **CQRS with MediatR** — rejected. MediatR moved to a commercial license in
  2025 for companies above $5M USD annual revenue, and CQRS adds indirection
  not currently justified by team size or by the complexity of the use cases
  planned so far.
- **A layered structure without strict Clean Architecture boundaries** —
  rejected. It would not enforce reference rules automatically and would be
  harder to keep consistent across 20 functional domains built through many
  independent, fragmented implementation tasks.

## Consequences

- Every future functional domain adds its code inside the existing four
  projects; no new project is ever created for a new domain.
- `DanmaobTisax.ArchitectureTests` enforces the reference rules on every
  build, so a violation is caught immediately rather than discovered later
  through code review.
- Adopting CQRS later, if ever justified by real complexity, requires a new
  ADR that explicitly supersedes this one — it must not be introduced
  silently in a single functional domain.
- Documentation and architecture-decision artifacts (like this one) are
  authored by the engineering orchestrator (Claude), not by the coding agent
  executing implementation tasks (Cline) — this keeps the coding agent's
  limited context budget dedicated exclusively to writing and verifying code.

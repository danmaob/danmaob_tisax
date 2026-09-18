# DanmaobTisax Compliance Manager — Backend

.NET backend for DanmaobTisax Compliance Manager, a B2B platform that helps
automotive-industry organizations (Tier 1/Tier 2 suppliers) implement,
manage, and maintain TISAX compliance using the VDA ISA catalog as the
normative reference.

## Prerequisites

- .NET 10.0 SDK (the exact version pinned for this repository is in
  `global.json`).
- A reachable SQL Server instance for anything beyond `dotnet build`/
  `dotnet test` (see "Secure configuration and secrets" below).

## Build

From `Net/`:
```
dotnet build DanmaobTisax.slnx
```

## Run the API

From `Net/`:
```
dotnet run --project src/DanmaobTisax.Api
```
A health check is available at `GET /health` (returns `200 OK` with body
`Healthy`). It does not touch the database, so it will respond even before a
real connection string is configured. The application will refuse to start
at all if `ConnectionStrings:DefaultConnection` is missing — see "Secure
configuration and secrets" below.

## Run tests

From `Net/`:
```
dotnet test DanmaobTisax.slnx
```
This runs three test projects, 9 tests total, none requiring a real
database: `DanmaobTisax.ArchitectureTests` (4 tests, static layer-boundary
checks), `DanmaobTisax.Infrastructure.IntegrationTests` — multi-tenancy
isolation checks using EF Core's InMemory provider (2 tests) plus
configuration validation checks using dummy connection strings (3 tests).

## Secure configuration and secrets

Full rationale in `docs/adr/ADR-0003-secure-configuration.md`.

**Local development — .NET User Secrets:**
```
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<your real connection string>" --project src/DanmaobTisax.Api
```
Stored outside the repository (in your OS user profile), never committed.

**Deployed environments (On-Premise, Private Cloud, SaaS) — environment
variables**, using ASP.NET Core's double-underscore convention for nested
keys:
```
ConnectionStrings__DefaultConnection=<value>
```
A managed secret store (e.g. a cloud Key Vault) for Private Cloud/SaaS is
deferred until a cloud provider is selected — see ADR-0003.

**Fail-fast validation.** The application checks required configuration keys
at startup via `RequiredConfigurationValidator.EnsurePresent(configuration, key, guidance)`
(`Infrastructure/Configuration/RequiredConfigurationValidator.cs`) and throws
a clear `InvalidOperationException` — naming the missing key, never the
value — if any is absent, rather than starting in a half-configured state.
This helper is intentionally generic: it is reused as-is by future technical
tasks with a similar requirement (e.g. TS-00-3's JWT signing key), not
rewritten per secret.

**EF Core CLI tooling uses a separate mechanism from User Secrets** — `dotnet
ef` commands do not read User Secrets, only environment variables. Before
running any `dotnet ef` command, export the connection string in your shell:
```
export ConnectionStrings__DefaultConnection="<your real connection string>"
dotnet ef migrations add <Name> --project src/DanmaobTisax.Infrastructure --startup-project src/DanmaobTisax.Infrastructure
dotnet ef database update --project src/DanmaobTisax.Infrastructure --startup-project src/DanmaobTisax.Infrastructure
```
`DanmaobTisax.Infrastructure` is self-sufficient for this tooling via its own
`IDesignTimeDbContextFactory` (`Persistence/DanmaobTisaxDbContextFactory.cs`)
— it does not need the `Api` project as a startup project.

**Operational boundary:** applying a migration or setting a real secret
against a real database is always done by a human operator directly, never
by the coding agent used on this project — see ADR-0003 for why.

## Solution layout

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
      ADR-0002-multitenancy-data-model.md
      ADR-0003-secure-configuration.md
  src/
    DanmaobTisax.Domain/
    DanmaobTisax.Application/
    DanmaobTisax.Infrastructure/
    DanmaobTisax.Api/
  tests/
    DanmaobTisax.ArchitectureTests/
    DanmaobTisax.Infrastructure.IntegrationTests/
  README.md
```

The solution file is `DanmaobTisax.slnx` — the XML-based format that is the
default for `dotnet new sln` starting in .NET 10 (replacing the legacy
`.sln`). All tooling here (`dotnet build`, `dotnet test`, `dotnet ef ...`)
works with it the same way it would with a `.sln` file.

## Layer responsibilities and reference rules

Full rationale in `docs/adr/ADR-0001-clean-architecture-backend.md`. Summary:

- **`DanmaobTisax.Domain`** — entities and business rules. No dependency on
  any other project in this solution, no external NuGet packages.
- **`DanmaobTisax.Application`** — use-case logic as plain services
  (`IXxxService` / `XxxService`, one method per use case — no CQRS, no
  mediator library). Depends only on Domain.
- **`DanmaobTisax.Infrastructure`** — persistence, identity, external
  services, multi-tenancy, configuration validation. Depends only on
  Application.
- **`DanmaobTisax.Api`** — ASP.NET Core Web API (controllers), composition
  root. Depends on Application and Infrastructure. No project in this
  solution depends on `DanmaobTisax.Api`.
- **`DanmaobTisax.ArchitectureTests`** — enforces the four rules above
  automatically on every `dotnet test` run, using NetArchTest.Rules.

## Multi-tenancy

Full rationale in `docs/adr/ADR-0002-multitenancy-data-model.md`.

**Making a new entity tenant-scoped:** implement `ITenantOwned` (from
`DanmaobTisax.Domain.Common`) and set its `TenantId` when creating instances.
`DanmaobTisaxDbContext` applies an EF Core global query filter to every
entity implementing `ITenantOwned` automatically. `Tenant` itself does
**not** implement `ITenantOwned` — it is the tenant root, not tenant-owned
data; its scope in this codebase is identity fields only (full administrative
lifecycle is Jira story US-20-1, not yet built).

**Configuration keys** (`MultiTenancy:Mode`, `MultiTenancy:FixedTenantId`):
currently in `appsettings.Development.json` (non-sensitive, unlike the
connection string, so it is not part of the secrets mechanism above).

**`MultiTenancy:Mode = MultiTenant` is not usable yet** — `ICurrentTenantProvider`
throws `NotSupportedException` on purpose until authentication (TS-00-3) and
the per-request tenant evaluation engine (US-20-2) exist. See ADR-0002.

## How to add a new functional domain

1. Add domain entities under `src/DanmaobTisax.Domain/<DomainName>/`. If the
   entity's data must be isolated per tenant (the normal case for business
   data), implement `ITenantOwned` on it — no other change is needed for
   isolation to apply.
2. Add the use-case service under
   `src/DanmaobTisax.Application/Services/<DomainName>/` as
   `I<DomainName>Service` + `<DomainName>Service`.
3. Register the new service inside `ApplicationServiceCollectionExtensions.AddApplication()`
   in `src/DanmaobTisax.Application/DependencyInjection.cs`.
4. Add any persistence/configuration code under
   `src/DanmaobTisax.Infrastructure/Persistence/<DomainName>/` and register
   it inside `InfrastructureServiceCollectionExtensions.AddInfrastructure()`
   in `src/DanmaobTisax.Infrastructure/DependencyInjection.cs`.
5. Add the corresponding controller under `src/DanmaobTisax.Api/Controllers/`.
6. If the domain introduces a new required secret (an API key, another
   connection string, etc.), use `RequiredConfigurationValidator.EnsurePresent`
   the same way the database connection string does — do not invent a
   different validation pattern.
7. Do not create a new class library project for the new domain — it always
   lives inside the four existing projects. `DanmaobTisax.ArchitectureTests`
   will fail the build automatically if a reference rule is violated.

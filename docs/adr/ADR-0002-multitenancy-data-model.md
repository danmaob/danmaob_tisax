# ADR-0002: Multi-Tenant Data Model

**Status:** Accepted
**Date:** 2026-09-11

## Context

DanmaobTisax must support two deployment topologies from one codebase and one
database schema: On-Premise/Private Cloud (a single organization) and SaaS
(multiple organizations on the same installation). Every future functional
domain (E01–E20) will need tenant-scoped data without reimplementing
isolation logic each time it is added.

## Decision

- **Isolation strategy:** a single shared schema with a `TenantId` column on
  every tenant-scoped entity — not separate schemas or separate databases per
  tenant. This is the only strategy consistent with the same schema working
  unmodified in both topologies.
- **Enforcement mechanism:** a base `DanmaobTisaxDbContext` in Infrastructure
  automatically applies an EF Core global query filter (`HasQueryFilter`) to
  every entity type that implements a new `ITenantOwned` interface (defined
  in Domain). The filter is applied generically, by looping over
  `modelBuilder.Model.GetEntityTypes()` after all entities have been
  registered in the model — not by scanning assemblies with raw reflection
  before registration, and not by writing a manual `HasQueryFilter` call for
  each entity by hand. Any future functional-domain entity gets tenant
  isolation for free just by implementing `ITenantOwned`.
- **Tenant identity resolution — `ICurrentTenantProvider`:** an interface in
  Application with two modes, `SingleTenant` and `MultiTenant`:
  - `SingleTenant` (On-Premise/Private Cloud): the tenant ID is a fixed value
    read from configuration (`MultiTenancy:FixedTenantId`). Fully implemented.
  - `MultiTenant` (SaaS): the tenant ID must be resolved from the
    authenticated request. **Not implemented yet** — the Infrastructure
    implementation throws `NotSupportedException` when this mode is
    selected. No authentication exists yet (TS-00-3, Sprint 2) and no
    per-request module/tenant evaluation engine exists yet (US-20-2, Sprint
    5). Shipping any interim mechanism now — for example, trusting an
    unauthenticated HTTP header to select a tenant — would risk a tenant
    seeing another tenant's data, which must never happen under any
    circumstance. It is safer to fail loudly than to ship an insecure
    placeholder.
- **`Tenant` entity scope:** a minimal `Tenant` entity (identity fields only:
  Id, Name, IsActive, CreatedAtUtc). `Tenant` does not implement
  `ITenantOwned` — it is the tenant root itself, not tenant-owned data. Full
  administrative lifecycle (create, suspend, reactivate, delete via an API)
  is out of scope here and belongs to Jira story **US-20-1** (Sprint 4).
- **New Infrastructure subfolder:** `Infrastructure/MultiTenancy/` holds the
  `ICurrentTenantProvider` implementation and related tenant-resolution
  code, alongside the `Persistence/`, `Identity/`, `ExternalServices/`
  folders already established by ADR-0001.
- **Testing strategy:** isolation and single-topology behavior are verified
  with EF Core's `Microsoft.EntityFrameworkCore.InMemory` provider in a new
  test project, `DanmaobTisax.Infrastructure.IntegrationTests`, using a
  dedicated test-only entity (`TestOnlyNote`) and a test double for
  `ICurrentTenantProvider`. A real, authenticated, end-to-end HTTP test is
  out of scope here — it comes with TS-00-3 and US-20-2.

## Alternatives Considered

- **Separate database per tenant** — rejected: contradicts the requirement
  that On-Premise and SaaS share the same schema, and adds operational
  complexity not justified at this stage.
- **Separate schema per tenant within one database** — rejected for the same
  reason, plus it does not scale cleanly to a large number of SaaS tenants.
- **Implementing a temporary/insecure `MultiTenant` resolution now** (e.g.
  trusting a request header) to have something working end-to-end sooner —
  rejected on security grounds; see the `MultiTenant` bullet above.

## Consequences

- Any new tenant-scoped entity only needs to implement `ITenantOwned` to be
  automatically isolated — no extra wiring per entity.
- The platform cannot yet be safely operated as true multi-tenant SaaS until
  TS-00-3 and US-20-2 land. This is expected and intentional, not a gap to
  "fix" ad hoc later — this ADR must be explicitly revisited once those tasks
  exist, not silently worked around.
- As with ADR-0001, this document was authored by the engineering
  orchestrator (Claude), not by the coding agent (Cline) — Cline's prompts
  for TS-00-2 assume this ADR already exists and focus exclusively on code.

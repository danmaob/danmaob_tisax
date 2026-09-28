# ADR-0003: Secure Configuration and Per-Environment Connection Strings

**Status:** Accepted
**Date:** 2026-09-13

## Context

DanmaobTisax must run in four environments — local development, On-Premise
(single client installation), Private Cloud, and SaaS — each with its own
SQL Server connection string and, eventually, other secrets (a JWT signing
key is a known near-term example, for TS-00-3). No secret may ever be
committed to the repository or appear in project documentation.

Separately, throughout Sprint 1 the coding agent (Cline) has repeatedly
diverged from precise instructions — inventing folder structures, omitting
required files, and fabricating unrelated implementations (see the TS-00-1
and TS-00-2 execution history). Given this, any mechanism that touches real
credentials or a real database must be designed so that a misbehaving
execution of a Cline prompt cannot expose a secret or damage real data,
regardless of what the agent does within that prompt's scope.

## Decision

- **Local development:** .NET User Secrets (`dotnet user-secrets`), scoped to
  `DanmaobTisax.Api`'s `UserSecretsId`. Never committed, stored outside the
  repository by design (in the OS user profile).
- **On-Premise, Private Cloud, SaaS (deployed environments):** environment
  variables, using ASP.NET Core's standard double-underscore convention for
  nested configuration keys (e.g. `ConnectionStrings__DefaultConnection`).
  This works identically across all three deployment topologies without
  requiring a managed secret store to exist yet.
- **A managed secret store (e.g. a cloud Key Vault) for Private Cloud/SaaS is
  explicitly deferred.** No cloud provider has been selected yet; deciding
  this now would be a guess, not an architecture decision. Environment
  variables are a legitimate, secure-enough interim mechanism (the underlying
  host/orchestrator is still responsible for injecting them securely — that
  responsibility does not change if a Key Vault is introduced later, only
  where the value originates from).
- **Fail-fast validation:** the application checks that every required
  configuration key is present at startup and throws a clear
  `InvalidOperationException` naming the missing key and how to supply it —
  never logging or including the actual value — if any is missing. The
  application does not start in a half-configured state.
- **The mechanism is generic, not connection-string-specific:** a single
  `RequiredConfigurationValidator.EnsurePresent(configuration, key, guidance)`
  helper in Infrastructure is used for the connection string now, and is
  intended to be reused as-is by TS-00-3 for the JWT signing key and by any
  future technical task with a similar requirement.
- **Operational boundary — who is allowed to touch what:**
  - Cline (the coding agent) builds and modifies the *mechanism* only:
    initializing User Secrets, removing placeholder values from committed
    files, writing the fail-fast validator, and writing tests that use
    dummy/nonexistent connection strings.
  - Cline never runs a command that sets a real secret value, and never runs
    a command that connects to, creates, modifies, or deletes a real
    database (`dotnet ef database update`, `dotnet ef database drop`, or
    equivalent) — under any circumstance, in any prompt, in this project.
  - The human operator performs both of those categories of action directly,
    outside of any Cline prompt.
  - This boundary is permanent for this project, not specific to TS-00-9 —
    it applies to every future technical task that touches secrets or a real
    database.

## Alternatives Considered

- **A cloud Key Vault (Azure Key Vault or equivalent) for every environment
  now** — rejected: no cloud provider has been selected; building against a
  specific vendor's SDK today would very likely need to be redone once that
  decision is made, and would not even help On-Premise installations, which
  by definition run outside any cloud.
- **Letting Cline set real secrets and apply migrations directly, with
  strict prompt wording as the only safeguard** — rejected. Prompt wording
  alone did not reliably constrain scope during TS-00-1/TS-00-2; a real
  credential or a real database is not something to protect with instructions
  alone when a structural boundary (simply never asking the agent to do it)
  is available instead.

## Consequences

- A developer cloning the repository for the first time has no working
  connection string until they explicitly configure one locally — this is
  intentional (AC1) and is guided by a clear fail-fast error message, not a
  silent failure.
- Deployed environments are configured purely through environment variables
  until a managed secret store is introduced by a future ADR; that future
  ADR only needs to change *where the environment variable's value comes
  from* (e.g., injected by a vault-integrated deployment pipeline), not the
  application code itself.
- Every technical task going forward that involves a real secret or a real
  database operation must split its Cline-facing prompts from the
  human-operator steps the same way TS-00-9 does — this ADR is the reference
  to point to instead of re-deriving the split each time.

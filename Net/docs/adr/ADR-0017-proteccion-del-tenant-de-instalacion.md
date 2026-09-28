# ADR-0017 — Protección del tenant de instalación

**Estado:** Aceptada
**Fecha:** 2026-09-26
**Historia relacionada:** US-20-13 — Protección del tenant de instalación (Sprint 6, historia nueva autorizada por Luis)
**Complementa:** ADR-0008 (ciclo de vida de tenants) y ADR-0014 (administradores de plataforma)

## Contexto

En On-Premise (`MultiTenancy:Mode = SingleTenant`), todos los usuarios de la instalación pertenecen al tenant fijo `MultiTenancy:FixedTenantId`. La baja de un tenant es definitiva: `Tenant.Reactivate` solo acepta tenants suspendidos. Nada impedía dar de baja ese tenant, y hacerlo dejaría fuera, para siempre, a todos los usuarios de la instalación; la única salida sería por SQL.

## Decisiones

### D1 — Regla de negocio

En modo `SingleTenant`, `TenantAdministrationService.DeactivateAsync` rechaza la baja cuando el Id solicitado es el tenant fijo de la instalación. Responde 409 con `errorCode` `"Tenant.InstallationTenantCannotBeDeactivated"`, con mensaje en español e inglés.

### D2 — La suspensión sigue permitida

Suspender es reversible, y desde ADR-0014 el Super Administrador no pertenece a ningún tenant: puede reactivarlo aunque el tenant de instalación esté suspendido.

### D3 — Alcance

La regla solo aplica en `SingleTenant`. El modo `MultiTenant` no tiene tenant de instalación, y sigue sin implementarse (ADR-0002).

## Consecuencias

- Los demás tenants se dan de baja igual que antes.
- La regla vive en el backend: aplica aunque una interfaz futura omita su propia validación. Esto va en línea con el AC de US-20-8.

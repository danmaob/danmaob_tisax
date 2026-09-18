# ADR-0004: Framework transversal de auditoría vía interceptor de EF Core

**Status:** Accepted
**Date:** 2026-09-14

## Context

TS-00-4 requiere que toda entidad relevante del dominio registre
quién/qué/cuándo/antes-después de cada cambio (RNF-AUDIT-01,
RNF-AUDIT-02, RF-T-01), protegido contra alteración no autorizada
(RNF-SEC-05), sin que cada dominio funcional construya su propio
mecanismo. Debe existir antes de TS-00-3 para que las entidades de RBAC
(`User`, `Role`, etc.) nazcan auditables desde su creación.

## Decision

1. **Orden de ejecución del sprint:** TS-00-4 se ejecuta antes que
   TS-00-3.
2. **Captura vía un único `SaveChangesInterceptor`** de EF Core
   (`AuditSaveChangesInterceptor`), que genera filas en una tabla
   `AuditLogs` centralizada para toda entidad que implemente la
   interfaz marcadora `IAuditable`.
3. **Opt-in explícito:** `IAuditable` no se hereda automáticamente de
   `BaseEntity` — cada entidad la implementa deliberadamente.
4. **Solo las propiedades que cambiaron** se serializan a JSON en
   `OldValuesJson`/`NewValuesJson`; las propiedades de navegación nunca
   se serializan.
5. **Atomicidad:** las filas de auditoría se agregan al mismo
   `SaveChanges` que el cambio de negocio que describen.
6. **Inmutabilidad solo a nivel aplicación en este sprint:** el mismo
   interceptor rechaza (`AuditLogImmutableException`) cualquier
   `Modified`/`Deleted` sobre `AuditLog` vía el `DbContext`. No protege
   contra una modificación SQL directa fuera de la aplicación —
   endurecimiento adicional (trigger o permisos de motor SQL Server)
   queda diferido a TS-00-7, para no mezclar dos tareas técnicas
   distintas.
7. **`ICurrentUserService` desacoplado de TS-00-3:** interfaz en
   Application, implementación stub `SystemCurrentUserService` en
   Infrastructure (siempre reporta "system"). TS-00-3 reemplaza el
   registro de DI por una implementación respaldada por el contexto
   HTTP/JWT, sin tocar el interceptor.
8. **`AuditLog` no implementa `ITenantOwned`:** esa interfaz (ya
   existente desde TS-00-2) exige `TenantId` no-nullable; `AuditLog`
   necesita `TenantId` nullable para acciones de plataforma sin tenant,
   así que gestiona su propio filtrado de tenant manualmente en
   `AuditLogQueryService`, no vía el filtro global automático.
9. **Configuración EF inline, no `IEntityTypeConfiguration<T>`
   separada:** siguiendo el patrón ya establecido en
   `DanmaobTisaxDbContext` para `Tenant`.
10. **Endpoint REST de consulta diferido a TS-00-3:** exponer
    `GET /api/v1/audit-logs` sin autorización real violaría
    RNF-SEC-01/RNF-SEC-05. Se construye como parte de TS-00-3, una vez
    que existe un permiso real (`Audit.View`) para protegerlo.

## Alternatives Considered

- **Auditoría por dominio (una tabla por módulo):** descartada, duplica
  el mecanismo veinte veces.
- **Triggers de base de datos como único mecanismo:** descartado como
  único mecanismo por acoplar la lógica al motor SQL Server concreto y
  dificultar pruebas con InMemory; queda como defensa adicional futura.
- **Librerías de auditoría de terceros (ej. Audit.NET):** descartadas
  para no atarse a un esquema de tabla ajeno cuando el mecanismo nativo
  de EF Core (interceptor + tabla propia) ya cubre el requisito.

## Consequences

- Toda entidad futura que requiera trazabilidad solo implementa
  `IAuditable`, sin código adicional en su propio módulo.
- Deuda técnica explícita: endurecimiento de inmutabilidad a nivel de
  base de datos se revisa en TS-00-7, no en este sprint.

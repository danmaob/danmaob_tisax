# ADR-0015 — Exclusión de datos sensibles del registro de auditoría

**Estado:** Aceptada
**Fecha:** 2026-09-25
**Historia relacionada:** US-20-14 — Exclusión de datos sensibles del registro de auditoría (Sprint 6, historia nueva autorizada por Luis)
**Requisitos relacionados:** RNF-SEC-03, RNF-SEC-07, RNF-AUDIT-01
**Complementa:** ADR-0004 (framework de auditoría transversal)

## Contexto

`AuditValueSerializer` guarda en `AuditLogs` todas las propiedades de cada entidad `IAuditable`. `User.PasswordHash` quedaba en `NewValuesJson` al crear un usuario, y en `OldValuesJson`/`NewValuesJson` al cambiar su contraseña. Cualquiera con el permiso `Audit.Read` podía leerlo en `GET /api/v1/audit-logs`. Con ADR-0014 pasaba lo mismo con `PlatformAdministrator.PasswordHash`.

## Decisiones

### D1 — Marca declarativa en el dominio

El atributo `AuditRedactedAttribute` (`DanmaobTisax.Domain.Auditing`) declara que el valor de una propiedad es sensible. La decisión vive junto a la propiedad, no en una lista de nombres dentro del serializador. Se aplica hoy a `User.PasswordHash` y `PlatformAdministrator.PasswordHash`.

### D2 — Enmascarar, no omitir

El serializador escribe el texto `"[REDACTED]"` en lugar del valor, en altas, bajas y cambios. El nombre de la propiedad sigue apareciendo, y `ChangedColumnsJson` sigue listándola. Así se conserva lo que pide RNF-AUDIT-01: se sabe que la contraseña cambió, quién la cambió y cuándo, sin exponer el valor.

### D3 — Una propiedad sensible nueva solo necesita la marca

Cualquier propiedad futura con credenciales, secretos o tokens se protege con `[AuditRedacted]`, sin tocar el serializador.

## Fuera de alcance

Los registros de auditoría ya existentes no se modifican: `AuditLogs` es inmutable por diseño (ADR-0004, RNF-SEC-05). El producto no está instalado en ningún cliente, así que solo la base de desarrollo contiene hashes guardados. Entre ellos está el alta del Super Administrador creado al cerrar US-20-6.

## Consecuencias

- Desde esta historia, ninguna consulta ni exportación de auditoría (incluida la de US-20-7) puede devolver un hash de contraseña.
- Una propiedad sin `PropertyInfo` (propiedad sombra de EF) nunca se enmascara; hoy no existe ninguna sensible de ese tipo.

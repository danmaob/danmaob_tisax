# ADR-0018 — Auditoría de acciones administrativas de plataforma

**Estado:** Aceptada
**Fecha:** 2026-09-26
**Historia relacionada:** US-20-7 — Auditoría de acciones administrativas de plataforma (Sprint 6)
**Requisitos relacionados:** RF-D20-17, RF-D20-29
**Complementa:** ADR-0004 (framework de auditoría transversal), ADR-0014 (administradores de plataforma), ADR-0015 (datos sensibles en auditoría)

## Contexto

`AuditLogs` ya registra automáticamente cada cambio de una entidad `IAuditable`. Faltaba poder consultarlo como auditoría **de plataforma**:

1. **La consulta existente no sirve.** `GET /api/v1/audit-logs` filtra por el tenant actual, y los cambios de `Tenant`, `Plan` y `PlanModule` se auditan sin tenant.
2. **El tenant afectado se guarda de dos maneras.** Un `Tenant` se audita sin `TenantId`, y su Id va en `EntityId`. Un `TenantModule` se audita con el `TenantId` del tenant afectado. Filtrar por "tenant vacío" dejaría fuera las excepciones de módulo.
3. **"Quién" solo guardaba un Id.** `PerformedByDisplayName` siempre quedaba vacío, porque se leía un claim que los tokens no traen.
4. **No existe una "configuración global"** fuera de los planes y la matriz de módulos.

## Decisiones

### D1 — Qué es auditoría de plataforma

Los registros de las entidades `Tenant`, `TenantModule`, `Plan`, `PlanModule` y `PlatformAdministrator` (`PlatformAuditEntities.Names`). Ninguna entidad propia de un tenant, como `User` o `Role`, aparece en ella.

### D2 — Tenant afectado derivado

Cada registro devuelve `AffectedTenantId`:
- para `Tenant`, el `EntityId`;
- para `TenantModule`, el `TenantId`;
- para planes y administradores, vacío.

El filtro `tenantId` aplica esas mismas dos reglas.

### D3 — Consulta

`GET /api/v1/platform/audit`, con filtros `tenantId`, `entityName`, `action`, `performedByUserId`, `fromUtc` y `toUtc`, más paginación `pageNumber` y `pageSize` (máximo 200). Ordena del más reciente al más antiguo.

### D4 — Exportación

`GET /api/v1/platform/audit/export`, con los mismos filtros y sin paginación; entrega como máximo 10 000 registros. El archivo es `platform-audit.csv`:
- en UTF-8 con BOM, para que Excel respete los acentos;
- todos los campos entre comillas y con las comillas internas duplicadas;
- los valores que empiezan con `=`, `+`, `-` o `@` se anteponen con `'`, para evitar inyección de fórmulas.

Excel y PDF quedan para la épica de reportes.

### D5 — Permiso

Nuevo `Platform.ReadAudit`. Por ADR-0014, solo funciona con token de plataforma; el Super Administrador lo recibe automáticamente al iniciar sesión.

### D6 — "Quién"

`CurrentUserService.DisplayName` lee el claim `email`, presente en los tokens de tenant y de plataforma. Aplica a toda la auditoría a partir de esta historia; los registros anteriores conservan su valor vacío.

## Consecuencias

- Los datos sensibles ya van enmascarados (ADR-0015): la consulta y el CSV nunca exponen hashes de contraseña.
- Cada inicio de sesión del Super Administrador genera un registro `Updated` de `PlatformAdministrator` (fecha de último acceso y contador de intentos), visible en esta auditoría.

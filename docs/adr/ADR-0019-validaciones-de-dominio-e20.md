# ADR-0019 — Validaciones de integridad de dominio en las entidades de E20

**Estado:** Aceptada
**Fecha:** 2026-09-26
**Historia relacionada:** US-20-8 — Validaciones de integridad y consistencia de datos a nivel de dominio (Sprint 6)
**Requisitos relacionados:** RF-D20-07

## Contexto

AC1 de US-20-8: una solicitud que viola una regla de negocio debe ser rechazada por el backend aunque se omita la validación de interfaz. Por decisión de Luis, el alcance se acotó a las entidades de E20: `Tenant`, `TenantModule`, `Plan`, `PlanModule`, `FunctionalModule` y `PlatformAdministrator`. `User` y `Role` quedan para E16.

Problemas encontrados en el código:

1. **Propiedades con `set` público** permitían saltarse las reglas:
   - `Tenant.Name` y `Tenant.CreatedAtUtc`;
   - `Plan.Code`, `Plan.Name` y `Plan.IsActive`. Con `IsActive`, `plan.IsActive = false` desactivaba el plan Free sin pasar por `Plan.Deactivate()`, que lo impide.
   - `FunctionalModule.Code` y `FunctionalModule.SortOrder`.
2. **El límite de 200 caracteres del nombre del tenant** solo existía en el controller y en la base; la entidad no lo validaba.
3. **Error 500 en lugar de rechazo.** Si una entidad rechazaba datos con `ArgumentException` y la excepción llegaba a la API, la respuesta era 500 "error inesperado", no un rechazo claro.

## Decisiones

### D1 — Estado solo a través de la entidad

Todas las propiedades de las entidades de E20 tienen `set` privado. La única excepción es `TenantModule.TenantId`, porque la interfaz `ITenantOwned` exige un `set` público. Una prueba de arquitectura (`E20Entities_HaveNoPublicSetters`) lo vigila en adelante.

### D2 — Reglas dentro de la entidad

El constructor de `Tenant` exige ahora un nombre de 200 caracteres como máximo. Las demás reglas de E20 ya vivían en sus entidades:
- obligatorios y longitudes de `Plan`, `PlanModule`, `TenantModule`, `FunctionalModule` y `PlatformAdministrator`;
- transiciones de estado de `Tenant` y `Plan`.

### D3 — Rechazo uniforme

`GlobalExceptionHandler` responde 400 con `errorCode` `"Domain.ValidationFailed"` cuando la excepción es un `ArgumentException` lanzado desde el ensamblado Domain. El mensaje de la entidad, genérico y seguro, va en `Detail`. Cualquier otra excepción conserva el 500 con mensaje saneado; nunca se expone el detalle interno.

## Consecuencias

- Sin cambios de esquema: EF Core escribe `set` privados, y el sembrado con `HasData` sigue funcionando. Se comprueba con `dotnet ef migrations has-pending-model-changes`.
- Las validaciones de interfaz (atributos de los DTO) siguen siendo la primera línea. El dominio es la segunda, y el manejador garantiza que su rechazo llegue como 400.

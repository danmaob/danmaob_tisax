# ADR-0016 — Excepciones de módulo por tenant: tres estados y administración

**Estado:** Aceptada
**Fecha:** 2026-09-25
**Historia relacionada:** US-20-5 — Habilitación/deshabilitación de módulo específico por tenant (Sprint 6)
**Requisitos relacionados:** RF-D20-21, RF-D20-22
**Modifica:** ADR-0013, decisión D1 (significado de una fila de `TenantModule`)

## Contexto

ADR-0013 dejó construida la regla "la excepción manda": si existe una fila `TenantModule` para el tenant y el módulo, esa fila decide sola; si no, decide la matriz del plan. Faltaban tres cosas:
- la administración de las excepciones (endpoints);
- la validación de los códigos contra el catálogo de módulos;
- una forma de **quitar** una excepción. Con `IsEnabled` booleano, una fila deshabilitada significa "denegado", no "sin excepción", y la política del producto es de bajas lógicas: las filas no se borran.

## Decisiones

### D1 — Tres estados en `TenantModule.IsEnabled`

- `true`: excepción que habilita.
- `false`: excepción que deshabilita.
- `null`: sin excepción; decide el plan.

La fila nunca se borra: `ClearException()` la deja en `null`. `ModuleAccessEvaluator` no cambia: ya trataba "no hay fila" y "valor vacío" igual, y en ambos casos consulta el plan.

### D2 — El código de módulo debe existir en el catálogo

- Nueva llave foránea de `TenantModules.ModuleCode` a `FunctionalModules.Code`, igual que la de `PlanModules`, con borrado restringido.
- El servicio valida el código antes de guardar y responde 400 `"Tenant.UnknownModuleCode"`. La validación no depende de la base: las pruebas usan base en memoria, que no aplica llaves foráneas.

### D3 — Administración

Ambos endpoints requieren `Platform.ManageTenants`, así que solo sirven con token de plataforma (ADR-0014).

- **`GET /api/v1/platform/tenants/{tenantId}/modules`:** devuelve los módulos del catálogo, en su orden. Para cada uno: lo que dice el plan (`enabledByPlan`), la excepción (`exceptionIsEnabled`: `true`, `false` o `null`) y el resultado efectivo (`isEnabled`). Responde 404 si el tenant no existe.
- **`PUT /api/v1/platform/tenants/{tenantId}/modules/{moduleCode}`:** recibe el cuerpo `{ "state": "Enabled" | "Disabled" | "Inherit" }` y devuelve el estado resultante de ese módulo. Errores:
  - 400 `"Tenant.InvalidModuleExceptionState"`;
  - 404 si el tenant no existe;
  - 400 `"Tenant.UnknownModuleCode"`.

  `Inherit` sobre un módulo sin fila no crea nada.

### D4 — Auditoría

`TenantModule` ya es `IAuditable` y `ITenantOwned`: cada alta o cambio de excepción queda en `AuditLogs` con el tenant afectado, sin código adicional. US-20-7 lo consultará desde la auditoría de plataforma.

## Consecuencias

- Una excepción puede ponerse a cualquier tenant existente, sin importar su estado. Mientras el tenant no esté activo, `TenantStatusMiddleware` sigue bloqueando el acceso.
- Antes de aplicar la migración en una base existente, hay que confirmar que ninguna fila de `TenantModules` tenga un código fuera del catálogo; si la hay, la llave foránea no se puede crear.

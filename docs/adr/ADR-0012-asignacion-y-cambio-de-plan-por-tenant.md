# ADR-0012 — Asignación y cambio de plan por tenant

**Estado:** Aceptada
**Fecha:** 2026-09-23
**Historia relacionada:** US-20-4 — Asignación y cambio de plan por tenant preservando integridad de datos (Sprint 5)
**Requisitos relacionados:** RF-D20-12, RF-D20-13

## Contexto

Cada tenant necesita un plan, y cambiarlo (upgrade o downgrade) no debe borrar datos, solo restringir o ampliar el acceso funcional.

## Decisiones

### D1 — Todo tenant tiene plan; el predeterminado es Free

`Tenant.PlanId` es obligatorio. Una empresa nueva recibe Free automáticamente (decisión de Luis): el valor inicial de la propiedad en el dominio es `PlanCatalog.FreePlanId`, así que el alta de tenants (US-20-1) no cambia. La columna en base de datos tiene como valor por defecto el identificador de Free, de modo que los tenants que ya existan en una base real quedan en Free al aplicar la migración `AddTenantPlan`. Hay clave foránea a `Plans` con borrado restringido.

### D2 — Cambio de plan

`PUT /platform/tenants/{id}/plan` con `{ planId }`, protegido por `Platform.ManageTenants`. Resultados:
- Tenant inexistente → 404.
- Plan inexistente o identificador vacío → 400 `Tenant.PlanNotFound`.
- Plan inactivo → 409 `Tenant.PlanInactive`.
- Éxito → 200 con el tenant, que ahora incluye `planId`.

El cambio se permite con cualquier estado del tenant; el estado sigue controlando el acceso por su cuenta (ADR-0010).

### D3 — Un cambio de plan nunca toca datos

Cambiar de plan solo actualiza `Tenant.PlanId`. No se borran ni modifican datos de módulos ni las excepciones por tenant (`TenantModule`). El efecto sobre el acceso lo aplica el evaluador en la siguiente solicitud (ADR-0013). El cambio queda en la auditoría del tenant, porque `Tenant` ya es auditable (columna `PlanId` en las columnas modificadas).

## Consecuencias

- Migración `AddTenantPlan` (columna, índice y clave foránea).
- Hoy no existen módulos de negocio con datos propios, así que "el downgrade conserva los datos" se verifica sobre el propio tenant y sus excepciones. Cada historia de dominio futura hereda esta regla: ningún cambio de plan elimina datos.

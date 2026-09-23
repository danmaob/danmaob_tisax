# ADR-0011 — Planes comerciales configurables y catálogo de módulos funcionales

**Estado:** Aceptada
**Fecha:** 2026-09-23
**Historia relacionada:** US-20-3 — Motor de planes comerciales configurables y matriz módulo-plan (Sprint 5)
**Requisitos relacionados:** RF-D20-19, RF-D20-20, RF-D20-28

## Contexto

US-20-3 pide que los planes (Free/Básico/Premium) sean entidades configurables y que cada uno de los 20 dominios funcionales pueda asociarse a uno o más planes, sin valores fijos en el código. Hasta Sprint 4 no existía un catálogo de módulos: los códigos de `TenantModule` eran texto libre (ADR-0009, D2). La composición real de cada plan es una decisión de negocio pendiente; esta historia entrega la capacidad de configurarla, no la matriz final.

## Decisiones

### D1 — Catálogo de módulos definido por el producto

Los 20 dominios se definen en código (`FunctionalModuleCodes`, en `Domain/Plans`) y se persisten en la tabla `FunctionalModules`, sembrada por migración (`HasData`) con identificadores fijos. El cliente no puede crear ni editar módulos: los módulos son del producto. Códigos aprobados por Luis, en el orden de los dominios 3.1 a 3.20: Organization, Scope, Catalog, GapAnalysis, Risk, Documents, Evidence, Capa, Audits, Readiness, Dashboard, Reports, ThirdParties, Assets, PhysicalPrototype, UsersAccess, Notifications, LabelLifecycle, KnowledgeBase, Administration.

Cualquier controller de negocio futuro protege su módulo con `[RequireModule(FunctionalModuleCodes.X)]`, nunca con un texto escrito a mano.

### D2 — Planes como entidades, con tres planes sembrados

La entidad `Plan` (`Code`, `Name`, `IsActive`) es auditable y no pertenece a ningún tenant. Se siembran FREE, BASIC y PREMIUM con identificadores fijos (`PlanCatalog`). El Super Administrador puede crear planes nuevos, renombrarlos, desactivarlos y reactivarlos. El código se normaliza a mayúsculas y es único; no se puede cambiar después de crearlo.

Reglas de desactivación:
- El plan Free (plan predeterminado de toda empresa nueva) no se puede desactivar.
- Un plan asignado a al menos un tenant no dado de baja no se puede desactivar.
- Un plan inactivo no se puede asignar a un tenant (ADR-0012).

No hay borrado de planes: la baja es lógica (política del proyecto).

### D3 — La matriz módulo-plan nunca borra filas

Cada celda de la matriz es una fila `PlanModule` (`PlanId`, `ModuleCode`, `IsEnabled`), auditable, con índice único por plan y módulo y clave foránea al catálogo por código. La API recibe la lista completa de módulos habilitados de un plan (`PUT /platform/plans/{id}/modules`) y el servicio habilita o deshabilita filas; nunca las elimina. Si la lista trae un código inexistente se rechaza toda la operación sin guardar nada.

La matriz arranca vacía: los tres planes sembrados no habilitan ningún módulo hasta que Luis defina la composición comercial.

### D4 — Permiso propio

Se agrega el permiso `Platform.ManagePlans` para planes y matriz. La asignación de plan a un tenant queda bajo `Platform.ManageTenants` (ADR-0012).

### D5 — Fuera de alcance

Cobros, suscripciones y facturación (RF-D20-28). El sistema solo refleja el estado del plan y sus efectos sobre los módulos habilitados. Tampoco hay interfaz de usuario (US-20-10, Sprint 7) ni textos traducidos de nombres de módulo; eso llega con el frontend.

## Consecuencias

- Tres tablas nuevas (`FunctionalModules`, `Plans`, `PlanModules`) en la migración `AddPlansAndModuleCatalog`, con los datos semilla.
- Agregar un módulo nuevo al producto requiere código y migración; es intencional.
- Endpoints: `GET /platform/plans/module-catalog`, `GET /platform/plans`, `GET /platform/plans/{id}`, `POST /platform/plans`, `PUT /platform/plans/{id}/name`, `POST /platform/plans/{id}/deactivate`, `POST /platform/plans/{id}/reactivate`, `PUT /platform/plans/{id}/modules`.

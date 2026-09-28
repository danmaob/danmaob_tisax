# ADR-0009 — Evaluación de módulos habilitados en cada solicitud

**Estado:** Aceptada. La decisión D2 fue sustituida por ADR-0013 (Sprint 5)
**Fecha:** 2026-09-19
**Historia relacionada:** US-20-2 — Aislamiento entre tenants y evaluación en tiempo real de módulos habilitados (Sprint 4)
**Requisitos relacionados:** RF-D20-10, RF-D20-22, RF-D20-23, RNF-MT-02

## Contexto

Los criterios de aceptación de US-20-2 exigen que la API bloquee el acceso a un módulo no habilitado (no solo ocultarlo en la interfaz) y que deshabilitarlo tenga efecto en la solicitud inmediata siguiente, sin caché. Los criterios no indican de dónde sale "habilitado". Los planes (US-20-3, US-20-4) y la habilitación por tenant (US-20-5) están en Sprints 5 y 6, y no existe un catálogo de módulos ni endpoints de negocio protegidos por módulo.

## Decisión

### D1 — El tenant de la solicitud sale del claim firmado `tenant`

La evaluación usa el claim `tenant` del JWT (firmado por el servidor; el cliente no puede elegirlo), no `ICurrentTenantProvider`, para no depender de cómo se resuelva el tenant en cada modo de despliegue. Nunca se confía en un header ni en datos no autenticados.

### D2 — SUPUESTO por confirmar: una fila `TenantModule` es la decisión de acceso explícita

> **Sustituida por ADR-0013 (2026-09-23).** Una fila `TenantModule` sigue decidiendo sola cuando existe, pero ahora es una excepción: sin fila decide el plan del tenant. Además, la migración de `TenantModules` que esta sección daba por hecha nunca se generó; se corrige en Sprint 5.

Se agrega la entidad `TenantModule` (`TenantId`, `ModuleCode`, `IsEnabled`). **Sin fila, el acceso se deniega.** Es `ITenantOwned` (el filtro global la protege por defecto; el evaluador usa `IgnoreQueryFilters()` con un predicado explícito por tenant), es `IAuditable` y no tiene clave foránea a `Tenants` porque el tenant de una instalación `SingleTenant` no tiene fila allí. Los códigos de módulo son texto libre con la misma grafía; el catálogo real llega con US-20-3. Es el mínimo necesario para poder verificar los dos criterios; US-20-3, US-20-4 y US-20-5 deberán conciliar cómo se combinan plan y excepciones con esta tabla. **Si Luis define otra fuente de datos para "habilitado", cambian los prompts 01 a 03 y 05.**

### D3 — Evaluación en cada solicitud, sin caché

`[RequireModule("código")]` establece la política `Module:<código>`; `PermissionPolicyProvider` la construye con un `ModuleRequirement`; `ModuleAuthorizationHandler` consulta la base de datos mediante `IModuleAccessEvaluator` en cada solicitud. No hay caché en ninguna capa (RNF-MT-02). Sin token: 401; con token pero sin acceso al módulo: 403.

### D4 — Pruebas con un controlador sonda

Como aún no hay endpoints protegidos por módulo, las pruebas registran, solo en su fábrica de pruebas mediante `AddApplicationPart`, un controlador del ensamblado de pruebas con una ruta protegida por módulo. No existe en producción.

## Fuera de alcance de US-20-2

- **Aplicar el estado del tenant (suspendido / dado de baja) al acceso.** No lo piden los criterios de US-20-2; se resuelve en US-20-11 (ADR-0010).
- Resolución `MultiTenant` de `ICurrentTenantProvider` y login que identifique el tenant en SaaS: decisión abierta; ADR-0002 sigue vigente.
- Planes y matriz módulo-plan (US-20-3), cambio de plan (US-20-4), API para gestionar las filas de `TenantModule` (US-20-5).
- Mensaje amigable al usuario final ante módulo no habilitado (RF-D20-27, prioridad S): hoy es el 403 estándar.
- Catálogo de módulos y uso de `[RequireModule]` en controllers de negocio.

## Consecuencias

- Cualquier controller de negocio futuro protege su módulo con `[RequireModule]` sin más código, y un cambio de habilitación es inmediato.
- Si el endpoint está protegido por módulo, cada solicitud agrega una consulta a la base de datos.
- Cambia el esquema: nueva tabla `TenantModules` con índice único (`TenantId`, `ModuleCode`). Requiere una migración nueva.

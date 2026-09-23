# ADR-0013 — Aplicación del plan en la evaluación de acceso a módulos

**Estado:** Aceptada
**Fecha:** 2026-09-23
**Historia relacionada:** US-20-12 — Aplicación del plan del tenant en la evaluación de acceso a módulos (Sprint 5, historia nueva autorizada por Luis)
**Requisitos relacionados:** RF-D20-13, RF-D20-22, RF-D20-23, RNF-MT-02
**Sustituye:** ADR-0009, decisión D2

## Contexto

Al revisar el código de cierre de Sprint 4 se confirmaron tres hechos:

1. `ModuleAccessEvaluator` decidía el acceso solo con filas de `TenantModule`. Ningún código consideraba el plan, así que cambiar de plan no habría cambiado el acceso y el AC2 de US-20-4 no podía cumplirse.
2. La tabla `TenantModules` nunca tuvo migración: no estaba en ninguna migración ni en el snapshot, y no existía en la base de datos de desarrollo (verificado por Luis con `sys.tables`). Las pruebas pasaban porque usan el proveedor en memoria.
3. En modo SingleTenant (On-Premise) el tenant de la instalación no tenía fila en `Tenants`: `AdminBootstrapper` solo creaba el usuario y el rol. Sin fila no hay dónde guardar su plan.

## Decisiones

### D1 — Regla de acceso: excepción, luego plan

Para un tenant y un módulo, en cada solicitud y sin caché:
1. Si existe una fila `TenantModule` para ese tenant y módulo, esa fila decide sola (habilitado o deshabilitado). Es una excepción comercial explícita; su administración llega con US-20-5 (Sprint 6).
2. Si no hay excepción, se busca el plan del tenant. Si el tenant no existe, se deniega.
3. Se concede solo si la matriz del plan tiene ese módulo habilitado.

Consecuencia: un upgrade o downgrade surte efecto en la siguiente solicitud, y las excepciones sobreviven a los cambios de plan. `ModuleAuthorizationHandler` y `[RequireModule]` no cambian.

Esto reemplaza la D2 de ADR-0009 ("sin fila, se deniega"): ahora, sin fila, decide el plan.

### D2 — Migración faltante como primera tarea del sprint

La migración `AddTenantModules` se genera antes de cualquier tabla de planes, para que cada migración corresponda a una sola historia.

### D3 — El tenant de la instalación existe como fila

`--bootstrap-admin` crea, si falta, la fila en `Tenants` con `Id = BootstrapAdmin:TenantId` y el plan Free. El nombre sale de la clave opcional `BootstrapAdmin:TenantName` (por defecto "Default Organization"). Se ejecuta antes de verificar si el administrador existe, así que correr el bootstrap otra vez en una instalación existente crea la fila que falte sin duplicar nada. En On-Premise `BootstrapAdmin:TenantId` debe ser igual a `MultiTenancy:FixedTenantId`; así la instalación puede cambiar de plan como cualquier otra empresa.

## Fuera de alcance

- Endpoints para administrar excepciones `TenantModule` (US-20-5).
- Validar que los códigos de `TenantModule` existan en el catálogo (US-20-5).
- El uso de `EnsureCreatedAsync()` en `Program.cs` junto con migraciones: en una base existente no hace nada, pero en una base vacía crea el esquema sin historial de migraciones y rompe `database update`. Queda señalado para una decisión futura de Luis; no se toca en este sprint.

## Consecuencias

- Cada solicitud a un endpoint protegido por módulo hace hasta tres consultas pequeñas (excepción, plan del tenant, matriz).
- Las pruebas de US-20-2 siguen válidas: sus tenants de prueba no tienen fila, así que sin excepción se deniega.

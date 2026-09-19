# ADR-0008 — Ciclo de vida de tenants: baja lógica, estados y API de plataforma

**Estado:** Aceptada
**Fecha:** 2026-09-18
**Historia relacionada:** US-20-1 — API de alta, suspensión, reactivación y baja de tenants (Sprint 4)
**Requisitos relacionados:** RF-D20-08, RF-D20-09, RF-D20-17, RF-D20-29
**Relación con otros ADRs:** reemplaza parcialmente la sección "`Tenant` entity scope" de ADR-0002 (que fijaba `Id, Name, IsActive, CreatedAtUtc` y delegaba el ciclo de vida a US-20-1). Se apoya en ADR-0004 (decisión 8: `AuditLog.TenantId` nullable para acciones de plataforma).

## Contexto

ADR-0002 dejó una entidad `Tenant` mínima con un booleano `IsActive` y difirió su administración a US-20-1. Esta historia necesita tres cosas: dar de alta un tenant, cambiar su estado comercial (suspender, reactivar, dar de baja) y dejar constancia auditable de quién y cuándo. Un booleano no distingue "suspendido" (temporal, reversible) de "dado de baja" (cierre de la relación comercial), y un borrado físico destruiría el historial que AC2 exige preservar.

## Decisión

### D1 — Principio de proyecto: bajas lógicas, no físicas

Decisión de Luis (Product Owner): en la medida de lo posible, ninguna entidad de negocio se elimina físicamente; se modela por estados. Aplica desde esta historia a `Tenant` y debe aplicarse a las historias futuras salvo justificación explícita. (Las entidades de relación como `UserRole`/`RolePermission` ya existentes no se reclasifican aquí.)

### D2 — `TenantStatus` reemplaza a `IsActive`

`TenantStatus` es un enum con valores numéricos explícitos: `Active = 1`, `Suspended = 2`, `Deactivated = 3`. El valor 0 queda intencionalmente sin definir para que un valor sin inicializar nunca se interprete como un estado válido. Se persiste como texto (`HasConversion<string>()`, longitud 20), igual que `AuditLog.Action`, con valor por defecto de base de datos `Active` para que las filas existentes queden activas al migrar. `IsActive` se conserva únicamente como propiedad calculada de solo lectura (`Status == Active`), ignorada explícitamente por EF, para no romper lecturas existentes.

### D3 — Reglas de transición

| Estado actual | Suspender | Reactivar | Dar de baja |
|---|---|---|---|
| Activo | → Suspendido | no permitido | → Dado de baja |
| Suspendido | no permitido | → Activo | → Dado de baja |
| Dado de baja | no permitido | no permitido | no permitido |

**Supuesto aún no confirmado por Luis:** la baja es terminal a nivel API (no existe transición desde "Dado de baja"). Es reversible cambiando una sola regla en `Tenant.Reactivate()` y sus pruebas, sin migración de datos.

### D4 — Transiciones en el dominio, resultados en el servicio

Las transiciones son métodos de la entidad (`Suspend`, `Reactivate`, `Deactivate`) que lanzan `InvalidTenantStatusTransitionException` (tipo específico, hereda de `InvalidOperationException`) ante una transición prohibida. `Status` tiene setter privado: el estado solo cambia por esos métodos. El servicio de aplicación captura exclusivamente esa excepción y devuelve un `TenantOperationResult`; los desenlaces esperados de negocio (no encontrado, nombre duplicado, transición inválida) nunca se propagan como excepciones. `GlobalExceptionHandler` queda reservado para fallos inesperados.

### D5 — Auditoría de plataforma reutilizando TS-00-4

`Tenant` implementa `IAuditable`; el interceptor existente registra `Created`/`Updated` sin código adicional. `Tenant` no es `ITenantOwned`, por lo que `AuditTenantResolver` produce `TenantId = null`: esos eventos son de plataforma, coherente con ADR-0004 (decisión 8). La consulta se expone en un endpoint propio (`GET /api/v1/platform/tenants/{id}/audit`) en lugar de reutilizar `AuditLogQueryService`, porque ese servicio gestiona su propio filtrado por tenant (ADR-0004, decisión 8) y no se modifica en esta historia. No se verificó cómo trata las filas con `TenantId` nulo; se evita depender de ello.

### D6 — Autorización: permiso `Platform.ManageTenants`

Se agrega el permiso `Platform.ManageTenants` al catálogo, distinto de `Tenant.ManageSettings`. Los endpoints viven bajo `/api/v1/platform/tenants` y usan `[RequirePermission]`, como `AuditLogController`.

**Limitación explícita (hasta US-20-6):** todavía no existen usuarios Super Administrador independientes de tenant. `AdminBootstrapper` asigna al rol `Administrator` de su tenant TODOS los permisos del catálogo, incluido `Platform.ManageTenants`. En una instalación de un solo tenant (On-Premise) el administrador de instalación es de hecho el administrador de plataforma, lo cual es aceptable; en SaaS no lo sería. US-20-6 debe (a) crear usuarios de plataforma independientes de cualquier tenant y (b) excluir todo permiso `Platform.*` de los roles de tenant.

### D7 — Unicidad de nombre

El nombre del tenant (sin espacios en los extremos, máximo 200 caracteres) es único sin distinguir mayúsculas. Se verifica en el servicio (`ToLower()` en ambos lados, para que el comportamiento coincida entre SQL Server e InMemory) y se refuerza con un índice único `IX_Tenant_Name`. Una creación concurrente del mismo nombre puede saltarse la verificación del servicio; el índice único protege la integridad y el segundo intento fallaría como error inesperado (500). Aceptado por tratarse de una operación de plataforma de muy baja frecuencia.

### D8 — Contrato de errores por códigos estables

Las respuestas de error usan `ProblemDetails` con una extensión `errorCode` estable (`Tenant.InvalidName`, `Tenant.NameAlreadyExists`, `Tenant.InvalidStatusTransition`). Los mensajes legibles se resuelven en el cliente a partir del código, alineado con el requisito de internacionalización, en lugar de textos fijos en el servidor. Se revisará cuando exista el frontend (US-20-9).

### D9 — Nombres de acción sin sufijo `Async`

Por defecto MVC recorta el sufijo `Async` de los nombres de acción (`SuppressAsyncSuffixInActionNames = true`), lo que rompe `CreatedAtAction(nameof(...))`. Las acciones de `TenantsController` no llevan ese sufijo. Los métodos de servicio sí lo conservan.

## Fuera de alcance de US-20-1

- **Efecto de la suspensión/baja sobre el acceso.** Hoy un tenant suspendido o dado de baja cambia de estado y queda auditado, pero SUS USUARIOS SIGUEN PUDIENDO INICIAR SESIÓN Y OPERAR. Bloquear el acceso según el estado del tenant se resuelve en US-20-2 (evaluación en cada solicitud), junto con la implementación real de `MultiTenant` que ADR-0002 dejó pendiente. Esta brecha es deliberada y está registrada aquí.
- Asignación y cambio de plan (US-20-3, US-20-4); habilitación de módulos (US-20-5).
- Usuarios Super Administrador independientes (US-20-6).
- Listado paginado de tenants y consulta de uso (RF-D20-14, prioridad S); motivo de suspensión; control de concurrencia optimista.
- Fila `Tenant` del tenant de instalación en modo `SingleTenant`: `AdminBootstrapper` crea usuarios con un `TenantId` fijo pero no crea la fila correspondiente en `Tenants`, y `User` no tiene clave foránea hacia `Tenants`. Debe resolverse cuando US-20-2 implemente la resolución `MultiTenant`.
- Reemplazar `EnsureCreatedAsync()` por `Migrate()` al arranque (deuda técnica previa; ver README).

## Consecuencias

- Toda entidad futura con ciclo de vida sigue el patrón: estado explícito + métodos de transición en el dominio + resultado (no excepción) en el servicio.
- Al dar de baja un tenant no se pierde ningún dato; el historial de auditoría permanece consultable.
- Los valores de estado en el JSON de auditoría (`OldValuesJson`/`NewValuesJson`) pueden aparecer como números (1, 2, 3) según cómo `AuditValueSerializer` serialice enums; los valores numéricos son estables por D2. Si resulta ilegible para auditores, la mejora corresponde al serializador (TS-00-4), no a esta historia.
- Cambia el esquema: nueva columna `Status`, se elimina `IsActive`, nuevo índice único `IX_Tenant_Name`. Requiere una migración nueva.

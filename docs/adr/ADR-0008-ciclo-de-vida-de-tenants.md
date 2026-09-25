# ADR-0008 — Ciclo de vida de tenants: baja lógica, estados y API de plataforma

**Estado:** Aceptada (versión revisada; reemplaza a la primera redacción de esta misma fecha)
**Fecha:** 2026-09-19
**Historia relacionada:** US-20-1 — API de alta, suspensión, reactivación y baja de tenants (Sprint 4)
**Requisitos relacionados:** RF-D20-08, RF-D20-09, RF-D20-17, RF-D20-29
**Relación con otros ADRs:** reemplaza parcialmente la sección "`Tenant` entity scope" de ADR-0002 (que fijaba `Id, Name, IsActive, CreatedAtUtc` y delegaba el ciclo de vida a US-20-1). Se apoya en ADR-0004 (decisión 8) y en ADR-0005 (decisión D6).

## Contexto

ADR-0002 dejó una entidad `Tenant` mínima con un booleano `IsActive` y difirió su administración a US-20-1. Esta historia necesita: dar de alta un tenant, cambiar su estado comercial (suspender, reactivar, dar de baja) y dejar constancia auditable de quién y cuándo. Un booleano no distingue "suspendido" (temporal, reversible) de "dado de baja" (cierre de la relación comercial), y un borrado físico destruiría el historial que el criterio de aceptación AC2 exige preservar.

## Decisión

### D1 — Principio de proyecto: bajas lógicas, no físicas

Decisión de Luis (Product Owner): en la medida de lo posible, ninguna entidad de negocio se elimina físicamente; se modela por estados. Aplica desde esta historia a `Tenant` y a las historias futuras, salvo justificación explícita.

### D2 — `TenantStatus` reemplaza a `IsActive`

`TenantStatus` es un enum con valores numéricos explícitos: `Active = 1`, `Suspended = 2`, `Deactivated = 3`. El valor 0 queda sin definir para que un valor sin inicializar nunca se interprete como estado válido. Se persiste como texto (`HasConversion<string>()`, longitud 20), igual que `AuditLog.Action`, con valor por defecto de base de datos `Active` para que las filas existentes queden activas al migrar. **`IsActive` se elimina por completo**: una búsqueda en `src` y `tests` confirmó que nada fuera de la propia clase `Tenant` lo lee ni lo escribe.

### D3 — Reglas de transición

| Estado actual | Suspender | Reactivar | Dar de baja |
|---|---|---|---|
| Activo | → Suspendido | no permitido | → Dado de baja |
| Suspendido | no permitido | → Activo | → Dado de baja |
| Dado de baja | no permitido | no permitido | no permitido |

**Supuesto aún no confirmado por Luis:** la baja es terminal a nivel API. Es reversible cambiando una sola regla en `Tenant.Reactivate()` y sus pruebas, sin migración de datos.

### D4 — Transiciones en el dominio, resultados en el servicio

Las transiciones son métodos de la entidad que lanzan `InvalidTenantStatusTransitionException` (hereda de `InvalidOperationException`) ante una transición prohibida. `Status` tiene setter privado. El servicio de aplicación captura exclusivamente esa excepción y devuelve un `TenantOperationResult`; los desenlaces esperados (no encontrado, nombre duplicado, transición inválida) no se propagan como excepciones. `GlobalExceptionHandler` queda reservado para fallos inesperados.

### D5 — Auditoría de plataforma reutilizando TS-00-4

`Tenant` implementa `IAuditable`; el interceptor registra `Created`/`Updated` sin código adicional. `Tenant` no es `ITenantOwned`, por lo que `AuditTenantResolver` produce `TenantId = null`: son eventos de plataforma (ADR-0004, decisión 8). **Verificado en código:** `AuditLogQueryService` filtra siempre por `a.TenantId == CurrentTenantId`, por lo que jamás devolvería esas filas; por eso la consulta vive en un endpoint propio (`GET /api/v1/platform/tenants/{id}/audit`) y `AuditLogQueryService` no se modifica.

### D6 — Autorización: permiso `Platform.ManageTenants`

Se agrega el permiso `Platform.ManageTenants`, distinto de `Tenant.ManageSettings`. Los endpoints viven en `/api/v1/platform/tenants` con `[RequirePermission]`, como `AuditLogController`.

**Limitación explícita (hasta US-20-6):** todavía no existen Super Administradores independientes de los tenants. `AdminBootstrapper` asigna al rol `Administrator` de su tenant TODOS los permisos del catálogo (solo cuando crea el rol), incluido `Platform.ManageTenants`. En una instalación de un solo tenant (On-Premise) el administrador de la instalación es de hecho el de plataforma; en SaaS no lo sería. US-20-6 debe (a) crear usuarios de plataforma independientes de cualquier tenant y (b) excluir todo permiso `Platform.*` de los roles de tenant.

**Actualización (Sprint 6):** limitación resuelta por ADR-0014 (US-20-6): los administradores de plataforma viven en su propia tabla, con su propio inicio de sesión, y los roles de tenant ya no pueden usar permisos `Platform.*`.

### D7 — Unicidad de nombre

El nombre del tenant (sin espacios en los extremos, máximo 200 caracteres) es único sin distinguir mayúsculas. Se verifica en el servicio (`ToLower()` en ambos lados, para que SQL Server e InMemory coincidan) y se refuerza con el índice único `IX_Tenant_Name`. Una creación concurrente del mismo nombre puede saltarse la verificación del servicio; el índice protege la integridad y el segundo intento fallaría como error inesperado (500). Aceptado por ser una operación de plataforma de muy baja frecuencia.

### D8 — Mensajes de error traducidos en el servidor, con código estable

ADR-0005 (D6) establece que el backend devuelve los mensajes de error ya traducidos. Por eso `TenantsController` usa `IStringLocalizer` con claves `Errors.Tenant*` en `es.json` y `en.json`, dentro de un `ProblemDetails` (ADR-0006, D4). Se añade además una extensión `errorCode` estable (`Tenant.InvalidName`, `Tenant.NameAlreadyExists`, `Tenant.InvalidStatusTransition`) para que los clientes puedan ramificar su lógica sin depender del texto. *(La primera redacción de este ADR decía que el cliente traduciría los códigos; contradecía ADR-0005 y se corrigió.)*

### D9 — Nombres de acción sin sufijo `Async`

Por defecto MVC recorta el sufijo `Async` de los nombres de acción (`MvcOptions.SuppressAsyncSuffixInActionNames`, valor por defecto `true`, según la documentación de Microsoft), lo que rompe `CreatedAtAction(nameof(...))`. Las acciones de `TenantsController` no llevan ese sufijo; los métodos de servicio sí.

### D10 — Los claims del JWT conservan su nombre original

`JwtBearerOptions.MapInboundClaims` vale `true` por defecto (documentación de Microsoft) y aplica también al `JsonWebTokenHandler` usado desde .NET 8: el claim `sub` se renombra a `ClaimTypes.NameIdentifier`. `CurrentUserService` lee el claim crudo `sub`, así que `UserId` habría sido `null` en toda solicitud HTTP real y la auditoría no habría registrado quién actuó (rompiendo AC2). Se fija `MapInboundClaims = false` en `Program.cs`. Ningún código lee `ClaimTypes.NameIdentifier/Role/Email` (verificado por búsqueda). `CurrentUserService.DisplayName` sigue siendo `null` porque no se emite ningún claim `name`; queda fuera de alcance.

### D11 — Ajuste de una prueba existente

`AuditInterceptorNonAuditableTests` usaba `Tenant` como ejemplo de entidad no auditable. Al volverse `Tenant` auditable, la prueba se ajusta para usar `Permission` (que no implementa `IAuditable`). Su propósito no cambia.

## Fuera de alcance de US-20-1

- Aplicar el estado del tenant al acceso: se resuelve en **US-20-11 (ADR-0010)**. Hasta que esa historia se ejecute, un tenant suspendido o dado de baja cambia de estado y queda auditado, pero sus usuarios siguen operando.
- Asignación de plan (US-20-3, US-20-4); módulos habilitados por tenant (US-20-5); usuarios de plataforma (US-20-6).
- Listado paginado de tenants (RF-D20-14, prioridad S); motivo de suspensión; control de concurrencia optimista.
- La fila `Tenant` del tenant de instalación en `SingleTenant`: `AdminBootstrapper` crea usuarios con un `TenantId` fijo pero no crea la fila en `Tenants`, y `User` no tiene clave foránea hacia `Tenants`.

## Consecuencias

- Toda entidad futura con ciclo de vida sigue el patrón: estado explícito + métodos de transición en el dominio + resultado (no excepción) en el servicio.
- Al dar de baja un tenant no se pierde ningún dato; su historial de auditoría permanece consultable.
- **Verificado en código:** `AuditValueSerializer` serializa con opciones por defecto de `System.Text.Json`, así que `Status` aparece como número (1, 2, 3) en `OldValuesJson`/`NewValuesJson`. Los valores son estables por D2. Si resultara ilegible para auditores, la mejora corresponde al serializador (TS-00-4), no a esta historia.
- Cambia el esquema: se elimina `IsActive`, nueva columna `Status`, nuevo índice único `IX_Tenant_Name`. Requiere una migración nueva.

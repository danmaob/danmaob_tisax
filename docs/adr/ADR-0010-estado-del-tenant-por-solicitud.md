# ADR-0010 — Aplicación del estado del tenant en cada solicitud

**Estado:** Aceptada
**Fecha:** 2026-09-19
**Historia relacionada:** US-20-11 — Bloqueo de acceso de tenants suspendidos o dados de baja (Sprint 4)
**Requisitos relacionados:** RF-D20-09, RF-D20-10
**Relación con otros ADRs:** completa ADR-0008 (estados del tenant, D2 y D3), que dejó pendiente el efecto de la suspensión sobre el acceso. Independiente de ADR-0009.

## Contexto

ADR-0008 define los estados `Active`, `Suspended` y `Deactivated`, pero un tenant suspendido o dado de baja seguía operando, porque ningún código consulta su estado al atender solicitudes. RF-D20-09 ("suspender, reactivar o dar de baja un tenant") solo tiene efecto real si el acceso se bloquea.

## Decisión

### D1 — Estado leído en cada solicitud autenticada, sin caché

Un middleware (`TenantStatusMiddleware`) corre inmediatamente después de `UseAuthentication`. Consulta el estado del tenant en la base de datos en cada solicitud autenticada, sin caché en ninguna capa, para que una suspensión o una reactivación apliquen en la solicitud siguiente.

### D2 — El tenant sale del claim firmado `tenant`

Se usa el claim `tenant` del JWT (firmado por el servidor; el cliente no puede elegirlo), no `ICurrentTenantProvider`. Nunca se confía en un header ni en datos no autenticados.

### D3 — Solo se bloquea si el tenant existe y no está activo

`IsTenantBlockedAsync` es verdadero únicamente cuando existe una fila en `Tenants` con estado distinto de `Active`. **Un tenant sin fila nunca se bloquea**: en una instalación `SingleTenant`, `AdminBootstrapper` crea usuarios y roles para un `TenantId` de configuración sin crear la fila `Tenant`; bloquear ante su ausencia dejaría sin acceso a toda instalación de un solo tenant. Tampoco se evalúa si el claim falta o no es un `Guid`.

### D4 — Respuesta

403 con un `ProblemDetails` (ADR-0006, D4) cuyo título está traducido en el servidor (ADR-0005, D6, clave `Errors.TenantNotActive`) y con la extensión `errorCode` = `Tenant.NotActive`.

### D5 — Las solicitudes anónimas no se bloquean (limitación conocida)

Login, refresh y logout no se ven afectados. Un usuario de un tenant bloqueado todavía puede obtener tokens, pero toda solicitud autenticada que haga es rechazada con 403. Bloquear el login o revocar los refresh tokens al suspender queda fuera de esta historia.

### D6 — Costo aceptado

Una consulta por clave primaria en cada solicitud autenticada. Se optimizará solo con evidencia medida.

## Limitaciones conocidas

- **Hoy no puede ocurrir** que un administrador se bloquee a sí mismo: la API crea los tenants con identificadores nuevos y el tenant de la instalación no tiene fila en `Tenants` (D3). Se volvería posible si más adelante se crea esa fila (pendiente señalado en ADR-0008) o al operar como SaaS con administradores que pertenezcan a un tenant con fila. Debe considerarse al diseñar US-20-6.
- No se verifica todavía el comportamiento del modo `MultiTenant` (no implementado; ADR-0002).

## Consecuencias

- Suspender o dar de baja un tenant, desde US-20-1, tiene efecto real sobre el acceso.
- No hay cambio de esquema ni migración.

# ADR-0014 — Administradores de plataforma independientes de los tenants

**Estado:** Aceptada
**Fecha:** 2026-09-24
**Historia relacionada:** US-20-6 — Usuarios Super Administrador de Plataforma independientes de los tenants (Sprint 6)
**Requisitos relacionados:** RF-D20-16
**Resuelve:** la "Limitación explícita (hasta US-20-6)" de ADR-0008, decisión D6

## Contexto

Revisión del código al inicio de Sprint 6:

1. El administrador de plataforma es un `User` que pertenece a un tenant (el de instalación en On-Premise; `TestTenantId` en las pruebas). Sus permisos `Platform.*` le llegan porque `AdminBootstrapper` asigna al rol `Administrator` del tenant todos los permisos del catálogo.
2. `TenantStatusMiddleware` bloquea toda solicitud cuyo claim `tenant` apunte a un tenant que no esté activo. Si el administrador suspende su propio tenant, pierde el acceso al endpoint que lo reactivaría. Si lo da de baja, la baja es definitiva (`Tenant.Reactivate` solo acepta tenants suspendidos) y la salida es por SQL.
3. Todavía no existe un endpoint que liste usuarios de un tenant (llega con US-16-1). El AC1 de US-20-6 se cumple por diseño: el Super Administrador no vive en la tabla de usuarios de tenant.

## Decisiones

### D1 — Entidad propia

`PlatformAdministrator` (tabla `PlatformAdministrators`) no implementa `ITenantOwned` y no tiene `TenantId`. Sí implementa `IAuditable`. El correo es único entre administradores de plataforma. El mismo correo puede existir además como usuario de un tenant: son cuentas distintas, con contraseñas distintas.

### D2 — Inicio de sesión propio

`POST /api/v1/platform/auth/login`. El token:
- no lleva claim `tenant` ni claims `role`;
- lleva `principal_type = platform`;
- lleva un claim `perm` por cada permiso del catálogo cuyo módulo sea `Platform`, consultados en cada inicio de sesión. Un permiso `Platform.*` nuevo se aplica al Super Administrador sin más código.

### D3 — Sin refresh token

La sesión dura lo que dura el token de acceso (`Jwt:AccessTokenLifetimeMinutes`, hoy 15 minutos). Al expirar se vuelve a iniciar sesión. `RefreshToken` sigue ligado solo a `User`.

### D4 — Los permisos `Platform.*` solo sirven con token de plataforma

`PermissionAuthorizationHandler` rechaza cualquier requisito `Platform.*` si el token no trae `principal_type = platform`, aunque traiga el claim `perm`. Un rol de tenant nunca puede administrar la plataforma, ni siquiera por error de configuración.

### D5 — El rol `Administrator` del tenant no recibe `Platform.*`

`--bootstrap-admin` ya no los asigna. Al ejecutarse sobre un administrador existente, elimina los que tenga el rol.

### D6 — Arranque del primer Super Administrador

Comando `--bootstrap-platform-admin`:
- claves `BootstrapPlatformAdmin:Email`, `BootstrapPlatformAdmin:Password` y, opcional, `BootstrapPlatformAdmin:FullName` (por defecto "Platform Administrator");
- valida la contraseña contra la política vigente;
- es idempotente: si el correo ya existe, no hace nada.

### D7 — Bloqueo por intentos fallidos

Misma política que los usuarios de tenant (`PasswordPolicy:MaxFailedAccessAttempts` y `PasswordPolicy:LockoutDurationMinutes`).

## Consecuencias

- `TenantStatusMiddleware` deja pasar los tokens sin claim `tenant`, así que suspender o dar de baja cualquier tenant ya no bloquea al Super Administrador. No hubo que modificar el middleware.
- Un token de plataforma no pasa ningún `[RequireModule]`: `ModuleAuthorizationHandler` exige claim `tenant`. Es lo correcto: el Super Administrador no opera módulos funcionales de un cliente.
- La auditoría registra `PerformedByUserId` con el Id del administrador de plataforma (claim `sub`).
- Las pruebas que usaban un usuario de tenant con `Platform.*` ahora usan un administrador de plataforma. `CreateUserWithPlatformPermissionsAndLoginAsync` se conserva para probar que un usuario de tenant con esos permisos recibe 403.

## Fuera de alcance

- Administrar Super Administradores desde la API (alta de otros, baja, cambio de contraseña). En Sprint 6 solo existe el arranque por comando.
- Registro de auditoría de plataforma consultable y exportable (US-20-7).

## Limitación conocida

`AuditValueSerializer` guarda todas las propiedades de una entidad auditada. Por eso `PasswordHash` queda en `NewValuesJson` al crear un `User`, y lo mismo pasará con `PlatformAdministrator`. Se reportó a Luis como candidata a historia nueva (US-20-14). No se corrige dentro de US-20-6.

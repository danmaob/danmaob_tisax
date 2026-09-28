# ADR-0020 — Gestión de usuarios del tenant

- **Estado:** Aceptado
- **Fecha:** 2026-09-28
- **Historia:** US-16-1 (E16 — Gestión de Usuarios y Accesos)
- **Relacionados:** ADR-0010 (estado del tenant), ADR-0014 (administradores de plataforma), ADR-0015 (auditoría con redacción), ADR-0019 (validaciones de dominio)

## Contexto

El Administrador de Organización necesita crear, editar y desactivar usuarios de su tenant sin operaciones manuales en la base. Antes de esta historia:

- `LoginAsync` ya rechazaba usuarios inactivos, pero `RefreshAsync` no revisaba `IsActive`. Un usuario desactivado podía renovar su sesión hasta 7 días.
- Un token de acceso vigente seguía sirviendo hasta 15 minutos después de la desactivación.
- El índice `IX_User_TenantId_Email` no era único.
- `User` tenía todas sus propiedades con `set` público.

## Decisión

1. **Endpoints** de tenant bajo `/api/v1/users`, protegidos solo con el permiso `Identity.ManageUsers`, sin candado de módulo: listar (filtros `isActive` y `search`, paginación de 1 a 200), consultar, crear, renombrar, desactivar y reactivar. Sin borrado físico.
2. **Alta:**
   - El administrador escribe la contraseña inicial, que debe cumplir la política vigente (`IPasswordPolicyValidator`). Si no la cumple: 400 `"User.PasswordPolicyViolation"`, con `violatedRules`.
   - El usuario se crea activo y sin roles. Los roles llegan con US-16-2.
3. **Correo:**
   - Se guarda sin espacios alrededor y no se puede editar.
   - Es único por tenant sin distinguir mayúsculas: validación en la aplicación (409 `"User.EmailAlreadyExists"`) más índice único en la base.
   - Un alta simultánea en carrera choca con el índice y responde 500. Se acepta: el dato queda protegido y el caso es improbable.
4. **Desactivación inmediata:**
   - Desactivar un usuario revoca todos sus refresh tokens activos, en el mismo guardado.
   - `RefreshAsync` rechaza usuarios inactivos.
   - `UserStatusMiddleware`, ubicado después de `TenantStatusMiddleware` y antes de la autorización, responde **401** `"User.Inactive"` cuando un token de tenant pertenece a un usuario existente e inactivo. Es una consulta por solicitud, el mismo patrón de ADR-0010.
   - Si el usuario no existe, no se bloquea (no hay borrado físico). Los tokens de plataforma no se revisan.
5. **Protecciones:**
   - Un administrador no puede desactivarse a sí mismo: 409 `"User.CannotDeactivateSelf"`.
   - Desactivar a alguien ya inactivo, o reactivar a alguien ya activo, responde 409 `"User.InvalidStatusTransition"`.
6. **Entidad `User`:**
   - Sin `set` públicos, excepto `TenantId`, que lo exige `ITenantOwned`.
   - Métodos de dominio `Rename`, `Deactivate` y `Reactivate`.
   - Longitudes máximas en el dominio: correo de 256 y nombre de 200. Según ADR-0019, su violación responde 400 `"Domain.ValidationFailed"`.
7. **Auditoría:** `User` ya es `IAuditable` y `PasswordHash` ya lleva `[AuditRedacted]`. Altas, cambios y desactivaciones quedan auditadas sin trabajo adicional.

## Consecuencias

- La desactivación surte efecto en la siguiente solicitud del usuario, al costo de una consulta ligera por solicitud autenticada de tenant.
- Hasta US-16-2, un usuario recién creado puede iniciar sesión pero no tiene permisos.
- Quedan fuera y pendientes:
  - restablecer la contraseña desde la aplicación (US-16-5);
  - forzar el cambio de contraseña en el primer inicio de sesión;
  - editar el correo;
  - la interfaz (US-16-7).

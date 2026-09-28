# ADR-0006 — API REST versionada y documentación OpenAPI

**Estado:** Aceptada
**Fecha:** 2026-09-18
**Historia relacionada:** TS-00-6 — API REST versionada con documentación OpenAPI/Swagger (Sprint 3)
**Requisitos relacionados:** RF-T-05, RNF-API-01, RNF-API-02, RNF-API-03, RNF-API-04, RNF-DOC-01

## Contexto

DANMAOB TISAX Compliance Manager expone su lógica de negocio vía API REST, consumida por el frontend web (futuro) y la app móvil (futura) — RNF-API-02 fija explícitamente el orden de desarrollo Backend/API → Frontend Web → App Móvil. El contrato de la API debe estar versionado (para no romper clientes existentes ante cambios incompatibles) y documentado en OpenAPI desde el día uno, incluso antes de que existan clientes reales, porque cambiar el contrato después de que el frontend empiece a consumirlo es mucho más costoso que diseñarlo bien ahora.

## Decisión

### D1 — Paquete de versionado: `Asp.Versioning.Mvc` (no `Microsoft.AspNetCore.Mvc.Versioning`)

`Microsoft.AspNetCore.Mvc.Versioning` es el nombre anterior del mismo proyecto — se renombró en 2022 al pasar a mantenimiento comunitario. Para un proyecto de controllers (no Minimal API, que es la arquitectura de este backend) se usan tres paquetes: `Asp.Versioning.Mvc` (soporte de versionado para MVC/controllers), `Asp.Versioning.Mvc.ApiExplorer` (metadata de versiones para OpenAPI) y `Asp.Versioning.OpenApi` (el puente entre el versionado y el generador de documentos OpenAPI nativo de .NET 10). Esta última liberó su versión estable (no release-candidate) apenas el 6 de agosto de 2026 — es la primera vez que este flujo completo está soportado de forma estable en .NET 10.

### D2 — Esquema de versionado: segmento de URL (`/api/v1/...`)

De las cuatro estrategias que soporta `Asp.Versioning` (segmento de URL, query string, header, media type), se elige **segmento de URL** — es la más visible, la más cacheable por proxies/CDN, y la única que se puede probar pegando un link en el navegador. Es una decisión de una sola vía: los clientes la codifican de forma fija, así que no se cambia después sin romper a todos. `AssumeDefaultVersionWhenUnspecified` queda en `false` — existe para retrofit de APIs ya consumidas sin versión, no es nuestro caso: preferimos un 404 explícito ante una versión no declarada, no una caída silenciosa a v1.

### D3 — Documentación: OpenAPI nativo de .NET 10 + Scalar (no Swashbuckle/Swagger UI)

Este proyecto ya usa el generador de documentos OpenAPI incorporado de .NET 10 (`Microsoft.AspNetCore.OpenApi`, ya presente en `Program.cs` desde TS-00-1) en vez de Swashbuckle — Microsoft discontinuó las recomendaciones hacia Swashbuckle a favor de este paquete nativo. Para la interfaz interactiva de exploración se usa **Scalar** (`Scalar.AspNetCore`), la alternativa que la propia documentación de .NET 10 recomienda sobre Swagger UI cuando no se usa Swashbuckle. Con `Asp.Versioning.OpenApi`, un solo `.WithDocumentPerVersion()` genera `/openapi/v1.json`, `/openapi/v2.json`, etc., cada uno con solo los endpoints de esa versión — sin loops manuales sobre `DescribeApiVersions()`, que era el workaround de antes de que el paquete llegara a estable.

**Riesgo conocido, documentado:** existe un bug de compatibilidad real entre `Microsoft.OpenApi` 3.0 (la versión que trae por defecto el paquete nativo de .NET 10) y `Scalar.AspNetCore`, que se manifiesta como una excepción en tiempo de ejecución (`Property or indexer 'IOpenApiMediaType.Example' cannot be assigned to -- it is read only`). El workaround confirmado es fijar `Microsoft.OpenApi` explícitamente a la versión `2.3.9`. No se aplica preventivamente — solo si el síntoma aparece — para no fijar una versión más antigua sin necesidad real.

**Convención de exclusión de UI en producción:** tanto el documento OpenAPI como Scalar quedan detrás de `if (app.Environment.IsDevelopment())`, igual que ya estaba `MapOpenApi()` antes de esta historia — no se expone documentación interactiva de la API en despliegues de producción por defecto.

### D4 — Contrato base de error: `ProblemDetails` (RFC 7807), forma únicamente

Se activa `AddProblemDetails()` (soporte nativo de ASP.NET Core desde .NET 7) como mecanismo de forma del contrato de error — de aquí en adelante, las respuestas de error del framework siguen automáticamente la forma estándar `ProblemDetails`, documentada en OpenAPI. **Esto define solo la forma, no el comportamiento de seguridad**: qué información se expone o se oculta en el cuerpo de un error (stack traces, cadenas de conexión) es responsabilidad de TS-00-7 (manejo controlado de errores), que construye su middleware sobre este mismo contrato ya fijado aquí.

### D5 — Convenciones diferidas para carga de binarios y offline/sync (RNF-API-04)

RNF-API-04 exige que el diseño de la API considere desde ahora la futura carga de binarios (evidencias, fotografías) y la operación offline/sync de la app móvil, para no rediseñar el contrato cuando E07/E15 lleguen. Esta historia no implementa esos endpoints — no existen entidades de evidencia todavía — pero deja fijadas dos convenciones para cuando existan:

- **Carga de binarios:** vía `multipart/form-data`, documentado en OpenAPI mediante el tipo de contenido nativo que soporta el generador de .NET 10 sin configuración adicional.
- **Offline/sync:** cualquier endpoint de escritura que un cliente pueda reintentar tras una desconexión debe aceptar una clave de idempotencia (por ejemplo, un header `Idempotency-Key`), para que un reintento no duplique la operación. Esto es una convención a seguir por las historias de dominio futuras, no una implementación de esta historia.

## Fuera de alcance de TS-00-6

- Migración de todos los controllers existentes — solo se versiona `AuthController` y `AuditLogController` como caso piloto (los únicos que existen hoy).
- El comportamiento del middleware de manejo de errores (qué se oculta, cómo se registra) — eso es TS-00-7.
- Endpoints reales de carga de binarios u offline/sync — eso son historias de dominio futuras (E06/E07/E15), que seguirán las convenciones fijadas en D5.

## Consecuencias

- Cualquier controller nuevo que se agregue de aquí en adelante debe declarar `[ApiVersion("1.0")]` y su ruta con el segmento `api/v{version:apiVersion}/...` desde el primer prompt que lo cree — no queda como un paso posterior.
- El primer cambio incompatible real (cuando llegue) se resuelve agregando `[ApiVersion("2.0")]` junto a la versión existente en el mismo controller, no reescribiéndolo.
- La versión de `Scalar.AspNetCore` fijada en el prompt de wiring es la de menor certeza de verificación de todo este ADR — las fuentes consultadas para confirmarla no son el propio NuGet.org sino réplicas/espejos; si `dotnet build` reporta que no resuelve, es la primera línea a revisar.

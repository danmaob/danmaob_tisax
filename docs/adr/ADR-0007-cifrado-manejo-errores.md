# ADR-0007 — Cifrado en reposo/tránsito y manejo controlado de errores

**Estado:** Aceptada
**Fecha:** 2026-09-18
**Historia relacionada:** TS-00-7 — Cifrado en tránsito/reposo y manejo controlado de errores (Sprint 3)
**Requisitos relacionados:** RNF-SEC-03, RNF-SEC-04, RNF-AVAIL-01, RNF-SEC-07 (nota de diseño)

## Contexto

Esta historia cubre dos capacidades transversales de seguridad que hoy no existen en el backend: un mecanismo reutilizable de cifrado en reposo para datos de negocio sensibles (evidencias, documentos — que todavía no existen como entidades, E06/E07/E15 son historias futuras) y un manejo controlado de errores que no exponga información interna. Se construye sobre el contrato `ProblemDetails` ya fijado en TS-00-6 (ADR-0006, decisión D4).

## Decisión

### D1 — Cifrado en reposo: Data Protection API de ASP.NET Core, con persistencia de claves explícita y obligatoria

Se usa `Microsoft.AspNetCore.DataProtection` (`IDataProtectionProvider`/`IDataProtector`) como mecanismo de cifrado simétrico reutilizable — es el mecanismo nativo del framework para este propósito, ya probado en producción, sin necesidad de traer una librería criptográfica externa.

**Decisión crítica, verificada, que cambia el diseño por defecto del framework:** el comportamiento por defecto de Data Protection, cuando no se configura explícitamente dónde persistir el llavero de claves, es detectar el entorno de ejecución (perfil de usuario de Windows, registro de IIS, Azure App Service) y, si ninguno de esos aplica — que es exactamente el caso de un contenedor Linux On-Premise sin perfil de usuario — **cae en un repositorio de claves efímero, solo en memoria**. Esto significa que cualquier dato cifrado se vuelve indescifrable en el siguiente reinicio del proceso, sin ningún error visible más allá de un warning en el log. Por lo tanto, esta historia **exige explícitamente** `.PersistKeysToFileSystem(...)` apuntando a un directorio configurable (con un valor por defecto seguro si no se configura, siguiendo el mismo patrón ya usado en `SupportedCulturesOptions` — configuración opcional, nunca ausencia silenciosa de comportamiento).

**Limitación reconocida, no resuelta en esta historia:** las claves persistidas en disco en Linux no quedan cifradas en reposo por sí mismas (a diferencia de Windows, donde DPAPI las protege automáticamente) — la protección del llavero de claves depende de los permisos del sistema de archivos del directorio configurado, que es responsabilidad operativa, documentada explícitamente en el prompt correspondiente, no automatizada por esta historia. Cifrar el llavero de claves con un certificado (`.ProtectKeysWithCertificate(...)`) queda fuera de alcance — es una mejora futura, no una carencia oculta.

### D2 — Servicio reutilizable de cifrado, sin entidad de negocio asociada todavía

Se construye un servicio delgado sobre `IDataProtector` (creado con un propósito fijo vía `CreateProtector(purpose)`), expuesto para que futuras historias de dominio (E06/E07/E15) lo consuman por inyección de dependencias sin rediseño. No se cifra ningún campo real en esta historia — no existe ninguna entidad de evidencia o documento todavía.

### D3 — TLS/HSTS: solo enforcement a nivel aplicación

Se activa `UseHsts()` (además de `UseHttpsRedirection()`, ya presente desde TS-00-1) únicamente para el entorno de no-desarrollo — HSTS le indica al navegador que fuerce HTTPS en visitas futuras, y no debe activarse en desarrollo local donde no hay certificado real. La gestión real de certificados y terminación TLS es responsabilidad de la topología de despliegue (TS-00-8), no de esta historia.

### D4 — Manejo controlado de errores: `IExceptionHandler`, sobre el `ProblemDetails` ya fijado

.NET introdujo desde la versión 8 la interfaz `IExceptionHandler` como el mecanismo preferido para centralizar el manejo de excepciones no controladas (reemplazando el patrón anterior de middleware manual). Se implementa un manejador que traduce cualquier excepción no capturada en una respuesta `ProblemDetails` sanitizada — sin stack traces, sin cadenas de conexión, sin detalles internos — registrando el detalle completo únicamente del lado servidor. Se registra con `AddExceptionHandler<T>()` + el ya existente `AddProblemDetails()` de TS-00-6, y se activa con `UseExceptionHandler()` sin argumentos. Se agrega también `UseStatusCodePages()` para que códigos de estado "desnudos" que no pasan por una excepción (como un 404 de enrutamiento — incluida la prueba que ya existe de TS-00-6 verificando que una ruta sin versión da 404) reciban también un cuerpo `ProblemDetails` consistente, sin cambiar el código de estado en sí.

### D5 — Validación de entrada del lado servidor: Data Annotations como línea base

Se establece Data Annotations + `ModelState` (ya disponible de forma nativa en los controllers de ASP.NET Core, sin paquete adicional) como el mecanismo de validación server-side independiente del cliente (RNF-SEC-04), aplicado como caso piloto sobre `AuthController`, que ya existe y ya está versionado desde TS-00-6. No se adopta FluentValidation ni ninguna librería adicional — no hay evidencia en el repo de que ya se use, y Data Annotations es suficiente para el caso piloto sin agregar una dependencia nueva sin necesidad real.

## Fuera de alcance de TS-00-7

- Cifrado del llavero de claves de Data Protection con certificado — mejora futura, documentada como limitación reconocida (D1).
- Gestión de certificados TLS reales y terminación — responsabilidad de TS-00-8 (pipeline CI/CD y despliegue multi-topología).
- Cifrado de campos reales de entidades de negocio — no existen todavía (E06/E07/E15).
- RNF-SEC-06 (política de contraseñas, bloqueo, expiración de sesión) — ya vive en TS-00-3, no se duplica aquí.

## Consecuencias

- Cualquier historia futura que necesite cifrar un campo sensible inyecta el servicio de cifrado ya construido aquí, sin rediseño.
- El directorio de persistencia de claves de Data Protection debe respaldarse junto con la base de datos en cualquier estrategia de backup futura — si se pierden las claves sin haber respaldado los datos cifrados con ellas, esos datos quedan permanentemente indescifrables. Esto debe quedar documentado explícitamente para quien opere el despliegue On-Premise.
- El manejador de excepciones se convierte en el punto único donde decidir, de aquí en adelante, cómo se traduce cada tipo de excepción de dominio a un código HTTP — evita que cada controller nuevo reinvente su propio manejo de errores.

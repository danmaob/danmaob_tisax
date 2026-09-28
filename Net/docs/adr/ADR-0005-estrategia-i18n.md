# ADR-0005 — Estrategia de internacionalización (i18n)

**Estado:** Aceptada
**Fecha:** 2026-09-18
**Historia relacionada:** TS-00-5 — Framework de internacionalización (i18n) ES/EN extensible (Sprint 3)
**Requisitos relacionados:** RF-T-03, RNF-I18N-01, RNF-I18N-02

## Contexto

DANMAOB TISAX Compliance Manager debe soportar Español e Inglés desde el día uno, y debe poder incorporar Alemán, Francés, Portugués u otros idiomas más adelante sin rediseñar componentes ni arquitectura (RNF-I18N-02). El backend expone su lógica de negocio vía API REST consumida por el frontend web (futuro) y la aplicación móvil (futura); ambos clientes deben recibir mensajes de error y validación ya traducidos, sin duplicar reglas de traducción en cada cliente (alineado con RF-T-05 / RNF-API-01).

## Decisión

### D1 — Mecanismo de localización: `IStringLocalizer`/`IStringLocalizerFactory` propios, basados en JSON

Se implementa una versión propia de `Microsoft.Extensions.Localization.IStringLocalizer` (`JsonStringLocalizer`) y de `IStringLocalizerFactory` (`JsonStringLocalizerFactory`), en vez de usar el proveedor por defecto basado en `.resx`/`ResourceManager`. Los archivos de recursos son JSON planos, uno por cultura (`es.json`, `en.json`, ...), embebidos como `EmbeddedResource` dentro de `DanmaobTisax.Infrastructure`.

**Por qué no `.resx`:** los `.resx` requieren generación de "satellite assemblies" por cultura y, en la práctica, tooling de Visual Studio para editarlos cómodamente; agregar un idioma nuevo implica una compilación distinta por assembly de recursos. Un archivo JSON plano se agrega, se traduce y se embebe con una sola recompilación del ensamblado principal — más alineado con el requisito de "agregar idiomas sin rediseño" (RNF-I18N-02).

### D2 — Convención de claves: namespaces por punto, sin agrupar por tipo CLR

Las claves de recurso siguen la convención `{Namespace}.{Clave}`, por ejemplo `Validation.Required`, `Common.WelcomeMessage`, `Errors.NotFound`. Los namespaces iniciales son `Common`, `Validation` y `Errors`. Esta convención es independiente del tipo CLR que solicita la traducción — por eso `JsonStringLocalizerFactory.Create(Type)` ignora deliberadamente el parámetro `resourceSource` y siempre devuelve la misma instancia compartida de `JsonStringLocalizer`, que conoce todas las claves de todas las culturas cargadas. Futuras historias que agreguen nuevos namespaces (por ejemplo `Catalog` cuando se traduzca contenido de VDA ISA, fuera de alcance de esta historia — ver "Fuera de alcance" abajo) simplemente agregan claves nuevas con su propio prefijo, sin tocar el mecanismo.

### D3 — Culturas soportadas y cultura por defecto: configurables, con valores por defecto seguros

`SupportedCulturesOptions` expone `DefaultCulture` (por defecto `"es"`) y `SupportedCultureCodes` (por defecto `["es", "en"]`), leídos opcionalmente desde la sección `"Localization"` de la configuración (`appsettings.json`, variables de entorno, etc.) vía `SupportedCulturesOptions.FromConfiguration(IConfiguration)`. Si la sección no existe o está vacía, se usan los valores por defecto — el sistema funciona sin ningún cambio de configuración. No se usó el patrón `IOptions<T>` de Microsoft: la clase se lee una vez al arrancar la aplicación y se registra como instancia singleton simple, evitando una dependencia adicional (`Microsoft.Extensions.Options`) para un caso de uso que no necesita recarga en caliente.

### D4 — Resolución de cultura por request: middleware estándar de ASP.NET Core

Se usa `RequestLocalizationMiddleware` (`app.UseRequestLocalization()`), configurado con `RequestLocalizationOptions.SupportedCultures`/`SupportedUICultures` construidas a partir de `SupportedCulturesOptions`. La resolución de cultura por el header `Accept-Language` estándar del cliente queda a cargo del proveedor por defecto del framework (`AcceptLanguageHeaderRequestCultureProvider`), sin código propio adicional.

### D5 — Fallback de claves faltantes: a la cultura por defecto, nunca una excepción

Si una clave no existe en la cultura solicitada, `JsonStringLocalizer` la busca en la cultura por defecto. Si tampoco existe ahí, devuelve la clave misma como texto (comportamiento estándar de `LocalizedString.ResourceNotFound = true`, igual que el proveedor `ResourceManagerStringLocalizer` de Microsoft) — nunca lanza una excepción por una clave faltante en tiempo de ejecución. La única excepción real que el mecanismo lanza es en el arranque de la aplicación, si el archivo de recursos de la cultura por defecto no se encuentra en absoluto (`JsonStringLocalizerFactory` falla rápido con `InvalidOperationException` — fail-fast en vez de degradar silenciosamente).

### D6 — El backend traduce y devuelve texto ya resuelto; la interfaz pura de cliente queda diferida

Los mensajes de error y validación que la API devuelve se resuelven server-side, en el idioma indicado por el request (D4), y se envían ya traducidos — ni el frontend web ni la app móvil necesitan mantener su propio diccionario de esos mismos mensajes. El texto puramente de interfaz (botones, menús, navegación) se traducirá del lado cliente con una librería de i18n de React, cuando esa fase del proyecto comience.

## Fuera de alcance de TS-00-5

- **Traducción del contenido del catálogo VDA ISA** (RNF-I18N-03) — es contenido normativo de una historia de dominio futura (D03), no del framework transversal.
- **Consumo del framework desde el frontend React** — diferido junto con el resto de la fase de frontend (ver nota de Sprint 3 sobre `React/` permaneciendo vacía).
- **Integración del localizador en un middleware global de manejo de errores** — esa pieza corresponde a TS-00-7 (manejo controlado de errores), que consumirá `IStringLocalizer<T>` una vez exista; TS-00-5 solo deja el mecanismo listo e inyectable.

## Consecuencias

- Agregar un idioma nuevo (ej. `de`) requiere: crear `de.json` con las mismas claves que `es.json`, agregarlo como `<EmbeddedResource>` en el `.csproj`, y agregar `"de"` a `SupportedCultureCodes` en configuración (o dejarlo fuera de configuración y solo dependerá del default si se decide que sea la nueva cultura por defecto). No requiere tocar `JsonStringLocalizer`, `JsonStringLocalizerFactory`, ni el wiring de `Program.cs`.
- Cualquier historia futura que necesite mensajes traducidos simplemente inyecta `IStringLocalizer<T>` (el tipo `T` es irrelevante para la resolución, por D2) y usa claves con el namespace que le corresponda.
- Si en el futuro el volumen de claves crece mucho, el enfoque de carga completa en memoria al arrancar (sin caché diferida) podría revisarse — no se considera un problema para el volumen actual del proyecto.

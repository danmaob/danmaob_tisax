# DANMAOB TISAX Compliance Manager — Contexto acumulado hasta el cierre de Sprint 2

Este documento resume todo lo aprendido y acordado durante Sprint 1 y Sprint 2, para arrancar Sprint 3 sin perder continuidad.

---

## 1. El proyecto

- DANMAOB TISAX Compliance Manager: plataforma B2B empresarial para que proveedores automotrices (principalmente Tier 1/Tier 2, enfoque LatAm/México) implementen y mantengan su cumplimiento TISAX, usando el catálogo VDA ISA como referencia normativa.
- Tres modelos de despliegue objetivo: On-Premise, Private Cloud, SaaS. Tres niveles de plan comercial, como eje ortogonal a los modelos de despliegue.
- Luis es el líder técnico, product owner, y autoridad final sobre todas las decisiones.

## 2. Stack técnico

- .NET 10.0 (`net10.0`, `LangVersion latest`), ASP.NET Core Web API con controllers (no minimal API).
- EF Core 10 con SQL Server (Docker en macOS).
- Clean Architecture: Domain / Application / Infrastructure / Api.
- Pruebas: xUnit y NetArchTest.Rules.
- Sin CQRS/MediatR — descartado por licenciamiento y complejidad injustificada.
- Solución: `DanmaobTisax.slnx` dentro de `Net/`. Carpetas hermanas: `Docs/`, `MAUI/`, `prompts/`.
- Raíz del workspace: `/Users/luisortiz/Desarrollo/DANMAOB/DANMAOB_TISAX/`

## 3. Modelo de equipo y flujo de sprints

- Gestión del proyecto en Jira; Luis traslada manualmente los resultados de Claude a Jira.
- Cada sesión de Claude equivale a un sprint.
- Si el ejecutor local se desvía a media tarea, Luis regresa a la MISMA sesión donde se generó ese prompt — nunca abre una nueva para corregir.
- El siguiente sprint arranca en una sesión completamente nueva, con este documento como carga inicial.
- Cada subtarea de Jira se traduce en uno o más prompts atómicos para el ejecutor local, cada uno corrido en una sesión `/newtask` limpia salvo que se esté probando deliberadamente correr varias subtareas relacionadas en una sola sesión (ver sección 6).

## 4. Roles

- **Claude**: arquitecto técnico, autor de especificaciones, ADRs, READMEs, y orquestador de ingeniería. Nunca escribe código C# literal en los prompts — solo el contrato exacto.
- **Ejecutor local (Cline u Ollama Code, corriendo un modelo vía Ollama)**: el "typeador" — traduce las especificaciones de Claude a C# real. Ver sección 7 para hallazgos de confiabilidad.
- **Luis**: autoridad final, corre y verifica todos los comandos de terminal él mismo, maneja credenciales reales y operaciones de base de datos reales.
- La documentación (ADRs, READMEs) la escribe exclusivamente Claude, nunca se delega al ejecutor local.

## 5. Estado actual (al cierre de esta sesión, 2026-09-17)

- **Sprint 1** (TS-00-1, TS-00-2, TS-00-9): completo y verificado.
- **Sprint 2**:
  - **TS-00-4** (framework de auditoría transversal): **completo y verificado**. 20/20 pruebas pasando (9 de Sprint 1 + 11 de TS-00-4), los 11 nombres de método `[Fact]` coinciden exactamente con la especificación de cada prompt, sin modificaciones no autorizadas en `src/`.
  - **TS-00-3** (RBAC/autenticación): **en progreso**, actualmente en la subtarea 12 (servicio de tokens JWT) de 24. Subtareas 1-11 completas y verificadas (entidades de dominio, configuración EF, migración aplicada).
- Antes de escribir prompts de un nuevo sprint, pedirle a Luis que corra `ls Net/docs/adr/` para confirmar la numeración real de ADRs — la memoria de Claude sobre qué números ya se usaron puede estar desactualizada.

## 6. Metodología de escritura de prompts (reglas no negociables)

1. **Cero bloques de código C# literal** en ningún prompt, ni siquiera para cosas triviales (enums, DTOs). Claude especifica solo el CONTRATO: nombres exactos de tipo/clase/interfaz/namespace, tablas de propiedades (nombre / tipo / nullable / valor por defecto), firmas de método en prosa, y comportamiento paso a paso en prosa.
2. **Todos los prompts en inglés**, sin excepción, aunque la comunicación con Luis sea en español.
3. **Un prompt por subtarea de Jira**, salvo que la complejidad real amerite dividirla más (ver la subtarea 4 y las de pruebas de TS-00-4 como ejemplo de cuándo sí conviene atomizar más).
4. **Rutas absolutas exactas** de archivo en cada prompt — nunca rutas relativas, nunca dejar que el ejecutor "busque" un archivo.
5. **El bloque de restricción obligatoria** (texto completo abajo) va al inicio de cada prompt.
6. Verificar SIEMPRE con `grep` antes de entregar cualquier ZIP: cero bloques de código, cero residuos en español.
7. Los archivos para Luis (`README.md`, `ADR-*.md`, `00-Tabla-Subtareas.md`) van en español — no confundir audiencias.
8. En prompts de configuración EF: **cada propiedad de cada entidad debe tener su propia llamada explícita a `.Property(e => e.X)`**, incluso las que no necesitan ninguna restricción — ver el bug real documentado en la sección 8.

### Bloque de restricción obligatoria (versión actual, la más reciente)

```
## MANDATORY FILESYSTEM AND SCOPE RESTRICTION — READ FIRST, NO EXCEPTIONS

You are only permitted to read and write files inside this exact
directory and its subdirectories:
`/Users/luisortiz/Desarrollo/DANMAOB/DANMAOB_TISAX/Net/`

You must NOT read, list, search, or access anything outside that
directory — this includes, but is not limited to, `../prompts/`,
`../Docs/`, `../MAUI/`, `../React/`, `../.clineignore`, or any other
sibling or parent folder of `Net/`. If a tool call would touch a path
outside `Net/`, do not make that call, under any circumstance.

You must do ONLY what is explicitly written in the "Task" section
below — nothing more, nothing less. Do not investigate, explore,
verify against external sources, or take any action not explicitly
instructed, even if you believe it would be helpful or more complete.
Do not substitute your own plan for what is written below, even
partially. If something in the Task section seems ambiguous, or you
believe you need more context to complete it, STOP and report that
instead of searching for it yourself.

Every fact you need — file paths, namespaces, type names, member
names, signatures — is already stated in the "Repository facts"
section below. Use those exact values. Do NOT assume, guess, presume,
or reconstruct a namespace, path, or member name from a pattern you
expect or recall, even if it looks obvious or familiar — copy it
exactly as written in "Repository facts". A namespace can always be
derived directly from the file path already given there (it follows
the folder structure); do not substitute a different namespace than
the one that path implies. If a fact you need is not present in
"Repository facts" or "Task", STOP and report that it is missing
instead of filling the gap with your own assumption.

This project targets .NET 10 (`net10.0`), with `LangVersion latest`,
using whatever major version of each NuGet package corresponds to that
target framework (for example, EF Core 10.x, not 6.x/7.x/8.x). Do NOT
default to syntax, APIs, class names, or package versions you recall
from earlier .NET or EF Core versions, even if they feel more familiar
or more commonly seen — some of them do not exist, or behave
differently, on this stack. If a build error suggests you used an API
that does not exist or does not match, your first hypothesis must be
"I used an outdated, pre-.NET-10 pattern" — check that before trying
anything else.

Ordinary, mechanical C# syntax is not covered by the restrictions
above and is expected of you: adding a `using` directive for any type
whose full namespace is already given in "Repository facts" or "Task"
(for example, a type written as `Namespace.Sub.TypeName`) is required
normal code-writing, not investigation, not an assumption, and not a
scope violation — do it without hesitation. The restriction is about
not inventing facts or exploring the repository on your own, not about
withholding standard language mechanics needed to compile the exact
code you were asked to write.

When modifying an existing file, edit only the specific lines the
Task section describes — never rewrite or regenerate the whole file
from scratch, even if you believe your version would be equivalent or
better. If you attempt to fix a compile error or apply a change and it
still fails after 3 attempts, STOP immediately. Do not keep retrying,
re-reading documentation, or exploring further. Instead, report back,
in Spanish, exactly which line(s) you believe need to change and the
exact code you intended to insert or modify there, and wait for the
user's guidance before touching the file again.

Ordinary, mechanical C# syntax is expected of you, as described above
— but adding a `using` directive is the limit of what counts as
"mechanical." Restructuring a method, introducing a new field, a new
private helper, or a different control-flow shape than what the Task
section describes is a design change, not mechanical syntax, even if
it "fixes" a compile error — if fixing an error this way would require
changing more than a `using` directive or the single line the error
points at, STOP and report instead of redesigning it yourself.

This restriction overrides any other instinct, habit, default
behavior, or prior context you may have. Violating it is a critical
failure of this task, regardless of whether the code you eventually
produce happens to be correct.

---
```

**Nota:** si algún prompt futuro toca `DanmaobTisaxDbContext.cs`, agregar además una línea explícita indicando su ruta absoluta completa y aclarando qué partes SÍ y NO se deben tocar — ese archivo específico ha demostrado ser el más propenso a que el modelo local lo "mejore" sin que se le pida (ver sección 8).

## 7. Hallazgos de confiabilidad del ejecutor local

- **Nunca confiar en el autorreporte** de build, pruebas, o estado de archivos — ni cuando suena alarmante ni cuando suena perfecto. Verificación real: Luis corre `dotnet build`/`dotnet test`/`cat`/`find` él mismo y pega el resultado literal. Un caso documentado: un resumen final decía "16 pruebas, corrección sospechosa de un constructor" — la realidad en disco eran 20 pruebas correctas y nada sospechoso tocado. Otro caso: un resumen decía "build exitoso" con formato `net6.0`, en inglés, con proyectos faltantes — resultó ser completamente fabricado, sin ejecución real detrás.
- **El modelo sustituye especificaciones explícitas por patrones memorizados de su entrenamiento** cuando el patrón es muy reconocible (ejemplo: reconstruyó un `SaveChangesInterceptor` con MediatR/`IPublisher` sin que se pidiera, un patrón común en templates de Clean Architecture .NET). Está más correlacionado con el tamaño del modelo que con la redacción del prompt.
- **`DanmaobTisaxDbContext.cs` es el archivo más frágil del proyecto**: en dos ocasiones distintas, sesiones que NO tenían ninguna tarea relacionada con ese archivo lo modificaron de todas formas, rompiendo el filtro de aislamiento por tenant de formas silenciosas (compila limpio, no rompe pruebas existentes, pero deja de filtrar por tenant). Vigilar este archivo específicamente en cualquier prompt futuro que pase cerca de él.
- **`qwen2.5-coder:7b` queda descartado** para tareas agénticas — confirmado en dos integraciones distintas (Cline y la extensión "Ollama Code") que no invoca herramientas reales, solo genera texto con forma de llamada a herramienta. No vale la pena seguir probándolo salvo que se encuentre una variante explícitamente compatible con tool-calling.
- **Sin rutas exactas de archivo, el modelo se pone a explorar todo el repo sin control** — confirmado con casi 2 horas de exploración descontrolada, dos compactaciones de contexto, y modificación no autorizada de un archivo de producción ya verificado. Conclusión definitiva: rutas exactas siempre, sin excepción.
- Un bug real de EF Core (no del modelo): propiedades `string?` de solo lectura, sin setter, nunca referenciadas en la configuración Fluent, se caen silenciosamente de la migración generada aunque el build compile limpio. Regla: configurar cada propiedad explícitamente, sin excepción.
- Un bug de concurrencia real (no del modelo): volver a consultar `ChangeTracker.Entries()` en bucles separados después de que un bucle previo ya mutó el tracker agregando nuevas filas causa "Collection was modified". Se corrigió recolectando en una lista y agregando todo junto al final, y consolidando la lógica compartida entre `SavingChanges`/`SavingChangesAsync` en un único método privado para que no puedan desincronizarse entre sí.
- Regla de proceso: cuando una pieza de código tiene comportamiento en tiempo de ejecución no trivial (interceptores de EF Core, lógica de `ChangeTracker`), no basta con exigir que compile — hay que exigir una prueba de ejecución real lo antes posible, no solo al final de la subtarea de pruebas.

## 8. Tooling — decisiones tomadas

- Modelo principal: **`qwen3.5:9b`**, apuntado directo (sin el `modelfile` personalizado con reglas/skills de "desarrollador DANMAOB") — decisión confirmada tras comparar contra `devstral:24b` y `gpt-oss:20b` (ambos con mejor evidencia de confiabilidad agéntica pero apretados en los 16GB de RAM del Mac M5). No proponer cambio de modelo de nuevo salvo que Luis lo traiga a colación.
- Extensión: se está evaluando **"Ollama Code"** (de Corey Gaspard, envuelve al agente OpenCode) como alternativa a Cline — dio resultados limpios y eficientes en varias subtareas de TS-00-4 y TS-00-3. Tiene un medidor de contexto con indicador de compactación visible, y un interruptor de "Thinking" en su menú de comportamiento. No abre los archivos automáticamente como Cline mientras trabaja — hay que expandir cada fila de la línea de tiempo para ver el diff.
- `qwen2.5-coder:7b`: descartado (ver sección 7).
- SQL Server Developer 2022 corre en Docker en macOS — no se necesita mientras se trabaja solo con el proveedor InMemory de EF Core para pruebas; sí se necesita para migraciones reales y para bootstrap de datos.
- En macOS usar `curl`, no `curl.exe`. Puerto 5000 ocupado por AirPlay Receiver. Las cadenas de conexión en shell deben ir entre comillas simples para que zsh no interprete los `;` como separadores de comando.
- `ConnectionStrings__DefaultConnection` debe estar en `~/.zshrc` (no solo exportada en una terminal puntual) para que la terminal que abre el ejecutor local también la herede.

## 9. Convenciones reales verificadas del repo (no asumir, no inferir)

- Clase base de entidad: `DanmaobTisax.Domain.Common.BaseEntity` — abstracta, `public Guid Id { get; set; } = Guid.NewGuid()`.
- Entidades con alcance de tenant implementan `DanmaobTisax.Domain.Common.ITenantOwned` con `Guid TenantId { get; set; }` (no-nullable) — dispara el filtro global de EF Core vía reflexión en `ApplyTenantQueryFilters` dentro de `DanmaobTisaxDbContext`.
- Construcción de entidades: constructor público con validación `ArgumentException` inline, más un constructor protected/private sin parámetros para EF Core. Sin métodos estáticos `Create()`. Referencia: `Domain/Tenants/Tenant.cs`.
- `Net/Directory.Build.props` fija `WarningsAsErrors=Nullable` — cualquier warning de nullable es error de compilación en toda la solución.
- Namespaces: `DanmaobTisax.Domain`, `DanmaobTisax.Application`, `DanmaobTisax.Infrastructure`, `DanmaobTisax.Api`.
- Resolución de tenant: `ICurrentTenantProvider` (Application/Interfaces) + `CurrentTenantProvider` (Infrastructure/MultiTenancy); el modo `MultiTenant` lanza `NotSupportedException` intencionalmente hasta que exista US-20-2 — nunca evadir esto con un mecanismo temporal inseguro.
- Relaciones de clave foránea sin propiedad de navegación se configuran con el patrón `entity.HasOne<TRelated>(e => null).WithMany().HasForeignKey(...)` — SÍ es válido en EF Core 10 aunque no exista una propiedad de navegación real en la entidad.
- Helper de configuración fail-fast: `RequiredConfigurationValidator.EnsurePresent(configuration, key, guidance)` — reutilizar para cualquier tarea futura que necesite un secreto.
- ADRs de Sprint 1: ADR-0001 Clean Architecture, ADR-0002 Multi-tenancy, ADR-0003 Configuración Segura (TS-00-9). Confirmar siempre la numeración real antes de asignar una nueva.

## 10. Pendientes

- Terminar TS-00-3 (subtareas 12-24 de 24).
- Validación legal pendiente: implicaciones de licenciamiento CC BY-ND 4.0 de VDA ISA para traducción al español de la interfaz, uso de marca VDA/ENX/TISAX, y atribución en documentos exportados.
- Preguntas abiertas: fuente de ISA 6 (sin fuente oficial aún), asignación de módulos a planes comerciales, y mapeo de objetivos de evaluación de prototipos en ISA 2027 (sin validar).
- Después del backend y del frontend en React viene la app móvil en .NET MAUI.

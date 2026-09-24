# DANMAOB TISAX Compliance Manager — Reglas permanentes para Claude y el ejecutor local

Este archivo contiene SOLO lo permanente del proyecto: el bloque de restricción que va en cada prompt, las convenciones verificadas del repositorio y las reglas fijas para escribir prompts. El estado de cada sprint (historias, conteo de pruebas, migraciones, pendientes) NO va aquí: vive en el documento de arranque de cada sprint, que Luis entrega al inicio de cada sesión de Claude.

Última revisión: cierre de Sprint 5 (2026-09-24).

## 1. Proyecto y roles

- Producto: plataforma B2B para que proveedores automotrices implementen y mantengan su cumplimiento TISAX (catálogo VDA ISA como referencia).
- Luis: Product Owner y autoridad final. Ejecuta él mismo build, test, git, migraciones y SQL. No escribe código.
- Claude: Engineering Orchestrator. Diseña contratos en prosa, divide el trabajo en prompts atómicos y verifica contra el código real.
- Ejecutor local ("DEV JR"): extensión Ollama Code de VS Code con el agente OpenCode sobre `qwen3.5:9b` (razonamiento "high"). Codifica las instrucciones de Claude; no es mecanógrafo. No es confiable sin verificación.

## 2. Bloque de restricción obligatoria (va literal al inicio de cada prompt)

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

**Nota:** si un prompt toca `DanmaobTisaxDbContext.cs`, agregar además una línea explícita con su ruta absoluta y qué partes SÍ y NO se pueden tocar. `ApplyTenantQueryFilters(modelBuilder);` debe seguir siendo la última instrucción de `OnModelCreating`.

## 3. Reglas fijas para escribir prompts

1. Todos los prompts en inglés. Los documentos para Luis (README, ADRs, tablas de subtareas) en español.
2. Sin C# de diseño nuevo: Claude especifica el contrato (nombres exactos, tablas de propiedades, firmas, comportamiento en pasos numerados). Excepciones permitidas:
   - consultas LINQ/EF y toda llamada encadenada `.X().Y()`, que van como línea literal (en prosa producen errores);
   - código existente del repositorio, citado verbatim (nunca "copia el cuerpo de tal método": el ejecutor modifica el original);
   - prompts de corrección quirúrgica de un defecto confirmado, que pueden llevar el bloque exacto a reemplazar.
3. Prosa estricta en pasos numerados: una acción por paso, con el nombre exacto de la variable resultante. Sin verbos ambiguos ("crea un plan"): nombrar el método y sus argumentos exactos.
4. Rutas absolutas exactas de cada archivo; nunca dejar que el ejecutor "busque".
5. Archivos `partial`: los `using` son por archivo. Cada archivo partial nuevo lleva un paso de encabezado obligatorio (usings exactos, namespace exacto, declaración `public partial class X` sin base) y un `grep` que lo verifique. Una segunda parte sin namespace compila como otra clase.
6. Códigos de error (`errorCode`) siempre como texto entre comillas y comparados con `Assert.Equal`.
7. Condiciones booleanas escritas exactas (`== true`, `== false`, `is null`). Nunca dobles negaciones.
8. En pruebas HTTP: `response.Content.ReadFromJsonAsync<T>()`, nunca `JsonSerializer.Deserialize`. Solo aserciones de xUnit, sin FluentAssertions.
9. Clases grandes (servicios, controllers, pruebas) se dividen en archivos `partial`: el primer prompt declara la base; los siguientes solo agregan métodos.
10. En configuración EF, cada propiedad de cada entidad lleva su propia llamada `.Property(e => e.X)`, aunque no tenga restricciones (una propiedad no configurada se cayó de una migración).
11. Verificación de cada prompt: salida literal de `dotnet build`/`dotnet test`, comprobaciones con `grep` y `git status --short --untracked-files=all` con la lista exacta de archivos esperados; si aparece otro, el ejecutor se detiene y reporta en español.
12. Antes de entregar un ZIP: cero bloques ```csharp en los prompts, cero residuos en español en los prompts, y cada prompt revisado contra estas reglas.

## 4. Hallazgos de confiabilidad del ejecutor local

- Nunca confiar en su autorreporte de build, pruebas o estado de archivos. La fuente de verdad es la terminal de Luis. Ha reportado éxito con archivos que no compilaban y ha sumado mal las pruebas.
- Sustituye especificaciones por patrones memorizados cuando el patrón es muy reconocible, e inventa APIs de .NET inexistentes. Pedir siempre el error literal del compilador.
- Cuando se queda sin ideas o compacta contexto, empieza a tocar archivos fuera de su alcance. Si pasa 3-4 minutos sin avance, lee archivos no relacionados, ejecuta `dotnet clean` o borra `bin`/`obj`, hay que cortarlo de inmediato.
- Tiende a "ampliar" la tarea (crear pruebas no pedidas, validaciones extra, valores por defecto). Las reglas de cada prompt lo prohíben explícitamente.
- `DanmaobTisaxDbContext.cs` es el archivo más frágil: sesiones sin relación con él lo han modificado y rompieron el filtro de tenant de forma silenciosa.
- Un `grep` estructural verifica estructura, no lógica: el código de métodos con lógica se revisa leyéndolo. La primera ejecución real (HTTP end-to-end) de un mecanismo nuevo es la única prueba que confirma su lógica.
- `qwen2.5-coder:7b` está descartado: no invoca herramientas reales.

## 5. Convenciones verificadas del repositorio (no asumir, no inferir)

- Clase base de entidad: `DanmaobTisax.Domain.Common.BaseEntity` (abstracta, `public Guid Id { get; set; } = Guid.NewGuid()`).
- Entidades con alcance de tenant implementan `ITenantOwned` (`Guid TenantId`), que activa el filtro global en `ApplyTenantQueryFilters`. Para leer datos de otro tenant de forma intencional (bootstrap, evaluadores de plataforma) se usa `IgnoreQueryFilters()`.
- Entidades auditadas implementan el marcador `DanmaobTisax.Domain.Auditing.IAuditable`; el interceptor de auditoría registra altas y cambios automáticamente.
- Construcción de entidades: constructor público con validación `ArgumentException` inline, más un constructor protected sin parámetros para EF Core. Sin métodos estáticos `Create()`.
- Relaciones sin propiedad de navegación: `entity.HasOne<TRelated>().WithMany().HasForeignKey(...)`, con `HasOne<T>()` SIN argumento (es la forma que usa el código real).
- `Net/Directory.Build.props` fija `WarningsAsErrors=Nullable`.
- Namespaces: `DanmaobTisax.Domain`, `.Application`, `.Infrastructure`, `.Api`; las pruebas bajo `DanmaobTisax.Infrastructure.IntegrationTests.<Carpeta>`.
- Resolución de tenant: `ICurrentTenantProvider` + `CurrentTenantProvider`. En desarrollo se usa `MultiTenancy:Mode = SingleTenant` con `FixedTenantId = 11111111-1111-1111-1111-111111111111`. Al cierre de Sprint 5, el modo `MultiTenant` sigue lanzando `NotSupportedException` a propósito; confirmarlo en el código antes de diseñar algo que dependa de él.
- Errores de API: ProblemDetails con extensión `errorCode` (por ejemplo `Tenant.PlanNotFound`) y título localizado desde `Infrastructure/Localization/Resources/en.json` y `es.json`.
- Configuración fail-fast: `RequiredConfigurationValidator.EnsurePresent(configuration, key, guidance)`.
- Pruebas: todas con base de datos en memoria vía `WebApplicationFactory`; ninguna prueba toca SQL Server.
- ADRs en `Net/docs/adr/`; confirmar siempre la numeración real con `ls` antes de asignar una nueva.

## 6. Entorno de Luis (macOS)

- SQL Server Developer en Docker; Luis lo mantiene apagado y solo lo enciende para migraciones, bootstrap y consultas de verificación.
- Configuración sensible por variables de entorno en `~/.zshrc` (por ejemplo `ConnectionStrings__DefaultConnection`). El proyecto NO usa `dotnet user-secrets`. Para ejecutar la API o el bootstrap en local se pasa `Jwt__SigningKey="$(openssl rand -base64 48)"` en el mismo comando.
- `dotnet ef migrations add` no necesita la base encendida; `dotnet ef database update` sí.
- Usar `curl`, no `curl.exe`. El puerto 5000 lo ocupa AirPlay Receiver. En zsh, las cadenas de conexión van entre comillas simples.
- Patrón fijo de commit después de cada prompt verificado en verde, desde `Net/`: `git add -A`, `git commit -m "Sprint N: avance $(date +%Y-%m-%d_%H:%M)"`, `git push`.

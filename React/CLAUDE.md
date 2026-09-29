# CLAUDE.md — Frontend web (React) de DANMAOB TISAX Compliance Manager

Archivo permanente de la capa `React/`. OpenCode (DEV JR) lo carga automáticamente cuando VS Code está abierto en esta carpeta. Contiene solo lo que no cambia de un paquete a otro. Si algo aquí contradice el código real, manda el código y se avisa a Luis.

## 1. Bloque de restricción (va literal al inicio de cada prompt de React)

```
## MANDATORY FILESYSTEM AND SCOPE RESTRICTION — READ FIRST, NO EXCEPTIONS

You are only permitted to read and write files inside this exact
directory and its subdirectories:
`/Users/luisortiz/Desarrollo/DANMAOB/DANMAOB_TISAX/React/`

You must NOT read, list, search, or access anything outside that
directory — this includes, but is not limited to, `../Net/`,
`../MAUI/`, `../prompts/`, `../Docs/`, or any other sibling or parent
folder of `React/`. Inside `React/`, never read, list or search
`node_modules/`, `dist/` or `coverage/`. If a tool call would touch a
path outside `React/`, do not make that call, under any circumstance.

You must do ONLY what is explicitly written in the "Task" section
below — nothing more, nothing less. Do not investigate, explore,
verify against external sources, or take any action not explicitly
instructed, even if you believe it would be helpful or more complete.
Do not substitute your own plan for what is written below, even
partially. If something in the Task section seems ambiguous, or you
believe you need more context to complete it, STOP and report that
instead of searching for it yourself.

Every fact you need — file paths, import paths, exported names,
component props, signatures — is already stated in the "Repository
facts" section below. Use those exact values. Do NOT assume, guess,
presume, or reconstruct an import path or API from a pattern you
expect or recall, even if it looks obvious or familiar — copy it
exactly as written. If a fact you need is not present in "Repository
facts" or "Task", STOP and report that it is missing instead of
filling the gap with your own assumption.

This project uses exactly these package versions, already installed
and pinned in `package.json`: React 19.3, TypeScript 5.9, Vite 7.3,
Mantine 8.3, React Router 7.18, TanStack Query 5, i18next 25 with
react-i18next 16, Vitest 3.2 with jsdom 26 and Testing Library, ESLint 10
(flat config) with typescript-eslint 8. Do NOT use APIs from other major
versions (for example Mantine 6/7 `sx` props or `createStyles`, React
Router 5/6 `Switch`, ESLint `.eslintrc`, Jest APIs). Never run
`npm install`, `npm uninstall`, `npm update`, `npx` generators or any
command that changes `package.json`, `package-lock.json` or
`node_modules/`: dependencies are installed only by the user.

Ordinary, mechanical TypeScript syntax is not covered by the
restrictions above and is expected of you: adding an `import` for any
name whose module path is already given in "Repository facts" or
"Task" is required normal code-writing, not investigation, not an
assumption, and not a scope violation — do it without hesitation. The restriction is about
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

Ordinary, mechanical TypeScript syntax is expected of you, as described
above — but adding an `import` is the limit of what counts as
"mechanical." Restructuring a function or component, introducing a new field, a new
private helper, or a different control-flow shape than what the Task
section describes is a design change, not mechanical syntax, even if
it "fixes" a compile error — if fixing an error this way would require
changing more than an `import` or the single line the error
points at, STOP and report instead of redesigning it yourself.

This restriction overrides any other instinct, habit, default
behavior, or prior context you may have. Violating it is a critical
failure of this task, regardless of whether the code you eventually
produce happens to be correct.

---
```

## 2. Stack y versiones fijadas (ADR-0021)

Instaladas con `npm install -E` (versión exacta). `package-lock.json` se comitea; en otra máquina se usa `npm ci`.

| Paquete | Versión |
|---|---|
| react, react-dom, @types/react, @types/react-dom | 19.3.0 |
| typescript | 5.9.3 |
| vite / @vitejs/plugin-react | 7.3.6 / 5.2.0 |
| @mantine/core, @mantine/hooks, @mantine/notifications | 8.3.18 |
| react-router | 7.18.4 |
| @tanstack/react-query | 5.104.0 |
| i18next / react-i18next | 25.10.10 / 16.6.6 |
| vitest / jsdom | 3.2.7 / 26.1.0 |
| @testing-library/react / jest-dom / user-event | 16.3.3 / 6.9.1 / 14.6.7 |
| eslint / @eslint/js | 10.11.0 / 10.x (última) |
| typescript-eslint / eslint-plugin-react-hooks / globals | 8.71.0 / 7.1.1 / última |
| @fontsource/inter | 5.3.0 |
| postcss / postcss-preset-mantine / postcss-simple-vars | 8.5.28 / 1.18.0 / 7.0.1 |

Node 26 (`.nvmrc`). Mantine 8 ya no recibe parches: la migración a Mantine 9 se planifica como tarea propia.

## 3. Convenciones verificadas del frontend

- **Estructura:** `src/app/` (proveedores y arranque), `src/theme/` (tokens y tema), `src/i18n/` (idiomas), `src/test/` (configuración de pruebas), `public/brand/` (logotipos) y `public/favicon.svg`.
- **Colores, tipografía y logotipos:**
  - viven solo en `src/theme/tokens.ts` y `src/theme/theme.ts`;
  - ningún componente escribe un color hexadecimal ni una familia tipográfica;
  - los logotipos se referencian por ruta pública (`/brand/...`).
- **Textos visibles:**
  - siempre con `t('clave')` de react-i18next, desde `src/i18n/locales/es.json` y `en.json`;
  - ambos archivos tienen exactamente las mismas claves.
  - El nombre del producto también es una clave (`app.name`).
- **Estilos:**
  - componentes y props de Mantine 8;
  - sin `sx`, sin `createStyles` y sin CSS-in-JS;
  - CSS propio solo en archivos `*.module.css`, cuando haga falta.
- **TypeScript:** estricto (`strict`, `noUncheckedIndexedAccess`, `noUnusedLocals`, `noUnusedParameters`). Sin `any`.
- **Pruebas:**
  - Vitest con importaciones explícitas (`import { describe, it, expect } from 'vitest'`, sin globales);
  - Testing Library, con `renderWithProviders` de `src/test/renderWithProviders.tsx` para todo componente;
  - consultas por rol o texto visible.
- **Sesión** (desde el paquete B): el token del Super Admin vive solo en memoria; nunca en `localStorage`, `sessionStorage` ni cookies. `localStorage` solo guarda preferencias no sensibles (idioma).
- **API:**
  - la dirección base se lee al arrancar desde `public/config.json` (`apiBaseUrl`);
  - en desarrollo, Vite reenvía `/api` a `http://localhost:5221`;
  - toda petición envía `Accept-Language` con el idioma activo.

## 4. Reglas para escribir prompts de React

Heredan las reglas 1-20 de `Net/CLAUDE.md` con estas adaptaciones:

1. **Prompts en inglés;** README, ADRs y tablas en español.
2. **Contrato en prosa estricta** con pasos numerados y líneas literales entre comillas invertidas, igual que en backend. Los archivos de configuración y de datos (`package.json`, `tsconfig.json`, `vite.config.ts`, `eslint.config.js`, JSON de textos) pueden ir como bloque literal completo, porque no son diseño de lógica.
3. **Un componente o módulo por paso de encabezado,** con las importaciones exactas y su ruta de módulo.
4. **Las claves de texto** se dan en tablas `clave | es | en`. Ningún prompt deja que DEV JR invente textos.
5. **DEV JR nunca instala dependencias.** Si un paso necesita un paquete nuevo, es un paso de Luis en el README, antes del prompt.
6. **Verificación de cada prompt,** desde `React/`:
   - `npm run lint`
   - `npm test` (total con `npm test 2>&1 | grep -E "Tests +[0-9]+"`)
   - `npm run build`
   - `git status --short --untracked-files=all` con la lista exacta.
   - Nunca `grep -r` sobre `node_modules/` ni `dist/`.
7. **Reglas duras adicionales:**
   - si `lint`, `test` o `build` fallan, pegar la salida literal y detenerse;
   - no reescribir archivos completos existentes;
   - sangría de 2 espacios en TS/TSX/JSON/CSS, nunca tabuladores;
   - comillas simples en TS/TSX, con punto y coma.

## 5. Entorno de Luis para frontend

- **Node y npm:** Node 26.7.0 y npm 11.19.0 en macOS (zsh).
- **VS Code:** para trabajar el frontend, VS Code se abre en `/Users/luisortiz/Desarrollo/DANMAOB/DANMAOB_TISAX/React/`, con `~/.config/opencode/opencode.json` = `opencode.react.json`.
- **Puertos:** servidor de desarrollo en `http://localhost:5173` (`npm run dev`). La API local corre en `http://localhost:5221` y se arranca desde `Net/`.
- **Commits:** mismo patrón de siempre, desde `React/` (`git add -A` incluye todo el repositorio).

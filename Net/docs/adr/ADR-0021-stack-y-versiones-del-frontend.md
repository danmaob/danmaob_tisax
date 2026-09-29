# ADR-0021 — Stack, versiones y conexión del frontend web

- **Estado:** Aceptado
- **Fecha:** 2026-09-29
- **Tarea:** TS-00-10 (E00 — Fundación del frontend web)
- **Relacionados:** ADR-0005 (i18n), ADR-0014 (sesión de plataforma)

## Contexto

`React/` estaba vacía. El producto exige una interfaz empresarial adaptativa, en español e inglés desde el inicio, con identidad visual personalizable por cliente y desplegable en On-Premise, Private Cloud y SaaS. El código lo escribe un modelo local pequeño (DEV JR), que conoce mejor las versiones mayores más difundidas.

## Decisión

1. **Stack:**
   - React 19 + TypeScript + Vite;
   - Mantine 8 como biblioteca de componentes;
   - React Router 7;
   - TanStack Query 5 para los datos del servidor;
   - i18next 25 / react-i18next 16;
   - Vitest 3 + Testing Library;
   - ESLint 10 (flat config) + typescript-eslint 8 + eslint-plugin-react-hooks 7.
2. **Versiones:**
   - Se usa la última versión de la **mayor anterior** cuando la mayor más reciente es desconocida para el equipo o incompatible.
   - TypeScript 7 queda descartado porque `typescript-eslint` exige `<6.1`; se usa 5.9.3.
   - Todo se instala con versión exacta (`npm install -E`), y `package-lock.json` se comitea. La tabla completa está en `React/CLAUDE.md`.
   - Mantine 8 ya no recibe parches: la migración a Mantine 9 se planifica como tarea propia.
   - Ajustes al instalar: `@testing-library/jest-dom` 6.10.0 fue retirada por su autor (cambios incompatibles en una versión menor), así que se usa 6.9.1. ESLint 9 dejó de tener soporte, así que se usa ESLint 10 con `eslint-plugin-react-hooks` 7 (compatible con ESLint 10).
3. **Identidad visual:**
   - Tokens solo en `src/theme/tokens.ts`: primario #245583, con una paleta de 10 tonos donde el tono 6 es el de marca; acento #FAF3C0, solo para fondos y resaltados.
   - Neutros y estados con las paletas de Mantine.
   - Tipografía Inter empaquetada (`@fontsource/inter`), sin depender de internet.
   - Solo tema claro.
   - Logotipos en `public/brand/`, reemplazables por cliente sin tocar código.
4. **Idiomas:** `es` (predeterminado) y `en`, con textos en `src/i18n/locales/*.json`. La preferencia de idioma se guarda en `localStorage` (no es un dato sensible). La API recibe `Accept-Language`, conforme a ADR-0005.
5. **Conexión con la API (C4):**
   - La dirección se lee al arrancar desde `public/config.json` (`apiBaseUrl`): una sola compilación sirve a todos los clientes.
   - En desarrollo, Vite reenvía `/api` a `http://localhost:5221`.
   - En producción con orígenes distintos (SaaS, Private Cloud) se requiere CORS configurable, que queda en TS-00-11 (Sprint 28).
   - No se usan cookies.
6. **Sesión del Super Admin (C5):** solo en memoria. Al recargar la página se vuelve a iniciar sesión, y al expirar el token de 15 minutos se cierra con aviso.
7. **Estructura (C6):** una sola aplicación, con el área `/platform` separada de la futura área del tenant. Los dos tipos de token nunca se mezclan.

## Consecuencias

- Una sola compilación para todas las instalaciones; la personalización se hace por archivos (`config.json`, logotipos) y tokens.
- Deuda planificada: migrar de mayor en Mantine, TypeScript, Vitest y ESLint cuando convenga, con su guía oficial.
- DEV JR trabaja el frontend con VS Code abierto en `React/`, `opencode.react.json` y `React/CLAUDE.md` cargado automáticamente.

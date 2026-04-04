---
name: "Frontend Architecture & Conventions"
description: "Use when working on the Client frontend: enforce the feature-based structure, React + TypeScript conventions, naming rules, services, hooks, and styling described below. Trigger on keywords: frontend, client, features, auth, components, services, React, TypeScript."
applyTo: "Client/src/**"
author: "assistant:agent-customization"
version: "1.0"
---

# Frontend: Architecture & Developer Instructions

Purpose

- Provide concise, deterministic guidance for working inside the frontend (Client) codebase.
- Apply to files under `Client/src/**`.

Scope

- This file only applies to the Client (frontend) portion of the repository. Backend changes (API/) are out-of-scope.

High-level rules (hard)

- Use TypeScript for all new source files under `Client/src/`.
- Follow the feature-based folder layout below; add new features under `src/features/<feature>`.
- Component and Page file names: PascalCase and match the default export (e.g., `LoginPage.tsx` exports default `LoginPage`).
- Folder names and feature keys: lowercase (e.g., `auth`, `chat`, `users`).
- Shared UI components: `Client/src/components/` (reusable primitives only).
- Do NOT add Google OAuth login flows (or other third-party auth flows) unless explicitly requested by the repo owner.

Recommended folder structure (follow this pattern)

- public/ # static files (index.html, favicon, images)
- src/
  - assets/ # images, fonts, icons
  - components/ # shared UI components (Button, Input, Modal)
  - config/ # environment/feature constants (api base, keys)
  - features/ # feature modules (each a self-contained domain)
    - auth/
      - components/ # LoginForm, RegisterForm
      - hooks/ # useLogin, useAuth
      - services/ # auth API client
      - store/ # redux slice / local store
    - chat/
      - components/
      - hooks/
      - services/
      - store/
    - users/
  - hooks/ # shared hooks (useDebounce, useClickOutside)
  - layouts/ # page layout components (MainLayout, AuthLayout)
  - pages/ # top-level page components (HomePage.tsx, LoginPage.tsx)
  - routes/ # router configuration
  - services/ # axios instance, interceptors, API clients
  - store/ # global store setup (Redux/Zustand)
  - styles/ # global CSS/SCSS or Tailwind config
  - utils/ # small helpers (formatDate, validators)
  - App.tsx # root component
  - index.tsx / main.tsx # entry point

Naming & code conventions

- Files:
  - React components & pages → PascalCase (`MyButton.tsx`, `LoginPage.tsx`).
  - Hooks → `use` prefix and camelCase (`useAuth.ts`).
  - Utilities → camelCase (`formatDate.ts`).
  - CSS for a page/component → match component name (e.g., `LoginPage.css`) and colocate or put in `styles/` depending on scope.
- Exports: follow the existing repo style — prefer default exports for components/pages, named exports for utilities.
- Components: use function components with hooks. Example: `const LoginForm: React.FC = () => { ... }; export default LoginForm;`
- Strict typing: prefer explicit props interfaces and avoid `any`.

Services / API

- Centralize HTTP logic in `Client/src/services/api.ts` (Axios instance) with:
  - baseURL coming from env (`REACT_APP_API_URL` for CRA, `VITE_API_URL` for Vite)
  - request/response interceptors for auth token handling
- Store JWT token in `localStorage` under the key `vocabmaster_token` (this repo uses that key).
- Feature-specific API wrappers live in `features/<feature>/services/` and use the central `api` instance.

State management

- Global store setup in `src/store/` (Redux recommended if already present). Feature-local slices live in `features/<feature>/store/`.
- Use Context for small, UI-scoped state only.

Styling

- Global styles in `src/styles/global.css` (or SCSS). Page-specific CSS can live next to the page file.
- Prefer utility-first or component styles consistently across codebase — do not mix widely differing patterns without agreement.

Testing

- Keep unit and integration tests close to the code: `__tests__` or `__tests__` inside feature folders.
- Use React Testing Library + Jest for components; mock API calls in service-layer tests.

Linting & Formatting

- Use ESLint + Prettier. Follow the repository's existing `eslintConfig` in `package.json`.
- Run lint & format before commit and CI (pre-commit hook recommended).

Security & Privacy

- Do not commit secrets to `appsettings` or `.env`. Use environment variables or secret stores.
- Avoid adding third-party login integrations without explicit approval.

Pull Request checklist

- Build succeeds (`npm run build` or `npm run build --prefix Client`).
- No ESLint errors and Prettier applied.
- Manual smoke test of affected pages.
- Remove debug `console.log` statements before merge.

Examples / How to add a new feature

1. `src/features/profile/`
   - `components/ProfileCard.tsx`
   - `hooks/useProfile.ts`
   - `services/profileApi.ts`
   - `store/profileSlice.ts`
2. Add routes in `src/routes/` and link in `App.tsx`.

Trigger phrases (how this instruction is discovered)

- frontend, client, React, TypeScript, features, auth, components, services, login, pages

Example prompts for the assistant (copy-and-paste)

- "Create a new feature module `features/auth` with a `LoginForm` component, `useAuth` hook, and `authApi` service. Follow the project conventions and add a unit test for `LoginForm`."
- "Refactor `src/components/Header.tsx` into the shared `components/` folder and update imports across the app."
- "Add an Axios interceptor to `src/services/api.ts` that reads `vocabmaster_token` from localStorage and injects the `Authorization` header."

Ambiguities / Questions for the repo owner

- Prefer CSS Modules, plain CSS, or a CSS-in-JS approach?
- Preferred global state library: Redux, Zustand, or React Context?
- Do you want default exports for all components, or to migrate to named exports?

Notes

- This instruction deliberately scopes to `Client/src/**` via `applyTo` to avoid loading for backend work.
- The repository currently had Google OAuth removed — this instruction enforces "no third-party OAuth integration" unless explicitly requested.

---

If you'd like, I can:

- Add this same instruction into `.github/` or `{{VSCODE_USER_PROMPTS_FOLDER}}/` instead.
- Create a small checklist script (pre-commit hook) to run lint/build/tests.
- Generate a sample `features/auth` skeleton using this structure.

Please tell me where you'd like this saved (root, `.github/`, or user prompts folder), and whether any rules above should be stricter or relaxed.

---
name: "Code Review"
description: "Checklist-based skill for reviewing pull requests and code changes against workspace conventions. Covers backend (.NET), frontend (React/TypeScript), API design, and cross-layer consistency."
---

# Code Review Skill

Use this skill when reviewing code changes, pull requests, or validating your own work before committing.

---

## Review Process

1. Identify which layers are affected (backend, frontend, API, infra).
2. Run the relevant checklists below.
3. Flag issues by severity: **blocker** (must fix), **warning** (should fix), **nit** (style preference).
4. Verify the change builds and runs without errors.

---

## Backend Checklist (.NET / C#)

### Structure
- [ ] New files are in the correct folder (Models, Controllers, Validators, Dtos, Mappings, Middleware, Data).
- [ ] One class per file. File name matches class name.
- [ ] File-scoped namespaces used.

### Models
- [ ] Navigation properties initialized: `public List<T> Items { get; set; } = [];`
- [ ] Foreign key pattern: explicit `int ForeignId` + nullable `ForeignEntity? Entity`.
- [ ] `[Column(TypeName = "decimal(12,2)")]` on monetary properties.
- [ ] Strings default to `string.Empty`.

### Controllers
- [ ] `[ApiController]` and `[Route("api/[controller]")]` attributes present.
- [ ] Actions return `Task<ActionResult<T>>` — async all the way.
- [ ] `CreatedAtAction` used for POST (201), not `Ok`.
- [ ] Server-controlled fields (Id, Status, dates) are NOT set from client input.
- [ ] XML doc comments on public actions.

### Validation
- [ ] FluentValidation used — NOT data annotations for business rules.
- [ ] Validator registered via assembly scanning (not manually).
- [ ] Invalid input returns `ValidationProblem(ModelState)`.
- [ ] `AddToModelState()` extension used to convert FluentValidation results.

### Error Handling
- [ ] `KeyNotFoundException` thrown for missing resources — NOT manual 404 responses.
- [ ] No bare `catch (Exception)` blocks.
- [ ] No swallowed exceptions.
- [ ] Structured logging used (`ILogger<T>`).

### Data Access
- [ ] `Include()` used for navigation properties needed in the response.
- [ ] Filtering uses `AsQueryable()` with conditional `Where` clauses.
- [ ] `SaveChangesAsync()` called (not sync version).

---

## Frontend Checklist (React / TypeScript)

### Types
- [ ] No `any` types. All props, state, and API responses are explicitly typed.
- [ ] Shared types in `types/`. Component-local types colocated.
- [ ] TypeScript types match backend DTO shapes (camelCase ↔ PascalCase handled by serializer).

### State Management
- [ ] Shared state uses Context + `useReducer`. No Redux/Zustand introduced.
- [ ] Custom context hook includes guard clause (`throw new Error` if context is null).
- [ ] Reducer is a pure function — no side effects.
- [ ] Actions use discriminated union type.
- [ ] Derived/computed values calculated in the Provider, not in individual components.

### Components
- [ ] Functional components only. No class components.
- [ ] Props interface defined. Destructured in function signature.
- [ ] Components that use context call the hook directly — no prop drilling of context data.
- [ ] Loading, error, and empty states all handled.

### Forms
- [ ] Controlled inputs with `onChange` handler.
- [ ] Validation on blur (touched tracking) and on submit.
- [ ] Numeric fields handle empty string: `value === "" ? "" : Number(value)`.
- [ ] Errors displayed inline near the field.

### Data Fetching
- [ ] Uses native `fetch` — no external HTTP libraries.
- [ ] `response.ok` checked before parsing.
- [ ] Async fetch dispatches `FETCH_START`, `FETCH_SUCCESS`, `FETCH_ERROR`.
- [ ] API base URL is consistent (`http://localhost:5000`).

### Styling
- [ ] CSS Modules used (`.module.css`), not inline styles or global CSS for components.
- [ ] Conditional classes use template literals, not classnames library.

---

## API Design Checklist

- [ ] RESTful routes: `api/[controller]` for top-level, `api/{parentId}/[controller]` for nested.
- [ ] Correct HTTP methods: GET (read), POST (create), PUT (update), DELETE (remove).
- [ ] Correct status codes: 200 (OK), 201 (Created), 204 (No Content), 400 (Bad Request), 404 (Not Found).
- [ ] Query parameters are nullable and optional for filtering.
- [ ] Error responses follow RFC 7807 Problem Details format.
- [ ] `.http` test file created/updated for new endpoints.

---

## Cross-Layer Checklist

- [ ] TypeScript interfaces align with C# DTOs/Models.
- [ ] New API endpoints have corresponding frontend fetch functions.
- [ ] Validation rules are consistent: FluentValidation (server) and form validation (client).
- [ ] Both `dotnet build` and `npm run build` pass.
- [ ] No secrets, connection strings, or API keys in source code.
- [ ] CORS configuration updated if new origins are needed.

---

## Severity Guide

| Severity | Criteria | Example |
|----------|----------|---------|
| **Blocker** | Broken functionality, security issue, data loss risk | Missing validation on user input, swallowed exception, exposed secrets |
| **Warning** | Convention violation, potential bug, maintainability concern | Wrong status code, missing error state in UI, sync-over-async |
| **Nit** | Style preference, minor inconsistency | Missing XML doc comment, import ordering |

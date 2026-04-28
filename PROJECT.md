# Project memory (agents)

Quick context for AI assistants working in this repository. **Canonical process:** [docs/AIDLC.md](docs/AIDLC.md).

**Last updated:** 2026-04-28

---

## Architecture overview

- **RecipeApp/** — .NET API (`RecipeAPI`), shared **Core** models, **frontend** React SPA (Vite), Docker Compose for local multi-service runs.
- **Data:** Recipe CRUD uses **Firestore** (see `RecipeApp/specs/architecture.md`); auth via **Firebase Auth** JWTs to the API.
- **ADR index:** [adr/](adr/) — start at numbered ADRs for durable decisions.

---

## Implemented features

### [#67](https://github.com/queen-of-code/alexa-recipe-app/issues/67) — Optional completed-dish recipe photo

- **Status:** Shipped (merged PR [#74](https://github.com/queen-of-code/alexa-recipe-app/pull/74)).
- **Feature folder:** [feature/recipe-completed-photo/](feature/recipe-completed-photo/)
- **What:** Optional JPEG/PNG/WebP upload on **create/edit** in the web app; thumbnail on recipe list; image on detail. Recipe documents carry `completedImageUrl`; bytes live in **Firebase Storage** under per-user paths. `POST /api/values/{userId}` returns **201** with the saved recipe including **`recipeId`** so the client can upload after first save.
- **ADR:** [adr/0001-recipe-completed-photo-firebase-storage.md](adr/0001-recipe-completed-photo-firebase-storage.md)
- **Validate / learn:** [feature/recipe-completed-photo/validate-scorecard.md](feature/recipe-completed-photo/validate-scorecard.md), [feature/recipe-completed-photo/learn-notes.md](feature/recipe-completed-photo/learn-notes.md)

---

## Conventions

- Open PRs against **`master`** (not `main`).
- AI-DLC skills live under **`.claude/skills`** (symlink to `.claude/deps/ai-dlc` submodule).

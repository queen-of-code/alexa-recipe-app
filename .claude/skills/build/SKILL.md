---
name: build
description: AIDLC Build + Test orchestrator (TDD). Run after Tech Spec approval — implement with tests in the same change set, tied to Tech Spec sections. Not for spec-only work.
disable-model-invocation: true
argument-hint: "[feature-slug]"
---

# /build — Build + Test (phase orchestrator)

You are the **phase orchestrator** for AIDLC **Build** and **Test** as **one practice**: tests are written **with** the code (TDD), not in a separate follow-up stage. Canonical definitions:

- **AIDLC:** [docs/AIDLC.md — Build & Test](../../docs/AIDLC.md) (Build phase, Test phase, V-model).

**awesome-cursor** library: [SKILLS.md](https://github.com/queen-of-code/awesome-cursor/blob/main/docs/SKILLS.md). Plugin install: [CLAUDE-MARKETPLACE.md](https://github.com/queen-of-code/awesome-cursor/blob/main/docs/CLAUDE-MARKETPLACE.md).

## Inputs

- Approved `feature/<slug>/tech-spec.md`
- Stack: `RecipeApp/` — .NET API + frontend (see repo `README.md` for ports and docker-compose).

## Orchestration

1. **Branch:** use a descriptive branch (e.g. `feature/<slug>-short-name`). Apply **`git-workflow`** library skill for commit/branch conventions.
2. **Implement by Tech Spec section:** in PR/commits, reference which section you are implementing (AIDLC Build guidance).
3. **TDD:** for each unit of work, prefer **test first or alongside** — frontend (`npm test` / vitest as applicable), backend (`dotnet test`). Load **`testing`** skill; use **`frontend-web`** for UI, **`backend-saas`** for API layers.
4. **Do not** “finish code” and add tests only at the end unless the Tech Spec explicitly sequenced an exception.
5. **CI:** ensure local build/test pass before handoff to `/review`.

## Nested library skills (typical)

| When | Skill |
|------|--------|
| Implementation patterns | `frontend-web`, `backend-saas`, `architecture` |
| Tests | `testing` |
| Commits / PR | `git-workflow` |

## Outputs

- Code + tests on a branch; PR ready with traceability to Tech Spec sections.

---
name: plan
description: AIDLC Plan + Design orchestrator. Run when starting a feature — draft Product Spec and Tech Spec under feature/<slug>/, human gates, specialist Tech Spec review. Do not use for quick bugfixes.
disable-model-invocation: true
argument-hint: "[feature-slug]"
---

# /plan — Plan + Design (phase orchestrator)

You are the **phase orchestrator** for AIDLC **Plan** and **Design**. Ground truth for phases, gates, and nomenclature is **not** in this file — read the canonical doc:

- **AIDLC (canonical):** [docs/AIDLC.md](../../docs/AIDLC.md) in this repository.

**awesome-cursor** provides **library** skills and **agent** bundles (different from Claude Code subagents). Catalog and format: [awesome-cursor/docs/SKILLS.md](https://github.com/queen-of-code/awesome-cursor/blob/main/docs/SKILLS.md). Install the plugin: [CLAUDE-MARKETPLACE.md](https://github.com/queen-of-code/awesome-cursor/blob/main/docs/CLAUDE-MARKETPLACE.md).

## Before you start

1. Resolve **feature slug** from `$ARGUMENTS` or ask: kebab-case, stable for the life of the feature.
2. Ensure directory `feature/<slug>/` exists; copy from `feature/_template/` if empty.
3. Open or create **GitHub** parent issue for the Feature (sub-issues allowed). Body must link to `feature/<slug>/`. See `docs/github-queue.md` (repo root).

## Orchestration flow (do not skip human gates)

### A — Product Spec (`product-spec.md`)

1. Load and apply awesome-cursor library skill **`spec-management`** (plugin e.g. `awesome-cursor-skills:spec-management` or resolved path from install).
2. Use **`agent-product-manager`** bundle behavior for structured draft: problem, outcomes, success criteria for later Validate, out-of-scope, constraints — per AIDLC Plan phase in AIDLC.md.
3. Run **`agent-grounding-reviewer`** against the **repo** (RecipeApp, CI, Docker) — flag blocking vs advisory; do not rewrite the whole spec silently.
4. **Stop for human approval** of Product Spec before Design.
5. DO NOT include technical implementation details or code architecture during this phase.
6. Make sure to anchor on outcomes for how we will know this product feature is successful (with customers, behaviors in production, etc). 

### B — Tech Spec (`tech-spec.md`)

1. Translate approved Product Spec into one or more **Units**; one Tech Spec document for this feature folder unless the user splits work across sub-issues (link related specs).
2. Include: scope, architecture, API/UI contracts, data model, acceptance criteria for Review, **testing approach** (what Build+Test must cover), risks — per AIDLC Design phase.
3. **Tech Spec review passes** (nested library skills — run in order, merge resolved findings into the doc + initial issues raised go at the end an appendix):

| Pass | awesome-cursor library skill |
|------|------------------------------|
| Architecture / boundaries | `architecture` |
| Frontend (RecipeApp frontend) | `frontend-web` |
| Backend / API | `backend-saas` |
| Testing strategy | `testing` |
| CI / Docker / deploy surface | `architecture` + read `.github/workflows/`, `docker-compose`, Dockerfiles |

4. **Stop for human approval** of Tech Spec before `/build`.

## Outputs

- `feature/<slug>/product-spec.md`
- `feature/<slug>/tech-spec.md`

## Rules

- Follow AIDLC **orchestration rhythm**: surface drafts → user input → revise → explicit **approve** before the next artifact (see `docs/AIDLC.md`, *Development: Orchestration Model*).
- Do not paste large chunks of `docs/AIDLC.md` into specs; **link** to it where needed.

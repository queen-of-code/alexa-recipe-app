# Linear (AIDLC transport)

Work is tracked in **Linear** on team **Queen of Code** (`QUE`), project **[Alexa Recipe App](https://linear.app/queen-of-code/project/alexa-recipe-app-d26902350ffe)**. GitHub is **code only** (PRs + CI).

Canonical Linear playbook (upstream): [AI-DLC/docs/LINEAR-AIDLC-PROJECT.md](https://github.com/queen-of-code/AI-DLC/blob/main/docs/LINEAR-AIDLC-PROJECT.md) (also in [`.claude/deps/ai-dlc/docs/LINEAR-AIDLC-PROJECT.md`](../.claude/deps/ai-dlc/docs/LINEAR-AIDLC-PROJECT.md)).

Process definition: [AIDLC.md](AIDLC.md).

---

## Workflow states = phases

| State | AIDLC phase | Skill |
|-------|-------------|-------|
| **Triage** | intake | — |
| **Plan** | Plan — Product Spec | `/plan` |
| **Design** | Design — Tech Spec + slice plan | `/design` |
| **Build+Test** | Build + Test (TDD) | `/build` |
| **Review** | Review gate | `/review` |
| **In Staging** | deployed validation | CI + slice checks |
| **Ship** | Validate + Learn | `/ship` (+ `/learn` on PASS) |
| **Done** | accepted | — |
| **Canceled** | dropped | — |

**No `aidlc_work:*` labels.** Moving the issue to the next state is the human gate.

---

## Specs live in Linear Documents

For new work, specs are **Linear Documents** on the Feature issue:

| Document title | Phase |
|----------------|-------|
| `Product Spec — <feature name>` | Plan |
| `Tech Spec — <feature name>` | Design |
| `Review report — <feature name>` | Review (mirror of PR comments) |
| `Validate scorecard — <feature name>` | Ship |

**ADRs** stay in `docs/adr/` in git.

> **Migration note:** Older demo features may still have files under `feature/<slug>/` until Phase 2 migration completes. New work uses Linear Documents only.

---

## Starting work (two entry points)

### A — From Cursor

1. Use **Linear MCP** to create a Feature issue:
   - `team`: Queen of Code
   - `project`: Alexa Recipe App
   - `state`: Triage (or Plan if scope is already clear)
2. When ready for a phase, move the issue to that state (you or the agent via `save_issue`).
3. Run the phase skill with the issue id, e.g. `/plan QUE-12` or paste the Linear issue URL in chat.
4. The agent reads/writes specs via Linear MCP (`list_documents`, `get_document`, `save_document`).

### B — From Linear

1. Create an issue in **Triage** in the Alexa Recipe App project.
2. Move to **Plan** when ready.
3. Open a **Cursor Cloud Agent** with the issue URL or `QUE-###` in the prompt and ask it to run the matching phase skill.

### Agent dispatch (future)

When **Cursor ↔ Linear delegation** is wired, setting the issue **delegate** to the coding agent will dispatch a run. Until then, start agents manually from Cursor as above.

Interactive agents can **subscribe** to issue state changes (`cursor-subscriptions-subscribe_linear_issue`) instead of polling while waiting on a human gate.

---

## Pull requests

- Include the Linear ticket key in **PR title and body** (e.g. `QUE-12`).
- Link the PR to the Linear issue (native GitHub integration when configured).
- **PR → state sync** (Build+Test → Review on PR ready; → In Staging on merge) is planned for a later phase; move states manually for now.

---

## Secrets

| Secret | Where | Purpose |
|--------|-------|---------|
| *(none required for interactive agents)* | Linear MCP uses Cursor OAuth | Read/write issues and documents |
| `LINEAR_API_KEY` | Cursor Cloud Agents dashboard (future) | Headless PR→state sync, automations |
| `CURSOR_API_KEY` | GitHub Actions (future) | Launch agents from Linear webhooks |

Do **not** commit API keys. GitHub Projects / `aidlc_work:*` automation has been removed from this repo.

---

## Retired: GitHub Projects queue

The previous GitHub Projects v2 board + label-based agent launch is **retired**. See git history for `docs/github-queue.md` and `.github/workflows/aidlc-*.yml` if you need the old pattern.

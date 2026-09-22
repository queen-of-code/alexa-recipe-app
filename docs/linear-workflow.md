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

**Migrated demo features** (2026-09-21): [QUE-5](https://linear.app/queen-of-code/issue/QUE-5/recipe-ingredient-search) (Review), [QUE-6](https://linear.app/queen-of-code/issue/QUE-6/recipe-favoriting) (Design), [QUE-7](https://linear.app/queen-of-code/issue/QUE-7/recipe-completed-photo-optional) (Design). Index: [feature/README.md](../feature/README.md).

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

### Agent dispatch (Cursor Automations)

Six **Cursor Cloud Agent automations** dispatch agents on Linear state changes (or PR open for Review). Config exports live in [docs/cursor-automations/](cursor-automations/):

| Phase | Trigger |
|-------|---------|
| Plan | Linear status → **Plan** |
| Design | Linear status → **Design** |
| Build+Test | Linear status → **Build+Test** |
| Review | GitHub PR **opened** on this repo |
| In Staging | Linear status → **In Staging** |
| Ship | Linear status → **Ship** |

Import or recreate them in the [Cursor Automations dashboard](https://cursor.com/automations). Agents set/clear the Linear `bot-working` label and post a Cursor run URL while working.

You can still start agents manually from Cursor (sections A/B above). Interactive agents can **subscribe** to issue state changes (`cursor-subscriptions-subscribe_linear_issue`) instead of polling while waiting on a human gate.

---

## Pull requests

- Include the Linear ticket key in **PR title and body** (e.g. `QUE-12`).
- Link the PR to the Linear issue (native GitHub integration when configured).
- **PR → state sync:** Build automation requests review on the PR; Review automation triggers on PR open. Merge → **In Staging** is handled by native Linear/GitHub integration when configured; otherwise move states manually.

---

## Secrets

| Secret | Where | Purpose |
|--------|-------|---------|
| *(none required for Linear MCP)* | Cursor OAuth | Read/write issues and documents |
| `AGENT_PROD_URL` | Cursor Cloud Agents → Environment | Deployed app URL for **`/ship`** UI validation |
| `AGENT_PROD_USERNAME` | Cursor Cloud Agents → Environment | Test account login (prod) |
| `AGENT_PROD_PASSWORD` | Cursor Cloud Agents → Environment | Test account password (prod) |
| `LINEAR_API_KEY` | Cursor Cloud Agents dashboard (future) | Headless PR→state sync, automations |
| `CURSOR_API_KEY` | GitHub Actions (future) | Launch agents from Linear webhooks |

Do **not** commit secrets. Full table: [AGENTS.md](../AGENTS.md) → **UI validation environments**.

## Post-deploy testing

No staging. After merge/deploy, move the issue to **Ship** and run **`/ship`**. Agents validate UI against **`$AGENT_PROD_URL`** per [INTERACTIVE-UI-VALIDATION.md](../.claude/deps/ai-dlc/docs/INTERACTIVE-UI-VALIDATION.md).

---

## Retired: GitHub Projects queue

The previous GitHub Projects v2 board + label-based agent launch is **retired**. See git history for `docs/github-queue.md` and `.github/workflows/aidlc-*.yml` if you need the old pattern.

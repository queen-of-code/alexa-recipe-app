# Agent instructions (robots only)

Humans: use [README.md](README.md) and [docs/aidlc-showcase.md](docs/aidlc-showcase.md). This file is for AI assistants.

## Canonical process (this repo)

- **AIDLC:** [docs/AIDLC.md](docs/AIDLC.md) — do not invent process outside this document.

## Skill library (AI-DLC)

- **Upstream catalog & format:** [AI-DLC/docs/SKILLS.md](https://github.com/queen-of-code/AI-DLC/blob/main/docs/SKILLS.md)
- **Submodule:** [`.claude/deps/ai-dlc`](.claude/deps/ai-dlc) — track branch **`main`**. Canonical bundles live under **`skills/`** in that repo. [`.claude/skills`](.claude/skills) is a **symlink** to `deps/ai-dlc/skills` (same tree as `skills/` — no second copy committed in *this* repo).
- **Consumer overrides:** [`.cursor/skills/`](.cursor/skills/) — **Linear-native** deltas for phase orchestrators; these win over the submodule when both exist.
- **Optional global install:** [CLAUDE-MARKETPLACE.md](https://github.com/queen-of-code/AI-DLC/blob/main/docs/CLAUDE-MARKETPLACE.md) — not required here.

## Phase orchestrators

Primary user invocations: **`/plan`**, **`/design`**, **`/build`**, **`/review`**, **`/ship`**.

| Skill | Generic (submodule) | Consumer override (Linear) |
|-------|---------------------|----------------------------|
| `/plan` | [.claude/skills/plan/SKILL.md](.claude/skills/plan/SKILL.md) | [.cursor/skills/plan/SKILL.md](.cursor/skills/plan/SKILL.md) |
| `/design` | [.claude/skills/design/SKILL.md](.claude/skills/design/SKILL.md) | [.cursor/skills/design/SKILL.md](.cursor/skills/design/SKILL.md) |
| `/build` | [.claude/skills/build/SKILL.md](.claude/skills/build/SKILL.md) | [.cursor/skills/build/SKILL.md](.cursor/skills/build/SKILL.md) |
| `/review` | [.claude/skills/review/SKILL.md](.claude/skills/review/SKILL.md) | [.cursor/skills/review/SKILL.md](.cursor/skills/review/SKILL.md) |
| `/ship` | [.claude/skills/ship/SKILL.md](.claude/skills/ship/SKILL.md) | [.cursor/skills/ship/SKILL.md](.cursor/skills/ship/SKILL.md) |

These **orchestrate** AIDLC phases and **pull in** library skills (`architecture`, `frontend-web`, `backend-saas`, `testing`, `git-workflow`, `spec-management`, …) and **agent bundles** as nested playbooks.

**Cursor:** loads `.claude/skills/` and `.cursor/skills/` per [Cursor Agent Skills](https://www.cursor.com/docs/context/skills). Optional rule: [.cursor/rules/aidlc.md](.cursor/rules/aidlc.md).

## Repo layout

- **Specs:** Linear **Documents** on the Feature issue — see Issue tracker below. Migrated features: [feature/README.md](feature/README.md).
- **ADRs:** `docs/adr/`
- **App code:** `RecipeApp/` (dotnet API + frontend; see README for ports)

## Issue tracker (AIDLC)

| Field | Value |
|--------|--------|
| **System** | `linear` |
| **Team** | Queen of Code (`QUE`) |
| **Project** | [Alexa Recipe App](https://linear.app/queen-of-code/project/alexa-recipe-app-d26902350ffe) (`P-QUE-2`) |
| **Work item for a Feature** | Linear issue; ticket pattern `QUE-###` |
| **Phase signal** | Linear **workflow state** (Triage / Plan / Design / Build+Test / Review / In Staging / Ship / Done / Canceled) |
| **Specs** | Linear **Documents** on the Feature issue (`Product Spec — …`, `Tech Spec — …`, etc.); ADRs in `docs/adr/` |
| **Ticket key on every PR** | `QUE-###` in title and body |
| **Agent dispatch** | [Cursor Automations](docs/cursor-automations/) on Linear state changes (+ PR open for Review); manual Cloud Agent still supported |
| **Automation entry points** | [docs/cursor-automations/](docs/cursor-automations/) — six phase automations; see [docs/linear-workflow.md](docs/linear-workflow.md) |

**Notes:** Use **Linear MCP** (`save_issue`, `save_document`, `list_documents`, `get_document`) for tracker I/O. Do not assume `feature/<slug>/` for new Features. Full playbook: [docs/linear-workflow.md](docs/linear-workflow.md) and [LINEAR-AIDLC-PROJECT.md](.claude/deps/ai-dlc/docs/LINEAR-AIDLC-PROJECT.md).

## UI validation environments

Procedure: [.claude/deps/ai-dlc/docs/INTERACTIVE-UI-VALIDATION.md](.claude/deps/ai-dlc/docs/INTERACTIVE-UI-VALIDATION.md) — **Chrome DevTools MCP** (`chrome-devtools`); not Playwright or `cursor-ide-browser` for agent UI evidence.

There is **no staging environment**. After deploy (or for **`/ship`** / **In Staging** / **Ship** Validate), exercise UI success criteria against **production** only.

| Field | Value |
|--------|--------|
| **Deployed test URL** | `$AGENT_PROD_URL` (Cursor Cloud Agent environment secret) |
| **Test login username** | `$AGENT_PROD_USERNAME` (environment secret — never commit) |
| **Test login password** | `$AGENT_PROD_PASSWORD` (environment secret — never commit) |
| **Local dev URL (optional, pre-PR)** | `http://localhost:3000` (web), `http://localhost:8080` (API) — see [README.md](README.md) |

**Agent rules:**

1. Read URL and credentials from the environment variables above; do not ask the human to paste secrets in chat.
2. Confirm **prod deploy / release CI** succeeded before browser-testing deployed behavior (`/ship` deploy gate).
3. Sign in via Chrome DevTools MCP (`fill_form`, `click`, `wait_for`) then exercise Product Spec UI criteria; **`take_screenshot`** for blocking mismatches.
4. Record evidence in the Linear **`Validate scorecard — …`** Document and/or PR comments.

## `/review` dimensions (orchestrator must cover all in scope)

The **`/review`** phase skill is not a shallow CI check. It must drive evaluation of:

1. **Tech Spec compliance** — trace criteria and contracts to code/tests.
2. **Practical testing sufficiency** — right behaviors proven, not coverage theater.
3. **DevOps** — rollout, deploy path, rollback, monitoring/observability vs Tech Spec.
4. **Frontend/UI** — when applicable: **`frontend-web`** skill plus **[INTERACTIVE-UI-VALIDATION.md](.claude/deps/ai-dlc/docs/INTERACTIVE-UI-VALIDATION.md)** (Chrome DevTools MCP). Pre-merge review may use **local dev** URLs from the table above; post-deploy Validate uses **`$AGENT_PROD_URL`** only.
5. **Security** — lightweight pass via **`agent-security-review`** (and **`backend-saas`** for API/auth patterns); see [.claude/skills/agent-security-review/SKILL.md](.claude/skills/agent-security-review/SKILL.md).
6. **Architectural soundness** — per [ARCHITECTURAL-SOUNDNESS.md](.claude/deps/ai-dlc/docs/ARCHITECTURAL-SOUNDNESS.md).

**DevOps dimension** is covered by **`agent-devops-review`**: [.claude/skills/agent-devops-review/SKILL.md](.claude/skills/agent-devops-review/SKILL.md).

**Delivery:** each dimension should post feedback **as GitHub PR comments** (e.g. `### AIDLC Review — Tech Spec`, …) when tools allow; mirror in a Linear Document **`Review report — <feature name>`** on the Feature issue.

**Handoff to `/build`:** after review comments exist, **`/build`** triages each thread — **fix** valid findings or **reply** with why invalid, then **resolve** the conversation. See [.claude/skills/build/SKILL.md](.claude/skills/build/SKILL.md) (“Review feedback loop”).

See [.cursor/skills/review/SKILL.md](.cursor/skills/review/SKILL.md).

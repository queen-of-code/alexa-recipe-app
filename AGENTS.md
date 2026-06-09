# Agent instructions (robots only)

Humans: use [README.md](README.md) and [docs/aidlc-showcase.md](docs/aidlc-showcase.md). This file is for AI assistants.

## Canonical process (this repo)

- **AIDLC:** [docs/AIDLC.md](docs/AIDLC.md) — do not invent process outside this document.

## Skill library (AI-DLC)

- **Upstream catalog & format:** [AI-DLC/docs/SKILLS.md](https://github.com/queen-of-code/AI-DLC/blob/main/docs/SKILLS.md)
- **Submodule:** [`.claude/deps/ai-dlc`](.claude/deps/ai-dlc) — track branch **`main`**. Canonical bundles live under **`skills/`** in that repo. [`.claude/skills`](.claude/skills) is a **symlink** to `deps/ai-dlc/skills` (same tree as `skills/` — no second copy committed in *this* repo).
- **AI-DLC plugin mirror (FYI):** Upstream also ships a **copied** bundle at `plugins/ai-dlc-skills/skills/` for the Claude Code marketplace (symlinks break in the plugin cache). It is kept in sync with `skills/` via `scripts/sync-plugin-skills.sh` in AI-DLC. **Do not** point this repo’s symlink at the plugin path — use `deps/ai-dlc/skills` as today.
- **Optional global install:** [CLAUDE-MARKETPLACE.md](https://github.com/queen-of-code/AI-DLC/blob/main/docs/CLAUDE-MARKETPLACE.md) — not required here; paths under `.claude/skills/<bundle>/SKILL.md` resolve through the symlink.

## Phase orchestrators (from AI-DLC submodule)

Primary user invocations: **`/plan`**, **`/build`**, **`/review`**, **`/ship`** — implemented as Claude Code **skills** (Agent Skills standard). Canonical definitions live in **[AI-DLC](https://github.com/queen-of-code/AI-DLC)**; this repo exposes them via **`.claude/skills`** → submodule (symlink), not a second committed copy.

| Skill | Path |
|-------|------|
| `/plan` | [.claude/skills/plan/SKILL.md](.claude/skills/plan/SKILL.md) |
| `/build` | [.claude/skills/build/SKILL.md](.claude/skills/build/SKILL.md) |
| `/review` | [.claude/skills/review/SKILL.md](.claude/skills/review/SKILL.md) |
| `/ship` | [.claude/skills/ship/SKILL.md](.claude/skills/ship/SKILL.md) |

These **orchestrate** AIDLC phases and **pull in** **library** skills (`architecture`, `frontend-web`, `backend-saas`, `testing`, `git-workflow`, `spec-management`, …) and **agent bundles** (e.g. `agent-product-manager`, `agent-grounding-reviewer`, `agent-reviewer`, `agent-security-review`, `agent-devops-review`, …) as **nested** playbooks — users are not expected to run those slashes separately for the default tutorial path.

**Cursor:** discovers `.claude/skills/` per [Cursor Agent Skills](https://www.cursor.com/docs/context/skills) compatibility paths; same `/` names. Optional rule: [.cursor/rules/aidlc.md](.cursor/rules/aidlc.md).

## Repo layout

- **Features:** `feature/<kebab-slug>/` — copy from [feature/_template/](feature/_template/) for new work.
- **App code:** `RecipeApp/` (dotnet API + frontend; see README for ports).

## Issue tracker (AIDLC)

| Field | Value |
|--------|--------|
| **System** | `github-projects-v2` |
| **Work item for a Feature** | GitHub issue on `queen-of-code/alexa-recipe-app`; URL pattern `github.com/queen-of-code/alexa-recipe-app/issues/NNN` |
| **Phase signal** | "AIDLC phase" single-select field on [project board #6](https://github.com/users/queen-of-code/projects/6); label `aidlc_work:unstarted` is the automation trigger; `aidlc_work:in_progress` means an agent run is active |
| **Parent ↔ `feature/<slug>/`** | Issue body includes `AIDLC feature folder: feature/<kebab-slug>/` |
| **Automation entry points** | **[`aidlc-agent-launch.yml`](.github/workflows/aidlc-agent-launch.yml)** — **`issues.labeled`** (`aidlc_work:unstarted`) launches the Cursor Cloud Agent against the phase read from Projects v2. **[`aidlc-board-label-sync.yml`](.github/workflows/aidlc-board-label-sync.yml)** — optional **`workflow_dispatch`** / **`repository_dispatch`** to apply **`aidlc_work:unstarted`** after a board move ([docs/github-queue.md](docs/github-queue.md)). **[`aidlc-phase-advance.yml`](.github/workflows/aidlc-phase-advance.yml)** — on merged phase PRs, advances board + label. |

**Notes:** GitHub Actions does **not** support **`on: projects_v2_item`**; org webhooks emit **`projects_v2_item`** per [official payloads](https://docs.github.com/en/webhooks/webhook-events-and-payloads?actionType=edited#projects_v2_item). Cursor agents clear **`aidlc_work:in_progress`** via **`$AIDLC_GH_CALLBACK_TOKEN`** (Cursor dashboard only — never commit). See [docs/github-queue.md](docs/github-queue.md).

## GitHub queue

- Parent Feature issue + sub-issues OK. Link body to `feature/<slug>/`. AIDLC phase field on the board drives automation — [docs/github-queue.md](docs/github-queue.md).

## `/review` dimensions (orchestrator must cover all in scope)

The **`/review`** phase skill is not a shallow CI check. It must drive evaluation of:

1. **Tech Spec compliance** — trace criteria and contracts to code/tests.
2. **Practical testing sufficiency** — right behaviors proven, not coverage theater.
3. **DevOps** — rollout, deploy path, rollback, monitoring/observability vs Tech Spec.
4. **Frontend/UI** — when applicable: **`frontend-web`** skill plus **browser MCP** (e.g. Cursor IDE browser tools) to exercise flows, usability, and design compliance; if MCP unavailable, ship a manual browser script and mark gaps.
5. **Security** — lightweight pass via **`agent-security-review`** (and **`backend-saas`** for API/auth patterns); see [.claude/skills/agent-security-review/SKILL.md](.claude/skills/agent-security-review/SKILL.md).

**DevOps dimension** is covered by **`agent-devops-review`**: [.claude/skills/agent-devops-review/SKILL.md](.claude/skills/agent-devops-review/SKILL.md).

**Delivery:** each dimension should post feedback **as GitHub PR comments** (e.g. `### AIDLC Review — Tech Spec`, …) when tools allow; mirror in `feature/<slug>/review-report.md`.

**Handoff to `/build`:** after review comments exist, **`/build`** triages each thread — **fix** valid findings or **reply** with why invalid, then **resolve** the conversation. See [.claude/skills/build/SKILL.md](.claude/skills/build/SKILL.md) (“Review feedback loop”).

See [.claude/skills/review/SKILL.md](.claude/skills/review/SKILL.md).

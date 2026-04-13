# Agent instructions (robots only)

Humans: use [README.md](README.md) and [docs/aidlc-showcase.md](docs/aidlc-showcase.md). This file is for AI assistants.

## Canonical process (this repo)

- **AIDLC:** [docs/AIDLC.md](docs/AIDLC.md) — do not invent process outside this document.

## Skill library (awesome-cursor)

- **Upstream catalog & format:** [awesome-cursor/docs/SKILLS.md](https://github.com/queen-of-code/awesome-cursor/blob/main/docs/SKILLS.md)
- **Submodule (source for updates):** [vendor/awesome-cursor](vendor/awesome-cursor) — sync into `.claude/skills/` with [`scripts/sync-awesome-cursor-skills.sh`](scripts/sync-awesome-cursor-skills.sh)
- **Optional global install:** [CLAUDE-MARKETPLACE.md](https://github.com/queen-of-code/awesome-cursor/blob/main/docs/CLAUDE-MARKETPLACE.md) — not required for this repo; prefer **vendored** paths under `.claude/skills/<bundle>/SKILL.md`.

## Phase orchestrators (vendored from awesome-cursor)

Primary user invocations: **`/plan`**, **`/build`**, **`/review`**, **`/ship`** — implemented as Claude Code **skills** (Agent Skills standard). Canonical definitions live in **awesome-cursor**; this repo **copies** them into `.claude/skills/` for idempotent tutorials.

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

## GitHub queue

- Parent Feature issue + sub-issues OK. Link body to `feature/<slug>/`. Manual Project Status — [docs/github-queue.md](docs/github-queue.md).

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

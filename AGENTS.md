# Agent instructions (robots only)

Humans: use [README.md](README.md) and [docs/aidlc-showcase.md](docs/aidlc-showcase.md). This file is for AI assistants.

## Canonical process (this repo)

- **AIDLC:** [docs/AIDLC.md](docs/AIDLC.md) — do not invent process outside this document.

## Skill library (ground in awesome-cursor)

- **Bundle format & IDs:** [awesome-cursor/docs/SKILLS.md](https://github.com/queen-of-code/awesome-cursor/blob/main/docs/SKILLS.md)
- **Install plugin (Claude Code):** [CLAUDE-MARKETPLACE.md](https://github.com/queen-of-code/awesome-cursor/blob/main/docs/CLAUDE-MARKETPLACE.md)
- **Naming:** Plugin skills are namespaced, e.g. `awesome-cursor-skills:architecture`. That is **not** a Claude Code **subagent** (subagents live under `.claude/agents/` per Anthropic docs).

## Phase orchestrators (this repo)

Primary user invocations: **`/plan`**, **`/build`**, **`/review`**, **`/ship`** — implemented as Claude Code **skills** (Agent Skills standard):

| Skill | Path |
|-------|------|
| `/plan` | [.claude/skills/plan/SKILL.md](.claude/skills/plan/SKILL.md) |
| `/build` | [.claude/skills/build/SKILL.md](.claude/skills/build/SKILL.md) |
| `/review` | [.claude/skills/review/SKILL.md](.claude/skills/review/SKILL.md) |
| `/ship` | [.claude/skills/ship/SKILL.md](.claude/skills/ship/SKILL.md) |

These **orchestrate** AIDLC phases and **pull in** awesome-cursor **library** skills (`architecture`, `frontend-web`, `backend-saas`, `testing`, `git-workflow`, `spec-management`, …) and **library agent bundles** (`agent-product-manager`, `agent-grounding-reviewer`, `agent-reviewer`, …) as **nested** playbooks — users are not expected to run those slashes separately for the default tutorial path.

**Cursor:** discovers `.claude/skills/` per [Cursor Agent Skills](https://www.cursor.com/docs/context/skills) compatibility paths; same `/` names. Optional rule: [.cursor/rules/aidlc.md](.cursor/rules/aidlc.md).

## Repo layout

- **Features:** `feature/<kebab-slug>/` — copy from [feature/_template/](feature/_template/) for new work.
- **App code:** `RecipeApp/` (dotnet API + frontend; see README for ports).

## GitHub queue

- Parent Feature issue + sub-issues OK. Link body to `feature/<slug>/`. Manual Project Status — [docs/github-queue.md](docs/github-queue.md).

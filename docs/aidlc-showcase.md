# AI-DLC tutorial showcase (alexa-recipe-app)

This repository demonstrates **Melissa’s AIDLC** (AI Development Lifecycle) on a real small app. **Canonical process** lives in **external-brain** — always treat this as the source of truth:

- **[AIDLC.md](https://github.com/queen-of-code/external-brain/blob/main/AIDLC.md)** — phases, gates, V-model, nomenclature.

**Reusable prompts and agents** come from **awesome-cursor**:

- **[docs/SKILLS.md](https://github.com/queen-of-code/awesome-cursor/blob/main/docs/SKILLS.md)** — skill vs agent bundles, `aidlc_phases`, tool contracts.
- **Install** the skills plugin: [CLAUDE-MARKETPLACE.md](https://github.com/queen-of-code/awesome-cursor/blob/main/docs/CLAUDE-MARKETPLACE.md) (Claude Code). For Cursor, use the [awesome-cursor install](https://github.com/queen-of-code/awesome-cursor/blob/main/README.md) script or clone the repo; skills land under `~/.cursor/skills/`.

## What you run (phase orchestrators)

You primarily invoke **four phase skills** — they **orchestrate** the lifecycle and **call into** domain skills (frontend, backend, testing, …) for you:

| Slash | AIDLC phases | Artifacts |
|-------|----------------|-----------|
| `/plan` | Plan + Design | `feature/<slug>/product-spec.md`, `tech-spec.md` |
| `/build` | Build + Test (TDD) | Code + tests on a branch |
| `/review` | Test gate + Review | Review report vs Tech Spec |
| `/ship` | Validate + Learn | `validate-scorecard.md`, `learn-notes.md` |

Skill definitions: [.claude/skills/](../.claude/skills/) (same paths work in Cursor via [compatibility loading](https://www.cursor.com/docs/context/skills)).

## Claude Code vs Cursor

| Tool | Phase skills | Library skills |
|------|----------------|----------------|
| **Claude Code** | Project `.claude/skills/{plan,build,review,ship}/` → `/plan` etc. | Plugin: `awesome-cursor-skills:<skill-id>` per marketplace install |
| **Cursor** | Loads `.claude/skills/` automatically; type `/` in Agent chat | Same Agent Skills standard; global `~/.cursor/skills/` + plugin paths |

Vendor references: [Claude Code — Skills](https://code.claude.com/docs/en/skills), [Cursor — Agent Skills](https://www.cursor.com/docs/context/skills).

## GitHub

Use a Project board as your queue: [github-queue.md](github-queue.md).

## Terminology (do not conflate)

- **Phase skill** (`/plan`, …) — this repo’s orchestrator prompts.
- **awesome-cursor library skill** — e.g. `testing`, `architecture` (bundle in Melissa’s library).
- **awesome-cursor `type: agent` bundle** — composed library roles (e.g. `agent-reviewer`); not the same as “Cursor Agent” or Claude **subagents**.

See [AGENTS.md](../AGENTS.md) for assistant-facing detail.

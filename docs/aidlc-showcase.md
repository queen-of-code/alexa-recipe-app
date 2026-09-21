# AI-DLC tutorial showcase (alexa-recipe-app)

This repository demonstrates **Melissa’s AIDLC** (AI Development Lifecycle) on a real small app. **Canonical process for this tutorial** is in-repo:

- **[AIDLC.md](AIDLC.md)** — phases, gates, V-model, nomenclature.

**Reusable prompts and agents** come from **[AI-DLC](https://github.com/queen-of-code/AI-DLC)** (public skills library):

- **[docs/SKILLS.md](https://github.com/queen-of-code/AI-DLC/blob/main/docs/SKILLS.md)** — skill vs agent bundles, `aidlc_phases`, tool contracts.
- **Install:** git submodule at [`.claude/deps/ai-dlc`](../.claude/deps/ai-dlc) + symlink [`.claude/skills`](../.claude/skills). Repo-specific **Linear** overrides in [`.cursor/skills/`](../.cursor/skills/).

## What you run (phase orchestrators)

| Slash | AIDLC phases | Artifacts (Linear-native) |
|-------|----------------|---------------------------|
| `/plan` | Plan | Linear Document `Product Spec — …` |
| `/design` | Design | Linear Document `Tech Spec — …` + slice sub-issues |
| `/build` | Build + Test (TDD) | **Open PR + green CI** (`QUE-###` in title); triage review comments after `/review` |
| `/review` | Test gate + Review | **PR comments** per dimension + Linear Document `Review report — …` |
| `/ship` | Validate | Linear Document `Validate scorecard — …`; then **`/learn`** for ADRs |

Skill definitions: generic [.claude/skills/](../.claude/skills/) + overrides [.cursor/skills/](../.cursor/skills/).

## Claude Code vs Cursor

| Tool | Phase skills | Library skills |
|------|----------------|----------------|
| **Claude Code** | `.claude/skills/{plan,design,build,review,ship}/` | Plugin: `ai-dlc-skills:<skill-id>` |
| **Cursor** | `.claude/skills/` + `.cursor/skills/` overrides | Same Agent Skills standard |

Vendor references: [Claude Code — Skills](https://code.claude.com/docs/en/skills), [Cursor — Agent Skills](https://www.cursor.com/docs/context/skills).

## Linear (work queue)

Features are tracked in **Linear**, not GitHub Projects:

- **Team:** Queen of Code (`QUE`)
- **Project:** [Alexa Recipe App](https://linear.app/queen-of-code/project/alexa-recipe-app-d26902350ffe)
- **Playbook:** [linear-workflow.md](linear-workflow.md)
- **Upstream guide:** [LINEAR-AIDLC-PROJECT.md](https://github.com/queen-of-code/AI-DLC/blob/main/docs/LINEAR-AIDLC-PROJECT.md)

Start from **Cursor** (Linear MCP: create issue in Triage) or **Linear** (create issue, open Cloud Agent with `QUE-###`).

## Terminology (do not conflate)

- **Phase skill** (`/plan`, …) — orchestrator; consumer overrides in `.cursor/skills/` for this repo.
- **AI-DLC library skill** — e.g. `testing`, `architecture` (bundle in the public library).
- **AI-DLC `type: agent` bundle** — composed library roles; not the same as “Cursor Agent” or Claude subagents.

See [AGENTS.md](../AGENTS.md) for assistant-facing detail.

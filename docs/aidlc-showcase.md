# AI-DLC tutorial showcase (alexa-recipe-app)

This repository demonstrates **Melissa’s AIDLC** (AI Development Lifecycle) on a real small app. **Canonical process for this tutorial** is in-repo:

- **[AIDLC.md](AIDLC.md)** — phases, gates, V-model, nomenclature.

**Reusable prompts and agents** come from **[AI-DLC](https://github.com/queen-of-code/AI-DLC)** (public skills library):

- **[docs/SKILLS.md](https://github.com/queen-of-code/AI-DLC/blob/main/docs/SKILLS.md)** — skill vs agent bundles, `aidlc_phases`, tool contracts.
- **Install** the skills plugin: [CLAUDE-MARKETPLACE.md](https://github.com/queen-of-code/AI-DLC/blob/main/docs/CLAUDE-MARKETPLACE.md) (Claude Code). For Cursor, clone with submodules so `.claude/skills` (symlink into [`.claude/deps/ai-dlc`](../.claude/deps/ai-dlc)) resolves; or use the [AI-DLC install](https://github.com/queen-of-code/AI-DLC/blob/main/README.md) script for global `~/.cursor/skills/`.

## What you run (phase orchestrators)

You primarily invoke **four phase skills** — they **orchestrate** the lifecycle and **call into** domain skills (frontend, backend, testing, …) for you:

| Slash | AIDLC phases | Artifacts |
|-------|----------------|-----------|
| `/plan` | Plan + Design | `feature/<slug>/product-spec.md`, `tech-spec.md` |
| `/build` | Build + Test (TDD) | **Open PR + green CI** + code/tests; **after `/review`**, triage PR comments (fix or reply + resolve) |
| `/review` | Test gate + Review | **PR comments** per dimension (spec, tests, DevOps, UI, security) + `review-report.md`; then hand off to `/build` for triage |
| `/ship` | Validate + Learn | `validate-scorecard.md`, `learn-notes.md` |

Skill definitions: [.claude/skills/](../.claude/skills/) (same paths work in Cursor via [compatibility loading](https://www.cursor.com/docs/context/skills)).

## Claude Code vs Cursor

| Tool | Phase skills | Library skills |
|------|----------------|----------------|
| **Claude Code** | Project `.claude/skills/{plan,build,review,ship}/` → `/plan` etc. | Plugin: `ai-dlc-skills:<skill-id>` per marketplace install |
| **Cursor** | Loads `.claude/skills/` automatically; type `/` in Agent chat | Same Agent Skills standard; global `~/.cursor/skills/` + plugin paths |

Vendor references: [Claude Code — Skills](https://code.claude.com/docs/en/skills), [Cursor — Agent Skills](https://www.cursor.com/docs/context/skills).

## GitHub

Use a Project board as your queue: [github-queue.md](github-queue.md).

## Terminology (do not conflate)

- **Phase skill** (`/plan`, …) — orchestrator prompts from AI-DLC via submodule + `.claude/skills` symlink.
- **AI-DLC library skill** — e.g. `testing`, `architecture` (bundle in the public library).
- **AI-DLC `type: agent` bundle** — composed library roles (e.g. `agent-reviewer`); not the same as “Cursor Agent” or Claude **subagents**.

See [AGENTS.md](../AGENTS.md) for assistant-facing detail.

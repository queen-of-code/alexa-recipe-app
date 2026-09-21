# Cursor skills (consumer overrides)

| Path | Role |
|------|------|
| [`.claude/skills/`](../.claude/skills/) | Generic AIDLC orchestrators + library (symlink → AI-DLC submodule) |
| **`.cursor/skills/`** (this directory) | **Repo-specific overrides** — Linear-native phase skills win over the submodule |

**Override rule:** When both trees define the same skill name (`plan`, `design`, `build`, `review`, `ship`), **this directory wins**.

Phase skills here encode:

- Specs as **Linear Documents** on Feature issues (not `feature/<slug>/`)
- Team **Queen of Code** (`QUE`), project **Alexa Recipe App**
- Workflow states as phase signals (see [docs/linear-workflow.md](../../docs/linear-workflow.md))

Update generic skills in [AI-DLC](https://github.com/queen-of-code/AI-DLC), then bump the submodule pointer in this repo.

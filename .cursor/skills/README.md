# Cursor skills path

Cursor discovers skills from [`.claude/skills/`](../.claude/skills/) per [Cursor Agent Skills — Skill directories](https://www.cursor.com/docs/context/skills).

**Vendored copies:** `.claude/skills/` is populated from [AI-DLC](https://github.com/queen-of-code/AI-DLC) via [git submodule `vendor/ai-dlc`](../vendor/ai-dlc) and [`scripts/sync-ai-dlc-skills.sh`](../scripts/sync-ai-dlc-skills.sh). Phase orchestrators (`/plan`, `/build`, `/review`, `/ship`) and library/agent bundles live there — edit upstream in AI-DLC, then sync and commit here.

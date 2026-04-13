# Cursor skills path

Cursor discovers skills from [`.claude/skills/`](../.claude/skills/) per [Cursor Agent Skills — Skill directories](https://www.cursor.com/docs/context/skills).

**Vendored copies:** `.claude/skills/` is populated from [awesome-cursor](https://github.com/queen-of-code/awesome-cursor) via [git submodule `vendor/awesome-cursor`](../vendor/awesome-cursor) and [`scripts/sync-awesome-cursor-skills.sh`](../scripts/sync-awesome-cursor-skills.sh). Phase orchestrators (`/plan`, `/build`, `/review`, `/ship`) and library/agent bundles live there — edit upstream in awesome-cursor, then sync and commit here.

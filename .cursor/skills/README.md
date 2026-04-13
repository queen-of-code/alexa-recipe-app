# Cursor skills path

Cursor discovers skills from [`.claude/skills/`](../.claude/skills/) per [Cursor Agent Skills — Skill directories](https://www.cursor.com/docs/context/skills).

**Single tree:** `.claude/skills` is a **symlink** to [`../../.claude/deps/ai-dlc/skills`](../../.claude/deps/ai-dlc/skills) (the [AI-DLC](https://github.com/queen-of-code/AI-DLC) git submodule). No duplicated skill files are committed in this repo — update skills in AI-DLC, then bump the submodule pointer.

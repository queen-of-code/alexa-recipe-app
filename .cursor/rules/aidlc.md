---
description: AIDLC tutorial repo — use phase skills /plan /build /review /ship; ground process in external-brain AIDLC.md; library skills from awesome-cursor plugin.
alwaysApply: false
---

# AIDLC (alexa-recipe-app)

- **Process:** [external-brain AIDLC.md](https://github.com/queen-of-code/external-brain/blob/main/AIDLC.md)
- **Library:** [awesome-cursor SKILLS.md](https://github.com/queen-of-code/awesome-cursor/blob/main/docs/SKILLS.md)
- **Phase orchestrators:** project `.claude/skills/{plan,build,review,ship}/SKILL.md` — invoke with `/plan`, `/build`, `/review`, `/ship` in Agent chat (Cursor loads `.claude/skills/` per Agent Skills compatibility).
- **Human vs robot docs:** [README.md](../README.md) for people; [AGENTS.md](../AGENTS.md) for assistants only.

When the user is doing feature work, prefer walking them through the phase skills in order unless they explicitly skip a gate.

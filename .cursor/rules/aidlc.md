---
description: AIDLC tutorial repo — use phase skills /plan /design /build /review /ship; Linear-native transport; ground process in docs/AIDLC.md.
alwaysApply: false
---

# AIDLC (alexa-recipe-app)

- **Process:** [docs/AIDLC.md](../docs/AIDLC.md)
- **Linear workflow:** [docs/linear-workflow.md](../docs/linear-workflow.md)
- **Library:** [AI-DLC SKILLS.md](https://github.com/queen-of-code/AI-DLC/blob/main/docs/SKILLS.md)
- **Phase orchestrators:** [.cursor/skills/](../skills/) overrides (Linear) + [.claude/skills/](../.claude/skills/) generic — invoke `/plan`, `/design`, `/build`, `/review`, `/ship`
- **Human vs robot docs:** [README.md](../README.md) for people; [AGENTS.md](../AGENTS.md) for assistants only

When the user is doing feature work, prefer walking them through the phase skills in order unless they explicitly skip a gate.

**Specs** for new work live as **Linear Documents** on the Feature issue (`QUE-###`). Use **Linear MCP** for tracker I/O.

**Post-deploy UI testing:** prod only via **`AGENT_PROD_URL`**, **`AGENT_PROD_USERNAME`**, **`AGENT_PROD_PASSWORD`** — see **`AGENTS.md` → UI validation environments** and [INTERACTIVE-UI-VALIDATION.md](../.claude/deps/ai-dlc/docs/INTERACTIVE-UI-VALIDATION.md).

**`/review`** posts **GitHub PR comments** per review dimension and mirrors to a Linear **Review report** Document. **`/build`** triages each thread: fix valid items, or reply why invalid and resolve.

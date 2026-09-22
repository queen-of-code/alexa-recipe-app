---
name: review
description: AIDLC Review for alexa-recipe-app — six PR comment dimensions; mirror to Linear Document. Overrides generic submodule skill.
type: skill
aidlc_phases: [review, test]
tags: [aidlc, orchestrator, review, linear, pr]
requires: []
author: Melissa Benua
created_at: 2026-09-21
updated_at: 2026-09-21
---

# /review — Test gate + Review — Linear override

**Overrides:** [.claude/skills/review/SKILL.md](../../.claude/skills/review/SKILL.md). **Ground truth:** [docs/AIDLC.md](../../docs/AIDLC.md), [AGENTS.md](../../AGENTS.md).

## Inputs

- Linear Feature issue in **Review** state
- **Tech Spec** Document on the issue (`Tech Spec — <feature name>`) — source of truth for “done”
- Open PR for this work (linked via `QUE-###`)
- CI results

## Delivery (preferred)

1. Post **one top-level PR comment per dimension** (`### AIDLC Review — Tech Spec`, …, `### AIDLC Review — Architectural Soundness`).
2. **Mirror** the same content in a Linear Document: `Review report — <feature name>` on the Feature issue (`save_document`).
3. If GitHub tools unavailable, write the Linear Document first and ask the human to paste to the PR.

Run all **six dimensions** from the generic skill (spec, testing, DevOps, UI/browser, security, architectural soundness).

**Frontend/UX pass:** [.claude/deps/ai-dlc/docs/INTERACTIVE-UI-VALIDATION.md](../../.claude/deps/ai-dlc/docs/INTERACTIVE-UI-VALIDATION.md) — Chrome DevTools MCP. Pre-merge: optional **local** URLs from **`AGENTS.md` → UI validation environments**. Do not use prod for review unless the change is already deployed and the human explicitly asks.

## Handoff

Tell the human to run **`/build`** to triage review threads, then move to **In Staging** / **Ship** per [docs/linear-workflow.md](../../docs/linear-workflow.md) when review is resolved.

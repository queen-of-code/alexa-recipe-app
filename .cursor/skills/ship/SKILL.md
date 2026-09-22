---
name: ship
description: AIDLC Validate (/ship) for alexa-recipe-app — scorecard as Linear Document. Overrides generic submodule skill.
type: skill
aidlc_phases: [validate]
tags: [aidlc, orchestrator, validate, ship, linear]
requires: []
author: Melissa Benua
created_at: 2026-09-21
updated_at: 2026-09-21
---

# /ship — Validate — Linear override

**Overrides:** [.claude/skills/ship/SKILL.md](../../.claude/skills/ship/SKILL.md). **Ground truth:** [docs/AIDLC.md](../../docs/AIDLC.md), [AGENTS.md](../../AGENTS.md).

## Inputs

- Linear Feature issue in **Ship** state
- **Product Spec** and **Tech Spec** Documents on the issue
- Merged or ship-candidate PR(s) with `QUE-###`
- Deploy/CI status

## Orchestration

Follow generic `/ship` with these repo defaults:

1. **Deploy gate:** Confirm production deploy succeeded (e.g. release / `prod_deploy` workflow) before UI testing.
2. **UI validation:** [.claude/deps/ai-dlc/docs/INTERACTIVE-UI-VALIDATION.md](../../.claude/deps/ai-dlc/docs/INTERACTIVE-UI-VALIDATION.md) against **`$AGENT_PROD_URL`** with **`$AGENT_PROD_USERNAME`** / **`$AGENT_PROD_PASSWORD`** — see **`AGENTS.md` → UI validation environments**. No staging; prod only.
3. **Scorecard:** Product Spec success criteria; 90% default threshold.

## Outputs

- Linear Document: `Validate scorecard — <feature name>` on the Feature issue
- Comment on issue with PASS/FAIL summary and proposed return phase on failure
- On PASS: hand off to **`/learn`** for ADRs and docs; human moves issue to **Done**

Do **not** use `feature/<slug>/validate-scorecard.md` for new work.

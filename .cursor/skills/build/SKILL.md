---
name: build
description: AIDLC Build + Test for alexa-recipe-app — implement per Linear Tech Spec; PR with QUE-### key. Overrides generic submodule skill.
type: skill
aidlc_phases: [build, test]
tags: [aidlc, orchestrator, build, test, linear, tdd]
requires: []
author: Melissa Benua
created_at: 2026-09-21
updated_at: 2026-09-21
---

# /build — Build + Test — Linear override

**Overrides:** [.claude/skills/build/SKILL.md](../../.claude/skills/build/SKILL.md). **Ground truth:** [docs/AIDLC.md](../../docs/AIDLC.md), [AGENTS.md](../../AGENTS.md).

## Inputs

- Linear Feature or **slice** issue in **Build+Test** state
- Approved **Tech Spec** Document on the parent Feature (`get_document` / `list_documents`)
- **If re-entering after `/review`:** open PR with `AIDLC Review — …` comments

## Orchestration

Follow generic `/build` (TDD, git-workflow, testing, frontend-web, backend-saas) with these deltas:

1. **Branch:** e.g. `cursor/que-12-short-name-a708` — include ticket key.
2. **PR title and body** must include **`QUE-###`** and link the Linear issue.
3. Implement by Tech Spec section; reference sections in commits/PR description.
4. **Open PR** with **green CI** before handoff — same bar as AIDLC.
5. **Review feedback loop:** triage every `AIDLC Review — …` thread (fix or reply + resolve) — same as generic skill.

## Outputs

- Open PR linked to Linear issue
- Comment on Linear issue with PR link when done

## Handoff

Human moves issue to **Review** (or automation later on PR ready). Do not run `/review` or `/ship` in the same run unless explicitly asked.

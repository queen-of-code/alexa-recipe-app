---
name: ship
description: AIDLC Validate + Learn orchestrator. Run when Review is done — scorecard vs Product Spec, learnings (ADRs, docs, retro). Merge-ready checklist.
disable-model-invocation: true
argument-hint: "[feature-slug]"
---

# /ship — Validate + Learn (phase orchestrator)

You are the **phase orchestrator** for AIDLC **Validate** and **Learn**. Full definition:

- **AIDLC:** [AIDLC.md — Validate (+ Learn)](https://github.com/queen-of-code/external-brain/blob/main/AIDLC.md) (scorecard, 90% default threshold, Learn outputs).

**awesome-cursor:** use **`architecture`**, **`git-workflow`**, and **`agent-learn`** bundle if available in your install for ADR/doc capture — see [SKILLS.md](https://github.com/queen-of-code/awesome-cursor/blob/main/docs/SKILLS.md) and `skills/agents/` in the repo.

## Inputs

- `feature/<slug>/product-spec.md` (success criteria)
- `feature/<slug>/tech-spec.md`
- Shipped or ship-candidate implementation; PR link

## Orchestration

1. **Validate:** For each success criterion in the Product Spec, record pass/fail and evidence. Compute an overall score; default **90%** gate per AIDLC — document if the team uses another threshold.
2. On failure: cite criteria, evidence, and **which phase to return to** (Plan, Design, Build, Test, Review) per AIDLC.
3. **Learn:** ADRs for significant decisions, README/docs updates, retrospective note on what differed from the Tech Spec — per AIDLC Learn; capture in `learn-notes.md` or linked ADRs under `docs/` or repo ADR folder if you add one.

## Outputs

- `feature/<slug>/validate-scorecard.md`
- `feature/<slug>/learn-notes.md` (or split ADRs + short pointer file)
- Merge / deploy checklist; close Feature issue when done.

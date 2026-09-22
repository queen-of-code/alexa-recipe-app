---
name: design
description: AIDLC Design phase for alexa-recipe-app — Tech Spec as a Linear Document; slices born inert. Overrides generic submodule skill.
type: skill
aidlc_phases: [design]
tags: [aidlc, orchestrator, design, linear, tech-spec]
requires: []
author: Melissa Benua
created_at: 2026-09-21
updated_at: 2026-09-21
---

# /design — Design (Tech Spec) — Linear override

**Overrides:** [.claude/skills/design/SKILL.md](../../.claude/skills/design/SKILL.md). **Ground truth:** [docs/AIDLC.md](../../docs/AIDLC.md), [AGENTS.md](../../AGENTS.md), [LINEAR-AIDLC-PROJECT.md](../../.claude/deps/ai-dlc/docs/LINEAR-AIDLC-PROJECT.md).

## Before you start

1. Resolve the **Linear Feature issue** (`QUE-###` or URL).
2. **Read** the approved Product Spec via Linear MCP: Document titled `Product Spec — <feature name>`. If missing or not approved, **stop** — run **`/plan`** or get explicit approval in chat.
3. Issue state should be **Design**.
4. **Do not** write to `feature/<slug>/tech-spec.md` for new work.

## Spec artifact (Linear Document)

- **Title:** `Tech Spec — <feature name>`
- **Parent:** Feature issue
- Include: scope, architecture, API/UI contracts, data model, acceptance criteria for Review, testing approach, risks — per AIDLC Design.
- **Architecturally-relevant changes** must follow [ARCHITECTURAL-SOUNDNESS.md](../../.claude/deps/ai-dlc/docs/ARCHITECTURAL-SOUNDNESS.md) (state machines, sequence diagrams, invariants).

## Slice plan (critical)

Per [LINEAR-AIDLC-PROJECT.md](../../.claude/deps/ai-dlc/docs/LINEAR-AIDLC-PROJECT.md):

- **1 slice:** no sub-issues; Feature itself is the unit.
- **2–9 slices:** create sub-issues in **backlog / not-started state** — **never `Build+Test`**. Chain with `blockedBy`. Leave **un-delegated**.
- **10+:** split into two related Features.

Document the slice plan in the Tech Spec Document.

## Review passes

Run architecture, frontend-web, backend-saas, testing passes (same as generic skill); merge findings into the Tech Spec Document.

## Outputs

- Linear Document: `Tech Spec — <feature name>`
- Sub-issues for slices (inert, backlog state) when applicable
- ADR drafts under `docs/adr/` when required

## Handoff

After human approves Tech Spec: move issue to **Build+Test** to start build (human gate). For multi-slice Features, release only the first unblocked slice into Build+Test per upstream Linear guide.

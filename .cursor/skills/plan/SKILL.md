---
name: plan
description: AIDLC Plan phase for alexa-recipe-app — Product Spec as a Linear Document on the Feature issue. Overrides generic submodule skill for Linear-native transport.
type: skill
aidlc_phases: [plan]
tags: [aidlc, orchestrator, plan, linear, product-spec]
requires: []
author: Melissa Benua
created_at: 2026-09-21
updated_at: 2026-09-21
---

# /plan — Plan (Product Spec) — Linear override

**Overrides:** [.claude/skills/plan/SKILL.md](../../.claude/skills/plan/SKILL.md) (generic). **Ground truth:** [docs/AIDLC.md](../../docs/AIDLC.md), [AGENTS.md](../../AGENTS.md), [docs/linear-workflow.md](../../docs/linear-workflow.md).

## Before you start

1. Resolve the **Linear Feature issue** from `$ARGUMENTS` (e.g. `QUE-12`, issue URL, or ask).
2. Confirm issue is on project **Alexa Recipe App**, team **Queen of Code**. State should be **Plan** (or move it there after Triage intake).
3. If no issue exists, use **Linear MCP** `save_issue` to create one (`team`: Queen of Code, `project`: Alexa Recipe App, `state`: Triage or Plan) and capture `QUE-###`.
4. **Do not** create `feature/<slug>/` for new work.

## Spec artifact (Linear Document)

- **Title:** `Product Spec — <feature name>` (human-readable name from the issue title).
- **Parent:** attach to the Feature issue via `save_document` (`issue`: the Feature id).
- **Read/write:** Linear MCP — `list_documents`, `get_document`, `save_document`.

## Orchestration

Follow the generic `/plan` orchestration (spec-management, agent-product-manager, agent-grounding-reviewer) but:

1. Draft content in chat, then persist to the **Linear Document** (not a git file).
2. **Conversation first** — ask before treating the spec as ready.
3. **Stop for human approval** of the Product Spec.
4. On handoff: tell the human to move the issue to **Design** and run **`/design`** (or start a Cloud Agent for that phase).

## Outputs

- Linear Document: `Product Spec — <feature name>` on the Feature issue.
- Optional: comment on the issue summarizing decisions and next gate.

## Rules

- No technical implementation or API design — that is **`/design`**.
- Link `docs/AIDLC.md` in the document where useful; do not paste the whole process doc.

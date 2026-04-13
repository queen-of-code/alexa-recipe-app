---
name: review
description: AIDLC Test (human gate) + Review orchestrator. Run when implementation and tests exist — sufficiency vs Tech Spec, spec trace, CI. Not a substitute for human reviewer sign-off.
disable-model-invocation: true
argument-hint: "[feature-slug]"
---

# /review — Test gate + Review (phase orchestrator)

You are the **phase orchestrator** for the **human gate after Build+Test** (“are the right things tested?”) and AIDLC **Review**. Canonical text:

- **AIDLC:** [AIDLC.md — Test & Review](https://github.com/queen-of-code/external-brain/blob/main/AIDLC.md)

**awesome-cursor** agent bundle for this phase: **`agent-reviewer`** (Review Orchestrator in library naming — [skills/agents/agent-reviewer](https://github.com/queen-of-code/awesome-cursor/tree/main/skills/agents/agent-reviewer)). Also use **`testing`**, **`architecture`**, **`git-workflow`** as needed. Library index: [SKILLS.md](https://github.com/queen-of-code/awesome-cursor/blob/main/docs/SKILLS.md).

## Inputs

- `feature/<slug>/tech-spec.md` (approved)
- Current branch / PR; CI results (GitHub Actions) if available

## Orchestration

1. **Test sufficiency (AIDLC Test → Review gate):** Check acceptance criteria in Tech Spec against tests — **not** coverage vanity. Apply **`testing`** skill. Document gaps or approve sufficiency; human must agree before treating Review as complete.
2. **Review phase:** Apply **`agent-reviewer`** bundle behavior: CI green, trace implementation to Tech Spec sections, API contracts, regressions, PR clarity.
3. Output a **structured review report** (markdown in `feature/<slug>/review-report.md` or PR comment as user prefers).

## Outputs

- `review-report.md` or equivalent; list of blocking vs non-blocking items; human sign-off still required per AIDLC.

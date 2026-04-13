---
name: review
description: AIDLC Test gate + Review — Tech Spec, tests, DevOps, browser UX (if UI), and security pass (secrets, auth, dependency pinning). Run after Build+Test; not a substitute for human sign-off.
disable-model-invocation: true
argument-hint: "[feature-slug]"
---

# /review — Test gate + Review (phase orchestrator)

You are the **phase orchestrator** for the **human gate after Build+Test** (“are the right things tested?”) and AIDLC **Review**. Canonical text:

- **AIDLC:** [docs/AIDLC.md — Test & Review](../../docs/AIDLC.md)

**awesome-cursor** library: apply **`agent-reviewer`**, **`testing`**, **`architecture`**, **`frontend-web`**, **`backend-saas`**, **`git-workflow`** as needed. Index: [SKILLS.md](https://github.com/queen-of-code/awesome-cursor/blob/main/docs/SKILLS.md).

There is **no separate “security” skill bundle** in the library today — run a dedicated **security reviewer pass** inside this orchestrator using the checklist in **§5** and **`backend-saas`** for auth/API patterns.

## Inputs

- `feature/<slug>/tech-spec.md` (approved) — **source of truth for “done”**
- Current branch / PR; **CI** (GitHub Actions) results
- Diff vs `master` — infer whether **frontend/UI**, **API**, **infra**, or mixed

## Orchestration — five review dimensions

Produce **`feature/<slug>/review-report.md`** with **five numbered sections** below (use those headings). Each section: **findings**, **blocking vs advisory**, **what to fix or verify**.

### 1. Tech Spec compliance

- Walk **`tech-spec.md`**: acceptance criteria, API/UI contracts, data model, out-of-scope boundaries.
- For each major item: **where in code/tests/PR** it is satisfied; **gaps** if not.
- Apply **`agent-reviewer`** behavior for spec-to-implementation trace and regression risk.

### 2. Practical testing sufficiency

- Apply **`testing`** skill: judge whether tests prove **the right behaviors** — not coverage percentage as a vanity metric.
- Distinguish **unit** vs **integration** appropriateness; flag missing cases that the Tech Spec implies.
- **CI must be green**; flag flakiness or skipped tests.
- Human gate: document whether **test sufficiency** is acceptable before treating Review as complete.

### 3. DevOps — rollout, deploy, monitoring

- Apply **`architecture`** + this repo’s **delivery surface**: `docker-compose`, `Dockerfile`, `.github/workflows`, Cloud Run / deployment docs in README or Tech Spec.
- Evaluate: **safe rollout** (ordering, migrations, config, secrets), **rollback path**, **feature flags** if specified.
- **Monitoring & operations:** logs, health checks, metrics, alerts — per Tech Spec or flag **explicit deferrals** as advisory gaps.
- If the feature changes runtime behavior without observability hooks the Spec requires — **blocking** finding.

### 4. Frontend / UX — when the change touches UI

**Trigger this section** if the PR touches **Website**, **Razor**, **wwwroot**, **CSS/JS**, **SPA/frontend** under `RecipeApp`, or **Tech Spec** lists UI acceptance criteria.

1. Apply **`frontend-web`** for code patterns, accessibility basics, and alignment with stated UI/UX in the Tech Spec.
2. **Browser or computer-use validation (required when UI is in scope):**
   - If **Cursor IDE browser MCP** tools are available (e.g. `browser_navigate`, `browser_snapshot`, screenshots) **or** another **browser / computer-use MCP** (e.g. Chrome DevTools MCP): **exercise the feature in a real browser** — happy path, obvious edge cases, layout/visual checks, and **usability** (clarity, errors, empty states as relevant).
   - Compare observed behavior to **Tech Spec** and **design compliance** (copy, hierarchy, components — as specified in Spec; call out deviations with severity).
   - Capture **evidence** in the review report (snapshot refs, screenshot paths, or short descriptions of what was exercised).
3. If **no** browser MCP or automated UI runner is available in this session: write a **step-by-step manual browser test script** for the human, and mark **“Browser MCP validation not run — manual execution required”** as **advisory** (or **blocking** if the team policy says UI must be agent-verified before merge).

### 5. Security review (lightweight, obvious issues)

Act as a **security reviewer**: not a full pentest — catch **obvious** mistakes before merge. Use **`backend-saas`** for API/auth patterns; scan the **PR diff** and touched files.

| Area | What to check |
|------|----------------|
| **Secrets & credentials** | No committed API keys, tokens, private keys, or live connection strings; no `sk-`, AWS key patterns, or `BEGIN OPENSSH/PRIVATE KEY` in source; `.env` / secrets files not tracked unless documented as safe placeholders; no passwords in config checked in. |
| **Auth & access** | Handlers/endpoints match **authorization** expectations in Tech Spec; no missing `[Authorize]` / policy where required; **IDOR**-style risks (user A accessing user B’s resources); public endpoints intentional. |
| **Dependencies** | **npm:** `package-lock.json` committed when `package.json` changes; avoid unbounded `latest` where team pins; **NuGet:** sensible version constraints; **Docker:** `FROM` images pinned or tagged (not bare `latest` without justification). |
| **Web & data** | Razor/HTML: unsafe raw HTML where user content flows; SQL/command construction — parameterization; CSRF for state-changing forms where applicable to this stack. |
| **Config & debug** | Debug/verbose modes not enabled for production paths; no default admin credentials introduced. |

Flag each finding **blocking** (must fix) vs **advisory** (follow-up). If uncertain, mark **advisory** and recommend human or SAST review.

## Synthesis

- Consolidate **blocking** items (must fix before ship) vs **advisory**.
- PR clarity: does the description list what changed and what reviewers should focus on?

## Outputs

- **`feature/<slug>/review-report.md`** with sections **1–5** (omit **§4** only if UI is genuinely out of scope per Tech Spec — state why; **§5** runs for every review unless the PR is docs-only — state if N/A).
- Optional: duplicate summary as PR comment.
- **Human sign-off** still required per AIDLC — this report feeds the human reviewer, not replaces them.

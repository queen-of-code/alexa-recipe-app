---
name: review
description: AIDLC Test gate + Review — runs five review passes (spec, tests, DevOps, UI, security); post feedback as GitHub PR comments; then hand off to /build for triage. Not a substitute for human sign-off.
disable-model-invocation: true
argument-hint: "[feature-slug]"
---

# /review — Test gate + Review (phase orchestrator)

You are the **phase orchestrator** for the **human gate after Build+Test** (“are the right things tested?”) and AIDLC **Review**. Canonical text:

- **AIDLC:** [docs/AIDLC.md — Test & Review](../../docs/AIDLC.md)

**awesome-cursor** library: apply **`agent-reviewer`**, **`testing`**, **`architecture`**, **`frontend-web`**, **`backend-saas`**, **`git-workflow`** as needed. Index: [SKILLS.md](https://github.com/queen-of-code/awesome-cursor/blob/main/docs/SKILLS.md).

There is **no separate “security” skill bundle** in the library today — run the **security reviewer pass** inside this orchestrator using **§5** and **`backend-saas`** for auth/API patterns.

## How this phase interacts with GitHub (preferred)

Each review **dimension** below behaves like a **dedicated reviewer**: it should produce **actionable feedback**.

**Preferred delivery:** post feedback **directly on the open PR** as **GitHub comments** so the **build** phase can respond in-thread.

- **One top-level PR comment per dimension** (§1–§5), using a clear header, e.g. `### AIDLC Review — Tech Spec`, `### AIDLC Review — Testing`, … so threads stay scannable.
- Within each comment, list findings with **blocking** vs **advisory** and file references.
- If **GitHub MCP**, **`gh pr comment`**, or the GitHub API is **not** available: write the same content into **`feature/<slug>/review-report.md`** and tell the user to paste or post manually — but **prefer automation** when tools exist.

Also write or update **`feature/<slug>/review-report.md`** as a **durable mirror** of the same content (copy from posted comments or generate once and post from the file).

## Inputs

- `feature/<slug>/tech-spec.md` (approved) — **source of truth for “done”**
- **Open PR** URL or number for this branch; **CI** (GitHub Actions) results
- Diff vs `master` — infer whether **frontend/UI**, **API**, **infra**, or mixed

## Orchestration — five review dimensions (each posts feedback)

Run each pass **as if** a separate reviewer; consolidate only at the end for the summary comment if useful.

### 1. Tech Spec compliance

- Walk **`tech-spec.md`**: acceptance criteria, API/UI contracts, data model, out-of-scope boundaries.
- For each major item: **where in code/tests/PR** it is satisfied; **gaps** if not.
- Apply **`agent-reviewer`** behavior for spec-to-implementation trace and regression risk.
- **Output:** PR comment `AIDLC Review — Tech Spec` + section in `review-report.md`.

### 2. Practical testing sufficiency

- Apply **`testing`** skill: judge whether tests prove **the right behaviors** — not coverage percentage as a vanity metric.
- Distinguish **unit** vs **integration** appropriateness; flag missing cases that the Tech Spec implies.
- **CI must be green**; flag flakiness or skipped tests.
- **Output:** PR comment `AIDLC Review — Testing` + section in `review-report.md`.

### 3. DevOps — rollout, deploy, monitoring

- Apply **`architecture`** + this repo’s **delivery surface**: `docker-compose`, `Dockerfile`, `.github/workflows`, Cloud Run / deployment docs in README or Tech Spec.
- Evaluate: **safe rollout**, **rollback path**, **feature flags** if specified; **monitoring** (logs, health, metrics, alerts) per Tech Spec.
- **Output:** PR comment `AIDLC Review — DevOps` + section in `review-report.md`.

### 4. Frontend / UX — when the change touches UI

**Trigger** if the PR touches **Website**, **Razor**, **wwwroot**, **CSS/JS**, **SPA/frontend** under `RecipeApp`, or **Tech Spec** lists UI acceptance criteria.

1. Apply **`frontend-web`** for code patterns, accessibility basics, and alignment with stated UI/UX in the Tech Spec.
2. **Browser / computer-use validation** when UI is in scope: use browser MCP if available; capture evidence; compare to Tech Spec for **usability and design compliance**.
3. If no browser MCP: **manual browser test script** in the comment; mark validation pending.
4. **Output:** PR comment `AIDLC Review — Frontend/UX` + section in `review-report.md`. Omit only if UI is out of scope — state **N/A** in a short comment or skip with explanation on the PR.

### 5. Security review (lightweight, obvious issues)

Act as a **security reviewer**; use **`backend-saas`** for API/auth patterns; scan **PR diff** and touched files.

| Area | What to check |
|------|----------------|
| **Secrets & credentials** | No committed keys/tokens/PEM/live connection strings; patterns like `sk-`, AWS keys; `.env` misuse. |
| **Auth & access** | Authorization vs Tech Spec; missing `[Authorize]` / policy; **IDOR** risks; intentional public endpoints. |
| **Dependencies** | **npm** lockfile with package changes; avoid careless `latest`; **NuGet**; **Docker** `FROM` pinning. |
| **Web & data** | Unsafe HTML, parameterization, CSRF where applicable. |
| **Config & debug** | Debug in prod, default creds. |

**Output:** PR comment `AIDLC Review — Security` + section in `review-report.md`. For docs-only PRs, state **N/A** briefly.

## After posting — handoff to **build**

When review feedback is on the PR (and mirrored in `review-report.md`), **stop** — the next step is **`/build`** (build orchestrator), **not** another full review pass.

The **build** orchestrator **triages** each review thread: fix valid issues or **reply** with why a finding is invalid and **resolve** the conversation. See [.claude/skills/build/SKILL.md](../build/SKILL.md) § “Review feedback loop”.

## Synthesis (optional)

- One short **summary** PR comment listing **blocking** vs **advisory** counts if helpful.

## Outputs

- **GitHub PR comments** for §1–§5 (preferred).
- **`feature/<slug>/review-report.md`** mirror.
- **Human sign-off** still required per AIDLC.

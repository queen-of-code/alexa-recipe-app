# GitHub Issues + Projects v2 (AIDLC automation)

Work is tracked on a **GitHub Projects v2** board with an **"AIDLC phase"** single-select field.

**`aidlc_work:unstarted`** is what wakes **`issues.labeled`** → [`aidlc-agent-launch.yml`](../.github/workflows/aidlc-agent-launch.yml). You get that label by: applying it on the issue manually; running **[`aidlc-board-label-sync.yml`](#apply-unstarted-label-event-driven)** once per board move (no polling); letting **[`aidlc-phase-advance.yml`](../.github/workflows/aidlc-phase-advance.yml)** apply it after you merge a phase PR; or (organization Projects only) driving **`repository_dispatch`** from a **`projects_v2_item`** webhook relay. Actions does **not** support **`on: projects_v2_item`** in workflow YAML ([gh-aw#25336](https://github.com/github/gh-aw/issues/25336)); webhook availability is [org-only in the official schema](https://docs.github.com/en/webhooks/webhook-events-and-payloads?actionType=edited#projects_v2_item).

Board: [AIDLC — alexa-recipe-app (project #6)](https://github.com/users/queen-of-code/projects/6)

---

## One-time setup

### 1 — GitHub secrets and variables

| Where | Name | Value |
|-------|------|-------|
| Repo Settings → Secrets → Actions | `CURSOR_API_KEY` | API key from [cursor.com/dashboard/integrations](https://cursor.com/dashboard/integrations) → **API Keys** |
| Repo Settings → Secrets → Actions | `AIDLC_PROJECT_PAT` | Personal access token with **`repo`** and **`project`** (read/write). Used by **`aidlc-agent-launch.yml`** and **`aidlc-board-label-sync.yml`** to call GraphQL for your user-owned Projects v2 board (same limitation documented in the agent launch workflow comments). Not the same secret as **`AIDLC_GH_CALLBACK_TOKEN`**. |
| Repo Settings → Variables → Actions (optional) | `AIDLC_PROJECT_OWNER` | `queen-of-code` |
| Repo Settings → Variables → Actions (optional) | `AIDLC_PROJECT_NUMBER` | `6` |

### 2 — Cursor Cloud Agents dashboard secret

Go to [cursor.com/dashboard/cloud-agents](https://cursor.com/dashboard/cloud-agents) → select repository `queen-of-code/alexa-recipe-app` → **Environment** → add secret:

| Name | Value |
|------|-------|
| `AIDLC_GH_CALLBACK_TOKEN` | GitHub PAT with **`repo`** scope (issues read/write). This lets the Cursor agent clear `aidlc_work:in_progress` when it finishes — without polling. |

> **Do not add this token to GitHub secrets or commit it anywhere.**

### 3 — Repository labels

Create these two labels in repo Settings → Labels:

| Label | Color suggestion |
|-------|-----------------|
| `aidlc_work:unstarted` | `#0075ca` (blue) |
| `aidlc_work:in_progress` | `#e4e669` (yellow) |

### 4 — Project board "AIDLC phase" field

The board already has an **"AIDLC phase"** single-select field. Confirm it has these options (in order):

`Idea` · `Plan` · `Design` · `Build` · `Review` · `Ship` · `Done` · `Won't do`

---

## AIDLC phase → automation mapping

| Board phase | AIDLC phase | Cursor skill invoked | Auto-creates PR |
|-------------|-------------|---------------------|----------------|
| Idea | — | Not automated | — |
| **Plan** | Plan | `/plan` (`skills/plan/SKILL.md`) | No |
| **Design** | Design | `/design` | No |
| **Build** | Build + Test | `/build` (`skills/build/SKILL.md`) | **Yes** |
| **Review** | Review | `/review` (`skills/review/SKILL.md`) | No |
| **Ship** | Validate + Learn | `/ship` (`skills/ship/SKILL.md`) | No |
| Done | — | Not automated | — |
| Won't do | — | Not automated | — |

Canonical phase definitions: [AIDLC.md](AIDLC.md).

---

## Per-feature workflow

### Start a feature

1. Create a GitHub issue. In the body include:
   ```
   AIDLC feature folder: feature/<kebab-slug>/
   ```
2. Add the issue to the [AIDLC project board](https://github.com/users/queen-of-code/projects/6).
3. Set the **AIDLC phase** field to `Idea` until you're ready to start.

### Trigger an agent run

1. Move the board card or set **AIDLC phase** so the issue is in **Plan**, **Design**, **Build**, **Review**, or **Ship**.
2. Ensure **`aidlc_work:unstarted`** — see **[Apply unstarted label (event-driven)](#apply-unstarted-label-event-driven)** or add the label manually.

That label fires **`aidlc-agent-launch.yml`**:
- Reads the current AIDLC phase from the board
- Swaps the label to `aidlc_work:in_progress`
- Launches a Cursor Cloud Agent with the right skill prompt
- Posts a tracking comment with a link to the agent

### While the agent runs

- The issue carries `aidlc_work:in_progress` — do not re-apply `unstarted` until the agent finishes.
- Watch progress at [cursor.com/agents](https://cursor.com/agents) or via the tracking comment link.

### When the agent finishes

The agent itself calls back to the GitHub API using `$AIDLC_GH_CALLBACK_TOKEN` to remove `aidlc_work:in_progress`. No polling required. It will also post a summary comment on the issue.

For **Build** phase the agent auto-creates a PR. Review and merge that PR before moving to **Review**.

### Manual trigger

To launch an agent without waiting for labels (e.g. for testing or re-runs):

```bash
gh workflow run aidlc-agent-launch.yml \
  -f issue_number=123 \
  -f phase=plan
```

---

## Apply unstarted label (event-driven)

Workflow: **[`aidlc-board-label-sync.yml`](../.github/workflows/aidlc-board-label-sync.yml)** — **`workflow_dispatch`** (input **`issue_number`**) or **`repository_dispatch`** (event type **`apply_aidlc_unstarted_if_actionable`**, **`client_payload.issue_number`**). Each run reads the linked issue’s **AIDLC phase** from Projects v2 via GraphQL; if the phase is actionable (Plan / Design / Build / Review / Ship), clears **`aidlc_work:in_progress`**, applies **`aidlc_work:unstarted`** (no cron, no snapshot file).

Patterns:

### After moving a card (manual or script)

```bash
gh workflow run aidlc-board-label-sync.yml -f issue_number=123
```

### From automation (`repository_dispatch`)

Uses a PAT with permission for **[repository dispatch events](https://docs.github.com/en/rest/repos/repos#create-a-repository-dispatch-event)**:

```bash
curl -L -X POST \
  -H "Accept: application/vnd.github+json" \
  -H "Authorization: Bearer $GH_TOKEN_FOR_DISPATCH" \
  https://api.github.com/repos/queen-of-code/alexa-recipe-app/dispatches \
  -d '{"event_type":"apply_aidlc_unstarted_if_actionable","client_payload":{"issue_number":123}}'
```

### Organization Projects + webhook

GitHub delivers **`projects_v2_item`** to **organization webhooks** ([availability](https://docs.github.com/en/webhooks/webhook-events-and-payloads?actionType=edited#projects_v2_item)), not **`on:` Actions triggers**. Middleware can forward **`POST /repos/{owner}/{repo}/dispatches`** so each field edit becomes one dispatch — that is genuinely event-driven.

### Merge path (already wired)

**[`aidlc-phase-advance.yml`](../.github/workflows/aidlc-phase-advance.yml)** updates the board **and** **`aidlc_work:unstarted`** when you merge an AIDLC phase PR.

---

## Why not native Project events in Actions?

- **`projects_v2_item` workflow trigger** — invalid YAML today ([discussion](https://github.com/github/gh-aw/issues/25336)).
- **`project_card`** — classic Projects only ([docs](https://docs.github.com/en/actions/using-workflows/events-that-trigger-workflows#project_card)).
- **`issues.labeled`** — reliable launcher once **`aidlc_work:unstarted`** exists.

Further context: **[AI-DLC GitHub+AIDLC queue doc](https://github.com/queen-of-code/AI-DLC/blob/main/docs/GITHUB-AIDLC-PROJECT.md)** · template **[aidlc-board-label-sync.yml](https://github.com/queen-of-code/AI-DLC/blob/main/docs/templates/github-workflows/aidlc-board-label-sync.yml)**.

# GitHub Issues + Projects v2 (AIDLC automation)

Work is tracked on a **GitHub Projects v2** board with an **"AIDLC phase"** single-select field.
Automation is **event-driven** — no cron. An agent launches the moment you signal readiness.

Board: [AIDLC — alexa-recipe-app (project #6)](https://github.com/users/queen-of-code/projects/6)

---

## One-time setup

### 1 — GitHub secrets and variables

| Where | Name | Value |
|-------|------|-------|
| Repo Settings → Secrets → Actions | `CURSOR_API_KEY` | API key from [cursor.com/dashboard/integrations](https://cursor.com/dashboard/integrations) → **API Keys** |
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

1. Move the board card to the target phase column (e.g. **Plan**).
2. Apply the label **`aidlc_work:unstarted`** to the issue.

That label fires the [`aidlc-launch.yml`](../.github/workflows/aidlc-launch.yml) workflow, which:
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

To launch an agent without touching the label (e.g. for testing or re-runs):

```bash
gh workflow run aidlc-launch.yml \
  -f issue_number=123 \
  -f phase=plan
```

---

## Why not `project_card` or `projects_v2_item`?

- **`project_card`** (Projects classic) — GitHub no longer allows creating classic projects on personal accounts (deprecated late 2023).
- **`projects_v2_item`** (Projects v2) — only fires for **org-owned** projects. `queen-of-code` is a personal account, so this event never reaches repo Actions.
- **`issues.labeled`** fires for all repos including personal accounts, making it the correct event-driven trigger here.

This pattern is documented in [AI-DLC/docs/GITHUB-AIDLC-PROJECT.md](https://github.com/queen-of-code/AI-DLC/blob/main/docs/GITHUB-AIDLC-PROJECT.md) as the personal-account path.

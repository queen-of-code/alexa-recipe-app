# GitHub Issues + Projects (manual queue)

Automation (Actions, webhooks on status change) is **out of scope** for this tutorial repo v1. Operate the queue **manually**.

## Feature issue

- Use a **parent issue** per Feature (epic). **Sub-issues** are encouraged for units of work.
- In the issue body, link the repo folder: `feature/<kebab-slug>/` (must match the directory you use locally).

## Project Status values (match AIDLC)

Align **one** Status field (or columns) with development phases. **Build** and **Test** are **one** stage on the board — tests are written with code (TDD), not as a separate column after implementation.

| Status | AIDLC |
|--------|--------|
| Plan | Plan — Product Spec |
| Design | Design — Tech Spec |
| Build + Test | Build + Test (TDD loop) |
| Review | Review (+ human judgment on test sufficiency) |
| Validate | Validate + Learn |
| Done | Closed after human sign-off |

Canonical phase definitions: [AIDLC.md](https://github.com/queen-of-code/external-brain/blob/main/AIDLC.md).

## Workflow

1. Create issue → set **Plan** when starting `/plan`.
2. Move to **Design** when Product Spec is approved and Tech Spec work is active.
3. **Build + Test** while `/build` is in progress.
4. **Review** when opening formal review (`/review`).
5. **Validate** when running `/ship` scorecard and learnings.
6. **Done** when the feature is accepted and merged (or your team’s definition of done).

Update Status by hand when phase outputs are ready; no required bots in this repo.

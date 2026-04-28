# Product Spec — Recipe favoriting

> AIDLC Plan phase: [docs/AIDLC.md](../../docs/AIDLC.md) — Phase 1 Plan. **Product language only** — implementation belongs in Design.

## Overview

| Field | Value |
|-------|-------|
| **Feature** | Recipe favoriting |
| **Status** | Draft — awaiting product approval |
| **Author** | AIDLC Plan agent (headless run) |
| **Created** | 2026-04-28 |
| **Last updated** | 2026-04-28 |
| **Tracker** | [queen-of-code/alexa-recipe-app#71](https://github.com/queen-of-code/alexa-recipe-app/issues/71) |
| **Feature folder** | `feature/recipe-favoriting/` |

## Problem and audience

**Problem:** People build a personal recipe library over time. There is no lightweight way to mark which recipes matter most right now or to pull those recipes to the front when browsing—so “go-to” meals stay buried with everything else.

**Audience:** Authenticated users who save and manage **their own** recipes in the Recipe App (web experience is in scope; voice/agent surfaces may consume the same behavior later but are not required for v1 unless already tied to the same library).

**Current experience (baseline):** All saved recipes appear in one undifferentiated list (or sort order) with no first-class “favorite” signal and no way to focus the list on favorites first or favorites only.

## Customer outcomes

- Users can **mark a recipe as a favorite** and **clear that mark** from anywhere they already view a recipe in context (at minimum: list and recipe detail), with state that **survives** closing the app or signing in again on another session.
- In the **default recipe list** (no extra filter chosen), **favorited recipes appear above** recipes that are not favorited, so “what I care about most” is visible without hunting.
- Users can **filter** the list to show **only favorites** (and clear that filter to return to the normal combined view / ordering above).
- Favorite status is **obvious at a glance** (e.g. distinct on/off affordance—exact control is Design).

## Success criteria (for Validate / scorecard)

| # | Criterion | How we’ll verify |
|---|-----------|------------------|
| 1 | Favorite on/off | From list and detail, user can favorite and unfavorite; after refresh or re-login, status matches what they last set. |
| 2 | Favorites first | With multiple favorited and non-favorited recipes, default list shows **all** recipes but **every favorited recipe appears before any non-favorited** (stable tie-break within each group is Design). |
| 3 | Favorites-only filter | User can turn on a filter that shows **only** favorited recipes; turning it off restores behavior in criterion 2. |
| 4 | No cross-user leakage | Another user’s library never shows this user’s favorites or mutates their favorite state. |
| 5 | Empty states | If the user has no favorites, “favorites only” shows a clear empty state; if they have favorites but none match other future filters, empty state remains understandable (copy is product/UX polish). |

## Out of scope

- Favoriting **other users’** recipes, public discovery, or social feeds.
- Sharing “favorite lists” with collaborators.
- Reordering favorites **within** the favorites group by manual drag-and-drop (ordering beyond “favorites before non-favorites” is out unless added later).
- Notifications, reminders, or “cook this favorite soon” workflows.
- Any **implementation** choice (storage shape, API shape, icon asset): Design / Tech Spec.

## Constraints

- Recipes are **per-user**; favorites apply only to **that user’s** recipes in their library.
- Behavior must remain correct when the user has **many** recipes (performance as the user experiences it—no long blocking spinners on common actions).

## Assumptions

*(Headless Plan run — no live product Q&A. Reasonable defaults; revise after human review.)*

- **“Top”** means **sort order** in the main list: favorites first, then non-favorites—not a separate pinned strip or second page.
- **Filter** is a **binary** mode: “show all (with favorites-first sort)” vs “show favorites only”—not a multi-select tag system in v1.
- **Surfaces in v1:** Web app recipe list + detail; voice/Alexa parity is **not** a gate for approving this Product Spec unless the product owner expands scope in Design.
- **Issue body** did not include `AIDLC feature folder: feature/<slug>/`; this folder **`feature/recipe-favoriting/`** is chosen to align with the issue title and existing repo notes under `RecipeApp/specs/recipe-favoriting.md` (that file is **not** the Product Spec — Design will supersede technical sketches).

## Grounding review (agent-grounding-reviewer)

Reviewed against repo context. **Severity** per grounding-reviewer intent (contradictions, impossible claims, overlaps): see [.claude/skills/agents/agent-grounding-reviewer/SKILL.md](../../.claude/skills/agents/agent-grounding-reviewer/SKILL.md).

| Severity | Finding |
|----------|---------|
| **Advisory** | `RecipeApp/specs/recipe-favoriting.md` is an **older technical sketch** (DynamoDB `IsFavorite`, sample endpoints). This **Product Spec** intentionally does **not** adopt that stack detail. Design must **reconcile** with current persistence (e.g. Firestore-oriented code paths) so Build does not follow the stale doc blindly. |
| **Advisory** | `RecipeApp/RecipeAPI/FirestoreModels/Meal.cs` includes `FavoriteOfUsers` — naming overlap with “favorite” but **meals ≠ recipes**. Product scope here is **recipes only**; Tech Spec should avoid conflating meal and recipe favoriting models. |
| **Advisory** | Issue text says “filter too” but does not define interaction with **other** future list filters (e.g. search). Default assumption: **favorites-only** narrows the set to favorited recipes; combination rules with other filters are **Design** unless product adds a conflict rule later. |
| **Blocking** | *None* — outcomes are achievable without contradicting stated app boundaries (personal recipe library). |

## Decisions (resolved for this draft)

| Date | Decision |
|------|----------|
| 2026-04-28 | Favorites-first is **default list ordering**, not optional sorting mode. |
| 2026-04-28 | **Favorites-only** is an explicit **filter**, separate from sort. |

## Related documents

- Prior internal sketch (technical, non-canonical for Plan): `RecipeApp/specs/recipe-favoriting.md`
- AIDLC process: [docs/AIDLC.md](../../docs/AIDLC.md)
- Issue: https://github.com/queen-of-code/alexa-recipe-app/issues/71

## Human approval

- [ ] Product owner approved before Design

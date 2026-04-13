# Product Spec — `recipe-ingredient-search`

> AIDLC Plan phase: [docs/AIDLC.md — Phase 1 Plan](../../docs/AIDLC.md).

## Problem and audience

Home cooks using the recipe app accumulate many saved recipes. When they want to cook with what they already have (or avoid buying something), they need a fast way to see **which recipes use specific ingredients** instead of opening each recipe one by one.

**Primary audience:** Authenticated users managing their own recipe library in the app.

## Customer outcomes

- Users can **narrow the recipe list** using ingredient-based criteria (for example, “show recipes that include X” or “only recipes that contain all of these ingredients”).
- Search or filter is **discoverable** from the main recipe browsing experience (users understand how to use it without training).
- Results **feel trustworthy**: what the user asked for matches what they see (clear empty states when nothing matches).
- The experience stays **pleasant on typical personal libraries** (responsive, no confusing delays for normal use).

## Success criteria (for Validate / scorecard)

- [ ] From the recipe list, a user can apply ingredient-based filter and/or search and see a **subset of recipes** that match the criteria.
- [ ] The user can **clear or reset** the ingredient criteria and return to the full list.
- [ ] When no recipes match, the user sees a **clear message** (not a blank screen or generic error).
- [ ] Behavior is covered by **automated tests** at the appropriate level (per team practice for UI + API).
- [ ] Success is demonstrable in **staging or local** with realistic sample recipes.

## Out of scope

- Public or cross-user recipe discovery (this remains **per-user** recipe data only).
- Nutrition, allergens, or diet labels unless they are **already** represented as plain ingredient text (no new regulatory or medical claims).
- A full **canonical ingredient database** or grocery taxonomy (synonyms, brands, units) unless explicitly pulled into a later phase.
- Voice-only (Alexa) flows **unless** the Product Spec is later amended; this spec targets the **web app** recipe experience first.

## Constraints

- Must respect **existing authentication and data ownership** (users only see their own recipes).
- Ingredient data in the product today is **free-text lines** per recipe; matching behavior should be honest about limitations (e.g. spelling and wording variations) in UX copy or help, without overpromising “smart pantry” semantics unless we add them in Design.

## Grounding review (repo context)

Reviewed against the current codebase **without** rewriting scope. Severity: **blocking** stops Plan until resolved; **advisory** informs Design.

| Finding | Severity | Notes |
|--------|----------|--------|
| Recipes already store **ingredients as a list of strings** per user recipe; no separate ingredient catalog. | Advisory | Aligns the feature with existing data; fuzzy matching and normalization are product/Design choices, not assumed. |
| The API today **lists all recipes** for a user; there is no dedicated ingredient query yet. | Advisory | Filtering may be implemented in more than one way; Tech Spec will choose (e.g. client vs server) against non-functional goals. |
| Frontend is a **React** app with a recipe list; feature will touch list UX and possibly API contracts. | Advisory | Consistent with [AIDLC](https://github.com/queen-of-code/alexa-recipe-app) multi-surface app layout. |

**Blocking issues:** none identified for Plan.

## Open questions (for Product owner before Design)

1. **Match mode:** Should “chicken, rice” mean *any* of those ingredients, *all* of them, or should the user **choose** (e.g. toggle)? Default assumption for v1 should be explicit in approval.
2. **Text behavior:** Is **substring** search enough (e.g. “tom” matches “tomato”), or do we want **whole-word** / token rules only for v1?

## Human approval

- [ ] Product owner approved before Design

---

**Tracking:** Create a **parent GitHub issue** for this Feature and set project status to **Plan** per [docs/github-queue.md](../../docs/github-queue.md). Issue body should link to `feature/recipe-ingredient-search/`.

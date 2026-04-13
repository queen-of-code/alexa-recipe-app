# Product Spec — `recipe-ingredient-search`

> AIDLC Plan phase: [docs/AIDLC.md](../../docs/AIDLC.md) — Phase 1 Plan.

## Problem and audience

**Problem:** People accumulate many saved recipes. When they have specific ingredients on hand (or are shopping for something), scanning the full list to see which recipes use those ingredients is slow and error-prone.

**Audience:** Authenticated users of the recipe web app who maintain a personal recipe library (same users who already create and browse recipes today).

## Customer outcomes

- Users can **narrow the recipe list** using ingredient-related input (search and/or filters—exact UX is left to Design).
- Matching behavior is **predictable**: users understand why a recipe appears or does not appear for a given query.
- The experience remains **responsive** for typical personal library sizes (dozens to hundreds of recipes per user).

## Success criteria (for Validate / scorecard)

- From the main recipe list flow, a user can apply ingredient-based criteria and see a **filtered subset** of their recipes (or a clear empty state when nothing matches).
- Product-approved rules for **multi-ingredient queries** (e.g. whether multiple terms mean “all” vs “any”) are documented and reflected in behavior.
- Automated tests prove the filtering/search behavior against those rules (exact layer covered in Tech Spec).

## Out of scope

- Public or social discovery of recipes across users.
- Structured nutrition data, allergens-as-first-class filters, or substitution engines (unless we explicitly expand scope later).
- OCR, barcode, or photo-based ingredient capture.
- Changing how ingredients are **entered** when creating/editing a recipe (free-text lines remain unless a separate feature says otherwise).

## Constraints

- Recipes are **per-user**; results must only ever include the signed-in user’s recipes.
- Ingredients are stored today as **human-entered strings** (not a global canonical ingredient catalog). Matching will work on that text—Design/Tech will spell out normalization (case, partial words, etc.).
- Broader architecture and storage are defined in the repo; this spec does not prescribe Firestore vs other backends—Tech Spec must match implementation.

## Human approval

- [ ] Product owner approved before Design

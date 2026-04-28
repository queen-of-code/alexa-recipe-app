# Product Spec — Recipe completed photo (optional)

> AIDLC Plan phase: [docs/AIDLC.md](../../docs/AIDLC.md) — Phase 1 Plan.

| Field | Value |
|-------|-------|
| **Feature** | Optional photo of the completed dish on a recipe |
| **Status** | Shipped — validate/scorecard in [validate-scorecard.md](./validate-scorecard.md) |
| **Tracker** | [queen-of-code/alexa-recipe-app#67](https://github.com/queen-of-code/alexa-recipe-app/issues/67) |
| **AIDLC feature folder** | `feature/recipe-completed-photo/` |

## Assumptions

Headless Plan run: no live product chat. Reasonable defaults below; Design may refine after human approval.

- **One photo per recipe** is enough for v1 (not a gallery), unless product explicitly expands later.
- **“Completed recipe”** means a single user-uploaded image representing the finished dish, not step-by-step cooking media, not video, and not automatic stock imagery.
- **Surfaces:** Users can attach, replace, or remove this photo when **creating** or **editing** a recipe in the **web app**; whether **Alexa / voice** can set or change the photo is **out of scope** unless a separate initiative adds equivalent UX (called out below).
- **Optional** means recipes without a photo behave as today; no requirement to upload to save.

## Problem and audience

**Problem:** Cooks want a quick visual reminder of what “done” looks like for a saved recipe. Today they rely on name and text only; an optional photo of the finished dish makes recipes easier to recognize and more satisfying to browse.

**Audience:** Authenticated users who maintain a **personal** recipe library in this app—the same people who create and edit recipes today.

**Baseline:** Recipes are text-oriented (name, steps, times, etc.); there is no first-class “finished dish” image on a recipe in product terms today.

## Customer outcomes

- Users can **add** an optional photo when **creating** a new recipe.
- Users can **change** or **remove** that photo when **editing** an existing recipe.
- The photo is clearly associated with **that** recipe and visible wherever the product already shows recipe detail or list previews (exact layout is Design), without breaking flows for recipes that have no photo.

## Success criteria (for Validate / scorecard)

| # | Criterion | How we’ll verify |
|---|-----------|------------------|
| 1 | **Create:** A signed-in user can save a new recipe **with** an optional completed-dish photo and later see that same photo on the recipe. | Manual or automated UI/API path per Tech Spec; evidence attached in Validate. |
| 2 | **Update:** A signed-in user can add a photo to a recipe that had none, replace an existing photo, or remove the photo and confirm the recipe saves correctly. | Same as above; empty state after remove is explicit. |
| 3 | **Privacy / ownership:** A user never sees another user’s uploaded image on **their** recipe views; deleting or replacing behaves predictably from the user’s perspective. | Validate against multi-user scenario or test accounts. |
| 4 | **Optional:** Saving a recipe **without** a photo remains supported; no new mandatory upload gate. | Regression on create/edit without image. |

## Out of scope

- **Voice / Alexa** flows for upload, capture, or crop (unless a future feature defines them).
- **Multiple images** per recipe, video, audio, or **in-recipe** step images.
- **Social** sharing, public recipe pages, or discovery across users.
- **OCR or ML** on the image (e.g. reading ingredients from a photo)—distinct from [feature/recipe-ingredient-search/product-spec.md](../recipe-ingredient-search/product-spec.md), which already defers photo-based ingredient capture.
- **Editing tools** beyond what the product already offers for images (heavy filters, markup, etc.)—v1 is attach / replace / remove.

## Constraints

- Recipes remain **per-user**; any image is tied to that user’s recipe and must not leak across tenants/users.
- **Abuse and storage:** Product expects **reasonable** limits (file types, size, maybe count) so the feature cannot be used as arbitrary bulk storage; exact enforcement is Design/engineering.
- **Accessibility:** Where the photo is shown, experience should still work for users who do not rely on the image (text remains primary for instructions).

## Grounding review (`agent-grounding-reviewer`)

Reviewed against this repo’s stated context and adjacent specs. Severity for Plan only—does not block drafting; Design resolves details.

| Severity | Finding |
|----------|---------|
| **Advisory** | Issue #67 body does not yet include `AIDLC feature folder: feature/recipe-completed-photo/`; this spec adds the folder path in the overview table for automation alignment. |
| **Advisory** | [recipe-ingredient-search](../recipe-ingredient-search/product-spec.md) explicitly defers **photo-based ingredient capture**; this Feature is **finished-dish** imagery only—no conflict if scopes stay separated. |
| **Advisory** | Tutorial app is **web + API**-oriented per README; treating voice upload as out of scope keeps the Product Spec consistent until a voice UX exists. |

_No blocking contradictions identified: this is additive product capability aligned with the issue title and body._

## Decisions (resolved for this draft)

| Item | Decision |
|------|----------|
| Feature slug | `recipe-completed-photo` (issue did not specify folder; chosen to match “picture of the **completed** recipe”). |
| Cardinality | One optional completed-dish image per recipe for v1. |

## Human approval

- [ ] Product owner approved before Design

# Product Spec — `recipe-photo`

> AIDLC Plan phase: [docs/AIDLC.md](../../docs/AIDLC.md) — Phase 1 Plan.

## Problem and audience

**Problem:** Users save recipes as text — name, ingredients, and steps — but have no way to show what the finished dish looks like. When browsing their recipe list or sharing a recipe, a visual reference is missing. Seeing the completed dish helps users remember which recipe is which, decide what to cook, and feel more connected to their collection.

**Audience:** Authenticated users with a personal recipe library in the web app. The photo is an **optional** enhancement — recipes that do not have a photo continue to work exactly as today.

## Customer outcomes

- When **creating or editing** a recipe, users can **optionally upload a photo** of the completed dish. The upload control is clearly separate from the recipe text fields and labeled so users understand it is optional.
- The photo is displayed on the **recipe detail** page, giving users a visual reference for the dish.
- On the **recipe list** page, a **thumbnail** (or placeholder icon) appears alongside each recipe name so users can quickly scan their collection visually.
- Users can **replace or remove** a previously uploaded photo when editing a recipe.
- Recipes without a photo show a **tasteful placeholder** (icon or neutral image) — the UI does not look broken or incomplete when no photo exists.
- Upload is constrained to **common image formats** (JPEG, PNG, WebP) and a **reasonable file size** (max 5 MB) so the experience stays fast and storage costs are predictable.

## Success criteria (for Validate / scorecard)

- **Upload:** From the recipe create/edit form, a user can select an image file, and after saving, the photo is persisted and associated with that recipe. Validated by **automated frontend tests** (mocked upload) and a **manual walkthrough**.
- **Display — detail:** The recipe detail page shows the uploaded photo. Automated test confirms the image element renders when a photo URL is present.
- **Display — list:** The recipe list shows a thumbnail for recipes with photos and a placeholder for those without. Automated test covers both states.
- **Replace / remove:** A user can replace the photo with a new one or remove it entirely from the edit form. Manual or automated test confirms the old image is no longer displayed.
- **Validation:** Attempting to upload a file that exceeds 5 MB or is not an accepted image type shows a clear **client-side error** before any network call. Automated frontend test.
- **No regression:** Existing recipe CRUD (create, read, update, delete) and ingredient search continue to work unchanged. Existing test suites pass.

## Out of scope

- **Multiple photos per recipe** — v1 supports exactly one optional photo.
- **Photo editing** (crop, rotate, filters) — the user uploads a finished image.
- **Photo galleries, social sharing, or public recipe discovery** — photos are per-user, same as recipes.
- **AI-generated images or OCR** — the user provides the photo manually.
- **Video uploads** — images only.
- **Offline support / progressive image loading** — standard web loading is sufficient for v1.

## Constraints

- Recipes are **per-user**; a photo must only be accessible to the user who uploaded it (same ownership model as recipe data).
- The photo field is **optional** — existing recipes without photos must not be affected.
- Photos are **binary blobs** that cannot live inside the existing Firestore recipe document (1 MiB doc limit vs 5 MB images). A separate object store (e.g. Firebase Storage / GCS) is required; the recipe document will store a **reference** (URL or path). Exact mechanism is a Design decision.
- Client-side validation prevents oversized or wrong-type uploads, but the backend or storage security rules must also enforce limits to protect against direct API calls.
- Storage costs should be proportional to usage; the design should avoid unbounded accumulation of orphaned files (e.g. when a photo is replaced or a recipe is deleted). Clean-up behavior is defined in Tech Spec.

## Open questions for product owner

> These questions surfaced during drafting. Answers should be resolved in conversation before approving this spec.

1. **Thumbnail on list page:** Should the list table gain an image column, or would a small thumbnail next to the recipe name (within the existing Name column) be preferred?
2. **Photo deletion on recipe delete:** When a recipe is deleted, should its photo be automatically cleaned up from storage, or is eventual cleanup acceptable?
3. **Maximum dimension / auto-resize:** Should the app auto-resize uploaded images to a max dimension (e.g. 1200px wide) to save storage, or store the original and only generate a smaller thumbnail?
4. **Access control model:** Should photos be accessible only while the user is authenticated (signed download URLs), or is it acceptable to use a stable URL that works as long as the user knows it (simpler but less private)?

## Human approval

- [ ] Product owner approved before Design

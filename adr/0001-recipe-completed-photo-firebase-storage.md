# ADR-0001: Completed-dish recipe photos — Firestore + Firebase Storage

| Field | Value |
|-------|-------|
| **Status** | Accepted |
| **Date** | 2026-04-28 |
| **Deciders** | Feature owner / engineering (tutorial repo) |
| **Related** | [feature/recipe-completed-photo/product-spec.md](../feature/recipe-completed-photo/product-spec.md), [feature/recipe-completed-photo/tech-spec.md](../feature/recipe-completed-photo/tech-spec.md), [queen-of-code/alexa-recipe-app#67](https://github.com/queen-of-code/alexa-recipe-app/issues/67), PR [#74](https://github.com/queen-of-code/alexa-recipe-app/pull/74) |

---

## Context

Users need an optional **photo of the finished dish** on a recipe. The app already stores recipe text in **Firestore** and authenticates with **Firebase Auth**. We needed a place for binary image bytes, tenant isolation per user, and a stable reference on the recipe document for list and detail views.

## Decision

Store **metadata on the recipe** (`completedImageUrl` on the Firestore `Recipe` / API `RecipeModel`) and store **file bytes in Firebase Storage** under `users/{firebaseUid}/recipes/{recipeId}/completed.{ext}`. The web client uploads via the Storage SDK; the API validates that any stored URL/path belongs to the authenticated user. **Create recipe** must return the server-assigned **`recipeId` in the JSON body** (HTTP **201**) so the client can upload to a deterministic path after the first save.

## Options considered

| Option | Summary | Why not chosen |
|--------|---------|----------------|
| A | Base64 image embedded in Firestore / API JSON | Inflates documents; worse for list payloads and quotas; awkward for large files. |
| B | External blob store not integrated with Firebase (e.g. raw GCS without Firebase rules) | More bespoke IAM and wiring; Firebase Storage + rules align with existing Auth. |
| C | Only client-local or session storage | Does not meet “see the same photo later” across devices/sessions. |

## Consequences

### Positive

- Small Firestore documents; images served via Storage URLs.
- Storage security rules can enforce per-user paths alongside existing JWT checks in RecipeAPI.
- One object per recipe path keeps abuse bounded when combined with size/type rules.

### Negative / trade-offs

- **Operational:** `storage.rules` must be deployed with the app; emulators (Storage + Auth) needed for faithful local integration tests.
- **Orphans:** If upload fails after recipe create, or delete fails, orphaned objects are acceptable at tutorial tier; cleanup on successful recipe delete is attempted.

### Neutral / follow-ups

- Client-side image compression is a possible future improvement (not required for v1).

## Compliance & verification

- **API:** `CompletedImageMetadata` unit tests enforce user ownership of paths/URLs; controller tests cover `POST` **201** + `recipeId` in body.
- **Review:** Security/Tech Spec dimensions on PR #74; Storage rules in repo for deployment verification.

## References

- [Firebase Storage documentation](https://firebase.google.com/docs/storage)
- Tech Spec: [feature/recipe-completed-photo/tech-spec.md](../feature/recipe-completed-photo/tech-spec.md)

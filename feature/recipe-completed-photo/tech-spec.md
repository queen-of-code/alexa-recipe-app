# Tech Spec — Recipe completed photo (optional)

> AIDLC Design phase: [docs/AIDLC.md](../../docs/AIDLC.md) — Phase 2 Design.  
> **Product input:** [product-spec.md](./product-spec.md) (issue [queen-of-code/alexa-recipe-app#67](https://github.com/queen-of-code/alexa-recipe-app/issues/67)).

| Field | Value |
|-------|-------|
| **Unit** | Single unit: optional completed-dish image end-to-end (Firestore metadata + object storage + React UI). |
| **Tracker** | Same as Product Spec |
| **AIDLC feature folder** | `feature/recipe-completed-photo/` |

## Units / scope

**In scope**

- One optional **completed-dish** image per recipe (cardinality per Product Spec).
- **Web app only:** add / replace / remove on **create** and **edit** (`RecipeApp/frontend` — `RecipeForm.jsx`, `recipeApi.js`).
- Persist a **stable reference** on the recipe document so list/detail views can render the image without re-uploading.
- **Per-user isolation:** only the owning Firebase UID can read/write their recipe’s image; API routes continue to enforce `userId` ↔ token UID (existing `ValuesApiController.EnsureRouteUserMatchesToken`).

**Out of scope** (per Product Spec)

- Voice/Alexa upload, galleries, video, step images, social/public URLs, OCR/ML, heavy image editing.

**Repo reality (grounding)**

- **RecipeAPI** uses **Firestore** (`RecipeApp/RecipeAPI/FirestoreModels/Recipe.cs`, `FirestoreRecipeService`) under `users/{userId}/recipes/{recipeId}`, not DynamoDB (see `RecipeApp/specs/architecture.md` — treat Dynamo wording as legacy for recipe CRUD).
- **Frontend** uses **Firebase Auth** (`RecipeApp/frontend/src/firebase.js`) and **JSON** calls to `RecipeAPI` with Bearer ID tokens (`RecipeApp/frontend/src/api/recipeApi.js`).
- **Firebase Storage is not initialized** in the frontend today; `firebase` dependency is present and can be extended for Storage.

## Architecture and components

```mermaid
flowchart LR
  subgraph client [React SPA]
    RF[RecipeForm]
    SA[Firebase Storage SDK]
  end
  subgraph api [RecipeAPI]
    VC[ValuesApiController]
    FS[FirestoreRecipeService]
  end
  subgraph gcp [Google Cloud]
    FStore[(Firestore)]
    FStoreB[(Firebase Storage)]
  end
  RF -->|JWT CRUD| VC
  VC --> FS
  FS --> FStore
  RF -->|upload/delete object| SA
  SA --> FStoreB
```

**Design review — architecture**

- **Metadata in Firestore, bytes in Storage:** Keeps recipe documents small, avoids base64-in-JSON, and matches Firebase’s usual pattern. Recipe document holds a **download URL** or **Storage path** string the client can turn into a display URL (prefer **path** if you want to revoke tokens later; **download URL** is simpler for v1 — pick one in Build and stay consistent).
- **Auth boundary:** RecipeAPI already verifies Firebase JWTs. Storage access must use **Firebase Storage Security Rules** so users cannot read/write another user’s `users/{otherUid}/...` tree.
- **Create flow dependency:** Today `POST /api/values/{userId}` returns `200 OK` with **no body** (`ValuesApiController.Post`), while the server assigns a new `recipeId` when the client omits it (`FirestoreRecipeService.SaveItem`). To attach an image **on first save**, the client needs the **server-assigned `recipeId`** before uploading to a deterministic Storage path. **Required contract change:** `POST` should return **201 Created** (or 200 with body — prefer 201) and a JSON body containing at least `{ "recipeId": "<id>", ... }` or the full external recipe shape. Build implements this without breaking clients that ignore the body.

**Design review — backend (RecipeAPI + Core)**

- Extend **`RecipeApp.Core.ExternalModels.RecipeModel`** with an optional JSON field, e.g. `completedImageUrl` or `completedImageStoragePath` (name aligned with JSON camelCase used elsewhere).
- Extend **`RecipeApp/RecipeAPI/FirestoreModels/Recipe.cs`** with matching `[FirestoreProperty]`; map in constructor and `GenerateExternalRecipe()`.
- **Validation:** Reject save if the field is non-null but fails basic checks (e.g. wrong prefix if using canonical paths, or URL not under allowed host). Keep rules minimal for v1: optional string, max length, allowed content types enforced primarily at upload time.
- **Delete recipe:** When `DeleteRecipe` succeeds, **orphan cleanup:** delete Storage object if path/URL is known (RecipeAPI can use **Firebase Admin SDK** `StorageClient` or HTTP delete via signed operation — choose one library pattern in Build). If delete fails, log and still remove Firestore document (eventual orphan acceptable for tutorial tier; document in ops).

**Design review — frontend**

- Initialize **`getStorage`** from the same Firebase app as Auth (`RecipeApp/frontend/src/firebase.js`); support emulator if the project adds `VITE_USE_STORAGE_EMULATOR` (optional for v1).
- **RecipeForm:** file input (`accept="image/jpeg,image/png,image/webp"`), preview, remove control, disabled state during upload, error surfacing. Do not block submit on image failure if user did not select a file; if they did, either transactional UX (“recipe saved but image failed — retry”) or block until both succeed — **recommend:** save recipe first (after contract returns `recipeId`), then upload, then `PUT` with image field; show clear rollback message if upload fails after recipe exists.
- **RecipeList / detail:** show thumbnail when field present; **decorative** `img` with empty `alt` or `alt=""` if name is adjacent (Product Spec: text remains primary). Lazy-load images if trivial.

## API / UI contracts

### REST (RecipeAPI) — existing routes

| Method | Route | Change |
|--------|-------|--------|
| `POST` | `/api/values/{userId}` | Response body **must** include assigned `recipeId` (and other fields as needed) on success. |
| `PUT` | `/api/values/{userId}/{recipeId}` | Accept optional completed-image field in JSON body. |
| `GET` | `/api/values/{userId}`, `/api/values/{userId}/{recipeId}` | Include optional completed-image field when set. |

**JSON (camelCase)** — illustrative field name (finalize in Build):

```json
{
  "completedImageUrl": "https://firebasestorage.googleapis.com/..."
}
```

Or storage-relative path + client-side URL resolution — document the chosen representation in code comments and OpenAPI/README if added later.

### Firebase Storage layout (recommended)

`users/{firebaseUid}/recipes/{recipeId}/completed{extension}`

- **Replace:** overwrite same logical key or use version suffix; simplest is **delete old object** then upload new after successful `PUT` of metadata.
- **Remove:** delete object + `PUT` with `null`/omit field per convention chosen in Build.

### Limits (abuse / storage)

| Constraint | Suggested default |
|------------|-------------------|
| Max file size | 5 MiB (configurable constant) |
| Allowed MIME | `image/jpeg`, `image/png`, `image/webp` |
| Count | 1 object per recipe (enforced by path + workflow) |

Enforce in **client** before upload and in **Storage rules** (max size / content type). Optional: **RecipeAPI** validation on the string field only (not full binary scan).

### UI contracts

- **Create:** optional file; recipe can save with no file unchanged.
- **Edit:** show current image if present; actions: **Replace** (new file), **Remove** (clear + delete blob).
- **List:** small thumbnail next to recipe name when URL/path present.

## Data model

**Firestore document** (`Recipe` entity): add optional string property for completed image (URL or Storage path). No migration required for existing documents (missing field = no image).

**No change** to MySQL / Identity for this feature.

## Acceptance criteria (for Review)

Traceable to Product Spec success criteria:

| # | Criterion | Review evidence |
|---|-----------|-----------------|
| A1 | Create recipe **with** optional image; same image visible on subsequent load | E2E or manual script + unit tests on mapping |
| A2 | Edit: add image to recipe without one; replace; remove with empty state | Same |
| A3 | User A cannot access User B’s image (Storage rules + API Forbid on wrong `userId`) | Rule tests or documented manual matrix |
| A4 | Create/edit **without** image unchanged | Regression tests on existing flows |
| A5 | Field appears in **GET** list/detail payloads when set | API contract tests |

## Testing approach (Build + Test)

- **Unit:** `Recipe` ↔ `RecipeModel` round-trip for new field; controller deserialization with/without field.
- **Integration:** Firestore emulator save/retrieve with image field; optional Storage emulator upload if configured in CI.
- **Frontend:** extend `RecipeForm.test.jsx` — file input presence, submit payload includes image URL after mock upload, remove clears field.
- **Manual / Validate:** checklist in Product Spec table (multi-user sanity on hosted Firebase).

## Risks and edge cases

| Risk | Mitigation |
|------|------------|
| Large images / mobile bandwidth | Client-side resize/compress optional stretch goal; hard cap in rules |
| Orphan Storage after failed PUT | Retry UX; background cleanup job out of scope |
| POST body change breaks unknown clients | Additive JSON response; existing clients that only check status may still work |
| Emulator parity | Document Firebase emulator suite for Auth + Firestore + Storage for local dev |

## Specialist review summary

- **Architecture:** Metadata/blobs split; contract fix for POST `recipeId`; Storage rules for tenant isolation.
- **Backend-saas:** JWT-bound CRUD unchanged; validate image metadata shape; cleanup on delete.
- **Frontend-web:** Accessible list/detail; progressive enhancement when image missing; clear upload errors.

## Human approval

- [ ] Engineering approved before Build

---

## Retrospective (ship)

Completed with merge of [PR #74](https://github.com/queen-of-code/alexa-recipe-app/pull/74) (2026-04-28).

**Aligned with plan**

- Firestore field **`completedImageUrl`**, Firebase Storage path layout, Storage rules, web-only UX (create/edit/list/detail), POST returns **201** with body including **`recipeId`** (`ValuesApiController.Post`), limits (5 MiB; JPEG/PNG/WebP).

**Differed or tightened in implementation**

- **`completedImageUrl`** holds the Firebase download URL (and paths are parsed for validation/deletion), matching the illustrative JSON in this spec.
- **Recipe delete:** attempts Storage object removal when metadata maps to an object path (non-emulator); failures logged — as anticipated for orphan tolerance.
- **CI / dev:** Firebase Storage emulator (9199) and docker-compose/`firebase.json` wiring landed during Build so integration tests and local dev stay aligned.

**Testing**

- Strong unit coverage on metadata + controller POST behavior; frontend tests cover create-with-photo flow. Replace/remove paths rely on code review for this tutorial tier; add tests if regressions appear.

**Process**

- Review flagged advisory items (e.g. manual Validate/E2E); scorecard in `validate-scorecard.md` documents evidence boundaries.

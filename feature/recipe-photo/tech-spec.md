# Tech Spec — `recipe-photo`

> AIDLC Design phase: [docs/AIDLC.md](../../docs/AIDLC.md) — Phase 2 Design. Product Spec: [product-spec.md](./product-spec.md).

## Units / scope

**Single Unit:** Optional recipe photo upload, storage, display, and lifecycle management.

- **Storage:** Firebase Storage (Cloud Storage for Firebase) bucket for photo blobs, organized by user. Security rules enforce per-user ownership.
- **Backend:** New fields on `Recipe` / `RecipeModel` to store the photo reference. New API endpoint for photo upload and deletion. Photo cleanup on recipe delete.
- **Frontend:** `RecipeForm` gains a file picker for optional photo upload. `RecipeDetail` displays the photo. `RecipeList` shows a thumbnail or placeholder.

Out of scope for this Unit: multiple photos per recipe, image editing/cropping, galleries, video, CDN/image optimization pipeline, Storage emulator in local Docker Compose (document as follow-up).

## Architecture and components

| Area | Proposal |
|------|----------|
| **Blob storage** | Firebase Storage (Cloud Storage for Firebase) — same GCP project (`queen-of-code`). Bucket path: `recipe-photos/{userId}/{recipeId}.{ext}`. One photo per recipe; replacing overwrites the same path. |
| **Upload flow** | **Client-direct upload** to Firebase Storage using the Firebase JS SDK (`firebase/storage`). The frontend authenticates with the user's Firebase Auth token; Storage security rules enforce `request.auth.uid == userId`. No binary data passes through the API server — keeps Cloud Run lightweight and avoids request body size limits. |
| **Photo reference in Firestore** | After a successful upload, the frontend retrieves the download URL from Storage and sends it to the API as part of the recipe JSON body (new `photoUrl` field on `RecipeModel`). The API persists it in the Firestore recipe document (new `PhotoUrl` field on `Recipe`). |
| **Photo display** | Frontend reads `photoUrl` from the recipe JSON. If present, renders an `<img>` tag; if absent, renders a placeholder. The download URL from Firebase Storage is a long-lived authenticated URL (includes token in query string) — no server-side signed URL generation needed. |
| **Photo deletion** | Two triggers: (1) User removes photo via edit form → frontend deletes the Storage object and clears `photoUrl` on the recipe. (2) User deletes the recipe → API-side cleanup: backend deletes the Storage object using Firebase Admin SDK before deleting the Firestore document. |
| **Backend library** | Add `Google.Cloud.Storage.V1` NuGet package (or `FirebaseAdmin` Storage helpers) to `RecipeAPI` for server-side photo deletion on recipe delete. |

## API / UI contracts

### Data model changes

**`RecipeModel` (Core/ExternalModels)** — add:

```csharp
[JsonPropertyName("photoUrl")]
public string PhotoUrl { get; set; }
```

**`Recipe` (FirestoreModels)** — add:

```csharp
[FirestoreProperty]
public string PhotoUrl { get; set; }
```

**Mapping:** Update `Recipe(RecipeModel)` constructor and `GenerateExternalRecipe()` to include `PhotoUrl`.

### HTTP — existing endpoints (changes)

Existing `POST` (create) and `PUT` (update) endpoints already accept `RecipeModel` as JSON body. The new `photoUrl` field flows through automatically via model binding. No new endpoints needed for the URL persistence — the upload itself goes directly to Firebase Storage from the client.

### HTTP — new endpoint: delete photo

`DELETE /api/values/{userId}/{recipeId}/photo`

- **Auth:** `Authorization: Bearer <Firebase ID token>` (unchanged).
- **Behavior:** Deletes the Storage object at `recipe-photos/{userId}/{recipeId}.*` and clears `PhotoUrl` on the Firestore document. Returns `200 OK` on success, `404` if no photo exists, `403` if userId mismatch.
- **Rationale:** While the frontend can delete from Storage directly, providing a server-side endpoint ensures the Firestore field and Storage object stay in sync, and enables cleanup from non-browser clients.

### Recipe delete — backend cleanup

When `DELETE /api/values/{userId}/{recipeId}` is called, the controller (or service layer) must also delete the associated Storage object if one exists. Check the recipe's `PhotoUrl` field before deleting the document; if non-empty, delete the corresponding Storage object.

### Firebase Storage security rules

```
rules_version = '2';
service firebase.storage {
  match /b/{bucket}/o {
    match /recipe-photos/{userId}/{allPaths=**} {
      allow read: if request.auth != null && request.auth.uid == userId;
      allow write: if request.auth != null && request.auth.uid == userId
                   && request.resource.size < 5 * 1024 * 1024
                   && request.resource.contentType.matches('image/(jpeg|png|webp)');
      allow delete: if request.auth != null && request.auth.uid == userId;
    }
  }
}
```

### Frontend

**`firebase.js`** — add Firebase Storage initialization:

```javascript
import { getStorage, connectStorageEmulator } from 'firebase/storage'

export const storage = getStorage(app)

if (import.meta.env.VITE_USE_EMULATOR === 'true') {
  connectStorageEmulator(storage, 'localhost', 9199)
}
```

**`recipeApi.js`** — add helper:

```javascript
export async function deleteRecipePhoto(userId, recipeId) {
  const headers = await getAuthHeaders()
  const res = await fetch(`${API_BASE}/api/values/${userId}/${recipeId}/photo`, {
    method: 'DELETE',
    headers,
  })
  if (!res.ok) throw new Error('Failed to delete photo')
}
```

**`RecipeForm.jsx`** — changes:

- Add `<input type="file" accept="image/jpeg,image/png,image/webp">` with client-side validation (max 5 MB, accepted MIME types).
- On file select: show a preview using `URL.createObjectURL`.
- On form submit (if a new file was selected): upload to Firebase Storage at `recipe-photos/{userId}/{recipeId}.{ext}` using `uploadBytes` / `uploadBytesResumable`, then `getDownloadURL`, and include the resulting URL in the recipe payload as `photoUrl`.
- Display existing photo if editing a recipe with a `photoUrl`.
- Provide a "Remove photo" button that calls `deleteRecipePhoto` and clears local state.

**`RecipeDetail.jsx`** — changes:

- If `recipe.photoUrl` is present, render `<img src={recipe.photoUrl} alt={recipe.name} />` in a styled container above or beside the recipe content.
- If absent, no image rendered (or a subtle placeholder icon).

**`RecipeList.jsx`** — changes:

- Add an image column (or inline thumbnail in the Name column). If `r.photoUrl` exists, render a small thumbnail `<img>`; otherwise, render a placeholder icon (e.g. a camera or utensil SVG).

## Data model

| Entity | Change |
|--------|--------|
| `Recipe` (Firestore) | Add `PhotoUrl` (string, nullable). Existing documents without this field remain valid — Firestore returns `null`/default for missing fields. |
| `RecipeModel` (API JSON) | Add `photoUrl` (string, nullable). Backward-compatible — existing clients that don't send it will leave it `null`. |
| Firebase Storage | New bucket path `recipe-photos/{userId}/{recipeId}.{ext}`. No Firestore schema migration needed. |

## Acceptance criteria (for Review)

- [ ] `RecipeForm` shows a file input for photo upload with client-side validation (type + size).
- [ ] Uploading a photo persists it to Firebase Storage and stores the download URL in the recipe's `photoUrl` field.
- [ ] `RecipeDetail` displays the photo when `photoUrl` is present.
- [ ] `RecipeList` shows a thumbnail when `photoUrl` is present and a placeholder when absent.
- [ ] Replacing a photo (uploading a new one on edit) overwrites the Storage object and updates `photoUrl`.
- [ ] Removing a photo (via edit form) deletes the Storage object and clears `photoUrl`.
- [ ] Deleting a recipe also deletes its Storage object (no orphaned files).
- [ ] `DELETE .../photo` endpoint works and is covered by tests.
- [ ] Firebase Storage security rules enforce per-user ownership and file constraints.
- [ ] No regression to existing recipe CRUD, list, and ingredient search.
- [ ] Existing test suites pass without modification.

## Testing approach (Build + Test)

| Layer | What to prove |
|-------|---------------|
| **Unit (C#)** | `Recipe` ↔ `RecipeModel` mapping includes `PhotoUrl` round-trip. Controller tests for the new `DELETE .../photo` endpoint with mocked services. |
| **API / Integration** | Photo deletion endpoint returns correct status codes (200, 404, 403). Recipe delete triggers Storage cleanup (mock or emulator). |
| **Frontend (Vitest)** | `RecipeForm`: file input renders, client-side validation rejects oversized / wrong-type files, preview renders on file select. `RecipeDetail`: image renders when `photoUrl` present, omitted when absent. `RecipeList`: thumbnail vs placeholder rendering. |
| **Manual** | End-to-end walkthrough: create recipe with photo, verify display on detail and list, edit to replace photo, remove photo, delete recipe. Verify Storage bucket state. |

## Risks and edge cases

| Risk | Mitigation |
|------|------------|
| **Storage emulator not in local Docker Compose** | Document manual setup for local dev. Add Storage emulator to `firebase.json` and `docker-compose.yml` as part of this feature (or as fast follow-up). |
| **Orphaned files on failed upload** | If the upload succeeds but the recipe save fails, a Storage object exists without a Firestore reference. Mitigate: upload first, then save recipe; on save failure, attempt to delete the just-uploaded file. Acceptable residual risk for v1. |
| **Large files / slow uploads** | Client-side 5 MB limit + Storage security rules. Consider `uploadBytesResumable` for progress indicator. Cloud Run is not involved in the upload path. |
| **Download URL expiry** | Firebase Storage download URLs include an access token and do not expire unless the token is revoked. If tokens are regenerated (e.g. via Firebase console), stored URLs break. Acceptable for v1; document the behavior. |
| **userId vs token (existing pattern)** | `ValuesApiController.EnsureRouteUserMatchesToken` enforces route `userId` == Firebase UID. Same pattern applies to the new `DELETE .../photo` endpoint. Storage rules use `request.auth.uid == userId` for the same effect on direct uploads. |
| **CORS for Storage uploads** | Firebase Storage uploads use Google's own endpoints; CORS on the ASP.NET API is not involved. If custom CORS config is needed on the Storage bucket, set it via `gsutil cors set`. |
| **File extension handling** | Store with a fixed name pattern (`{recipeId}.jpg`) or preserve original extension. Recommend: use the file's MIME type to determine extension, overwrite on replace. |

## Appendix: Tech Spec review findings

### Architecture / boundaries

| # | Finding | Severity | Resolution |
|---|---------|----------|------------|
| A1 | Backend needs a new dependency on `Google.Cloud.Storage.V1` for server-side photo deletion. This introduces a coupling from the API service to Cloud Storage that did not exist before. | Advisory | Encapsulate Storage operations behind an interface (e.g. `IRecipePhotoStorage`) injected into `FirestoreRecipeService` or the controller, so tests can mock it and the pattern follows existing DI in `Startup.cs`. |
| A2 | `DELETE .../photo` endpoint introduces a sub-resource pattern (`{recipeId}/photo`). The existing API only has flat `{userId}` and `{userId}/{recipeId}` routes. | Advisory | Acceptable — sub-resource REST is standard. Document in controller comments. |
| A3 | Client-direct upload means the backend never validates the image content itself (only Storage rules do). If a future requirement needs server-side processing (resize, thumbnail generation), the architecture will need a Cloud Function or API proxy. | Advisory | Acceptable for v1. Document as a known limitation and potential future evolution path. |

### Frontend (frontend-web)

| # | Finding | Severity | Resolution |
|---|---------|----------|------------|
| F1 | Images on `RecipeDetail` and `RecipeList` should use `loading="lazy"` for performance, especially on list pages with many thumbnails. | Advisory | Include `loading="lazy"` on `<img>` tags in both components. |
| F2 | File input needs accessible labeling (`<label>` + `aria-describedby` for validation errors) per existing form patterns in `RecipeForm.jsx`. | Advisory | Follow existing input pattern: `<label htmlFor="photo">`, `aria-describedby` for error text. |
| F3 | `URL.createObjectURL` for preview must be cleaned up with `URL.revokeObjectURL` to avoid memory leaks. | Advisory | Add cleanup in a `useEffect` return or on component unmount / file change. |
| F4 | Upload progress indicator would improve UX for larger files (up to 5 MB). | Advisory | Use `uploadBytesResumable` with an `onProgress` callback to show a progress bar or percentage. Optional for v1 but recommended. |

### Backend / API (backend-saas)

| # | Finding | Severity | Resolution |
|---|---------|----------|------------|
| B1 | The `DELETE .../photo` endpoint should return `204 No Content` on success (RESTful convention for DELETE) rather than `200 OK`. | Advisory | Update endpoint to return `204`. |
| B2 | Server-side photo cleanup on recipe delete is a cross-cutting concern. If the Storage delete fails (network error, bucket misconfigured), the Firestore document still gets deleted, leaving an orphan. | Advisory | Best-effort cleanup: attempt Storage delete, log failure, still delete Firestore doc. A background sweep job can clean orphans later if needed (v2). |
| B3 | The Cloud Run service account (`recipe-app-backend@...`) needs `storage.objects.delete` IAM permission on the photo bucket. This is a deployment/IAM task, not a code change. | Advisory | Document in deployment checklist. |

### Testing strategy

| # | Finding | Severity | Resolution |
|---|---------|----------|------------|
| T1 | Frontend tests should use `vi.mock('firebase/storage')` to avoid importing real Firebase Storage SDK in unit tests. | Advisory | Mock `uploadBytes`, `getDownloadURL`, `deleteObject` at module level. |
| T2 | Backend integration tests with Storage emulator are ideal but the emulator is not currently configured. Unit tests with mocked `IRecipePhotoStorage` are sufficient for v1. | Advisory | Add Storage emulator to `firebase.json` and `docker-compose.yml` as a follow-up for integration test coverage. |
| T3 | Regression tests: existing `RecipeForm.test.jsx`, `RecipeDetail.test.jsx`, `RecipeList.test.jsx` should continue to pass without modification since `photoUrl` is optional/nullable. Verify this during Build. | Advisory | Run existing test suite before and after changes. |

### CI / Docker / deploy surface

| # | Finding | Severity | Resolution |
|---|---------|----------|------------|
| D1 | `firebase.json` does not include a Storage emulator. Add `"storage": { "port": 9199, "host": "0.0.0.0" }` to the `emulators` section. | Advisory | Update as part of this feature or immediate follow-up. |
| D2 | `docker-compose.yml` does not expose port 9199 for the Storage emulator. | Advisory | Add port mapping alongside existing emulator ports. |
| D3 | Production deploy (`prod_deploy.yml`) does not deploy Firebase Storage security rules. Rules must be deployed manually or via a separate workflow step using `firebase deploy --only storage`. | Advisory | Document manual deployment step. Consider adding to CI as follow-up. |
| D4 | `VITE_FIREBASE_STORAGE_BUCKET` env var may be needed for the frontend to initialize Storage with the correct bucket. | Advisory | Add to `.env` files, `docker-compose.yml` env, and `prod_deploy.yml` build env. |

## Human approval

- [ ] Engineering approved before Build

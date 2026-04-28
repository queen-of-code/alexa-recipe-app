# AIDLC Review report — recipe completed photo (issue #67)

**PR:** https://github.com/queen-of-code/alexa-recipe-app/pull/74  
**Tech Spec:** [tech-spec.md](./tech-spec.md)  
**CI:** Green on latest checks (`build_test`, `integration_test`, `build_frontend`, XUnit, Integration Test Results).

Mirrors PR comments posted during Review phase.

---

### AIDLC Review — Tech Spec

**Blocking**

- None observed against `feature/recipe-completed-photo/tech-spec.md` for this branch.

**Advisory**

- **A3 (cross-user image access):** Tech Spec calls for Storage rules plus API `Forbid` on wrong `userId`. API JWT binding is unchanged; **Storage rules** are added (`RecipeApp/frontend/storage.rules`). There is **no automated test** proving rule behavior or a documented manual matrix for A3—consider a short **manual validation script** (two test users) in Validate or a rules unit test if the project adds the harness later.
- **Traceability:** `POST` **201** with body including `recipeId`, `completedImageUrl` on model/Firestore, list/detail thumbnails, create **POST → upload → PUT** flow, delete **best-effort** Storage cleanup (skipped under Firestore emulator) match the spec. **OpenAPI/README** for the new field is optional per spec; still **advisory** to add a one-line contract note for future clients.

**Pass criteria:** Implementation maps to acceptance table A1–A5 in substance; A3 evidence is the main **process** gap, not a spec miss in code.

---

### AIDLC Review — Testing

**Blocking**

- None. **CI is green** on this PR (including `build_test`, `integration_test`).

**Advisory**

- **E2E / A1 path:** Create **with** image and “visible on subsequent load” is covered by **unit** (Vitest create+upload+PUT) and **integration** for metadata round-trip; there is **no full browser E2E** against real Storage + refresh. Acceptable for tutorial tier if **Validate** runs the Product Spec checklist on hosted Firebase.
- **Negative paths:** `CompletedImageMetadata` unit tests cover invalid URLs/paths; consider an explicit **controller/service** test that `PUT` with a rejected `completedImageUrl` returns **400** (if not already asserted)—small gap for operability when debugging bad clients.
- **Frontend:** `RecipeForm.test.jsx` exercises file input and happy-path upload sequence; **edit replace/remove** flows are not mirrored in tests (acceptable advisory).

---

### AIDLC Review — DevOps

**Blocking**

- None for rollout of this feature.

**Advisory**

- **Deploy checklist:** Ensure **`storage.rules`** are **deployed** to the Firebase project alongside existing hosting/Firestore workflow; PR adds rules file and emulator wiring—**production** Firebase console must receive the same rules or uploads fail in prod.
- **Backend credentials:** GCS delete uses `StorageClient` when **not** on Firestore emulator; production/staging need **ADC/service account** with `storage.objects.delete` on the bucket. Document or verify in runbooks if not already standard for this app.
- **Config:** `FIREBASE_STORAGE_BUCKET` / `GCP_PROJECT_ID` defaults (`FirestoreRecipeService`)—confirm env matches actual Firebase bucket name in all deployed environments.

---

### AIDLC Review — Frontend/UX

**Blocking**

- None.

**Advisory**

- **Browser MCP:** Not available in this run—**manual script** for Validate (recommended): log in → create recipe **with** JPEG &lt; 5 MB → confirm thumbnail on list and image on detail → edit **replace** → **remove** → confirm empty placeholder → create **without** photo (regression). Optional: two browsers/incognito for multi-user sanity with Storage rules.
- **Accessibility:** Decorative `img` uses `alt=""` where appropriate; list placeholder uses `aria-hidden` on empty cell—good. **Submit** disables during upload (“Saving…”)—good. Consider whether **replace/remove** controls need clearer **focus** order for keyboard users (minor).
- **Errors:** Create flow surfaces message if recipe created but upload fails—matches spec “rollback message” intent.

---

### AIDLC Review — Security

**Blocking**

- None obvious from diff review (no committed secrets observed).

**Advisory**

- **Storage rules:** Rules enforce auth **uid == path userId**, **5 MiB**, **JPEG/PNG/WebP** on **write**—aligns with Tech Spec. **Read** requires auth; recipe images are not world-readable—confirm that matches product intent (likely yes for private recipes).
- **Metadata validation:** API restricts `completedImageUrl` to **same-user** Storage path or **Firebase download URL** parsing `firebasestorage.googleapis.com`—reduces arbitrary URL injection on the recipe document. **Edge cases:** alternate Firebase URL shapes or future CDN fronts would need parser updates.
- **Delete object:** Backend uses **Google Cloud Storage** client (not Firebase Admin Storage) with **bucket + object path**—acceptable; ensure **service account** scope is least-privilege for delete only as needed.
- **Dependencies:** New **`Google.Cloud.Storage.V1`** — keep pinned as in PR; routine dependency audit applies.

---

## Summary counts

| Severity   | Count |
|------------|-------|
| Blocking   | 0     |
| Advisory   | Several (see sections) |

**Handoff:** `/build` should triage each thread—fix or reply, then resolve conversations before human sign-off.

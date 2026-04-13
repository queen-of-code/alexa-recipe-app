# Tech Spec — `recipe-ingredient-search`

> AIDLC Design phase: [docs/AIDLC.md](../../docs/AIDLC.md) — Phase 2 Design. Product Spec: [product-spec.md](./product-spec.md).

## Units / scope

**Single Unit:** Ingredient-based recipe discovery for the signed-in user.

- **Backend:** HTTP API that returns recipes matching ingredient criteria (structured **and/or** natural-language input), plus a **pure matching** layer over `Recipe.Ingredients` (list of strings).
- **Frontend:** Recipe list page (`RecipeList`) gains ingredient **search/filter** UI: text input, **AND / OR** dropdown, wired to the new API (or equivalent client flow).
- **Agent path:** Same API contract so a voice/agent integration does not fork matching rules; agents send **natural-language** `query` (or structured fields if the agent parses first).

Out of scope for this Unit: Firestore schema changes, ingredient taxonomy, changing the recipe editor, Alexa skill code (only the **API** must be callable by a future agent).

## Architecture and components

| Area | Proposal |
|------|----------|
| **Data access** | Reuse `IFirestoreRecipeService.GetAllRecipesForUser(userId)` to load the user’s recipes, then **filter in memory**. Firestore has no first-class full-text search on arbitrary ingredient strings in this model; for typical library sizes (hundreds), this is acceptable. Document a **future** optimization (server-side index, denormalized tokens) if profiling requires it. |
| **Matching** | New **static or injectable** component, e.g. `IngredientMatcher` / `RecipeIngredientFilter`, that takes `IEnumerable<Recipe>` (or `RecipeModel`) plus a **normalized query model** and returns the filtered sequence. Keeps `ValuesApiController` thin and maximizes unit-test surface. |
| **Query model** | Two request shapes converge to one internal representation: (1) **Structured:** list of **terms** (strings) + **combine mode** `All` (AND) vs `Any` (OR). (2) **Natural language:** single **raw string** parsed into the same term list + mode **or** into an evaluable predicate. **v1:** NL parsing can be **heuristic** (split on commas, detect “and” / “or”, trim, case-fold)—nested boolean trees are **optional**; if omitted, document that multi-clause NL falls back to a single mode or first-pass heuristic. |
| **Internal evaluation** | Implement AND/OR with straightforward loops or a small **predicate builder** (e.g. `Func<Recipe, bool>` composed from per-term checks). **System.Linq.Expressions** is optional—not required unless we need composable trees for a richer grammar later. |
| **API placement** | Extend `RecipeAPI` under the existing **`api/values`** surface **or** add a dedicated route (e.g. `POST api/values/{userId}/search`) to avoid overloading `GET`. Prefer **POST** with a JSON body so ingredient lists and NL strings do not require awkward query-string encoding. |

## API / UI contracts

### HTTP (Firebase JWT — same scheme as existing `api/values`)

**Proposed:** `POST /api/values/{userId}/search`

- **Auth:** `Authorization: Bearer <Firebase ID token>` (unchanged).
- **Body (one of two modes):**

**Mode A — structured (web UI):**

```json
{
  "ingredients": ["tomato", "cheddar"],
  "combine": "All"
}
```

`combine`: `"All"` (every term must match) or `"Any"` (at least one term matches).

**Mode B — natural language (agent):**

```json
{
  "query": "tomatoes and cheddar"
}
```

Server parses `query` into terms + combine mode per Tech rules below. If both `query` and `ingredients` are sent, define precedence in implementation (recommend: **structured wins** if `ingredients` non-empty).

- **Response:** `200 OK` + JSON array of `RecipeModel` (same shape as existing list GET). Empty array when no matches.
- **Errors:** `400` for malformed body; `401`/`403` as today.

**Optional:** `GET` list unchanged; search is always explicit POST so list behavior stays predictable.

### Matching rules (normative for tests)

- **Term normalization:** Trim whitespace; **case-insensitive** comparison.
- **Match definition (v1):** A term **matches** a recipe if **any** ingredient line **contains** the term as a **substring** (after optional normalization). This matches messy home-entered lines (“2 cups diced tomatoes”).
- **AND (`All`):** Recipe is included iff **every** term matches at least one ingredient line.
- **OR (`Any`):** Recipe is included iff **at least one** term matches at least one ingredient line.
- **NL parsing (v1):** Document the exact heuristic (e.g. split on `,`, detect “ and ” / “ or ” between chunks). Ambiguous input: prefer deterministic behavior + tests over “smart” guessing.

### Frontend (`RecipeList.jsx` + `recipeApi.js`)

- Add **controlled inputs:** ingredient text (single field with comma-separated terms, or one term per line—pick one and document), **AND/OR** `<select>`.
- Call `searchRecipes(userId, payload)` → POST as above; replace or supplement local `recipes` state with response when filter is active; **clear** filter restores full list (new GET or POST with empty criteria—define one).
- **Accessibility:** label the dropdown and input; preserve existing table layout.

## Data model

- **No** Firestore or `Recipe` / `RecipeModel` schema changes required for v1.
- Ingredients remain `List<string>` on each recipe document.

## Acceptance criteria (for Review)

- [ ] `POST .../search` returns only recipes for `userId` whose ingredients satisfy the chosen AND/OR rules per tests.
- [ ] Structured body (`ingredients` + `combine`) works from the web client.
- [ ] `query` body path is implemented and covered by tests (at least one AND and one OR NL case, even if heuristic).
- [ ] Recipe list UI exposes ingredient filter + AND/OR and reflects API results (including empty state).
- [ ] No regression to existing recipe CRUD and list GET.
- [ ] Product Spec constraints: per-user data only; no cross-user leakage.

## Testing approach (Build + Test)

| Layer | What to prove |
|-------|----------------|
| **Unit (C#)** | `IngredientMatcher` (or equivalent): matrix of recipes × terms × All/Any; edge cases (empty ingredients, empty term list, Unicode/case). |
| **API** | Controller or integration tests for `POST .../search` with mocked `IFirestoreRecipeService` returning canned recipes (pattern used elsewhere in `RecipeAPI.Tests`). |
| **Frontend (Vitest)** | `RecipeList`: user can change AND/OR and ingredient input; mocked `fetch`/`searchRecipes` receives correct combine mode and shows filtered rows. |
| **Manual / scripted agent** | Document a **curl** or script example for `query` body for Validate demo. |

## Risks and edge cases

| Risk | Mitigation |
|------|------------|
| **NL ambiguity** (“chicken or fish and rice”) | v1 heuristic + tests; document limitations; structured API remains source of truth for exact AND/OR. |
| **Performance** | Full scan of all recipes per search; acceptable for MVP; add profiling note if user libraries grow large. |
| **Substring false positives** (“fish” vs “fishing”) | Accept for v1 or add word-boundary heuristic later; call out in Review if product wants tighter matching. |
| **userId vs token** | Existing API pattern passes `userId` in URL; align with current app. **Review:** consider enforcing `userId == JWT subject` for defense in depth (follow-up if not already enforced). |

## Human approval

- [x] Engineering approved before Build

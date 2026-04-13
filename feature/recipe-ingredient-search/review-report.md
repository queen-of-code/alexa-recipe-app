# Review report — `recipe-ingredient-search`

> AIDLC Review pass for PR (ingredient search). Mirror of GitHub PR comments.

## 1. Tech Spec compliance

**Source:** [tech-spec.md](./tech-spec.md)

| Criterion | Status | Notes |
|-----------|--------|--------|
| `POST /api/values/{userId}/search` | Met | Implemented with structured `ingredients` + `combine` and NL `query`. |
| In-memory filter after `GetAllRecipesForUser` | Met | Matches MVP approach. |
| OR-of-AND NL parsing | Met | `IngredientQueryParser` + `IngredientMatcher`. |
| Recipe list UI: ingredients + AND/OR | Met | `RecipeList.jsx` + `searchRecipes`. |
| Per-user data only | **Gap** | Tech Spec risk row: *userId vs token* — path `userId` is **not** validated against Firebase UID (see Security). |

**Blocking:** Spec intent (“per-user data only”) is not fully satisfied until the API binds requests to the authenticated principal.

---

## 2. Practical testing sufficiency

**Strengths**

- `IngredientMatcherTests`, `IngredientQueryParserTests` cover matcher/parser semantics.
- `ValuesControllerTests` cover structured search, NL `query`, bad request, structured-over-query precedence.

**Gaps (advisory unless noted)**

- **Auth / access:** No automated test that rejects `userId` ≠ JWT subject (would document expected 403/401 once enforced).
- **Controller NL:** Parser has tests; controller has one NL case — consider one more edge (e.g. comma-only `query`) if regressions are a concern.
- **Frontend:** Vitest covers search wiring; no test for error recovery or empty submit.

---

## 3. DevOps — rollout, deploy, monitoring

- From CLI, `gh pr checks` reported **no checks** on the branch at review time — **verify** GitHub Actions (or equivalent) run on this PR before merge.
- No change to Docker/infra in this feature; rollout path unchanged.

---

## 4. Frontend / UX

**Strengths**

- Labeled ingredient field and Match control; Search / Clear; distinct empty copy for “no match” vs “no recipes yet.”

**Findings**

- **Blocking (UX):** On any `error` (including failed search), `RecipeList` returns **only** the error paragraph — user loses the form and table with **no in-page recovery** (must reload). Prefer inline error + keep layout.
- **Advisory:** Submitting Search with **no** ingredient terms does nothing (no feedback).

**Browser MCP:** Not run in this pass; manual spot-check recommended after error-handling improvements.

---

## 5. Security (auth / access)

**Blocking**

- **IDOR-style access:** Endpoints use `userId` from the URL while auth only verifies a valid Firebase JWT. There is no check that `userId` equals the token’s UID (`ClaimTypes.NameIdentifier` in `FirebaseAuthHandler`). An authenticated user could pass another user’s `userId` and read/search/modify their recipes. This pattern predates search but **search extends** the same surface area.

**Advisory**

- Substring ingredient matching can false-positive; acceptable for v1 per Tech Spec.
- NL parsing is heuristic-only; not an injection vector in the same way as SQL, but treat as untrusted user text for logging/abuse only.

---

## Blocking vs advisory summary

| Severity | Topic |
|----------|--------|
| **Blocking** | Bind `userId` route parameter to authenticated Firebase UID (or derive user id only from claims). |
| **Blocking** | Recipe list: do not replace entire page with error-only view on search failure. |
| **Advisory** | Empty search submit feedback; extra NL/controller tests; confirm CI runs on PR. |

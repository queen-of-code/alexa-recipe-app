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
| Per-user data only | Met | Route `userId` must match JWT UID (`ValuesApiController.EnsureRouteUserMatchesToken`); unit tests for forbid path. |

**Build triage (post-review):** Addressed — see GitHub PR **### AIDLC Build — Review triage** comment.

---

## 2. Practical testing sufficiency

**Strengths**

- `IngredientMatcherTests`, `IngredientQueryParserTests` cover matcher/parser semantics.
- `ValuesControllerTests` cover structured search, NL `query`, bad request, structured-over-query precedence.

**Gaps (advisory unless noted)**

- **Auth / access:** `Get_UserId_Mismatch_Forbid`, `Search_UserId_Mismatch_Forbid` assert **403** (`ForbidResult`) when route UID ≠ claim.
- **Controller NL:** Parser has tests; controller has one NL case — consider one more edge (e.g. comma-only `query`) if regressions are a concern.
- **Frontend:** Vitest covers search wiring, empty-submit hint, and dismissible error banner.

---

## 3. DevOps — rollout, deploy, monitoring

- From CLI, `gh pr checks` reported **no checks** on the branch at review time — **verify** GitHub Actions (or equivalent) run on this PR before merge.
- No change to Docker/infra in this feature; rollout path unchanged.

---

## 4. Frontend / UX

**Strengths**

- Labeled ingredient field and Match control; Search / Clear; distinct empty copy for “no match” vs “no recipes yet.”

**Findings (post–build triage)**

- **Resolved:** Dismissible `role="alert"` error banner; form and table remain visible.
- **Resolved:** Empty submit shows `role="status"` hint under the ingredient field.

**Browser MCP:** Optional manual spot-check of the banner + hint still welcome.

---

## 5. Security (auth / access)

**Blocking (resolved in build triage)**

- **IDOR:** `EnsureRouteUserMatchesToken` on all `userId`-scoped routes; `403` when route UID ≠ JWT UID. Covered by `Get_UserId_Mismatch_Forbid` and `Search_UserId_Mismatch_Forbid`.

**Advisory**

- Substring ingredient matching can false-positive; acceptable for v1 per Tech Spec.
- NL parsing is heuristic-only; not an injection vector in the same way as SQL, but treat as untrusted user text for logging/abuse only.

---

## Blocking vs advisory summary

| Severity | Topic |
|----------|--------|
| **Resolved** | `userId` vs Firebase UID enforced (`403` on mismatch). |
| **Resolved** | Inline error banner + dismiss; empty-search hint. |
| **Advisory** | Confirm CI runs on PR (DevOps); optional extra NL edge tests. |

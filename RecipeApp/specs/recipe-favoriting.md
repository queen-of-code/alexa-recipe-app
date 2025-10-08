# Feature Spec: Recipe Favoriting

## Overview
Allow users to mark/unmark recipes as "favorites" for quick access. Favorited recipes can be viewed in a dedicated list. This feature will require changes to both the backend (API, data model) and frontend (UI, service calls).

---

## Requirements
- Users can favorite or unfavorite any recipe they own.
- Users can view a list of their favorited recipes.
- Favorited status is persisted per user and recipe.
- Favorited status is visible in the recipe list and details views.

---

## Backend Changes

### 1. **Data Model**
- **Option A:** Add a `Favorites` list (of recipe IDs) to the user profile (if such a profile exists in DynamoDB or MySQL).
- **Option B (Minimal):** Add a boolean `IsFavorite` property to the `Recipe` model (per user/recipe pair).
- **Recommended:** Use Option B for minimal change and atomicity.

#### DynamoDB `Recipe` Model
- Add property:
  ```csharp
  [DynamoDBProperty]
  public bool IsFavorite { get; set; }
  ```

### 2. **API Changes**
- Update `RecipeAPI` endpoints to support toggling and querying favorite status:
  - `GET /api/values/{userId}?favoritesOnly=true` — Return only favorited recipes if query param is set.
  - `PUT /api/values/{userId}/{recipeId}/favorite` — Mark as favorite.
  - `DELETE /api/values/{userId}/{recipeId}/favorite` — Remove from favorites.
- Update existing endpoints to include `IsFavorite` in the returned `RecipeModel`.

### 3. **Unit Tests**
- Add tests for new endpoints and model changes.
- Test favoriting/unfavoriting logic and query filtering.

---

## Frontend Changes (Website)

### 1. **UI Updates**
- Add a star/heart icon to each recipe in the list and details views.
- Clicking the icon toggles favorite status (filled = favorited, outline = not favorited).
- Add a "Show Favorites" filter/button to the recipe list page.

### 2. **Service Layer**
- Update `RecipeService` to call new API endpoints for favoriting/unfavoriting.
- Update recipe fetch methods to support the `favoritesOnly` filter.

### 3. **Unit Tests**
- Add tests for UI logic (e.g., toggling favorite state, filtering by favorites).

---

## Atomic Steps

### Backend
1. Update DynamoDB `Recipe` model to include `IsFavorite` property.
2. Update API endpoints to support:
   - Toggling favorite status (`PUT`/`DELETE` endpoints)
   - Filtering by favorites (`GET` endpoint with query param)
   - Returning `IsFavorite` in all recipe responses
3. Add/Update unit tests for new endpoints and model logic.

### Frontend
4. Update `RecipeService` to support new API endpoints and query param.
5. Update recipe list and details pages to show favorite icon and allow toggling.
6. Add "Show Favorites" filter/button to recipe list page.
7. Add/Update unit tests for UI and service logic.

---

## Test Plan

### Backend
- **Unit tests:**
  - Favoriting/unfavoriting a recipe sets/clears `IsFavorite`.
  - Filtering by favorites returns only favorited recipes.
  - API returns correct `IsFavorite` status.
- **Integration tests (optional):**
  - End-to-end test: user favorites a recipe, fetches favorites, unfavorites, and verifies removal.

### Frontend
- **Unit tests:**
  - UI correctly displays favorite status.
  - Clicking icon toggles favorite state and calls API.
  - "Show Favorites" filter only displays favorited recipes.
- **Manual test:**
  - User can favorite/unfavorite recipes and see changes reflected immediately in UI and after refresh.

---

## Notes
- This spec assumes each user can favorite their own recipes. If you want to allow favoriting of shared/public recipes, further changes may be needed.
- If you prefer to store favorites separately from the recipe (e.g., in a user profile), adjust the data model and API accordingly. 
# Test Overview

This document summarizes the current automated test coverage for the Alexa Recipe App, broken down by unit and integration tests, and by backend (RecipeAPI) and frontend (Website).

---

## Unit Tests

### Backend (RecipeAPI)
- **DynamoRecipeServiceTests.cs**
  - Covers: Saving recipes (valid, invalid, missing ID), SaveItem logic, entity ID assignment, and validation logic.
  - Mocks DynamoDB context to test service logic in isolation.
- **DynamoTableCreationTests.cs**
  - Covers: Table creation logic for DynamoDB models.
- **MealTests.cs**
  - Covers: Basic model logic for Meal entity.
- **RecipeTests.cs**
  - Covers: Basic model logic for Recipe entity (e.g., equality, hash code, validation).
- **ValuesControllerTests.cs**
  - Covers: API controller logic for GET, PUT endpoints, including response types and mapping between models and API responses.

**Gaps:**
- No unit tests for all possible error paths in controllers.
- No unit tests for authentication/authorization logic.
- No unit tests for all edge cases in model validation.

### Frontend (Website)
- **RecipeServiceTests.cs**
  - Covers: Service logic for creating a recipe (success path, using mocked HTTP client).

**Gaps:**
- No unit tests for UI components (Razor Pages).
- No unit tests for error handling or edge cases in RecipeService.
- No tests for authentication, authorization, or user flows.

---

## Integration Tests

### Backend (RecipeAPI)
- **RecipeAPITests.cs**
  - Covers: End-to-end test for saving and deleting a recipe via the API, using real HTTP requests and JWT authentication.

**Gaps:**
- No integration tests for all API endpoints (e.g., GET all recipes, update, error cases).
- No tests for authentication/authorization failures.
- No tests for DynamoDB failure scenarios.

### Frontend (Website)
- **SmokeTests.cs**
  - Covers: Basic HTTP GET requests to main pages (Index, Contact, About, Login) to verify the site is up and pages are accessible.

**Gaps:**
- No integration tests for recipe CRUD flows via the UI.
- No tests for user authentication, registration, or protected routes.
- No tests for error handling or edge cases in the UI.

---

## Summary
- **Strengths:**
  - Core backend service logic and models are well-covered by unit tests.
  - Basic API and website availability are checked by integration tests.
- **Weaknesses:**
  - Limited coverage for UI, authentication, and error scenarios.
  - Integration tests do not cover all user flows or edge cases.

**Recommendation:**
- Add unit and integration tests for new features (e.g., recipe favoriting).
- Expand integration tests to cover more end-to-end user scenarios, especially for the UI and authentication flows. 
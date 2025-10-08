# Alexa Recipe App - Architecture Overview

## Purpose
The Alexa Recipe App is a web-based platform for storing, managing, and accessing personal recipes, with support for Alexa voice integration. Users can create, edit, categorize, and back up recipes, and interact with their recipe library via both a web UI and Alexa-enabled devices.

## High-Level Architecture

The application is composed of several services, orchestrated via Docker Compose:

- **Website (Web UI):** ASP.NET Core Razor Pages application for user interaction, recipe management, and authentication.
- **RecipeAPI:** ASP.NET Core Web API for CRUD operations on recipes, backed by DynamoDB.
- **MySQL:** Relational database for user authentication and identity management.
- **DynamoDB (Local):** NoSQL database for storing recipes, meal plans, and related data.

### Docker Compose Services
- `website` (port 3000): The main user-facing web application.
- `recipeapi` (port 8080): The backend API for recipe data.
- `mysql` (port 3306): User and identity data.
- `dynamodb` (port 8000): Recipe and meal plan data.

## Data Structures

### Recipe (DynamoDB)
- **UserId** (string): Partition key, identifies the recipe owner.
- **EntityId** (long): Sort key, unique per recipe.
- **Name** (string): Recipe name.
- **LastUpdateTime** (DateTime): Last modified timestamp.
- **Ingredients** (list of string): Ingredients list.
- **Steps** (list of string): Step-by-step instructions.
- **Servings** (int): Number of servings.
- **PrepTimeMins** (int): Preparation time in minutes.
- **CookTimeMins** (int): Cooking time in minutes.
- **VersionNumber** (int?): Optional versioning.

Other models include `Meal`, `Plan`, and `Person` for meal planning and user context.

## Client Access Patterns

### Web UI (Website)
- Users interact with Razor Pages for recipe CRUD, meal planning, and account management.
- Authenticated via ASP.NET Identity (MySQL backend) and optionally Microsoft/Facebook OAuth.
- The Website project uses a `RecipeService` class to call the RecipeAPI via HTTP, using JWT authentication for secure API access.

### API (RecipeAPI)
- Exposes RESTful endpoints under `/api/values` for recipe CRUD:
  - `GET /api/values/{userId}`: List all recipes for a user.
  - `GET /api/values/{userId}/{recipeId}`: Get a specific recipe.
  - `POST /api/values/{userId}`: Create a new recipe.
  - `PUT /api/values/{userId}/{recipeId}`: Update a recipe.
  - `DELETE /api/values/{userId}/{recipeId}`: Delete a recipe.
- All endpoints require JWT Bearer authentication.
- API interacts with DynamoDB via a service layer (`IDynamoRecipeService`).

## Key Architectural Choices

- **Separation of Concerns:** Web UI and API are separate services, allowing for independent scaling and deployment.
- **NoSQL for Recipes:** DynamoDB is used for recipe and meal plan data, enabling flexible, scalable storage for user-generated content.
- **Relational DB for Identity:** MySQL is used for user authentication and identity, leveraging ASP.NET Identity.
- **JWT Authentication:** Secure API access between Website and RecipeAPI using JWT tokens.
- **Dockerized Development:** All services can be run locally via Docker Compose for easy onboarding and consistent environments.
- **Extensible Models:** Recipe and related models are designed for easy extension (e.g., meal planning, Alexa integration).

## Open Questions / TODOs
- How is Alexa integration handled (direct API calls, or via a separate skill service)?
- Are there additional access patterns (e.g., mobile app, public API)?
- What are the backup/restore strategies for DynamoDB and MySQL data?

---
This document provides a high-level overview. For more details, see the individual service READMEs and code documentation. 
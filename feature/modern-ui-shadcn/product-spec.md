# Product Spec — Modern UI with ShadCN

> AIDLC Plan phase: [docs/AIDLC.md](../../docs/AIDLC.md) — Phase 1 Plan. **Product language only** — implementation belongs in Design.

## Overview

| Field | Value |
|-------|-------|
| **Feature** | Modern UI with ShadCN |
| **Status** | Draft — awaiting product approval |
| **Author** | AIDLC Plan agent (headless run) |
| **Created** | 2026-06-09 |
| **Last updated** | 2026-06-09 |
| **Tracker** | [queen-of-code/alexa-recipe-app#87](https://github.com/queen-of-code/alexa-recipe-app/issues/87) |
| **Feature folder** | `feature/modern-ui-shadcn/` |

## Problem and audience

**Problem:** The Recipe App web experience feels **plain and dated**. Layout and controls look utilitarian rather than polished; some surfaces (especially navigation and the recipe list) **do not work well on phones and small tablets**. The result is a site that functions but does not feel **modern, inviting, or responsive** — undermining confidence in a product people use for personal recipes they care about.

**Audience:** Everyone who uses the **web app**: visitors on marketing pages (Home, About, Contact), people signing in, and authenticated users managing recipes (list, detail, create/edit). Voice/Alexa and API-only clients are **out of scope** for this Feature unless they share no UI.

**Current experience (baseline):** A React single-page app styled with **hand-written Tailwind utility classes** on raw HTML elements. Visual patterns (buttons, inputs, cards, alerts) are **repeated inconsistently** across pages. The top navigation is a **horizontal link row** without a mobile-friendly pattern. The recipe list uses a **wide table** that is hard to use on narrow viewports. Confirmations use the browser’s native dialog. Loading and empty states are minimal text.

**Stakeholder direction (from issue #87):** Adopt **ShadCN** as the component approach because it will look **prettier**, **very modern**, and **responsive**.

## Customer outcomes

- The web app has a **cohesive, modern visual language** — consistent spacing, typography, color use, and interactive states (hover, focus, disabled) across **all existing pages and flows**.
- **ShadCN-based shared UI primitives** replace one-off styling so new screens (including future Features) can reuse the same look and behavior without re-copying long class strings.
- The experience is **fully usable on mobile, tablet, and desktop**: navigation is accessible on small screens; forms and the recipe list remain readable and operable without horizontal scrolling or cramped tap targets.
- **Core journeys feel polished**, not merely restyled: sign-in/register, browse recipes, search/filter by ingredients (when present), view recipe detail, create/edit/delete recipes, and static marketing pages all feel like one product.
- **Destructive actions** (e.g. delete recipe) use a **clear in-app confirmation** instead of the browser’s generic confirm dialog.
- **Loading, error, and empty states** are intentional and consistent — users always know whether content is loading, failed, or genuinely empty.

## Success criteria (for Validate / scorecard)

| # | Criterion | How we’ll verify |
|---|-----------|------------------|
| 1 | **Visual consistency** | On Home, About, Contact, Login, recipe list, detail, and form, primary actions, text fields, cards, and alerts share the **same design system** (no mix of unrelated button colors or ad-hoc error banners). |
| 2 | **Mobile navigation** | At a **~375px-wide** viewport, users can reach Home, About, Contact, My Recipes (when signed in), and Login/Logout **without overflow or unusably small links** (e.g. collapsible menu or equivalent). |
| 3 | **Responsive recipe list** | At **~375px-wide**, the recipe list remains **scannable and actionable** (view/edit/delete and search controls usable); no requirement to scroll horizontally to complete common tasks. |
| 4 | **Responsive forms** | Create/edit recipe form fields (including prep/cook/servings and ingredient lines) stack or reflow so labels and inputs are readable on **phone-width** viewports. |
| 5 | **Modern polish** | Interactive elements show visible **focus** and **hover** states; typography and whitespace feel intentional on marketing and app pages (not “default browser form”). |
| 6 | **Delete confirmation** | Deleting a recipe from list or detail shows an **in-app confirmation**; cancel keeps the recipe; confirm removes it as today. |
| 7 | **No functional regression** | All existing routes and behaviors work: auth, CRUD, ingredient search/filter (if shipped), photo upload on recipe form, and navigation between pages. **Automated frontend tests** updated/passing where selectors or markup changed. |
| 8 | **ShadCN foundation** | A **documented, reusable set of ShadCN-based components** is available for the app shell and forms (exact inventory is Design); at minimum buttons, inputs, cards, and alerts/dialogs used on migrated surfaces. |

## Out of scope

- **New product capabilities** (favoriting, new search modes, meal planning, Alexa behavior changes) — this Feature is **look, feel, and responsiveness** only. If other Features land in parallel, they should **consume** this design system but are not required deliverables here.
- **Backend, API, or data model** changes except those strictly required for UI (none anticipated).
- **Full rebrand**: new logo, brand strategy, marketing copy rewrite, or new illustration/photography direction (minor footer/date fixes and fixing broken image references are OK as polish).
- **Dark mode** or user-selectable themes in v1.
- **Internationalization** or accessibility audit beyond sensible defaults (focus rings, labels, contrast) that ShadCN patterns provide.
- **Redesign of non-web clients** (Alexa skill, admin tools outside `RecipeApp/frontend`).
- **Prescribing** TypeScript migration, specific ShadCN install steps, or file layout — Design / Tech Spec.

## Constraints

- Work applies to the **existing Recipe App frontend** (`RecipeApp/frontend`) and **all routes** currently exposed in the app shell.
- **Tailwind CSS is already in use**; the visual refresh should build on that stack rather than introducing a second styling paradigm.
- **ShadCN** is the **requested** component library per the Feature owner; Design may choose compatible configuration (e.g. JSX vs TSX) but the shipped UI must be **ShadCN-based** as specified.
- Changes must not break **Firebase authentication**, **recipe API** integration, or **photo upload** flows.
- **CI** must remain green after migration (tests and lint adjusted as needed — implementation detail for Build).

## Assumptions

*(Headless Plan run — no live product Q&A. Reasonable defaults; revise after human review.)*

- **Feature folder:** Issue #87 did not include `AIDLC feature folder: feature/<slug>/`; this folder **`feature/modern-ui-shadcn/`** is chosen from the issue title and ShadCN direction.
- **Brand color:** Existing **violet/indigo** accent can remain the primary brand hue unless the product owner requests a palette change during approval.
- **Scope of migration:** **All current pages** in one Feature (not a phased “login only” release), delivered as a single cohesive refresh.
- **Ingredient search UI:** If ingredient search/filter is already on the recipe list, it is **restyled and made responsive** as part of this work; no change to search rules or API semantics.
- **Logo:** Nav references `/logo.png` but the asset may be missing; fixing or replacing the logo display is **in scope** as part of shell polish, not a separate brand project.
- **Dark mode:** Excluded from v1 unless the owner expands scope before Design.
- **TypeScript:** The app is **JavaScript (JSX)** today; staying on JSX is acceptable if ShadCN can be integrated without a full TS migration (Design decides).

## Grounding review (agent-grounding-reviewer)

Reviewed against repo context (frontend sources, existing Feature specs, AIDLC boundaries). Severity per grounding-reviewer intent:

| Severity | Finding |
|----------|---------|
| **Advisory** | **ShadCN is not present** today; frontend uses raw elements + Tailwind v4 (`RecipeApp/frontend/package.json`, `src/index.css`). This Feature is a **greenfield design-system adoption**, not swapping an existing component library. |
| **Advisory** | **JavaScript, not TypeScript** — ShadCN docs often assume TSX. Design must choose a viable JSX setup; Product does not require TS migration. |
| **Advisory** | **Parallel Features** (`feature/recipe-favoriting/`, `feature/recipe-ingredient-search/`) may add UI after or during this work. This spec covers **existing surfaces** and the **shared component foundation**; those Features should reuse the system without duplicating one-off styles. |
| **Advisory** | **Orphan `App.css`** and **stale footer year** are cosmetic debt; safe to clean up during migration but not success-criteria gates on their own. |
| **Advisory** | README still mentions port **3000** while Vite dev server uses **5173** — documentation mismatch only; not a Product Spec deliverable. |
| **Blocking** | *None* — a ShadCN-based responsive refresh of the current SPA is achievable without contradicting app boundaries or other approved Product Specs. |

## Decisions (resolved for this draft)

| Date | Decision |
|------|----------|
| 2026-06-09 | **ShadCN** is in scope as the component approach per issue author request. |
| 2026-06-09 | **All current web routes** are in scope for visual/responsive refresh in v1. |
| 2026-06-09 | **Dark mode** is out of scope for v1. |

## Related documents

- AIDLC process: [docs/AIDLC.md](../../docs/AIDLC.md)
- Issue: https://github.com/queen-of-code/alexa-recipe-app/issues/87
- Frontend entry: `RecipeApp/frontend/`

## Human approval

- [ ] Product owner approved before Design

# Product Spec — `recipe-ingredient-search`

> AIDLC Plan phase: [docs/AIDLC.md](../../docs/AIDLC.md) — Phase 1 Plan.

## Problem and audience

**Problem:** People accumulate many saved recipes. When they have specific ingredients on hand, scanning the full list to see which recipes use those ingredients is slow and error-prone—especially when they are not at a keyboard.

**Audience:** Authenticated users with a personal recipe library. We need **two first-class surfaces**: (1) **agentic / conversational** (e.g. at home: “I have tomatoes and cheddar—what recipes use those?”) and (2) the **web app recipe list**, where users can **search or filter by ingredients** without voice. Both should hit the same backend behavior so AND/OR rules stay consistent.

## Customer outcomes

- On the **recipe list** page, users can **narrow recipes by ingredients** (search and/or filter—exact control pattern is Design) with an explicit **AND vs OR** choice. For **simplicity**, Product expects something like a **dropdown** (AND / OR) next to the ingredient input rather than a full boolean expression builder in v1.
- Users can **ask in natural language** via an **agent or voice path** which of **their** recipes match ingredients they care about, and get the same **trustworthy list** (or a clear “none” answer) as the web flow.
- The **backend** does the **heavy lifting** on matching (normalization, applying rules)—including accepting a **raw string** when that keeps clients thin—while the **web UI** sends ingredient text plus the selected **AND/OR** mode so behavior stays predictable without requiring users to type “AND” in the box.
- Matching supports **compound intent**: at minimum **AND** and **OR** across multiple ingredients. Nested boolean expressions beyond “one mode for this query” are optional in v1 if Tech can keep them testable; otherwise defer.
- Responses stay **fast enough** for both web and conversational use with typical personal library sizes (dozens to hundreds of recipes per user).

## Success criteria (for Validate / scorecard)

- **Web:** From the recipe list, a user can enter ingredient criteria, choose **AND or OR**, and see a **filtered list** (or empty state). **Automated frontend tests** cover the control and that results reflect the selected mode (per Tech Spec mocking strategy).
- **API:** A **documented contract** returns recipe matches for a signed-in user given ingredient input and **AND/OR** semantics, with **explicit, testable rules**.
- **Backend tests** prove matching rules at the appropriate layer (unit and/or API per Tech Spec).
- **Agent / voice story** is demonstrable in Validate (agent or scripted client acceptable if voice is not automated).

## Out of scope

- Public or social discovery of recipes across users.
- Structured nutrition data, allergens-as-first-class filters, or substitution engines (unless we explicitly expand scope later).
- OCR, barcode, or photo-based ingredient capture.
- Changing how ingredients are **entered** when creating/editing a recipe (free-text lines remain unless a separate feature says otherwise).
- Prescribing **how** the agent or device turns speech into text—only that the **recipe service** accepts ingredient-oriented requests and returns matches for **that user’s** recipes.

## Constraints

- Recipes are **per-user**; results must only ever include the signed-in user’s recipes.
- Ingredients are stored today as **human-entered strings** (not a global canonical ingredient catalog). Matching operates on that text; fuzzy behavior (partial words, case, etc.) is defined in Tech Spec.
- **Implementation choices** (e.g. parsing libraries, internal representation of boolean criteria) belong in the Tech Spec—Product requires **predictable** AND/OR semantics from the caller’s perspective, not a specific stack feature.

## Decisions (resolved in Plan)

- **Surfaces:** **Web recipe list** ingredient search/filter **and** **agentic / conversational** use are both **in scope** for this Feature—not optional add-ons.
- **Web AND/OR UX:** Use a simple control (e.g. **dropdown**: AND / OR) next to ingredient input for v1; avoid a full expression UI unless we expand later.
- **Backend-heavy:** Server applies matching rules; web sends **ingredients + mode**; agent path may still send **raw natural language** for the backend to interpret—Tech Spec unifies these so rules do not diverge.
- **Boolean logic:** At minimum **AND** and **OR** across multiple ingredients; nested expressions are optional in v1. Internal representation (e.g. expression trees in .NET) is a Design detail.

## Human approval

- [x] Product owner approved before Design

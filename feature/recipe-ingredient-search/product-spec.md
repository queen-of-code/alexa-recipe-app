# Product Spec — `recipe-ingredient-search`

> AIDLC Plan phase: [docs/AIDLC.md](../../docs/AIDLC.md) — Phase 1 Plan.

## Problem and audience

**Problem:** People accumulate many saved recipes. When they have specific ingredients on hand, scanning the full list to see which recipes use those ingredients is slow and error-prone—especially when they are not at a keyboard.

**Audience:** Authenticated users with a personal recipe library. The **primary experience** is **agentic / conversational** (e.g. at home: “I have tomatoes and cheddar cheese—what recipes do I have that use those?”). A **web or other UI** may reuse the same capability later, but the Product outcome is defined around that ask-and-answer loop.

## Customer outcomes

- Users can **ask in natural language** (via an agent or voice path) which of **their** recipes match ingredients they care about, and get a **trustworthy list** (or a clear “none” answer).
- The **backend** is responsible for interpreting the request—including accepting a **raw string** from the client or agent and doing the **heavy lifting** (normalization, tokenization, and application of matching rules)—so clients stay thin.
- Matching supports **compound intent**: not only single ingredients, but **logical combinations**—at minimum **AND** and **OR** (e.g. “tomatoes AND cheddar” vs “basil OR oregano”). Exact grammar and nesting are specified in Design/Tech so behavior is testable and consistent.
- Responses stay **fast enough** for conversational use with typical personal library sizes (dozens to hundreds of recipes per user).

## Success criteria (for Validate / scorecard)

- A **documented API** (or equivalent contract) returns recipe matches for a signed-in user given ingredient-related input, with **explicit, testable rules** for AND/OR (and any nesting we support).
- **Automated tests** prove those rules end-to-end at the appropriate layer (unit and/or API tests per Tech Spec).
- The **primary user story** (natural-language style ask → list of matching recipes) is demonstrable in Validate (agent or scripted client acceptable if voice is not automated).

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

- **Primary surface:** Agentic / conversational (“what can I make with X?”), not a web-only filter as the definition of done.
- **Backend-heavy:** Sending a **raw string** (or agent-produced text) to the **backend** for interpretation and matching is **in scope** and preferred over pushing all parsing to thin clients.
- **Boolean logic:** Support **AND** and **OR** (and **nested** combinations if we can specify and test them clearly). How this is represented in code (e.g. expression trees in .NET) is a Design detail, not a Product requirement by name.

## Human approval

- [ ] Product owner approved before Design

# Cursor Automations (Linear AIDLC)

Version-controlled exports of **Cursor Cloud Agent automations** for the Alexa Recipe App Linear workflow.

Import or recreate these in the [Cursor Automations dashboard](https://cursor.com/automations). They dispatch agents when Linear issues move through AIDLC phases (or when a PR opens for Review).

## Automations

| File | Name | Trigger |
|------|------|---------|
| [agentic-linear-plan.json](agentic-linear-plan.json) | Agentic Linear - Plan | Linear status → **Plan** |
| [agentic-linear-design.json](agentic-linear-design.json) | Agentic Linear - Design | Linear status → **Design** |
| [agentic-linear-build.json](agentic-linear-build.json) | Agentic Linear - Build | Linear status → **Build+Test** |
| [agentic-linear-review.json](agentic-linear-review.json) | Agentic Linear - Review | GitHub PR **opened** on this repo |
| [agentic-linear-in-staging.json](agentic-linear-in-staging.json) | Agentic Linear - In Staging | Linear status → **In Staging** |
| [agentic-linear-ship.json](agentic-linear-ship.json) | Agentic Linear - Ship | Linear status → **Ship** |

## Scope

- **Team:** Queen of Code (`5e36991d-7a2b-4776-a9fe-e7d6c4abc31a`)
- **Project:** Alexa Recipe App (`e8693be3-5af1-4089-b908-944f9967d541`)
- **Linear MCP:** server id `5424658`
- **Chrome DevTools MCP:** server id `5491420` (In Staging, Ship)

## Environments

| Automation | `environmentPublicId` |
|------------|-------------------------|
| Plan, Design, Build, Review, Ship | `6e835264-408a-4726-8cf9-fbd0c15f22af` |
| In Staging | `ad5b5006-b6a2-11f1-bb68-864e54d14197` |

## Prerequisites

1. **Linear `bot-working` label** on the Queen of Code team (agents set/clear it while running).
2. **Cursor ↔ Linear** integration and **Linear MCP** connected in Cursor.
3. **Environment secrets** for UI validation: `AGENT_PROD_URL`, `AGENT_PROD_USERNAME`, `AGENT_PROD_PASSWORD` (see [AGENTS.md](../../AGENTS.md)).
4. **GitHub** repo linked for PR triggers and Review automation.

## Human gates

Agents draft specs and implement slices, but **state moves are human approval gates**:

- Plan → Design = Product Spec approved
- Design → Build+Test = Tech Spec + slice plan approved (also dispatches first build)
- Review sign-off + merge → In Staging (native automation when configured)

See [linear-workflow.md](../linear-workflow.md) for the full playbook.

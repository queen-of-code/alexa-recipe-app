# Alexa Recipe Tutorial App

![Continuous Integration](https://github.com/queen-of-code/alexa-recipe-app/workflows/Alexa%20Recipe%20App%20CI/CD/badge.svg)
![Prod Continuous Delivery](https://github.com/queen-of-code/alexa-recipe-app/workflows/Prod%20Continuous%20Delivery/badge.svg)

---

## AI-DLC tutorial showcase

This repo doubles as a **hands-on demo** for the **AI Development Lifecycle (AIDLC)**. The process definition is **[docs/AIDLC.md](docs/AIDLC.md)** (canonical copy for this repo).

**Bundled skills:** [`.claude/skills/`](.claude/skills/) contains a **committed copy** of the phase orchestrators (`plan`, `build`, `review`, `ship`) plus library skills and agent bundles from **[AI-DLC](https://github.com/queen-of-code/AI-DLC)** ([skills index](https://github.com/queen-of-code/AI-DLC/blob/main/docs/SKILLS.md)), so a plain `git clone` has everything needed. The upstream source is tracked as a **git submodule** at [`vendor/ai-dlc`](vendor/ai-dlc) (branch `main`).

**Refresh vendored skills** (maintainers): `git submodule update --init --remote vendor/ai-dlc` then `./scripts/sync-ai-dlc-skills.sh`, then commit changes under `.claude/skills/`.

**Clone with submodule:** `git clone --recurse-submodules <url>` (or after clone: `git submodule update --init --recursive`).

**What to run:** phase orchestrator skills **`/plan`**, **`/build`**, **`/review`**, **`/ship`** (Claude Code / Cursor Agent — see [.claude/skills/](.claude/skills/)). Those skills pull in domain skills (frontend, backend, testing, …) for you.

**Docs:** [docs/aidlc-showcase.md](docs/aidlc-showcase.md) · [docs/github-queue.md](docs/github-queue.md) · [AGENTS.md](AGENTS.md) (for AI assistants only)

**Git:** this repository’s default branch is **`master`** — open PRs against `master`, not `main`.

---

# Introduction

This is a simple dotnet core demo project for standing up a website / API / Database using Docker containers and docker-compose for local development.

# Getting Started

1.  Install AND run Docker for Windows/Mac/WhateverYourOSIs
2.  Install dotnet core 9.0 at a minimum
3.  Something to edit C# in (VS Code or Cursor are good choices).

If you are attending an in-person tutorial, please ensure that you have successfully run Docker before class. Your laptop may require several reboots to enable virtualization. NOTE: if you are using a MacBook with Windows, virtualization is complicated. You may have to soft-boot from MacOS into Windows to get it to work.

# Build and Test

From within the RecipeApp folder, you can build each component independently, or merely run 'docker-compose build' to compile all dependencies and create the requisite images.

You will need to set an environment variable 'TAG' in order to run.

Depends on your environment.
In powershell, doing \$env:TAG = 'my-tag' will work.
In terminal (MacOS), doing 'export TAG=my-tag' will work.

Then you can run 'docker-compose up' to run the services.

In order to run the application, you'll need to ensure that all the secrets exist (as text files) in your home directory. For example, `~/.docker/secrets/Authentication.Microsoft.ApplicationId.txt` should exist. It doesn't matter what's in the file for most purposes, since you will not be testing with Microsoft or Facebook accounts.

# Running

By default, the application runs on the following ports:
Website - http://localhost:3000
API - http://localhost:8080

# Tutorial Participants

Melissa Benua
Janna Loeffler

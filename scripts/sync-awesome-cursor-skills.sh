#!/usr/bin/env bash
# Idempotent: copy allowlisted awesome-cursor skill bundles into .claude/skills/
# Prerequisites: git submodule at vendor/awesome-cursor (see README).
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
SRC="${REPO_ROOT}/vendor/awesome-cursor/skills"
DST="${REPO_ROOT}/.claude/skills"

if [[ ! -d "${REPO_ROOT}/vendor/awesome-cursor/skills" ]]; then
  echo "error: vendor/awesome-cursor/skills missing. Run: git submodule update --init --recursive" >&2
  exit 1
fi

sync_one() {
  local from_rel="$1"
  local to_name="$2"
  local from="${SRC}/${from_rel}"
  local to="${DST}/${to_name}"
  if [[ ! -d "$from" ]]; then
    echo "error: missing source directory: $from" >&2
    exit 1
  fi
  mkdir -p "$to"
  rsync -a --delete "${from}/" "${to}/"
  echo "synced ${from_rel} -> .claude/skills/${to_name}"
}

TOP_LEVEL=(
  plan
  build
  review
  ship
  architecture
  backend-saas
  frontend-web
  git-workflow
  spec-management
  testing
)

for name in "${TOP_LEVEL[@]}"; do
  sync_one "${name}" "${name}"
done

AGENTS=(
  agent-planner
  agent-reviewer
  agent-grounding-reviewer
  agent-product-manager
  agent-learn
  agent-security-review
  agent-devops-review
)

for name in "${AGENTS[@]}"; do
  sync_one "agents/${name}" "${name}"
done

echo "done: ${#TOP_LEVEL[@]} top-level + ${#AGENTS[@]} agent bundles"

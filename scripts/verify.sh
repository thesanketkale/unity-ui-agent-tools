#!/usr/bin/env bash
# scripts/verify.sh: the single answer to "is this change OK?"
#
# Run by you, the git pre-commit hook, and every agent (Claude Code and Antigravity):
#   bash scripts/verify.sh fast   # < ~30 s, no test suites: lint, format, types, staged-file checks
#   bash scripts/verify.sh full   # everything: fast + tests, build, analysis, migrations, validate
# Exit codes: 0 ok (INCOMPLETE still exits 0, so read the last line), 1 a check failed, 3 full ran no checks at all (not green).

set -o pipefail
MODE="${1:-full}"
case "$MODE" in fast|full) ;; *) echo "usage: verify.sh [fast|full]" >&2; exit 64 ;; esac
ROOT="$(git rev-parse --show-toplevel 2>/dev/null || pwd)"
cd "$ROOT" || exit 1

# Ensure agent-team managed tools (~/.agent-team/bin) are in PATH
if [ -d "$HOME/.agent-team/bin" ]; then
  case ":$PATH:" in *":$HOME/.agent-team/bin:"*) ;; *) PATH="$HOME/.agent-team/bin:$PATH" ;; esac
fi

FAILED=""; SKIPPED=""; RAN=0
step() {
  local label="$1"; shift
  printf '\n> %s\n' "$label"; RAN=$((RAN + 1))
  if "$@"; then printf 'ok  %s\n' "$label"; else printf 'FAIL %s\n' "$label"; FAILED="$FAILED, $label"; fi
}
staged() { git diff --cached --name-only --diff-filter=ACMR -- "$@"; }
skip() {
  case "$SKIPPED," in *", $1,"*) return 0 ;; esac
  printf 'SKIPPED %s\n' "$1" >&2; SKIPPED="$SKIPPED, $1"
}
have() { if command -v "$1" >/dev/null 2>&1 || [ -x "$1" ]; then return 0; fi; skip "$1 (not installed)"; return 1; }

# ---------------------------------------------------------------- FAST ----
fast_checks() {
  # dotnet format check on Core.Tests
  if have dotnet; then
    if [ -d "tests/Core.Tests" ]; then
      step "dotnet format Core.Tests" dotnet format --verify-no-changes tests/Core.Tests
    elif [ -d "tests/UiContext.Core.Tests" ]; then
      step "dotnet format Core.Tests" dotnet format --verify-no-changes tests/UiContext.Core.Tests
    fi
  fi

  # CLI lint and typecheck
  if have npm && [ -f "cli/package.json" ]; then
    if grep -q '"lint"' cli/package.json; then
      step "npm run lint (cli)" npm --prefix cli run lint
    fi
    if grep -q '"typecheck"' cli/package.json; then
      step "npm run typecheck (cli)" npm --prefix cli run typecheck
    fi
  fi

  # Node check-meta script
  if have node && [ -f "scripts/check-meta.mjs" ]; then
    step "check-meta" node scripts/check-meta.mjs
  fi

  # Actionlint workflow validation
  if have actionlint && [ -d ".github/workflows" ]; then
    step "actionlint" actionlint
  fi
}

# ---------------------------------------------------------------- FULL ----
full_checks() {
  # dotnet test suite
  if have dotnet; then
    if [ -d "tests/Core.Tests" ]; then
      step "dotnet test Core.Tests" dotnet test tests/Core.Tests --verbosity normal
    elif [ -d "tests/UiContext.Core.Tests" ]; then
      step "dotnet test Core.Tests" dotnet test tests/UiContext.Core.Tests --verbosity normal
    fi
  fi

  # TypeScript CLI and MCP test suite
  if have npm && [ -f "cli/package.json" ]; then
    if grep -q '"test"' cli/package.json; then
      step "npm test (cli)" npm --prefix cli test
    fi
  fi

  # Validate goldens against schemas
  if have node && [ -f "scripts/validate-goldens.mjs" ]; then
    step "validate goldens (ajv)" node scripts/validate-goldens.mjs
  fi
}

fast_checks
if [ "$MODE" = "full" ]; then full_checks; fi

if [ -n "$FAILED" ]; then
  printf '\nverify %s: FAILED (%s)\n' "$MODE" "${FAILED#, }"
  exit 1
fi
if [ -n "$SKIPPED" ]; then
  printf '\nverify %s: INCOMPLETE. %d checks passed, NOT VERIFIED (skipped): %s\n' "$MODE" "$RAN" "${SKIPPED#, }"
  exit 0
fi
if [ "$RAN" -eq 0 ]; then
  printf '\nverify %s: NO CHECKS RAN. Nothing is configured yet; fill fast_checks/full_checks (init-project does this).\n' "$MODE"
  if [ "$MODE" = "full" ]; then exit 3; fi
  exit 0
fi
printf '\nverify %s: OK (%d checks)\n' "$MODE" "$RAN"

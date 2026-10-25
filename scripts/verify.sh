#!/usr/bin/env bash
set -euo pipefail

MODE="${1:-full}"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

echo "=== Starting unity-ui-agent-tools Verification ($MODE mode) ==="

# 1. Clean-room & secrets check
echo ""
echo "[Step 1/5] Checking clean-room and secrets baseline..."
bash "$SCRIPT_DIR/../tools/guard.sh" "$REPO_ROOT"

# 2. Meta-file integrity check
echo ""
echo "[Step 2/5] Checking Unity .meta file integrity..."
MISSING_META=0
for ROOT in "$REPO_ROOT/Packages/io.github.thesanketkale.ui-context" "$REPO_ROOT/Projects/Sample/Assets"; do
    if [ -d "$ROOT" ]; then
        while IFS= read -r -d '' ITEM; do
            case "$ITEM" in
                *.meta|*~|*~/|*/bin/*|*/obj/*|*/Documentation~/*) continue ;;
            esac
            if [ ! -f "$ITEM.meta" ]; then
                echo "Missing .meta file for: $ITEM"
                MISSING_META=$((MISSING_META + 1))
            fi
        done < <(find "$ROOT" \( -type f -o -type d \) -print0)
    fi
done

if [ "$MISSING_META" -gt 0 ]; then
    echo "FAILED: $MISSING_META missing .meta file(s) found!"
    exit 1
fi
echo "Unity .meta check PASSED."

# 3. Workflow syntax check
echo ""
echo "[Step 3/5] Validating GitHub Actions workflows..."
if [ -d "$REPO_ROOT/.github/workflows" ]; then
    for WF in "$REPO_ROOT"/.github/workflows/*.yml; do
        if [ -f "$WF" ]; then
            grep -q "name:" "$WF" || { echo "Invalid workflow: $WF"; exit 1; }
            grep -q "on:" "$WF" || { echo "Invalid workflow: $WF"; exit 1; }
        fi
    done
    echo "Workflow syntax check PASSED."
fi

if [ "$MODE" = "fast" ]; then
    echo ""
    echo "=== Verification PASSED (fast mode complete) ==="
    exit 0
fi

# 4. .NET Core engine unit tests
echo ""
echo "[Step 4/5] Running .NET Core test suite..."
TEST_CSPROJ="$REPO_ROOT/tests/UiContext.Core.Tests/UiContext.Core.Tests.csproj"
if [ -f "$TEST_CSPROJ" ]; then
    dotnet test "$TEST_CSPROJ" -c Release --verbosity normal
    echo ".NET Core test suite PASSED."
fi

# 5. TypeScript CLI and MCP test suite
echo ""
echo "[Step 5/5] Running TypeScript CLI & MCP test suite..."
CLI_DIR="$REPO_ROOT/cli"
if [ -f "$CLI_DIR/package.json" ]; then
    (
        cd "$CLI_DIR"
        if [ ! -d "node_modules" ]; then
            npm install --silent
        fi
        npm test
    )
    echo "CLI & MCP test suite PASSED."
fi

echo ""
echo "=== Verification PASSED (full mode complete) ==="
exit 0

#!/usr/bin/env bash
set -euo pipefail

# Repository integrity, clean-room, and secret guard.
TARGET_DIR="${1:-.}"

FORBIDDEN_PATTERNS=(
    "VueXR"
    "Trium Designs"
    "Fudge3d"
    "BEGIN (RSA|OPENSSH|EC|DSA)? PRIVATE KEY"
    "ghp_[A-Za-z0-9_]{36}"
    "AKIA[0-9A-Z]{16}"
)

echo "Running repository clean-room and secret guard (bash)..."

VIOLATIONS=0

for pattern in "${FORBIDDEN_PATTERNS[@]}"; do
    if grep -rInE \
        --exclude-dir=".git" \
        --exclude-dir=".pipeline" \
        --exclude-dir="node_modules" \
        --exclude-dir="bin" \
        --exclude-dir="obj" \
        --exclude-dir="Library" \
        --exclude="guard.sh" \
        --exclude="guard.ps1" \
        "$pattern" "$TARGET_DIR" 2>/dev/null; then
        echo "GUARD VIOLATION: Pattern '$pattern' detected!"
        VIOLATIONS=$((VIOLATIONS + 1))
    fi
done

if [ "$VIOLATIONS" -gt 0 ]; then
    echo "Guard check FAILED with $VIOLATIONS violation(s)."
    exit 1
fi

echo "Guard check PASSED: Clean-room and secret requirements satisfied."
exit 0

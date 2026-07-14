#!/bin/bash
# Fail if a deprecated glossary alias (docs/GLOSSARY.md) appears outside its
# sanctioned historical homes (decisions.md's historical table, GLOSSARY.md
# itself, INTENT.md, docs/adr/). See docs/GLOSSARY.md's Status legend --
# an ENFORCED row is one this script actually checks.

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
cd "$PROJECT_ROOT"

EXCLUDE_DIRS=(--exclude-dir=bin --exclude-dir=obj --exclude-dir=adr)
EXCLUDE_FILES=(--exclude=decisions.md --exclude=GLOSSARY.md --exclude=INTENT.md)

FOUND=0

check() {
    local pattern="$1"
    local label="$2"
    local matches
    # --include must precede --exclude*/--exclude-dir*: some grep builds
    # (observed: Git-for-Windows' bundled GNU grep 3.0) silently drop an
    # --exclude that appears before a later --include flag.
    matches=$(grep -rnE --include="*.md" --include="*.cs" \
        "${EXCLUDE_DIRS[@]}" "${EXCLUDE_FILES[@]}" "$pattern" docs/ src/ 2>/dev/null || true)
    if [ -n "$matches" ]; then
        echo "GLOSSARY VIOLATION ($label):"
        echo "$matches"
        echo ""
        FOUND=1
    fi
}

check 'Package 3|\bP3\b' 'deprecated alias of "Evolution Engine" -- use "Evolution Engine" or "EE-N"'
check 'Package 1 Phase N|P1-PhaseN' 'deprecated alias of "P1-N"'
check 'IChunkingService|SemanticChunkingService' 'deprecated alias of "IChunker / SemanticChunkerAdapter"'

if [ "$FOUND" -ne 0 ]; then
    echo "Glossary lint failed -- deprecated terms found above. See docs/GLOSSARY.md for canonical names."
    exit 1
fi

echo "Glossary lint passed -- no deprecated aliases found."

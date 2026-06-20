#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "$0")/../.." && pwd)"
CLI_DIR="$ROOT_DIR/src/IronShield.Cli"
TEMP_DIR=$(mktemp -d "/tmp/ironshield-test-XXXXXX")
PASSWORD="TestPassword123!"
EXIT_CODE=0

cleanup() {
    rm -rf "$TEMP_DIR"
}

trap cleanup EXIT

echo "================================================"
echo " IronShield CLI - Smoke Test Scenarios"
echo "================================================"
echo ""

# ---- Build ----
echo "=== [1/10] Building solution ==="
dotnet build "$ROOT_DIR" --nologo -q
echo "  -> OK"
echo ""

# ---- Automated tests ----
echo "=== [2/10] Running automated tests ==="
dotnet test "$ROOT_DIR/tests/IronShield.Cli.Tests" --nologo -q 2>&1 | tail -1
dotnet test "$ROOT_DIR/tests/IronShield.Storage.Tests" --nologo -q 2>&1 | tail -1
dotnet test "$ROOT_DIR/tests/IronShield.Cryptography.Tests" --nologo -q 2>&1 | tail -1
echo ""

# ---- Help ----
echo "=== [3/10] Root --help ==="
dotnet run --project "$CLI_DIR" -- --help 2>&1 | head -15
echo ""

echo "=== [4/10] protect --help ==="
dotnet run --project "$CLI_DIR" -- protect --help 2>&1
echo ""

echo "=== [5/10] unprotect --help ==="
dotnet run --project "$CLI_DIR" -- unprotect --help 2>&1
echo ""

# ---- Version ----
echo "=== [6/10] --version ==="
dotnet run --project "$CLI_DIR" -- --version 2>&1
echo ""

# ---- Protect file ----
echo "=== [7/10] Protect a file ==="
echo "My secret content" > "$TEMP_DIR/test.txt"
dotnet run --project "$CLI_DIR" -- protect "$TEMP_DIR/test.txt" \
    -p "$PASSWORD" \
    -o "$TEMP_DIR/test.txt.iron" \
    --overwrite 2>&1 | grep -E "SUCCESS|ERROR"
if [ -f "$TEMP_DIR/test.txt.iron" ]; then
    echo "  -> File created: $(wc -c < "$TEMP_DIR/test.txt.iron") bytes"
else
    echo "  !! FAIL: .iron file not created"
    EXIT_CODE=1
fi
echo ""

# ---- Unprotect file ----
echo "=== [8/10] Unprotect a file ==="
dotnet run --project "$CLI_DIR" -- unprotect "$TEMP_DIR/test.txt.iron" \
    -p "$PASSWORD" \
    -o "$TEMP_DIR/restored.txt" \
    --overwrite 2>&1 | grep -E "SUCCESS|ERROR"
if diff "$TEMP_DIR/test.txt" "$TEMP_DIR/restored.txt" > /dev/null 2>&1; then
    echo "  -> Content matches original"
else
    echo "  !! FAIL: restored content differs"
    EXIT_CODE=1
fi
echo ""

# ---- Directory protect/unprotect ----
echo "=== [9/10] Protect and restore a directory ==="
mkdir -p "$TEMP_DIR/mydocs"
echo "alpha" > "$TEMP_DIR/mydocs/alpha.txt"
echo "beta"  > "$TEMP_DIR/mydocs/beta.txt"
dotnet run --project "$CLI_DIR" -- protect "$TEMP_DIR/mydocs" \
    -p "$PASSWORD" \
    -o "$TEMP_DIR/mydocs.iron" \
    --overwrite 2>&1 | grep -E "SUCCESS|ERROR"
dotnet run --project "$CLI_DIR" -- unprotect "$TEMP_DIR/mydocs.iron" \
    -p "$PASSWORD" \
    -o "$TEMP_DIR/restored-dir.zip" \
    --overwrite 2>&1 | grep -E "SUCCESS|ERROR"

if [ -f "$TEMP_DIR/restored-dir.zip" ]; then
    unzip -o "$TEMP_DIR/restored-dir.zip" -d "$TEMP_DIR/extracted" > /dev/null 2>&1
    if diff "$TEMP_DIR/mydocs/alpha.txt" "$TEMP_DIR/extracted/alpha.txt" > /dev/null 2>&1 \
       && diff "$TEMP_DIR/mydocs/beta.txt" "$TEMP_DIR/extracted/beta.txt" > /dev/null 2>&1; then
        echo "  -> Directory content matches original"
    else
        echo "  !! FAIL: extracted directory content differs"
        EXIT_CODE=1
    fi
else
    echo "  !! FAIL: restored zip not found"
    EXIT_CODE=1
fi
echo ""

# ---- Error scenarios ----
echo "=== [10/10] Error handling ==="
# Wrong password
echo "wrong content" > "$TEMP_DIR/dummy.iron"
dotnet run --project "$CLI_DIR" -- unprotect "$TEMP_DIR/test.txt.iron" \
    -p "WrongPassword" \
    -o "$TEMP_DIR/fail.txt" 2>&1 | grep -E "SUCCESS|ERROR"

# Non-existent path
dotnet run --project "$CLI_DIR" -- protect "/nonexistent/path.txt" \
    -p "$PASSWORD" 2>&1 | grep -E "SUCCESS|ERROR"

echo ""
echo "================================================"
if [ $EXIT_CODE -eq 0 ]; then
    echo " All scenarios passed! "
else
    echo " Some scenarios FAILED (see above) "
fi
echo "================================================"
exit $EXIT_CODE

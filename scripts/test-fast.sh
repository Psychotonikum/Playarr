#!/bin/bash
set -e

# Fast parallel unit test runner for Playarr
# Builds once, then runs all unit test projects in parallel

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"
SRC_DIR="$ROOT_DIR/src"
SETTINGS="$SRC_DIR/unit.runsettings"

# Unit test projects only (skip Integration, Automation, Windows, Mono)
UNIT_PROJECTS=(
  "Playarr.Api.Test"
  "Playarr.Common.Test"
  "Playarr.Core.Test"
  "Playarr.Host.Test"
  "Playarr.Libraries.Test"
  "Playarr.Update.Test"
)

echo "=== Building all test projects ==="
dotnet build "$SRC_DIR/Playarr.sln" -c Debug --no-incremental -v q 2>&1 | tail -5

echo ""
echo "=== Running ${#UNIT_PROJECTS[@]} test projects in parallel ==="

PIDS=()
RESULTS_DIR="$ROOT_DIR/_temp/test-results"
rm -rf "$RESULTS_DIR"
mkdir -p "$RESULTS_DIR"

for proj in "${UNIT_PROJECTS[@]}"; do
  PROJ_PATH="$SRC_DIR/$proj/$proj.csproj"
  if [ -f "$PROJ_PATH" ]; then
    echo "  Starting: $proj"
    dotnet test "$PROJ_PATH" \
      --no-build \
      --settings "$SETTINGS" \
      --filter "Category!=IntegrationTest&Category!=AutomationTest&Category!=ManualTest&Category!=WINDOWS" \
      --results-directory "$RESULTS_DIR/$proj" \
      -v q \
      2>&1 | sed "s/^/  [$proj] /" > "$RESULTS_DIR/$proj.log" 2>&1 &
    PIDS+=("$!:$proj")
  fi
done

echo ""
echo "=== Waiting for results ==="

FAILED=0
TOTAL=0
for entry in "${PIDS[@]}"; do
  PID="${entry%%:*}"
  NAME="${entry##*:}"
  TOTAL=$((TOTAL + 1))
  if wait "$PID"; then
    echo "  PASS: $NAME"
  else
    echo "  FAIL: $NAME"
    cat "$RESULTS_DIR/$NAME.log" | tail -20
    FAILED=$((FAILED + 1))
  fi
done

echo ""
echo "=== Summary: $((TOTAL - FAILED))/$TOTAL projects passed ==="

if [ "$FAILED" -gt 0 ]; then
  echo ""
  echo "Failed project logs in: $RESULTS_DIR/"
  exit 1
fi

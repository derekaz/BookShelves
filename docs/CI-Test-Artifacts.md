# CI Test Artifacts Guide

This document explains how to use CI test artifacts produced by the validation workflow.

## What Gets Published

The validation workflow uploads two artifact types:

- **TRX files** (`*.trx`) - test run results (pass/fail, duration, stack traces, standard output).
- **Coverage files** (`coverage.cobertura.xml`) - code coverage data in Cobertura format.

## Where to Find Them

1. Open a workflow run in GitHub Actions.
2. Scroll to **Artifacts**.
3. Download:
   - `BookShelves-validation-test-results-<run-id>`
   - `BookShelves-validation-coverage-<run-id>`

For flaky-monitor runs, download `BookShelves-flaky-monitor-...` artifacts.

## Practical Troubleshooting Workflow

1. Start with the TRX file from the failed run.
2. Identify failed tests and capture:
   - failing test names
   - exception messages
   - stack traces
   - timing/duration signals
3. Re-run the failing test(s) locally with the same target project.
4. Compare against prior successful artifacts if the failure looks intermittent.

## How to Read TRX Quickly

Inside a TRX file, look for:

- `ResultSummary` - overall totals (passed/failed/skipped).
- `UnitTestResult` with `outcome="Failed"` - individual failures.
- `Output/ErrorInfo` - detailed exception text.

## Open TRX in Visual Studio (Windows)

1. Download and extract the artifact that contains `.trx` files.
2. In Visual Studio, open **Test Explorer**.
3. Use **Open Test Results** (or open the `.trx` file directly from File Explorer if associated with Visual Studio).
4. Inspect failed tests, stack traces, and output in the test results view.

If needed, you can still open `.trx` as raw XML in any text editor.

## Using Coverage Artifacts

Use `coverage.cobertura.xml` to:

- identify under-tested files in changed areas,
- validate that new tests increased coverage where expected,
- prioritize future test work by risk and low coverage.

Coverage artifacts are informational unless explicit thresholds are configured.

## Generate Local HTML Coverage Report from Cobertura

Example workflow on Windows PowerShell:

```powershell
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"TestResults/**/coverage.cobertura.xml" -targetdir:"TestResults/CoverageReport" -reporttypes:"HtmlInline_AzurePipelines;Cobertura"
start TestResults/CoverageReport/index.html
```

This produces a browsable HTML report and a merged Cobertura output.

## Local Reproduction Command Pattern

Use the same structure as CI when reproducing:

```powershell
dotnet test <path-to-test-project.csproj> --configuration Debug --collect:"XPlat Code Coverage" --logger "trx;LogFileName=<name>.trx" --results-directory TestResults
```

## Hygiene Recommendations

- Keep TRX and coverage artifacts for failed runs until root cause is resolved.
- Link important failing artifacts in PR comments/issues.
- Prefer fixing flaky tests over rerunning pipelines repeatedly.

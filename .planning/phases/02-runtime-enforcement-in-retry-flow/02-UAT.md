---
status: testing
phase: 02-runtime-enforcement-in-retry-flow
source: 02-01-SUMMARY.md, 02-02-SUMMARY.md, 02-03-SUMMARY.md
started: 2026-06-14T15:53:00+03:00
updated: 2026-06-14T15:53:00+03:00
---

## Current Test

number: 1
name: Stopwatch starts at beginning of sync retry execution
expected: |
  When RetryInternal is called, a Stopwatch starts before the do-while loop.
  The elapsed time accumulated across the retry attempts is used for time limit checks.
awaiting: user response

## Tests

### 1. Stopwatch starts at beginning of sync retry execution
expected: Stopwatch starts via Stopwatch.StartNew() before do-while loop in all sync RetryInternal methods
result: [pending]

### 2. Elapsed time check halts retry loop when budget exceeded
expected: When elapsed time >= ErrorProcessingTimeLimit at start of loop iteration, SetFailed() is called and loop exits
result: [pending]

### 3. Time limit check before error processor invocation
expected: HandleException checks elapsed time before calling SaveError/SaveErrorAsync or _saveErrorProcessor.Process
result: [pending]

### 4. Async retry follows same time limit pattern
expected: All 4 async RetryInternalAsync variants have Stopwatch.StartNew(), time check, and stopwatch.Stop()
result: [pending]

### 5. SetFailed called without exception or cancellation
expected: When time limit exceeded, result.SetFailed() is called (not SetFailedAndCanceled), IsCanceled remains false
result: [pending]

### 6. Null time limit skips all checks
expected: When ErrorProcessingTimeLimit is null, time checks return false and existing retry behavior is unchanged
result: [pending]

### 7. Sync retry test with actual timeout
expected: With 50ms limit and slow delegate, retry stops before 3 attempts complete
result: [pending]

### 8. Async retry test with actual timeout
expected: With 50ms limit and slow async delegate, retry stops before 3 attempts complete
result: [pending]

## Summary

total: 8
passed: 0
issues: 0
pending: 8
skipped: 0
blocked: 0

## Gaps

- [none yet]

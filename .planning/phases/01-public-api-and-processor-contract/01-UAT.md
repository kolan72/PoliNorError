---
status: testing
phase: 01-public-api-and-processor-contract
source: 01-01-SUMMARY.md, 01-02-SUMMARY.md
started: 2026-06-14T13:07:16+03:00
updated: 2026-06-14T13:07:16+03:00
---

## Current Test

number: 1
name: ErrorProcessingTimeLimit defaults to null on DefaultRetryProcessor
expected: |
  When creating a new DefaultRetryProcessor without specifying time limit,
  the ErrorProcessingTimeLimit property returns null.
awaiting: user response

## Tests

### 1. ErrorProcessingTimeLimit defaults to null on DefaultRetryProcessor
expected: ErrorProcessingTimeLimit property returns null when DefaultRetryProcessor created without time limit parameter
result: [pending]

### 2. ErrorProcessingTimeLimit stores supplied budget on DefaultRetryProcessor
expected: When DefaultRetryProcessor is created with a TimeSpan, ErrorProcessingTimeLimit returns that value
result: [pending]

### 3. ErrorProcessingTimeLimit stores null when explicitly null on DefaultRetryProcessor
expected: When DefaultRetryProcessor is created with explicit null, ErrorProcessingTimeLimit returns null
result: [pending]

### 4. ErrorProcessingTimeLimit stores supplied budget with bulk processor on DefaultRetryProcessor
expected: When DefaultRetryProcessor is created with bulk processor and time limit, ErrorProcessingTimeLimit returns the time limit
result: [pending]

### 5. ErrorProcessingTimeLimit defaults to null on RetryPolicy
expected: When creating a new RetryPolicy without calling WithErrorProcessingTimeLimit, ErrorProcessingTimeLimit returns null
result: [pending]

### 6. WithErrorProcessingTimeLimit is fluent and retains limit on RetryPolicy
expected: Calling WithErrorProcessingTimeLimit(TimeSpan.FromSeconds(30)) returns RetryPolicy and ErrorProcessingTimeLimit returns 30 seconds
result: [pending]

### 7. WithErrorProcessingTimeLimit propagates to DefaultRetryProcessor on RetryPolicy
expected: After calling WithErrorProcessingTimeLimit, the Internal RetryProcessor (DefaultRetryProcessor) has matching ErrorProcessingTimeLimit
result: [pending]

### 8. WithErrorProcessingTimeLimit does not throw for custom retry processor on RetryPolicy
expected: Calling WithErrorProcessingTimeLimit on RetryPolicy with non-DefaultRetryProcessor does not throw exception
result: [pending]

### 9. WithNoErrorProcessingTimeLimit leaves processor limit null on RetryPolicy
expected: RetryPolicy without WithErrorProcessingTimeLimit has null ErrorProcessingTimeLimit on its processor
result: [pending]

## Summary

total: 9
passed: 0
issues: 0
pending: 9
skipped: 0
blocked: 0

## Gaps

- [none yet]

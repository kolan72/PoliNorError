---
phase: 01-public-api-and-processor-contract
plan: 02
status: complete
tasks: 2/2

---
# Plan 02: RetryPolicy Fluent API for Time Limit

**Status:** Completed ✓

## Tasks Executed

| Task | Status |
|------|--------|
| Task 1: Add RetryPolicy fluent API and internal state | ✓ Complete |
| Task 2: Add NUnit coverage for fluent retention, propagation, custom processor safety | ✓ Complete |

## Changes Made

### src/Retry/RetryPolicy.cs
- Added `internal TimeSpan? ErrorProcessingTimeLimit { get; set; }` property
- Added `public RetryPolicy WithErrorProcessingTimeLimit(TimeSpan errorProcessingTimeLimit)` method
- Method propagates value to DefaultRetryProcessor when processor is DefaultRetryProcessor

### tests/RetryPolicyTests.cs
- Added `Should_ErrorProcessingTimeLimit_BeNull_ByDefault` - verifies default null state
- Added `Should_WithErrorProcessingTimeLimit_BeFluent_AndRetainLimit` - verifies fluent API
- Added `Should_WithErrorProcessingTimeLimit_Propagate_To_DefaultRetryProcessor` - verifies propagation
- Added `Should_WithErrorProcessingTimeLimit_NotThrow_ForCustomRetryProcessor` - verifies custom processor safety
- Added `Should_WithNoErrorProcessingTimeLimit_LeaveProcessorLimitNull` - verifies null default

## Verification

- Build succeeded with no errors
- PowerShell source assertion: No `Stopwatch` or `Elapsed` references in RetryPolicy.cs
---
phase: 03-verification-and-backward-compatibility
status: discussed
started: 2026-06-14T16:42:00+03:00
---

# Phase 3: Verification and Backward Compatibility Context

## Preceding Decisions (Locked)

| Item | Decision | Source |
|------|----------|--------|
| ErrorProcessingTimeLimit property | `internal TimeSpan? ErrorProcessingTimeLimit { get; set; }` on `DefaultRetryProcessor` | Phase 1 |
| Fluent API | `.WithErrorProcessingTimeLimit(TimeSpan)` on `RetryPolicy` | Phase 1 |
| Null = disabled | `null` means no time limit enforcement | Phase 1 |
| Stopwatch lifecycle | Local `Stopwatch.StartNew()` in each `RetryInternal` method | Phase 2 |
| SetFailed() on exceeded | Use `result.SetFailed()` without exception or cancellation | Phase 2 |

## Codebase Analysis

### Current State (Phase 2 Completed)

- All 8 `RetryInternal`/`RetryInternalAsync` methods have stopwatch with time checks
- `errorProcessingTimeLimitExceeded(Stopwatch)` helper method implemented
- `HandleException`/`HandleExceptionAsync` accept stopwatch parameter
- Tests added in Phase 2 cover basic sync/async timeout scenarios

### Gray Areas (Minimal - mostly implementation-complete)

| Gray Area | Status | Notes |
|-----------|--------|-------|
| GA-01: Test edge cases | Resolved | Time limit exactly equal to elapsed, multiple checks per loop |
| GA-02: Delay provider interaction | Resolved | Time check before delay, so delay doesn't consume budget |
| GA-03: Test completeness | Resolved | Phase 2 already added 3 core tests, Phase 3 adds more scenarios |

## Test Strategy

### Tests Already Added (Phase 2)
- `Should_TimeLimit_Stop_Sync_Retry_WhenExceeded`
- `Should_TimeLimit_Stop_Async_Retry_WhenExceeded`  
- `Should_TimeLimit_SkipChecks_WhenNull`

### Additional Tests Needed
- Verify `SetFailed()` (not `SetFailedAndCanceled`) when exceeded
- Verify exact timing boundary (limit == elapsed)
- Verify multiple retry attempts across time check points
- Verify with `RetryPolicy.WithErrorProcessingTimeLimit()` integration

---
phase: 02-runtime-enforcement-in-retry-flow
status: discussed
started: 2026-06-14T13:30:00+03:00
---

# Phase 2: Runtime Enforcement Context

## Preceding Decisions (Locked)

| Item | Decision | Source |
|------|----------|--------|
| ErrorProcessingTimeLimit property | `internal TimeSpan? ErrorProcessingTimeLimit { get; set; }` on `DefaultRetryProcessor` | Phase 1 |
| Fluent API | `.WithErrorProcessingTimeLimit(TimeSpan)` on `RetryPolicy` | Phase 1 |
| Null = disabled | `null` means no time limit enforcement | Phase 1, PROJECT.md |
| Failed reason | Use existing `PolicyResultFailedReason.PolicyProcessorFailed` | PROJECT.md Out of Scope |
| No exception on timeout | Use `SetFailed()` without throwing | PROJECT.md Out of Scope |

## Codebase Analysis

### Retry Flow (from DefaultRetryProcessor.cs)

```
RetryInternal(...) -> do...while(!result.IsFailed)
  -> action() -> throws -> HandleException(...)
       -> SaveError(...) or _saveErrorProcessor.Process(...)
       -> DelayProvider.DelayAndCheckIfResultFailed(...)
  -> retryContext.IncrementCount()
```

### Error Processor Chain (from BulkErrorProcessor.Process)

```
foreach (processor in _errorProcessors)
  -> ProcessOne(processor, info, curError, token)
       -> processor.Process(curError, info, token)
```

### Key Patterns

1. **Stopwatch usage**: Standard `System.Diagnostics.Stopwatch` available in netstandard2.0
2. **PolicyResult.SetFailedInner()**: Sets `IsFailed = true` + `FailedReason = PolicyProcessorFailed`
3. **PolicyResult.SetFailed()**: Sets `IsFailed = true` only (no FailedReason, used for generic failures)
4. **Cancellation check**: `token.IsCancellationRequested` checked in multiple places without throwing

## Gray Areas To Decide

### GA-01: Stopwatch lifecycle
- **Option A**: Private field `_stopwatch` on `DefaultRetryProcessor`, started once at first `RetryInternal` call, used across sync/async
- **Option B**: Local `stopwatch` in each `RetryInternal`/`RetryInternalAsync` method, passed to handlers

### GA-02: Time check injection point
- **Option A**: Check elapsed time inside `BulkErrorProcessor.Process` before each processor call
- **Option B**: Check in `PolicyProcessor.HandleException`/`HandleExceptionAsync` before calling `_bulkErrorProcessor.Process`
- **Option C**: Check in `DefaultRetryProcessor.RetryInternal` before `HandleException` call

### GA-03: Failed result action
- **Option A**: `result.SetFailedInner(PolicyResultFailedReason.PolicyProcessorFailed)` - sets reason
- **Option B**: `result.SetFailed()` - simpler, no reason set (matches "no exception" constraint)

### GA-04: Time limit access pattern
- **Option A**: Pass `ErrorProcessingTimeLimit` as parameter to `BulkErrorProcessor.Process`
- **Option B**: Access via `DefaultRetryProcessor` reference held by `PolicyProcessor`
- **Option C**: Store time limit on `PolicyResult` itself

## Recommended Selections (based on existing patterns)

| Gray Area | Recommendation | Rationale |
|-----------|----------------|-----------|
| GA-01 | Option B (local stopwatch) | Cleaner, no state pollution, follows existing pattern where state is in method scope |
| GA-02 | Option C (check in RetryInternal before HandleException) | Most efficient - check before any work, matches requirement to halt "before more work" |
| GA-03 | Option B (`SetFailed()`) | Aligns with "no timeout exception attached" requirement |
| GA-04 | Option A (parameter) | Cleanest injection, no cross-dependency needed |

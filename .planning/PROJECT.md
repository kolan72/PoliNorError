# PoliNorError

## What This Is

PoliNorError is a .NET Standard 2.0 error handling library that provides Retry, Fallback, Simple, and TryCatch policies. Inspired by Polly, it focuses on handling exceptions within the catch block with extensible error processors, flexible filters, and policy composition. The library has zero runtime dependencies and targets broad .NET compatibility (.NET Framework 4.6.1+, .NET Core 2.0+, .NET 5+).

## Core Value

Reliable, composable error handling policies that let developers control exactly what happens when exceptions occur — with extensible error processing chains and fine-grained retry behavior.

## Requirements

### Validated
- ✓ Retry policy with configurable retry count, delay types (constant, linear, exponential, time series), and jitter — existing
- ✓ Fallback policy for graceful degradation — existing
- ✓ Simple policy for basic error handling — existing
- ✓ TryCatch policy for explicit try/catch semantics — existing
- ✓ Error processor chain (BulkErrorProcessor) with sequential processing — existing
- ✓ Exception filtering (include/exclude predicates) — existing
- ✓ Policy composition via PolicyDelegateCollection and PolicyCollection — existing
- ✓ Policy wrapping (Retry-Then-Fallback, etc.) — existing
- ✓ PolicyResult with detailed failure/success/cancellation tracking — existing
- ✓ PolicyResult handlers for post-processing — existing
- ✓ Typed error processors (DefaultErrorProcessor<T>, DefaultInnerErrorProcessor) — existing
- ✓ Parameterized delegate support (Action<TParam>, Func<TParam, T>, etc.) — existing
- ✓ Pipeline composition via PipelineFuncBuilder — existing
- ✓ CatchBlockHandlers for sync/async handler creation — existing
- ✓ Error processing time limit for RetryPolicy — implemented in v1 milestone

### Active

- [ ] Error processing time limit for RetryPolicy — configurable total time budget (Stopwatch-based) that halts error processing and marks PolicyResult as failed when exceeded
- [ ] Fluent API on RetryPolicy for time limit configuration (.WithErrorProcessingTimeLimit)
- [ ] Time limit propagation from RetryPolicy to DefaultRetryProcessor
- [ ] Time limit enforcement before each error processor call in BulkErrorProcessor
- [ ] Time limit enforcement before each retry cycle in the retry loop
- [ ] Both sync and async path support for time limit

### Out of Scope
- Per-attempt time limits — only total budget across all retry attempts was implemented
- New PolicyResultFailedReason enum value — used existing PolicyProcessorFailed
- Exception-based halt behavior — implemented SetFailed() without exception
- CancellationToken-based halt for time limit — separate mechanism from cancellation

## Context

- **Existing codebase**: Mature library at v2.24.20 with comprehensive test coverage (100+ test files)
- **Target framework**: .NET Standard 2.0 — no System.Diagnostics.Stopwatch restrictions, fully available
- **Test framework**: NUnit 4.5.1 with NSubstitute 5.3.0 for mocking
- **Architecture**: Policy → PolicyProcessor → BulkErrorProcessor → IErrorProcessor chain
- **Retry flow**: `RetryInternal` → `do...while` loop → `try/catch` → `HandleException` → `BulkErrorProcessor.Process` → `DelayProvider.DelayAndCheckIfResultFailed`
- **Key files**: `src/Retry/DefaultRetryProcessor.cs` (retry loop), `src/ErrorProcessors/BulkErrorProcessor.cs` (processor chain), `src/Retry/RetryPolicy.cs` (public API)

## Constraints

- **Target Framework**: .NET Standard 2.0 — must use APIs available in netstandard2.0
- **Zero dependencies**: No new runtime NuGet dependencies — library is self-contained
- **Backward compatibility**: All existing public APIs must remain unchanged
- **Test runtime**: .NET Framework 4.7.2 for tests

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| Stopwatch-based time budget | System.Diagnostics.Stopwatch is available in netstandard2.0 and provides high-resolution timing | ✓ Implemented in v1 |
| Total budget (not per-attempt) | User wants to limit total error processing time across all retries | ✓ Implemented in v1 |
| SetFailed() without exception | User explicitly requested no exception on timeout | ✓ Implemented in v1 |
| Existing PolicyProcessorFailed reason | No need to distinguish timeout from other processor failures | ✓ Implemented in v1 |
| Fluent API on RetryPolicy | Consistent with existing WithWait, WithErrorProcessor patterns | ✓ Implemented in v1 |
| TimeSpan? nullable storage | Consistent with existing optional parameter patterns | ✓ Implemented in v1 |
| Property on RetryPolicy (not constructor) | Matches Delay property pattern for optional features | ✓ Implemented in v1 |

## Evolution

This document evolves at phase transitions and milestone boundaries.

**After each phase transition** (via `/gsd-transition`):
1. Requirements invalidated? → Move to Out of Scope with reason
2. Requirements validated? → Move to Validated with phase reference
3. New requirements emerged? → Add to Active
4. Decisions to log? → Add to Key Decisions
5. "What This Is" still accurate? → Update if drifted

**After each milestone** (via `/gsd-complete-milestone`):
1. Full review of all sections
2. Core Value check — still the right priority?
3. Audit Out of Scope — reasons still valid?
4. Update Context with current state

---
*Last updated: 2026-06-13 after initialization*

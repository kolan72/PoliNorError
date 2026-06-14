# Phase 1: Public API and Processor Contract - Context

**Gathered:** 2026-06-14
**Status:** Ready for planning

## Phase Boundary

Phase 1 delivers: A .NET consumer can configure a RetryPolicy error-processing time budget through the existing fluent API, and that value reaches the retry processor while preserving existing defaults. The time limit is represented as `TimeSpan?` with null=disabled, stored as an internal property on RetryPolicy, and accessible to DefaultRetryProcessor for enforcement in Phase 2.

## Implementation Decisions

### Fluent API Design
- **D-01:** `.WithErrorProcessingTimeLimit(TimeSpan)` added directly to `RetryPolicy` class, following the existing `.WithWait()` pattern for fluent configuration methods.
- **D-02:** The method receives `TimeSpan value` (not nullable) since null can be passed explicitly if needed; property internally stores `TimeSpan?`.

### Time Limit Representation
- **D-03:** Use `TimeSpan?` with null representing "disabled" (no time limit enforced), consistent with existing optional parameter patterns like `IDelayProvider` being nullable.
- **D-04:** When null, all time-check code in Phase 2 skips execution entirely for backward compatibility.

### Internal State Storage
- **D-05:** Store as internal property `ErrorProcessingTimeLimit` on `RetryPolicy` (similar to the `Delay` property pattern shown in ARCHITECTURE.md).
- **D-06:** This allows processor access via `((DefaultRetryProcessor)RetryProcessor)` cast when needed in retry flow.

### Propagation to Processor
- **D-07:** Set property after construction in RetryPolicy constructor body, then processor accesses via cast during execution.
- **D-08:** RetryPolicy stores the value and makes it available to DefaultRetryProcessor when casting during `Handle`/`HandleAsync` methods.

## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Architecture & Flow
- `.planning/codebase/ARCHITECTURE.md` — Policy → PolicyProcessor → BulkErrorProcessor flow, retry loop in RetryInternal
- `.planning/codebase/CONVENTIONS.md` — Fluent API patterns (`.WithWait()`), CRTP interfaces, constructor overloads
- `src/Retry/RetryPolicy.cs` — Public API surface, constructor overloads, Delay property pattern (line 577)
- `src/Retry/DefaultRetryProcessor.cs` — RetryInternal/RetryInternalAsync methods, SaveError pattern (lines 32-103)

### Processor Infrastructure
- `src/ErrorProcessors/BulkErrorProcessor.cs` — Sequential processor chain, Process/ProcessAsync methods (lines 40-75)
- `src/PolicyProcessor.cs` — HandleException chain, ExceptionFilter → PolicyRule → BulkErrorProcessor
- `src/PolicyResult.cs` — SetFailed() method for failure marking without exceptions

### Requirements Traceability
- `.planning/REQUIREMENTS.md` — RETRY-EP-01, RETRY-EP-06, RETRY-EP-07, RETRY-EP-08 for Phase 1

## Existing Code Insights

### Reusable Assets
- `RetryPolicy.Delay` property pattern (internal property storing optional configuration)
- Constructor overload chain in RetryPolicy (multiple overloads delegating to each other)
- `DefaultRetryProcessor` internal constructors accepting `IDelayProvider` and `IBulkErrorProcessor`

### Established Patterns
- Fluent builder: All `.With*` methods return `this` for chaining
- Optional configuration via nullable types: `IDelayProvider` nullable, optional parameters
- Processor access via cast: `((DefaultRetryProcessor)RetryProcessor)` used in Handle methods
- Internal properties for optional features: `Delay` stored as internal property

### Integration Points
- RetryPolicy constructor chain → DefaultRetryProcessor instantiation
- RetryInternal/RetryInternalAsync → HandleException → BulkErrorProcessor.Process
- PolicyResult.SetFailed() for failure without exception attachment

## Specific Ideas

- Follow the exact pattern of `.WithWait(TimeSpan)` → internal property storage → processor access via cast
- Use `ProcessingErrorInfo` or a new context type to pass timing state through the processor chain
- Stopwatch instance will be created and started at the beginning of `RetryInternal`/`RetryInternalAsync`

## Deferred Ideas

None — discussion stayed within phase scope

---
*Phase: 1-Public API and Processor Contract*
*Context gathered: 2026-06-14*
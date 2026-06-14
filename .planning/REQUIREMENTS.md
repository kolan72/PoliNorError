# Requirements: PoliNorError — Error Processing Time Limit

**Defined:** 2026-06-14
**Core Value:** Reliable, composable error handling policies that let developers control exactly what happens when exceptions occur

## v1 Requirements

### Error Processing Time Limit

- [ ] **RETRY-EP-01**: DefaultRetryProcessor accepts an optional TimeSpan ErrorProcessingTimeLimit that defines a total time budget for the entire retry operation
- [ ] **RETRY-EP-02**: A Stopwatch is started when RetryInternal/RetryInternalAsync begins execution
- [ ] **RETRY-EP-03**: Before each error processor is called in BulkErrorProcessor, the elapsed time is checked — if it exceeds the limit, execution halts
- [ ] **RETRY-EP-04**: Before each retry cycle (next iteration of the do...while loop), the elapsed time is checked — if it exceeds the limit, execution halts
- [ ] **RETRY-EP-05**: When the time limit is exceeded, PolicyResult.SetFailed() is called (no exception attached, no cancellation triggered)
- [ ] **RETRY-EP-06**: If ErrorProcessingTimeLimit is null, the time limit check is skipped entirely (backward-compatible default)
- [ ] **RETRY-EP-07**: RetryPolicy exposes fluent API .WithErrorProcessingTimeLimit(TimeSpan) that propagates the limit to DefaultRetryProcessor
- [ ] **RETRY-EP-08**: DefaultRetryProcessor accepts ErrorProcessingTimeLimit through constructor overload
- [ ] **RETRY-EP-09**: Both sync and async retry paths enforce the time limit
- [ ] **RETRY-EP-10**: Unit tests verify time limit enforcement in sync retry path
- [ ] **RETRY-EP-11**: Unit tests verify time limit enforcement in async retry path
- [ ] **RETRY-EP-12**: Unit tests verify backward compatibility when ErrorProcessingTimeLimit is null

## v2 Requirements

### Error Processing Time Limit Enhancements

- **RETRY-EP-V2-01**: Per-attempt time limit option (separate from total budget)
- **RETRY-EP-V2-02**: Optional callback delegate (e.g., OnTimeLimitExceeded) for custom handling when the limit is hit
- **RETRY-EP-V2-03**: Optional delay override from within the catch block (e.g., Polly-style RetryAfter)
- **RETRY-EP-V2-04**: Configurable halt behavior (fail vs cancel vs custom PolicyResultFailedReason)

## Out of Scope

| Feature | Reason |
|---------|--------|
| Per-attempt time limits | Only total budget across all retries specified |
| New PolicyResultFailedReason enum value | Use existing PolicyProcessorFailed |
| Exception-based halt behavior | User explicitly requested no exception on timeout |
| CancellationToken-based halt for time limit | Separate mechanism from cancellation |
| RetryAfter-style delay override | Original idea evolved into time budget concept |

## Traceability

| Requirement | Phase | Status |
|-------------|-------|--------|
| RETRY-EP-01 | Phase 1: Public API and Processor Contract | Pending |
| RETRY-EP-02 | Phase 2: Runtime Enforcement in Retry Flow | Pending |
| RETRY-EP-03 | Phase 2: Runtime Enforcement in Retry Flow | Pending |
| RETRY-EP-04 | Phase 2: Runtime Enforcement in Retry Flow | Pending |
| RETRY-EP-05 | Phase 2: Runtime Enforcement in Retry Flow | Pending |
| RETRY-EP-06 | Phase 1: Public API and Processor Contract | Pending |
| RETRY-EP-07 | Phase 1: Public API and Processor Contract | Pending |
| RETRY-EP-08 | Phase 1: Public API and Processor Contract | Pending |
| RETRY-EP-09 | Phase 2: Runtime Enforcement in Retry Flow | Pending |
| RETRY-EP-10 | Phase 3: Verification and Backward Compatibility | Pending |
| RETRY-EP-11 | Phase 3: Verification and Backward Compatibility | Pending |
| RETRY-EP-12 | Phase 3: Verification and Backward Compatibility | Pending |

**Coverage:**
- v1 requirements: 12 total
- Mapped to phases: 12
- Unmapped: 0 ✓

---
*Requirements defined: 2026-06-14*
*Last updated: 2026-06-14 after initial definition*

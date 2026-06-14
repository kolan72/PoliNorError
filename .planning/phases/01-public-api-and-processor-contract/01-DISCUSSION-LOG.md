# Phase 1: Public API and Processor Contract - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-06-14
**Phase:** 1-Public API and Processor Contract
**Areas discussed:** Fluent API surface, Time limit representation, Internal state storage, Propagation to processor

---

## Fluent API Surface

| Option | Description | Selected |
|--------|-------------|----------|
| RetryPolicy class | Consistent with existing .WithWait() pattern - add to RetryPolicy class | ✓ |
| IRetryPolicy interface | Requires adding method to IRetryPolicy interface - broader change | |
| Extension method | Extension method approach - keeps core class unchanged | |

**User's choice:** RetryPolicy class
**Notes:** Follows existing fluent patterns in codebase, minimal surface change

---

## Time Limit Representation

| Option | Description | Selected |
|--------|-------------|----------|
| Nullable TimeSpan | null means disabled - consistent with IDelayProvider=null and optional parameters | ✓ |
| Zero sentinel | TimeSpan.Zero (00:00:00) means disabled - explicit sentinel value | |
| Boolean flag | Separate boolean HasTimeLimit property alongside TimeSpan | |

**User's choice:** Nullable TimeSpan
**Notes:** Consistent with existing optional parameter patterns in RetryPolicy

---

## Internal State Storage

| Option | Description | Selected |
|--------|-------------|----------|
| Private field in processor | New private field _errorProcessingTimeLimit in DefaultRetryProcessor, null-skipped behavior | |
| Pass to BulkErrorProcessor | Pass to BulkErrorProcessor so it can check before each processor call internally | |
| Property on RetryPolicy | Property on RetryPolicy like Delay property, read by processor at execution time | ✓ |

**User's choice:** Property on RetryPolicy
**Notes:** Matches the Delay property pattern; processor accesses via cast

---

## Propagation to Processor

| Option | Description | Selected |
|--------|-------------|----------|
| Constructor parameter | Add parameter to existing internal constructors, propagate alongside delay provider and bulk error processor | |
| Property assignment | Set property after default construction (like how Delay is assigned in constructor body) | ✓ |
| Per-execution pass | Pass through to processor on each Handle/HandleAsync call | |

**User's choice:** Property assignment
**Notes:** Similar to how Delay property is assigned in constructor body at line 27 of RetryPolicy.cs

---

## Deferred Ideas

None
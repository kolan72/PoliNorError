<!-- refreshed: 2026-06-13 -->
# Architecture

**Analysis Date:** 2026-06-13

## System Overview

```text
┌──────────────────────────────────────────────────────────────────────────────┐
│                           Public API Surface                                  │
│  Policy Types: RetryPolicy, FallbackPolicy, SimplePolicy, TryCatch           │
│  Composition: PolicyCollection, PolicyDelegateCollection                      │
│  Pipeline: PipelineFuncBuilder                                               │
│  Convenience: DelegateInvoking extension methods                              │
├───────────────────┬──────────────────┬───────────────────────────────────────┤
│   RetryPolicy     │  FallbackPolicy  │  SimplePolicy / TryCatch             │
│  `src/Retry/`     │  `src/Fallback/` │  `src/Simple/` / `src/TryCatch/`     │
└────────┬──────────┴────────┬─────────┴──────────────────┬────────────────────┘
         │                   │                              │
         ▼                   ▼                              ▼
┌──────────────────────────────────────────────────────────────────────────────┐
│                     Policy Layer (abstract Policy)                            │
│  `src/Policy.cs` — base class with Wrap, Handler collection, PolicyProcessor │
│  `src/IPolicyBase.cs` — public interface (Handle, HandleAsync, PolicyName)   │
│  `src/PolicyBuilding.cs` — WrapPolicy, WrapUp, WithPolicyName extensions     │
└─────────────────────────────────────┬────────────────────────────────────────┘
                                      │
                                      ▼
┌──────────────────────────────────────────────────────────────────────────────┐
│                     PolicyProcessor Layer                                     │
│  `src/PolicyProcessor.cs` — abstract base: ExceptionFilter, BulkErrorProcessor│
│  Concrete: DefaultRetryProcessor, DefaultFallbackProcessor,                   │
│            SimplePolicyProcessor                                              │
│  Error chain: ExceptionFilter → PolicyRule → BulkErrorProcessor              │
│  CatchBlockHandlers: sync/async handler creation                             │
└─────────────────────────────────────┬────────────────────────────────────────┘
                                      │
                                      ▼
┌──────────────────────────────────────────────────────────────────────────────┐
│                     Error Processing Chain                                    │
│  `src/ErrorProcessors/IErrorProcessor.cs` — single processor interface       │
│  `src/ErrorProcessors/BulkErrorProcessor.cs` — sequential processor chain    │
│  `src/ErrorProcessors/ProcessingErrorContext.cs` — context for processors    │
│  Typed processors: DefaultErrorProcessor<T>, DefaultTypedErrorProcessor,      │
│                    DefaultInnerErrorProcessor, DelayErrorProcessor            │
└──────────────────────────────────────────────────────────────────────────────┘
         │
         ▼
┌──────────────────────────────────────────────────────────────────────────────┐
│                     Exception Filtering                                       │
│  `src/ExceptionFilter/ExceptionFilterSet.cs` — include/exclude predicate sets │
│  `src/ExceptionFilter/PolicyProcessor.ExceptionFilter.cs` — filter logic     │
│  `src/CatchBlockHandlers/CatchBlockFilter.cs` — filter + empty/non-empty     │
│  `src/ErrorSet.cs` — typed error set definitions                             │
└──────────────────────────────────────────────────────────────────────────────┘
```

## Component Responsibilities

| Component | Responsibility | File |
|-----------|----------------|------|
| `IPolicyBase` | Public contract: Handle/HandleAsync + PolicyProcessor + PolicyName | `src/IPolicyBase.cs` |
| `Policy` | Abstract base: wraps delegates, manages PolicyResultHandlers, Wrap support | `src/Policy.cs` |
| `PolicyProcessor` | Abstract base: ExceptionFilter, BulkErrorProcessor, HandleException chain | `src/PolicyProcessor.cs` |
| `RetryPolicy` | Retry N times (or infinite) with optional delay; exposes fluent API | `src/Retry/RetryPolicy.cs` |
| `DefaultRetryProcessor` | Core retry loop: RetryCountInfo, RetryDelay, error saving | `src/Retry/DefaultRetryProcessor.cs` |
| `FallbackPolicy` / `FallbackPolicyBase` | Execute delegate, fallback to alternative on failure | `src/Fallback/FallbackPolicy.cs`, `src/Fallback/FallbackPolicyBase.cs` |
| `DefaultFallbackProcessor` | Execute delegate + call fallback action/func on exception | `src/Fallback/DefaultFallbackProcessor.cs` |
| `SimplePolicy` | Execute delegate, catch and process exception (no retry/fallback) | `src/Simple/SimplePolicy.cs` |
| `SimplePolicyProcessor` | Single-execute with exception handling via PolicyProcessor chain | `src/Simple/SimplePolicyProcessor.cs` |
| `TryCatch` / `TryCatchBase` | Builder-pattern try/catch with CatchBlockHandlers | `src/TryCatch/ITryCatch.cs`, `src/TryCatch/TryCatchBase.cs` |
| `TryCatchBuilder` | Fluent builder: add CatchBlockFilteredHandler / CatchBlockForAllHandler | `src/TryCatch/TryCatchBuilder.cs` |
| `PolicyCollection` | Ordered collection of policies; HandleDelegate runs them sequentially | `src/PolicyComposition/PolicyCollection.cs` |
| `PolicyWrapperFactory` | Creates PolicyWrapperSingle or PolicyWrapperCollection for Wrap | `src/Wrap/PolicyWrapperFactory.cs` |
| `OuterPolicyRegistrar<T>` | Returned by WrapUp; exposes `.OuterPolicy` | `src/Wrap/OuterPolicyRegistrar.cs` |
| `PolicyResult` / `PolicyResult<T>` | Execution result: IsSuccess, IsFailed, IsCanceled, Errors, Result | `src/PolicyResult.cs` |
| `PolicyDelegate` / `PolicyDelegate<T>` | Packs a policy + delegate into one object | `src/PolicyDelegate.cs`, `src/PolicyDelegate.T.cs` |
| `BulkErrorProcessor` | Runs a list of `IErrorProcessor` in sequence | `src/ErrorProcessors/BulkErrorProcessor.cs` |
| `IErrorProcessor` | Single error processing unit (Process / ProcessAsync) | `src/ErrorProcessors/IErrorProcessor.cs` |
| `PipelineFuncBuilder` | Functional pipeline builder: StartWith, AddStep, Build | `src/PipelineFunc/PipelineFuncBuilder.cs` |
| `DelegateInvoking` | Convenience extension methods on delegates (InvokeWithRetry, etc.) | `src/DelegateInvoking/DelegateInvoking.cs` |

## Pattern Overview

**Overall:** Policy-based error handling library using Strategy + Builder + Decorator patterns.

**Key Characteristics:**
- Each policy type (Retry, Fallback, Simple) follows a **Policy + Processor** separation — the Policy class manages the public API and wrapping, while the Processor class contains the execution logic.
- Policies are composed via **Wrap** (decorator pattern): an outer policy wraps an inner policy's delegate execution.
- **PolicyCollection** enables sequential policy chains — each policy handles the same delegate in order.
- **Fluent builder** pattern used throughout: all Policy methods return `this` for chaining.
- **Error processing** uses a pipeline of `IErrorProcessor` instances managed by `BulkErrorProcessor`.
- **CatchBlockHandlers** in TryCatch use a filter + processor model, similar to try/catch blocks.
- **CRTP (Curiously Recurring Template Pattern)** used for fluent method return types via generic interfaces like `IWithErrorFilter<T>`, `ICanAddErrorFilter<T>`.

## Layers

**Public API Layer:**
- Purpose: Entry points for users to create and configure policies
- Location: `src/Retry/RetryPolicy.cs`, `src/Fallback/FallbackPolicy.cs`, `src/Simple/SimplePolicy.cs`, `src/TryCatch/TryCatchBuilder.cs`
- Contains: Policy constructors, fluent configuration methods (IncludeError, ExcludeError, WithWait, AddPolicyResultHandler), Handle/HandleAsync execution methods
- Depends on: Policy base class, PolicyProcessor implementations
- Used by: Consumer code, extension methods, DelegateInvoking

**Policy Base Layer:**
- Purpose: Abstract base providing common behavior for all policies
- Location: `src/Policy.cs`, `src/IPolicyBase.cs`, `src/PolicyBuilding.cs`
- Contains: Wrap delegation, PolicyResult handler management, WrapDelegateIfNeed
- Depends on: PolicyProcessor, PolicyWrapperFactory, PolicyResultHandlerCollection
- Used by: All concrete policy types

**Processor Layer:**
- Purpose: Core execution logic for each policy type
- Location: `src/Retry/DefaultRetryProcessor.cs`, `src/Fallback/DefaultFallbackProcessor.cs`, `src/Simple/SimplePolicyProcessor.cs`
- Contains: Retry loop, fallback execution, exception handling chain (HandleException/HandleExceptionAsync)
- Depends on: BulkErrorProcessor, ExceptionFilter, CatchBlockHandler infrastructure
- Used by: Corresponding Policy classes

**Error Processing Layer:**
- Purpose: Process exceptions through a configurable chain of error processors
- Location: `src/ErrorProcessors/` (30 files)
- Contains: IErrorProcessor interface, BulkErrorProcessor (sequential runner), typed processors (DefaultErrorProcessor<T>, DefaultInnerErrorProcessor, DelayErrorProcessor), ProcessingErrorContext
- Depends on: ExceptionFilter for filtering
- Used by: PolicyProcessor.HandleException/HandleExceptionAsync, CatchBlockHandlers

**Exception Filtering Layer:**
- Purpose: Include/exclude exceptions from handling based on type and predicate
- Location: `src/ExceptionFilter/ExceptionFilterSet.cs`, `src/ExceptionFilter/PolicyProcessor.ExceptionFilter.cs`, `src/CatchBlockHandlers/CatchBlockFilter.cs`, `src/ErrorSet.cs`
- Contains: Include/exclude expression lists, compiled predicate logic, catch block filter infrastructure
- Depends on: System.Linq.Expressions
- Used by: PolicyProcessor, CatchBlockHandlers

**Composition Layer:**
- Purpose: Combine multiple policies or delegates into orchestrated sequences
- Location: `src/PolicyComposition/` (6 files), `src/Collections/` (25 files), `src/Wrap/` (10 files), `src/PipelineFunc/` (11 files)
- Contains: PolicyCollection, PolicyDelegateCollection, PolicyWrapper hierarchy, PipelineFuncBuilder
- Depends on: All policy types, PolicyDelegate, PolicyResult
- Used by: Public API (WrapUp, HandleDelegate, PipelineFuncBuilder.StartWith)

## Data Flow

### Primary Request Path (e.g., RetryPolicy.Handle)

1. User calls `RetryPolicy.Handle(action, token)` (`src/Retry/RetryPolicy.cs:72`)
2. `WrapDelegateIfNeed(action, token)` wraps delegate if Wrap is configured (`src/Policy.cs:104`)
3. `RetryProcessor.Retry(Act, RetryInfo, token)` invokes `DefaultRetryProcessor` (`src/Retry/DefaultRetryProcessor.cs:32`)
4. Inside retry loop: delegate is executed; on exception → `PolicyProcessor.HandleException()` (`src/PolicyProcessor.cs:175`)
5. `HandleException` evaluates ExceptionFilter → runs BulkErrorProcessor chain → evaluates PolicyRule
6. If `ExceptionHandlingResult.Accepted`, loop continues (retry); if `Handled`, return result
7. `PolicyResult` is set with errors, status; returned to user
8. PolicyResult handlers run via `HandlePolicyResult(retryResult, token)` (`src/Policy.cs:67`)

### Policy Composition (Wrap)

1. User calls `retryPolicy.WrapPolicy(fallbackPolicy)` → `Policy.SetWrap(fallbackPolicy)` (`src/Policy.cs:82`)
2. Or `retryPolicy.ThenFallback()` → `this.WrapUp(new FallbackPolicy())` → `OuterPolicyRegistrar` (`src/Wrap/OuterPolicyRegistrar.cs:9`)
3. On Handle: `WrapDelegateIfNeed` creates `PolicyWrapperSingle` from `PolicyWrapperFactory` (`src/Wrap/PolicyWrapperFactory.cs:30`)
4. Wrapper executes inner policy first; if it fails, outer policy catches and handles

### PolicyCollection Execution

1. `PolicyCollection.Create(retryPolicy, fallbackPolicy).HandleDelegate(action)` (`src/PolicyComposition/PolicyCollection.HandleDelegate.cs:17`)
2. Internally creates `PolicyDelegateCollection` from policies + action (`src/PolicyComposition/PolicyCollection.cs:402`)
3. Builds `PolicyDelegateCollectionHandler` (`src/Collections/PolicyDelegateCollectionHandler.cs`)
4. `PolicyDelegatesHandler.HandleAllSync/Async` iterates policies sequentially (`src/Collections/PolicyDelegatesHandler.cs:11`)
5. Stops when a policy succeeds (no longer failed/canceled)

**State Management:**
- `PolicyResult` is the primary mutable state carrier — tracks IsSuccess, IsFailed, IsCanceled, Errors, Result, WrappedPolicyResults
- `PolicyDelegateResult` packages a PolicyDelegate + PolicyResult together
- `PolicyDelegateCollectionResult` aggregates results from a PolicyCollection execution

## Key Abstractions

**IPolicyBase / Policy:**
- Purpose: Common interface and base class for all error handling policies
- Examples: `src/IPolicyBase.cs`, `src/Policy.cs`
- Pattern: Template Method — Policy base provides WrapDelegateIfNeed/HandlePolicyResult; subclasses implement execution logic

**IPolicyProcessor / PolicyProcessor:**
- Purpose: Abstraction for policy execution engines with error processing
- Examples: `src/IPolicyProcessor.cs`, `src/PolicyProcessor.cs`, `src/Retry/IRetryProcessor.cs`, `src/Fallback/IFallbackProcessor.cs`, `src/Simple/ISimplePolicyProcessor.cs`
- Pattern: Strategy — each policy type has its own processor; PolicyProcessor base provides HandleException chain

**IErrorProcessor / BulkErrorProcessor:**
- Purpose: Modular error processing pipeline
- Examples: `src/ErrorProcessors/IErrorProcessor.cs`, `src/ErrorProcessors/BulkErrorProcessor.cs`
- Pattern: Chain of Responsibility — processors run sequentially, each transforms the exception

**ErrorContext<T>:**
- Purpose: Typed context passed through the error handling chain (retry count, error context type, params)
- Examples: `src/CatchBlockHandlers/ErrorContext.cs`, `src/Retry/RetryErrorContext.cs`
- Pattern: Context Object

**CatchBlockHandler:**
- Purpose: Models a try/catch block with filter + processors (used by TryCatch)
- Examples: `src/CatchBlockHandlers/CatchBlockHandler.cs`
- Pattern: Strategy — CatchBlockFilteredHandler (filtered) vs CatchBlockForAllHandler (catch-all)

**PolicyWrapper / PolicyWrapperFactory:**
- Purpose: Decorator that wraps inner policy execution for the outer policy
- Examples: `src/Wrap/PolicyWrapper.cs`, `src/Wrap/PolicyWrapperFactory.cs`, `src/Wrap/PolicyWrapperSingle.cs`, `src/Wrap/PolicyWrapperCollection.cs`
- Pattern: Decorator / Factory Method

## Entry Points

**Policy.Handle / Policy.HandleAsync:**
- Location: Each policy class (e.g., `src/Retry/RetryPolicy.cs:72`, `src/Fallback/FallbackPolicyBase.cs:31`, `src/Simple/SimplePolicy.cs:40`)
- Triggers: User code calling policy execution
- Responsibilities: Wrap delegate if needed, delegate to processor, process PolicyResult handlers

**PolicyCollection.HandleDelegate / HandleDelegateAsync:**
- Location: `src/PolicyComposition/PolicyCollection.HandleDelegate.cs:17`
- Triggers: User code executing a delegate through a policy collection
- Responsibilities: Build delegate collection, iterate policies sequentially

**DelegateInvoking Extension Methods:**
- Location: `src/DelegateInvoking/DelegateInvoking.cs`
- Triggers: Convenience methods like `action.InvokeWithRetry(3)`, `func.InvokeWithFallbackAsync(fallback)`
- Responsibilities: Create ephemeral policy, handle delegate, return result

**TryCatchBuilder.CreateAndBuild / Build:**
- Location: `src/TryCatch/TryCatchBuilder.cs:24`
- Triggers: User code creating a TryCatch instance
- Responsibilities: Build CatchBlockHandler list, create TryCatch wrapping a SimplePolicy

**PipelineFuncBuilder.StartWith:**
- Location: `src/PipelineFunc/PipelineFuncBuilder.cs:18`
- Triggers: User code building a functional pipeline
- Responsibilities: Create PipelineDelegateHolder, return PipelineFuncBuilder for chaining

## Architectural Constraints

- **Threading:** Single-threaded execution per Handle/HandleAsync call. No internal concurrency. CancellationToken propagation is thorough throughout all layers.
- **Global state:** No module-level singletons or shared mutable state. Each Policy/Processor instance is independent.
- **Circular imports:** None detected. The dependency graph flows downward: Policy → PolicyProcessor → ErrorProcessors/ExceptionFilter.
- **InternalsVisibleTo:** `PoliNorError.Tests` and `DynamicProxyGenAssembly2` have access to internal members (`src/PoliNorError.csproj:41-49`).
- **.NET Standard 2.0 target:** No Span<T>, no IAsyncEnumerable, limited async support compared to modern .NET.
- **Single wrapped policy limit:** A policy can only wrap one other policy or one PolicyCollection — not multiple individual wraps (`src/Policy.cs:86-87`).

## Anti-Patterns

### Partial Class Proliferation

**What happens:** Many policy and processor classes are split across 3-6 partial class files (e.g., `RetryPolicy` has `RetryPolicy.cs`, `RetryPolicy.WithInnerErrorProcessorOf.cs`, `RetryPolicy.WithTypedErrorProcessor.cs`; `DefaultRetryProcessor` has 7+ partial files).
**Why it's wrong:** Makes it harder to understand the full API surface of a class; requires reading multiple files for the complete picture.
**Do this instead:** Consider grouping by feature within a single file when the partial files are small, or use regions. Current structure is acceptable for organization but should not be extended further.

### Extensive Method Overloading for Sync/Async/Param Variants

**What happens:** Each policy type has dozens of Handle/HandleAsync overloads for Action, Func<T>, Action<TParam>, Func<TParam, T>, Action<TErrorContext>, Func<TErrorContext, T>, etc. (`src/Retry/RetryPolicy.cs` alone has 607 lines).
**Why it's wrong:** Makes the public API surface very large and increases maintenance burden.
**Do this instead:** The current approach is a deliberate design choice for usability. Extension methods in `DelegateInvoking` partially mitigate this by offering a simpler path.

## Error Handling

**Strategy:** Multi-layered exception handling with configurable filtering and processing.

**Patterns:**
- **Exception Filter Chain:** Include/exclude by type and predicate (`src/ExceptionFilter/ExceptionFilterSet.cs`) → compiled to a single `Func<Exception, bool>`
- **CatchBlockHandler Model:** Each catch block has a `CatchBlockFilter` (include/exclude) and a `BulkErrorProcessor` (process chain)
- **Policy Rule:** A `Func<ErrorContext<T>, CancellationToken, bool>` that determines if the exception should be handled by the policy (e.g., retry only if count allows)
- **ProcessingOrder:** Configurable — `EvaluateThenProcess` (filter first, then run processors) or `ProcessThenEvaluate` (run processors first, then filter)
- **ExceptionHandlingBehavior:** `Handle` (always process) or `ConditionalRethrow` (rethrow if filter not satisfied)
- **Result Propagation:** All errors stored in `PolicyResult.Errors`, `PolicyResult.CatchBlockErrors`, and `PolicyResult.PolicyResultHandlingErrors`

## Cross-Cutting Concerns

**Logging:** No built-in logging framework. Errors are captured in `PolicyResult.Errors` and `BulkProcessResult` collections.

**Validation:** Null delegate checks at the start of every Handle method — returns `PolicyResult` with `PolicyResultFailedReason.DelegateIsNull`.

**Authentication:** Not applicable (library has no auth concerns).

**Cancellation:** Thorough `CancellationToken` support throughout all Handle/HandleAsync methods and error processors. Cancellation detected via `token.IsCancellationRequested` and `OperationCanceledException` catch clauses with `when` guards.

---

*Architecture analysis: 2026-06-13*

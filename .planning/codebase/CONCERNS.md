# Codebase Concerns

**Analysis Date:** 2026-06-13

## Tech Debt

**Obsolete API Surface (19 instances):**
- Issue: 19 `[Obsolete]` attributes spread across the codebase from multiple refactoring cycles. These are preserved for backward compatibility but create API surface noise.
- Files:
  - `src/PolicyProcessor.cs:15` — `_isPolicyAliasSet` field
  - `src/PolicyProcessor.cs:23,35` — constructors accepting `PolicyAlias`
  - `src/CatchBlockHandlers/PolicyProcessorCatchBlockHandlerBase.cs:35,61` — old methods
  - `src/PolicyResultExtensions.cs:37` — old method
  - `src/ErrorProcessors/BulkErrorProcessor.cs:25` — `PolicyAlias` constructor
  - `src/ErrorProcessors/BulkErrorProcessor.cs:199` — `BulkProcessStatus` enum
  - `src/Retry/RetryDelay.cs:17,22` — `InnerDelay`, `InnerDelayValueProvider` properties
  - `src/Fallback/FallbackFuncExecResult.cs:48,59` — old methods
  - `src/ErrorProcessors/ProcessingErrorInfo.cs:23` — `CurrentRetryCount` property
  - `src/Utilities/DelayProvider.cs:64,88` — `BackoffSafely`/`BackoffSafelyAsync` methods
  - `src/Fallback/FallbackFuncExtensions.cs:86,110` — old fallback conversion methods
  - `src/ErrorProcessors/ProcessingErrorContext.cs:12` — `CurrentRetryCount` property
  - `src/TryCatch/TryCatchBuilder.cs:174` — `AddCatchBlock` with `Action<IBulkErrorProcessor>`
- Impact: Increases cognitive load for consumers; some obsolete members are still used internally via `#pragma warning disable CS0618`.
- Fix approach: Establish a deprecation timeline. Remove obsolete members in the next major version (v3.0). Currently all guarded with `#pragma warning disable S1133`.

**Internal Use of Obsolete Members:**
- Issue: Three `RetryDelay` subclasses (`ConstantRetryDelay`, `LinearRetryDelay`, `ExponentialRetryDelay`) internally use the deprecated `InnerDelay` property, suppressing CS0618 warnings.
- Files:
  - `src/Retry/ExponentialRetryDelay.cs:17-19`
  - `src/Retry/LinearRetryDelay.cs:16-18`
  - `src/Retry/ConstantRetryDelay.cs:20-22`
  - `src/Retry/RetryDelay.cs:82-84,108-127`
  - `src/Retry/RetryProcessingErrorInfo.cs:7-9`
- Impact: Self-referencing deprecation; risks confusion when new contributors touch these files.
- Fix approach: Refactor subclasses to use `DelayValueProvider` directly, then remove `InnerDelay`/`InnerDelayValueProvider`.

**Test Project Uses Legacy Format:**
- Issue: `tests/PoliNorError.Tests.csproj` uses the old-style MSBuild format (non-SDK-style `ToolsVersion="15.0"`) targeting .NET Framework 4.7.2, while the library itself targets .NET Standard 2.0. This requires explicit `<Compile Include>` for every test file and `packages.config`-style NuGet references.
- Files: `tests/PoliNorError.Tests.csproj`
- Impact: Increased maintenance burden for adding tests; manual file inclusion; multiple accumulated NUnit package import conditions (lines 3-7 include imports for NUnit 4.2.2, 4.3.0, 4.3.1, 4.3.2, 4.5.1).
- Fix approach: Migrate to SDK-style .csproj targeting `net472` (or multi-target including `net6.0+`), which would simplify project file and enable modern test features.

**Expression Compilation Cost in Exception Filters:**
- Issue: `ExceptionFilterSet.CompilePredicate()` calls `Expression.Compile()` each time a new filter is added. This compiles LINQ expression trees to delegates at runtime.
- Files: `src/ExceptionFilter/ExceptionFilterSet.cs:35-54`
- Impact: Minor performance overhead on policy construction (not at execution time). Expression compilation is relatively expensive but happens only during configuration.
- Fix approach: Consider caching compiled predicates or compiling lazily only when first needed.

**Multiple NotImplementedException Throws as Assertion-Like Guards:**
- Issue: 22 `throw new NotImplementedException()` calls in source code. Most serve as assertion guards in switch statements for `SyncType` enum values or as interface method stubs, rather than representing truly unimplemented functionality.
- Files:
  - `src/PolicyResult.cs:201` — `SetErrors` protected method throws unconditionally
  - `src/Retry/RetryDelay.cs:71,124` — switch default cases
  - `src/ExceptionFilter/PolicyProcessor.ExceptionFilter.cs:44,70` — switch defaults
  - `src/CatchBlockHandlers/CatchBlockFilter.cs:29,52` — switch defaults
  - `src/Policy.cs:86` — wrapped policy limitation
  - `src/HandlerRunners/ASyncHandlerRunner.cs:36` — sync method on async runner
  - `src/HandlerRunners/SyncHandlerRunner.cs:36` — async method on sync runner
  - `src/HandlerRunners/SyncHandlerRunnerT.cs:47` — async method on sync runner
  - `src/HandlerRunners/ASyncHandlerRunnerT.cs:46` — sync method on async runner
  - `src/HandlerRunners/PolicyResultHandlerCollection.cs:102,119`
  - `src/HandlerRunners/HandlerRunnersCollection.cs:31`
  - `src/Utilities/ThrowHelper.cs:11`
- Impact: Not a runtime risk in practice (enum exhaustive switches are safe), but `NotImplementedException` is semantically wrong for assertion guards. `InvalidOperationException` or `NotSupportedException` would be more appropriate.
- Fix approach: Replace switch-default `NotImplementedException` with `NotSupportedException` or `ArgumentOutOfRangeException`. The handler runner stubs should throw `NotSupportedException` since calling the wrong direction is a programming error.

**Unsupported Wrapped Policy Chain Limitation:**
- Issue: `Policy.SetWrap` throws `NotImplementedException("More than one wrapped policy is not supported.")` — this is an architectural limitation, not an unimplemented feature.
- Files: `src/Policy.cs:86`
- Impact: Users cannot chain multiple wrapping policies. This limits composability for advanced scenarios.
- Fix approach: If this is intended to remain a limitation, change to `InvalidOperationException` with a descriptive message. If multi-wrap support is planned, implement a policy chain mechanism.

## Known Bugs

**HACK in Test Code:**
- Symptoms: Test uses `await Task.Delay(1)` as a workaround to force async continuation.
- Files: `tests/PipelineFuncBuilderTests.cs:1213`
- Trigger: The test for cancellation during pipeline error processing needs a brief async yield to allow `CancellationTokenSource.Cancel()` to propagate.
- Workaround: The `//HACK` comment with `await Task.Delay(1)` is explicitly marked by the author.

## Security Considerations

**Exception Serialization Suppressed:**
- Risk: Multiple custom exception classes suppress the `S3925: "ISerializable" should be implemented correctly` analyzer rule. In .NET Standard 2.0, this matters for cross-AppDomain scenarios.
- Files:
  - `src/Exceptions/CatchBlockException.cs:9`
  - `src/Exceptions/PolicyExceptions.cs:6,12`
  - `src/Collections/PolicyDelegateCollectionException.cs:9,44`
- Current mitigation: Suppressed with `Justification = "<Pending>"` — no actual justification documented.
- Recommendations: Add proper justification text explaining why ISerializable is not needed (e.g., "Library targets netstandard2.0 and is not expected to be used in cross-AppDomain scenarios"). Or implement ISerializable for full .NET Framework compatibility.

**Thread-Local Random Number Generator:**
- Risk: `StaticRandom` uses `ThreadLocal<Random>` which is correct for thread safety but does not use cryptographically secure randomization. This is appropriate for retry jitter but should not be used for security-sensitive purposes.
- Files: `src/Utilities/StaticRandom.cs:8-9`
- Current mitigation: Only used for retry delay jitter (`StandardJitter`, `ExponentialRetryDelay.DecorrelatedJitter`). Not used for security.
- Recommendations: Document that jitter randomization is non-cryptographic. Consider using `Random.Shared` when eventually targeting .NET 6+.

## Performance Bottlenecks

**Sync-over-Async Pattern in Error Processors:**
- Problem: `ErrorProcessorFromAsyncRunner` wraps async error processor functions for synchronous execution using `.Wait()` and `.Wait(token)`, which blocks threads.
- Files:
  - `src/ErrorProcessors/ErrorProcessorFromAsyncRunner.cs:18,31`
  - `src/Utilities/FuncEntensions.cs:42,47,70,75,88,93,137,163,168`
  - `src/PolicyResultExtensions.cs:230,265`
  - `src/Collections/PolicyDelegateSafeHandling.cs:74,95`
- Cause: The library supports both sync and async delegates. When an async error processor is configured but called from a sync policy path, the code blocks the calling thread.
- Improvement path: Clearly document that configuring async error processors with sync policy handling will cause thread blocking. Consider offering a warning or alternative API surface.

**Expression.Compile in Filter Construction:**
- Problem: `ExceptionFilterSet.CompilePredicate()` calls `Expression.Compile()` to create filter delegates from expression trees.
- Files: `src/ExceptionFilter/ExceptionFilterSet.cs:44,48`
- Cause: Each time `CompilePredicate()` is called, it rebuilds and recompiles the full filter expression from scratch.
- Improvement path: Cache the compiled predicate and invalidate only when filters are added/removed. The current implementation in `PolicyProcessor.ExceptionFilter.cs` already caches via `GetCanHandle()` pattern, but the underlying `CompilePredicate` could benefit from memoization.

**Closure Allocation Elimination (Recently Addressed):**
- Status: Version 2.24.20 explicitly addressed closure allocations across all processor methods. Internal overloads accepting original delegates directly were introduced.
- Files: `src/Retry/DefaultRetryProcessor.cs`, `src/Fallback/DefaultFallbackProcessor.cs`, `src/Simple/SimplePolicyProcessor.cs`
- Remaining concern: The library has extensive method overloading (8+ overloads per core method) which increases binary size and API surface area. Monitor for further opportunities to reduce overloads with default parameter patterns (limited by netstandard2.0 constraints).

## Fragile Areas

**DefaultRetryProcessor Complexity:**
- Files: `src/Retry/DefaultRetryProcessor.cs` (566 lines)
- Why fragile: Contains 8+ `RetryInternal`/`RetryInternalAsync` method overloads split across multiple partial class files. Each handles a different combination of (TParam/no-TParam, T/no-T, RetryDelay/no-RetryDelay, limited/infinite retries). Changes to shared logic (retry loop, error saving, delay handling) must be replicated across all variants.
- Safe modification: Changes to retry logic should be validated against ALL overloads. Use the existing test suite (`DefaultRetryProcessorTests.cs`, `DefaultRetryProcessorAsyncTests.cs`, `RetryPolicyWithTypedErrorProcessorTests.cs`) as regression guards.
- Test coverage: 87.6% line coverage, 76.3% branch coverage (per `CODE_COVERAGE.md`).

**SimplePolicy and FallbackPolicyBase:**
- Files: `src/Simple/SimplePolicy.cs` (664 lines), `src/Fallback/FallbackPolicyBase.cs` (377 lines)
- Why fragile: Both classes have extensive method overloads for Handle/HandleAsync with combinations of TParam, TErrorContext, T, configureAwait, and CancellationToken. The SimplePolicy is the largest file in the codebase.
- Safe modification: When adding new overloads, follow the existing pattern of delegation chains (public -> internal -> processor method).
- Test coverage: SimplePolicy 85% line / 75% branch; FallbackPolicyBase 81.5% line / 75% branch.

**PolicyResult State Machine:**
- Files: `src/PolicyResult.cs` (403 lines)
- Why fragile: PolicyResult has many boolean state flags (`NoError`, `IsFailed`, `IsCanceled`, `_executed`, `ErrorFilterUnsatisfied`) that interact in complex ways. The `Status` property (lines 271-299) derives a composite state from multiple flags with specific precedence rules.
- Safe modification: Any change to state flag semantics or precedence in `Status` requires validation across ALL policy types (Retry, Fallback, Simple, wrapped policies).
- Test coverage: 90.7% line / 77.2% branch.

## Scaling Limits

**netstandard2.0 API Constraints:**
- Current capacity: Library targets `netstandard2.0` exclusively.
- Limit: Cannot use `IAsyncDisposable`, `Random.Shared`, `System.Text.Json`, nullable reference types annotations (without polyfill), `Span<T>` optimizations, or `IAsyncEnumerable<T>`.
- Scaling path: Multi-targeting `netstandard2.0;net6.0` (or `net8.0`) would allow conditional compilation for modern APIs while maintaining backward compatibility. This would improve performance-critical paths (jitter, task handling) and enable better async patterns.

**Policy Wrap Depth:**
- Current capacity: Supports wrapping one policy with another, or one policy with a `PolicyCollection`.
- Limit: Explicitly limited to one level — `Policy.SetWrap` throws if called twice (`src/Policy.cs:86`).
- Scaling path: If multi-level wrapping is needed, implement a chain pattern with explicit execution order documentation.

## Dependencies at Risk

**NUnit Version Accumulation in Test Project:**
- Risk: The test `.csproj` contains conditional imports for NUnit versions 4.2.2, 4.3.0, 4.3.1, 4.3.2, and 4.5.1 simultaneously (lines 3-7). The reference points to 4.5.1 but build conditions check for older versions.
- Impact: Build fragility; old package references may cause confusion.
- Migration plan: Clean up to reference only the current NUnit 4.5.1 and remove dead conditional imports.

**No Runtime Dependencies:**
- Status: The library has ZERO runtime NuGet dependencies — it depends only on `netstandard2.0` BCL. All code is self-contained.
- This is a strength but means all utility code (retry delays, jitter, synchronized collections, expression helpers) is maintained in-house.

## Missing Critical Features

**No CI Pipeline:**
- Problem: No `.github/workflows/` directory exists. Only `.github/dependabot.yml` is present for automated dependency updates.
- Blocks: Automated build verification, test execution on PRs, NuGet publishing automation, cross-platform testing.

**No Benchmarking Infrastructure:**
- Problem: No benchmark project or BenchmarkDotNet configuration exists.
- Blocks: Performance regression detection, optimization validation (e.g., verifying closure allocation elimination effectiveness in v2.24.20).

## Test Coverage Gaps

**Low Coverage Classes (per CODE_COVERAGE.md):**
- What's not tested:
  - `ApplyFuncs` — 25% line coverage (`src/Utilities/ApplyFuncs.cs`)
  - `ExceptionDelegatesHelper` — 0% line / 0% branch (`src/Utilities/ExceptionDelegatesHelper.cs`)
  - `CollectionExtensions` — 0% line / 0% branch (`src/Utilities/CollectionExtensions.cs`)
  - `SynchronizedList<T>` — 26.7% line coverage (`src/Utilities/SynchronizedList.cs`)
  - `TaskExtensions` — 43.3% line / 25% branch (`src/Utilities/TaskExtensions.cs`)
  - `DelayErrorProcessorRegistration` — 43.7% line coverage (`src/Retry/DelayErrorProcessorRegistration.cs`)
  - `PolicyErrorProcessorRegistration` — 51.5% line coverage (`src/PolicyExtensions/PolicyErrorProcessorRegistration.cs`)
  - `RetryPolicyCustomErrorSaverRegistration` — 8.8% line / 50% branch (`src/Retry/RetryPolicyCustomErrorSaverRegistration.cs`)
  - `RetryProcessorCustomErrorSaverRegistration` — 35.7% line coverage (`src/Retry/RetryProcessorCustomErrorSaverRegistration.cs`)
  - `SingleDelegateContainer` — 56.5% line / 0% branch (`src/SingleDelegateContainer.cs`)
  - `SingleDelegateContainer<T>` — 62.9% line / 0% branch (`src/SingleDelegateContainerT.cs`)
- Files: Listed above
- Risk: Utility classes like `TaskExtensions` (which implements `WithCancellation` via `ContinueWith`) and `SynchronizedList` (thread-safe collection) are foundational and undertested. `ExceptionDelegatesHelper` at 0% coverage suggests dead code or untested edge paths.
- Priority: High — `TaskExtensions`, `SynchronizedList`, and `ExceptionDelegatesHelper` should be prioritized as they affect correctness guarantees.

**Stale Coverage Report:**
- What's not tested: The `CODE_COVERAGE.md` was generated on 2024-03-24, which is over 2 years old. Versions 2.19.x through 2.24.20 have added substantial new code without updated coverage reporting.
- Files: `CODE_COVERAGE.md`
- Risk: Actual current coverage may differ significantly from reported figures.
- Priority: Medium — regenerate coverage report with current version.

## Build Warnings & Suppressions

**Compiler Warning Suppression NoWarn 1591:**
- Issue: Both Debug and Release configurations suppress warning CS1591 ("Missing XML comment for publicly visible type or member").
- Files: `src/PoliNorError.csproj:25,29`
- Impact: Public API members may lack XML documentation without build warnings. The library does have `GenerateDocumentationFile` enabled, but the missing-comment warning is globally suppressed.
- Recommendation: Remove NoWarn 1591 and address missing XML comments incrementally, or suppress only on specific files/members.

**Analyzer Rule Suppressions (S1133, RCS1163, RCS1194, S3925):**
- Issue: Multiple `#pragma warning disable` directives suppress Sonar/Roslyn analyzers across 73 locations:
  - `S1133` (Deprecated code should be removed) — 14 suppressions around `[Obsolete]` members
  - `RCS1163` (Unused parameter) — 2 suppressions in `PolicyProcessor.cs:37` and `BulkErrorProcessor.cs:27`
  - `RCS1194` (Implement exception constructors) — 12 suppressions across exception classes
  - `S3925` (ISerializable should be implemented) — 6 suppressions across exception classes
  - `CS0618` (Type or member is obsolete) — used in RetryDelay subclasses and RetryProcessingErrorInfo
- Files: Scattered across `src/PolicyProcessor.cs`, `src/ErrorProcessors/BulkErrorProcessor.cs`, `src/Exceptions/*.cs`, `src/Retry/*.cs`, `src/Fallback/*.cs`, `src/TryCatch/TryCatchBuilder.cs`, `src/Collections/PolicyDelegateCollectionException.cs`
- Impact: Technical debt indicator; these suppressions signal areas where proper implementation was deferred.

## Version Management

**Approach:**
- Version defined in `src/PoliNorError.csproj:18` as `<Version>2.24.20</Version>`
- `AssemblyVersion` set to `2.24.20.0` (line 20)
- `FileVersion` set to `0.0.0.0` (line 21) — this is unusual; normally FileVersion matches the package version
- `CHANGELOG.md` is comprehensive (702 lines) covering every release from 1.0.3 through 2.24.20
- No automated version bumping or changelog generation detected

**Concern: FileVersion is 0.0.0.0:**
- Files: `src/PoliNorError.csproj:21`
- Impact: The `FileVersion` shown in Windows file properties will display as `0.0.0.0`, which may confuse users inspecting the DLL. 
- Fix approach: Set `<FileVersion>2.24.20.0</FileVersion>` or use a CI-driven versioning strategy.

## Backward Compatibility

**netstandard2.0 Constraint:**
- The library exclusively targets `netstandard2.0`, ensuring compatibility with .NET Framework 4.6.1+, .NET Core 2.0+, and all .NET 5+ runtimes.
- This prevents use of newer BCL APIs but maximizes reach.
- The test project targets .NET Framework 4.7.2, which is a valid consumer of netstandard2.0.

**Breaking Changes History:**
- Major version 2.0.0 had 6 pre-release stages (alpha through rc5) with significant API churn (renames, accessibility changes, interface removals).
- Post-2.0.0, the library has maintained backward compatibility with additions only.
- The `InternalsVisibleTo` for `DynamicProxyGenAssembly2` (Moq/Castle proxy) exposes internals to mocking frameworks — this is intentional but means internal API changes could theoretically break test mocks.

## Thread Safety

**Pattern: Lock-Based Synchronized Collections:**
- `SynchronizedList<T>` (`src/Utilities/SynchronizedList.cs`) uses `lock(_root)` on every operation, providing coarse-grained thread safety.
- Used by `FlexSyncEnumerable<T>` (`src/Utilities/FlexSyncEnumerable.cs`) for async-safe error processor collections.
- The lock object is `((ICollection<T>)list).SyncRoot`, which is the list's own sync root.

**Atomic Counter:**
- `RetryContext.IncrementCountAtomic()` uses `Interlocked.Increment` (`src/Retry/RetryErrorContext.cs:54`).
- However, `RetryContext.IncrementCount()` uses non-atomic `_currentRetryCount++` (line 52).
- Both methods coexist — `IncrementCount()` is used in sync paths, `IncrementCountAtomic()` in paths needing thread safety.

**Policy instances are NOT thread-safe:**
- `Policy` and its subclasses store mutable state (error processors, filters, handlers) without synchronization.
- The intended usage pattern is: configure a policy once, then use it from multiple threads.
- Concurrent modification of a policy's configuration while it's being used is not protected.

## Cancellation Token Handling

**Pattern: Linked Token Support:**
- Since v2.24.12, all core policy processors support cancellation via linked tokens.
- The library creates `CancellationTokenSource.CreateLinkedTokenSource` internally to support the `CancellationToken` parameter alongside library-managed cancellation.
- Files: `src/PolicyProcessor.cs`, `src/Retry/DefaultRetryProcessor.cs`, `src/Fallback/DefaultFallbackProcessor.cs`, `src/Simple/SimplePolicyProcessor.cs`

**ConfigureAwait Consistency:**
- All async methods consistently use `.ConfigureAwait(configureAwait)` throughout (170+ occurrences).
- Since v2.24.20, new method overloads with required `CancellationToken` parameter (no `configureAwait`) delegate to existing methods with `configureAwait` set to `false`.
- This is a well-executed pattern.

## Memory/GC Considerations

**Closure Allocation Elimination (v2.24.20):**
- The latest release explicitly targeted closure allocations by introducing internal overloads that accept original delegates directly.
- `PolicyResult` uses `MethodImplOptions.AggressiveInlining` on factory methods (`ForSync`, `ForNotSync`, `InitByConfigureAwait`).

**RetryErrorContext Reuse:**
- Since v2.15.0, a single `RetryErrorContext` instance is reused across retry iterations to reduce allocations.
- `EmptyErrorContext` is similarly reused as a singleton for Simple/Fallback policies.

**Collection Allocations:**
- `List<T>` is used throughout (25+ instances of `new List<>()`). These are appropriate for small collections typical in error processing pipelines.
- `Dictionary<Type, ...>` in `FallbackFuncsProvider` (6 dictionaries) could grow if many generic type registrations are made.

## License & Third-Party Notices

**Library License:** MIT (`LICENSE`)

**Third-Party Code:**
- Jittering algorithm adapted from [Polly](https://github.com/App-vNext/Polly) under BSD 3-Clause License.
- Files: `src/Retry/ExponentialRetryDelay.DecorrelatedJitter.cs`, `src/Retry/StandardJitter.cs`
- `THIRD-PARTY-NOTICES.txt` properly documents this attribution.

**Copyright:**
- Copyright 2023 Andrey Kolesnichenko (per `LICENSE` and `PoliNorError.csproj:10`)

---

*Concerns audit: 2026-06-13*

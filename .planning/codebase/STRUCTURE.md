# Codebase Structure

**Analysis Date:** 2026-06-13

## Directory Layout

```
PoliNorError/
├── .github/                        # GitHub CI/CD workflows
├── .planning/                      # Planning documents
│   └── codebase/                   # Codebase analysis docs
├── src/                            # Main library source (PoliNorError.csproj)
│   ├── CatchBlockHandlers/         # Catch block handler infrastructure
│   ├── Collections/                # PolicyDelegate collections and handlers
│   ├── ConvertExceptionDelegates.cs
│   ├── DelegateInvoking/           # Extension methods for quick delegate invocation
│   ├── ErrorProcessors/            # Error processor pipeline (IErrorProcessor, BulkErrorProcessor)
│   ├── ErrorSet.cs                 # Typed error set definitions
│   ├── ExceptionFilter/            # Exception filtering infrastructure
│   ├── Exceptions/                 # Custom exception types
│   ├── Extensions/                 # Internal extension method groups
│   │   ├── PolicyErrorFiltering/
│   │   └── PolicyResultHandling/
│   ├── Fallback/                   # Fallback policy and processor
│   ├── HandlerRunners/             # Sync/async handler runner infrastructure
│   ├── PipelineFunc/               # Functional pipeline builder
│   ├── PolicyComposition/          # PolicyCollection and composition
│   ├── PolicyExtensions/           # Policy-level extension methods
│   ├── PolicyProcessorExtensions/  # PolicyProcessor-level extension methods
│   ├── Properties/                 # Assembly info
│   ├── Retry/                      # Retry policy, processor, delay strategies
│   ├── Simple/                     # Simple policy and processor
│   ├── TryCatch/                   # TryCatch builder pattern
│   ├── Utilities/                  # Shared utilities (Unit, PredicateBuilder, etc.)
│   ├── Wrap/                       # Policy wrapping infrastructure
│   ├── Policy.cs                   # Abstract base class for all policies
│   ├── PolicyProcessor.cs          # Abstract base class for all processors
│   ├── IPolicyBase.cs              # Public policy interface
│   ├── IPolicyProcessor.cs         # Public processor interface
│   ├── PolicyResult.cs             # Execution result types
│   ├── PolicyDelegate.cs           # Policy + delegate pairing
│   ├── PolicyBuilding.cs           # WrapPolicy/WrapUp/WithPolicyName extensions
│   └── PoliNorError.csproj         # Project file (netstandard2.0)
├── tests/                          # Test project (PoliNorError.Tests.csproj)
│   ├── Properties/                 # Test assembly info
│   └── *.cs                        # Test files (co-located, not in subdirectories)
├── PoliNorError.sln                # Solution file
├── CHANGELOG.md
├── CODE_COVERAGE.md
├── README.md
└── THIRD-PARTY-NOTICES.txt
```

## Directory Purposes

**`src/Retry/`:**
- Purpose: Retry policy implementation — retry count, delay strategies, processor
- Contains: 41 files. Policy class, processor, delay types (Constant, Exponential, Linear, TimeSeries), RetryCountInfo, RetryDelay
- Key files: `RetryPolicy.cs` (public API), `DefaultRetryProcessor.cs` (core retry loop), `RetryDelay.cs` (delay strategy base), `IRetryProcessor.cs` (processor interface)

**`src/Fallback/`:**
- Purpose: Fallback policy — execute delegate, call fallback action/func on failure
- Contains: 28 files. Policy class, processor, FallbackFuncsProvider, partial files for typed/inner error processors
- Key files: `FallbackPolicy.cs` (public API), `FallbackPolicyBase.cs` (shared base), `DefaultFallbackProcessor.cs` (core fallback logic), `FallbackFuncsProvider.cs` (fallback delegate management)

**`src/Simple/`:**
- Purpose: Simple policy — execute once, catch and process exception (no retry, no fallback)
- Contains: 9 files. Policy class, processor, partial files for extensions
- Key files: `SimplePolicy.cs` (public API), `SimplePolicyProcessor.cs` (core execute logic), `ISimplePolicyProcessor.cs` (processor interface)

**`src/TryCatch/`:**
- Purpose: Builder-pattern TryCatch mimicking C# try/catch blocks
- Contains: 10 files. ITryCatch interface, TryCatch implementation, TryCatchBuilder, TryCatchResult
- Key files: `TryCatchBuilder.cs` (fluent builder), `ITryCatch.cs` (TryCatch class + interface), `TryCatchBase.cs` (decorator base), `CatchBlockHandlerCollectionWrapper.cs` (converts handlers to SimplePolicy)

**`src/ErrorProcessors/`:**
- Purpose: Error processing pipeline — single processors and bulk runner
- Contains: 30 files. IErrorProcessor interface, BulkErrorProcessor (sequential chain), typed processors, registration extensions
- Key files: `IErrorProcessor.cs` (processor interface), `BulkErrorProcessor.cs` (sequential runner), `ErrorProcessor.cs` (convenience base), `DefaultErrorProcessor.T.cs` (typed context processor), `ProcessingErrorContext.cs` (context type)

**`src/CatchBlockHandlers/`:**
- Purpose: Catch block infrastructure — filters, handlers, error context
- Contains: 13 files. CatchBlockHandler hierarchy, CatchBlockFilter, ErrorContext<T>, sync/async handlers
- Key files: `CatchBlockHandler.cs` (CatchBlockFilteredHandler, CatchBlockForAllHandler, CatchBlockHandlerFactory), `ErrorContext.cs` (ErrorContext<T>, EmptyErrorContext), `CatchBlockFilter.cs` (filter logic)

**`src/ExceptionFilter/`:**
- Purpose: Exception filtering — include/exclude predicates, compiled filter evaluation
- Contains: 2 files
- Key files: `ExceptionFilterSet.cs` (include/exclude lists, compile logic), `PolicyProcessor.ExceptionFilter.cs` (ExceptionFilter class used by PolicyProcessor)

**`src/Wrap/`:**
- Purpose: Policy wrapping — outer policy wraps inner policy's delegate execution
- Contains: 10 files. PolicyWrapper hierarchy, PolicyWrapperFactory, OuterPolicyRegistrar
- Key files: `PolicyWrapperFactory.cs` (creates wrappers), `PolicyWrapperSingle.cs` (wraps one policy), `PolicyWrapperCollection.cs` (wraps a collection), `OuterPolicyRegistrar.cs` (returned by WrapUp)

**`src/PolicyComposition/`:**
- Purpose: PolicyCollection — ordered list of policies with HandleDelegate
- Contains: 6 files. PolicyCollection (WithPolicy, HandleDelegate, error processors, filtering)
- Key files: `PolicyCollection.cs` (main class), `PolicyCollection.HandleDelegate.cs` (HandleDelegate/HandleDelegateAsync), `PolicyCollection.WithPolicy.cs` (fluent WithRetry, WithFallback, WithSimple)

**`src/Collections/`:**
- Purpose: PolicyDelegate collections and sequential execution handlers
- Contains: 25 files. PolicyDelegateCollection, PolicyDelegateCollectionHandler, PolicyDelegatesHandler, result types
- Key files: `PolicyDelegatesHandler.cs` (HandleAllSync/HandleAllBySyncType), `PolicyDelegateCollection.cs` (collection + registrar), `PolicyDelegateCollectionHandler.cs` (handler orchestration)

**`src/PipelineFunc/`:**
- Purpose: Functional pipeline builder — chain functions with individual policies per step
- Contains: 11 files. PipelineFuncBuilder, PipelineDelegateHolder, PipelineResult, step builders
- Key files: `PipelineFuncBuilder.cs` (StartWith, StartWithRetry, StartWithFallback), `IPipelineFuncStepBuilder.TIn.TMid.TOut.cs` (AddStep interface), `PipelineResult.cs` (pipeline result)

**`src/DelegateInvoking/`:**
- Purpose: Extension methods on Action/Func for quick one-liner error handling
- Contains: 12 files. InvokeWithRetry, InvokeWithFallback, InvokeWithSimple, parameterized/error-context variants
- Key files: `DelegateInvoking.cs` (main extension methods), parameterized and error-context partial files

**`src/HandlerRunners/`:**
- Purpose: Internal infrastructure for running sync/async handlers
- Contains: 11 files. IHandlerRunner interfaces, sync/async runners, collection
- Key files: `HandlerRunnerBase.cs`, `SyncHandlerRunner.cs`, `ASyncHandlerRunner.cs`, `PolicyResultHandlerCollection.cs`

**`src/Extensions/`:**
- Purpose: Internal extension methods organized by concern
- Contains: 2 subdirectories
- Key files: `PolicyErrorFiltering/PolicyErrorFiltering.cs` (IncludeError/ExcludeError extensions), `PolicyResultHandling/PolicyResultHandling.cs` (AddHandlerForPolicyResult extensions)

**`src/PolicyExtensions/`:**
- Purpose: Extension methods on Policy for error processor registration
- Contains: 4 files. PolicyErrorProcessorRegistration for WithErrorProcessorOf, WithErrorContextProcessorOf
- Key files: `PolicyErrorProcessorRegistration.cs`, `.ForTypedError.cs`, `.ForInnerError.cs`, `.ForErrorContext.cs`

**`src/PolicyProcessorExtensions/`:**
- Purpose: Extension methods on PolicyProcessor for error processor and filter registration
- Contains: 5 files
- Key files: `ErrorProcessorRegistration.cs`, `PolicyProcessorErrorFiltering.cs`

**`src/Exceptions/`:**
- Purpose: Custom exception types used by the library
- Contains: 9 files
- Key files: `CatchBlockException.cs` (exception with source info), `NoDelegateException.cs`, `PolicyResultHandlerFailedException.cs`, `OperationFailedAndCanceledException.cs`

**`src/Utilities/`:**
- Purpose: Shared utility types and helpers
- Contains: 23 files
- Key files: `Unit.cs` (void substitute for generics), `PredicateBuilder.cs` (expression composition), `FlexSyncEnumerable.cs` (sync/async collection), `DelayProvider.cs`, `ExpressionHelper.cs`, `ExceptionExtensions.cs`

## Key File Locations

**Entry Points:**
- `src/Retry/RetryPolicy.cs`: Main Retry policy class — constructors, Handle/HandleAsync, fluent config
- `src/Fallback/FallbackPolicy.cs`: Main Fallback policy — WithFallbackAction/Func, Handle/HandleAsync
- `src/Simple/SimplePolicy.cs`: Main Simple policy — Execute-style Handle/HandleAsync
- `src/TryCatch/TryCatchBuilder.cs`: TryCatch builder — CreateAndBuild, CreateFrom, AddCatchBlock
- `src/DelegateInvoking/DelegateInvoking.cs`: Quick invocation extensions (InvokeWithRetry, etc.)
- `src/PolicyComposition/PolicyCollection.cs`: Policy composition — Create, WithPolicy, HandleDelegate
- `src/PipelineFunc/PipelineFuncBuilder.cs`: Functional pipeline — StartWith, StartWithRetry

**Configuration:**
- `src/PoliNorError.csproj`: Project file — netstandard2.0, version 2.24.20, MIT license
- `.env*` files: Not present (library has no external service dependencies)

**Core Logic:**
- `src/Policy.cs`: Abstract base — WrapDelegateIfNeed, HandlePolicyResult, PolicyWrapperFactory integration
- `src/PolicyProcessor.cs`: Abstract base — HandleException/HandleExceptionAsync chain (ExceptionFilter → PolicyRule → BulkErrorProcessor)
- `src/PolicyResult.cs`: Result types — PolicyResult, PolicyResult<T>, PolicyResultFailedReason, WrappedPolicyStatus
- `src/ErrorProcessors/BulkErrorProcessor.cs`: Sequential error processor chain execution
- `src/ExceptionFilter/ExceptionFilterSet.cs`: Include/exclude filter compilation

**Testing:**
- `tests/*.cs`: Test files — 100+ test files, flat structure in tests/ directory

## Naming Conventions

**Files:**
- PascalCase for all files: `RetryPolicy.cs`, `DefaultRetryProcessor.cs`, `BulkErrorProcessor.cs`
- Partial classes use dot-separated suffixes: `RetryPolicy.WithTypedErrorProcessor.cs`, `DefaultRetryProcessor.RetryOverloads.cs`, `FallbackPolicy.WithInnerErrorProcessorOf.cs`
- Generic type parameters in filenames use dots: `IPipelineFuncBuilder.TIn.TOut.cs`, `PolicyDelegate.T.cs`

**Directories:**
- PascalCase singular nouns: `Retry/`, `Fallback/`, `Simple/`, `TryCatch/`, `Wrap/`, `CatchBlockHandlers/`
- Plural for collections of related types: `ErrorProcessors/`, `Extensions/`, `Exceptions/`, `Utilities/`, `HandlerRunners/`

**Namespaces:**
- Root namespace: `PoliNorError` — all core types live here regardless of directory
- Sub-namespace: `PoliNorError.TryCatch` — TryCatch types use their own sub-namespace
- Sub-namespace: `PoliNorError.Extensions.PolicyErrorFiltering` and `PoliNorError.Extensions.PolicyResultHandling` — internal extension groups

**Classes/Interfaces:**
- Interfaces: `I` prefix — `IPolicyBase`, `IPolicyProcessor`, `IErrorProcessor`, `IRetryProcessor`, `IFallbackProcessor`
- CRTP interfaces: Generic self-referencing — `IWithErrorFilter<T>`, `ICanAddErrorFilter<T>`, `IWithPolicy<T>`
- Abstract classes: No prefix — `Policy`, `PolicyProcessor`, `FallbackPolicyBase`, `TryCatchBase`
- Sealed classes: `sealed` keyword on leaf policy types — `RetryPolicy`, `FallbackPolicy`, `SimplePolicy`
- Processors: `Default` prefix for default implementations — `DefaultRetryProcessor`, `DefaultFallbackProcessor`

## Where to Add New Code

**New Policy Type:**
1. Create directory: `src/NewPolicy/`
2. Implement processor: `src/NewPolicy/NewPolicyProcessor.cs` extending `PolicyProcessor`
3. Implement policy: `src/NewPolicy/NewPolicy.cs` extending `Policy`
4. Add interface: `src/NewPolicy/INewPolicy.cs` extending `IPolicyBase`
5. Add WithPolicy registration: `src/PolicyComposition/PolicyCollection.WithPolicy.cs`
6. Add DelegateInvoking extensions: `src/DelegateInvoking/DelegateInvoking.NewPolicy.cs`
7. Add tests: `tests/NewPolicyTests.cs`

**New Error Processor:**
- Implementation: `src/ErrorProcessors/NewErrorProcessor.cs`
- Registration extensions: Already handled by generic `ErrorProcessorRegistration` in `src/PolicyProcessorExtensions/`

**New Delay Strategy (Retry):**
- Implementation: `src/Retry/NewRetryDelay.cs` extending `RetryDelay` (`src/Retry/RetryDelay.cs`)

**New Pipeline Step Type:**
- Implementation: `src/PipelineFunc/` directory — follow existing `PipelineDelegateHolder` pattern

**Utilities/Helpers:**
- Shared helpers: `src/Utilities/`

**Tests:**
- All tests: `tests/` (flat, single directory)

## Special Directories

**`src/docs/`:**
- Purpose: NuGet package documentation (NuGet.md)
- Generated: No
- Committed: Yes

**`src/Properties/`:**
- Purpose: Assembly info
- Generated: Auto-generated by SDK
- Committed: No (generated)

**`tests/Properties/`:**
- Purpose: Test assembly info
- Generated: Auto-generated by SDK
- Committed: Yes (`tests/Properties/AssemblyInfo.cs`)

**`.planning/`:**
- Purpose: Planning and analysis documents
- Generated: By tooling
- Committed: Yes

## Public API Surface Areas

**Policy Types (leaf classes):**
- `RetryPolicy` (`src/Retry/RetryPolicy.cs`) — constructors, Handle/HandleAsync (12+ overloads), IncludeError/ExcludeError, WithWait, AddPolicyResultHandler, ThenFallback, AddErrorFilter, WithErrorContextProcessorOf
- `FallbackPolicy` (`src/Fallback/FallbackPolicy.cs`) — constructors, WithFallbackAction/Func, WithAsyncFallbackFunc, Handle/HandleAsync, IncludeError/ExcludeError, AddPolicyResultHandler, WithErrorContextProcessorOf, AddErrorFilter
- `FallbackPolicyBase` (`src/Fallback/FallbackPolicyBase.cs`) — shared base with all Fallback methods (non-`new` versions)
- `FallbackPolicyWithAction` (`src/Fallback/FallbackPolicyWithAction.cs`) — Handle/HandleAsync on FallbackPolicy with action configured
- `FallbackPolicyWithAsyncFunc` (`src/Fallback/FallbackPolicyWithAsyncFunc.cs`) — Handle/HandleAsync on FallbackPolicy with async func configured
- `SimplePolicy` (`src/Simple/SimplePolicy.cs`) — constructors, Handle/HandleAsync, IncludeError/ExcludeError, AddPolicyResultHandler, WithErrorContextProcessorOf, AddErrorFilter, ThenFallback

**TryCatch API:**
- `ITryCatch` (`src/TryCatch/ITryCatch.cs`) — Execute, ExecuteAsync, CatchBlockCount, HasCatchBlockForAll
- `TryCatchBuilder` (`src/TryCatch/TryCatchBuilder.cs`) — CreateAndBuild, CreateFrom, AddCatchBlock, Build
- `CatchBlockHandlerFactory` (`src/CatchBlockHandlers/CatchBlockHandler.cs`) — FilterExceptionsBy, ForAllExceptions

**Composition API:**
- `PolicyCollection` (`src/PolicyComposition/PolicyCollection.cs`) — Create, WithPolicy, HandleDelegate/HandleDelegateAsync, WrapUp, IncludeErrorForAll/ExcludeErrorForAll, AddPolicyResultHandlerForAll/ForLast
- `PolicyDelegate` / `PolicyDelegate<T>` (`src/PolicyDelegate.cs`) — Handle, HandleAsync

**Pipeline API:**
- `PipelineFuncBuilder` (`src/PipelineFunc/PipelineFuncBuilder.cs`) — StartWith, StartWithRetry, StartWithFallback
- `IPipelineFuncStepBuilder<TIn, TMid, TOut>` (`src/PipelineFunc/IPipelineFuncStepBuilder.TIn.TMid.TOut.cs`) — AddStep, AddRetry, AddFallback, Build

**Convenience API:**
- `DelegateInvoking` (`src/DelegateInvoking/DelegateInvoking.cs`) — InvokeWithRetry, InvokeWithFallback, InvokeWithSimple (sync/async/infinite/parameterized variants)

**Building/Config API:**
- `PolicyBuilding` (`src/PolicyBuilding.cs`) — WrapPolicy, WrapPolicyCollection, WrapUp, WithPolicyName
- `OuterPolicyRegistrar<T>` (`src/Wrap/OuterPolicyRegistrar.cs`) — OuterPolicy property

**Result Types:**
- `PolicyResult` / `PolicyResult<T>` (`src/PolicyResult.cs`) — IsSuccess, IsFailed, IsCanceled, NoError, Errors, Result, WrappedPolicyResults, PolicyName, FailedReason
- `TryCatchResult` / `TryCatchResult<T>` (`src/TryCatch/TryCatchResult.cs`)
- `PolicyDelegateCollectionResult` / `PolicyDelegateCollectionResult<T>` (`src/Collections/PolicyDelegateCollectionResult.cs`)
- `PipelineResult` / `PipelineResult<T>` (`src/PipelineFunc/PipelineResult.cs`)

---

*Structure analysis: 2026-06-13*

# Coding Conventions

**Analysis Date:** 2026-06-13

## Naming Patterns

**Classes:**
- PascalCase, descriptive names matching their responsibility: `RetryPolicy`, `FallbackPolicyBase`, `DefaultRetryProcessor`, `PolicyResult`
- Abstract base classes use `Base` suffix: `FallbackPolicyBase`, `PolicyProcessor`, `PolicyDelegateBase`, `PolicyWrapperBase`, `ErrorProcessorBase`
- Default implementations use `Default` prefix: `DefaultRetryProcessor`, `DefaultFallbackProcessor`, `DefaultErrorProcessor`, `DefaultInnerErrorProcessor`
- Partial classes split across files using dot-separated suffixes describing the concern:
  - `RetryPolicy.cs` / `RetryPolicy.WithInnerErrorProcessorOf.cs` / `RetryPolicy.WithTypedErrorProcessor.cs`
  - `DefaultRetryProcessor.cs` / `DefaultRetryProcessor.RetryOverloads.cs` / `DefaultRetryProcessor.RetryAsync.ConfigureAwaitFalse.cs`
  - `PolicyProcessor.cs` / `PolicyProcessor.ExceptionFilter.cs`

**Interfaces:**
- Always use `I` prefix: `IPolicyBase`, `IRetryProcessor`, `IFallbackProcessor`, `IErrorProcessor`, `IBulkErrorProcessor`, `ICanAddErrorProcessor`, `ICanAddErrorFilter<T>`
- Generic constraint interfaces use descriptive suffix: `IWithErrorFilter<T>`, `IWithInnerErrorFilter<T>`, `ICanAddErrorFilter<T>`
- Marker interfaces are acceptable: `IRetryPolicy : IPolicyBase { }`, `IFallbackPolicy : IPolicyBase { }`

**Methods:**
- PascalCase for all public/internal methods: `Handle()`, `HandleAsync()`, `Retry()`, `RetryAsync()`
- Async methods always use `Async` suffix: `HandleAsync()`, `RetryAsync()`, `ProcessAsync()`, `FallbackAsync()`
- Fluent builder methods use `With` prefix: `WithWait()`, `WithFallbackAction()`, `WithAsyncFallbackFunc()`, `WithErrorProcessorOf()`, `WithPolicyName()`
- Extension methods for registration use `Of` suffix: `WithErrorProcessorOf()`, `WithErrorContextProcessorOf()`
- Include/Exclude filter methods: `IncludeError<T>()`, `ExcludeError<T>()`, `IncludeErrorSet<T1, T2>()`, `ExcludeInnerError<T>()`
- Factory methods: `ForSync()`, `ForNotSync()`, `CreateDefault()`, `InfiniteRetries()`

**Properties:**
- PascalCase for public properties: `PolicyName`, `IsFailed`, `IsCanceled`, `NoError`, `IsSuccess`, `Result`
- PascalCase for internal properties: `RetryProcessor`, `RetryInfo`, `Async`, `Status`
- Expression-bodied properties for simple getters: `public bool IsSuccess => !IsFailed && !IsCanceled;`

**Fields:**
- Private/protected fields use `_camelCase` prefix: `_policyName`, `_policyWrapperFactory`, `_fallbackProcessor`, `_bulkErrorProcessor`, `_errors`
- Internal fields also use `_camelCase`: `_executed`, `_unprocessedError`, `_fallback`, `_fallbackAsync`
- Static readonly fields use `_camelCase`: `_retryErrorContextCreator`, `_policyRuleFunc`
- Constants use SCREAMING_SNAKE_CASE: `EXCEPTION_DATA_ERRORFILTERUNSATISFIED_KEY` (see `src/PolinorErrorConsts.cs`)
- Readonly fields are preferred for injected dependencies

**Type Parameters:**
- Single uppercase letter for simple generics: `T`, `TResult`
- Descriptive PascalCase for constrained generics: `TException`, `TInnerException`, `TParam`, `TErrorContext`, `TWrapperPolicy`

**Enums:**
- PascalCase for both enum name and values: `PolicyResultFailedReason.None`, `CancellationType.Precancelable`, `RetryDelayType.Constant`

## Code Style

**Indentation:**
- Use tabs for indentation (not spaces)
- Observe in all source files: `src/Policy.cs`, `src/Retry/RetryPolicy.cs`, etc.

**Braces:**
- Opening brace on same line for namespaces, classes, methods, control flow (K&R style)
- Exception: single-line method bodies may use expression-bodied members or inline on same line
- `if`/`else` blocks always use braces even for single statements

```csharp
public class PolicyResult
{
    public void SetFailed()
    {
        if (!IsFailed)
        {
            IsFailed = true;
        }
    }
}
```

**Spacing:**
- Single blank line between methods
- No blank line after opening brace of method
- Blank line between logical sections within a class

**Expression-bodied members:**
- Used for simple one-line methods, properties, and read-only properties:

```csharp
public bool IsSuccess => !IsFailed && !IsCanceled;
public bool HasFallbackAction() => _fallbackFuncsProvider.HasFallbackAction();
```

**Tuple deconstruction:**
- Used for returning multiple values:

```csharp
var (Act, Wrapper) = WrapDelegateIfNeed(action, token);
```

## Import Organization

**Order:**
1. `System.*` namespaces
2. Third-party namespaces (e.g., `NSubstitute`)
3. Project namespaces (e.g., `PoliNorError.Extensions.PolicyErrorFiltering`)

**No explicit sorting rules** — imports appear in the order needed. No file-scoped namespaces used; always block-scoped:

```csharp
namespace PoliNorError
{
    // ...
}
```

## Access Modifiers

**Pattern:**
- Public API: classes, interfaces, and their public members that form the library surface
- `internal` used extensively for implementation details, processor internals, helpers, and test-accessible members
- `protected` for base class members intended for override
- `private protected` is used where both constraints apply: `private protected FallbackPolicyBase(...)`
- Tests access internals via `InternalsVisibleTo("PoliNorError.Tests")` in `src/PoliNorError.csproj`
- Tests access internal mocks via `InternalsVisibleTo("DynamicProxyGenAssembly2")` for Moq/NSubstitute

**Typical internal members:**
- Processor internals: `internal IRetryProcessor RetryProcessor { get; }`
- Result manipulation: `internal void SetFailed()`, `internal void AddError()`
- Factory methods: `internal static PolicyResult ForSync()`

## Interface Design

**CRTP Pattern (Curiously Recurring Template Pattern):**
- Used extensively for fluent API self-typing:

```csharp
public interface IWithErrorFilter<T> where T : IWithErrorFilter<T>
{
    T IncludeError<TException>(Func<TException, bool> func = null) where TException : Exception;
    T ExcludeError<TException>(Func<TException, bool> func = null) where TException : Exception;
}

public interface ICanAddErrorFilter<T> where T : ICanAddErrorFilter<T>
{
    T AddErrorFilter(NonEmptyCatchBlockFilter filter);
}
```

**Implementation in concrete classes:**
```csharp
public sealed partial class RetryPolicy : Policy, IRetryPolicy, IWithErrorFilter<RetryPolicy>, ...
```

## Extension Method Patterns

**File locations:**
- Policy-level extensions in `src/PolicyExtensions/`: `PolicyErrorProcessorRegistration.cs`
- Policy processor extensions in `src/PolicyProcessorExtensions/`: `ErrorProcessorRegistration.cs`
- Result extensions in `src/PolicyResultExtensions.cs`
- Building/composition in `src/PolicyBuilding.cs`, `src/PolicyAsyncHandling.cs`, `src/PolicyDelegateCreation.cs`

**Naming pattern:**
- Extension classes are `static class` with descriptive names: `PolicyBuilding`, `PolicyAsyncHandling`, `PolicyDelegateCreation`
- Extension methods for the same concept are grouped in a single static class

**Fluent registration extensions:**
- Generic extension methods that delegate to the CRTP-typed interface:

```csharp
public static T WithErrorProcessorOf<T>(this T policy, Action<Exception> actionProcessor) where T : ICanAddErrorProcessor
{
    policy.WithErrorProcessor(new ErrorProcessor(actionProcessor));
    return policy;
}
```

**Fluent builder extensions that return `this`:**
```csharp
public RetryPolicy WithWait(TimeSpan delay)
{
    this.WithErrorProcessor(new DelayErrorProcessor(delay));
    return this;
}
```

## XML Documentation

**Approach:**
- `<GenerateDocumentationFile>true</GenerateDocumentationFile>` in `src/PoliNorError.csproj`
- Warning 1591 (missing XML comment) is suppressed in both Debug and Release
- XML docs are present on public API members but not consistently on all public members
- Inheritdoc used to avoid duplication: `///<inheritdoc cref = "PolicyResultHandlerRegistration.SetPolicyResultFailedIfInner{RetryPolicy}"/>`
- `<see cref="..."/>` and `<typeparamref name="..."/>` used for cross-references
- `<summary>`, `<param>`, `<returns>`, `<remarks>`, `<list>` tags are all used
- Parameter docs often omitted when self-evident

**Typical documented members:**
- Public class constructors
- Key public methods (Handle, HandleAsync)
- Public properties (IsFailed, IsCanceled, etc.)
- Enum values

## Async Patterns

**Core convention:**
- All async methods return `Task` or `Task<T>`
- Async method names always end with `Async`: `HandleAsync()`, `RetryAsync()`, `ProcessAsync()`, `FallbackAsync()`
- `CancellationToken` parameter always comes last with `= default`
- `configureAwait` parameter (bool, defaults to `false`) is always passed through to `ConfigureAwait()`:

```csharp
public async Task<PolicyResult> HandleAsync(Func<CancellationToken, Task> func, bool configureAwait = false, CancellationToken token = default)
{
    // ...
    retryResult = await RetryProcessor.RetryAsync(Fn, RetryInfo, configureAwait, token).ConfigureAwait(configureAwait);
    await HandlePolicyResultAsync(retryResult, configureAwait, token).ConfigureAwait(configureAwait);
    return retryResult;
}
```

- Some methods have convenience overloads without `configureAwait` that call the full overload with `false`:

```csharp
public Task<PolicyResult> HandleAsync<TParam>(Func<TParam, CancellationToken, Task> func, TParam param, CancellationToken token)
{
    return HandleAsync(func, param, false, token);
}
```

**Overload pattern:**
- Sync and async versions exist side-by-side
- Overloads for: `Action`, `Func<T>`, `Action<TParam>`, `Func<TParam, T>`, plus error-context variants
- Async delegates always take `CancellationToken` as parameter: `Func<CancellationToken, Task>`

## Error Handling Within the Library

**Policy execution pattern:**
1. Check for null delegate → return `PolicyResult` with `NoDelegateException`
2. Check for early cancellation → return canceled `PolicyResult`
3. Execute delegate within catch block
4. Process errors through `BulkErrorProcessor`
5. Apply error filters (include/exclude)
6. Store results in `PolicyResult`
7. Run `PolicyResultHandler`s

**Exception types:**
- `NoDelegateException` — thrown when delegate is null (`src/Exceptions/NoDelegateException.cs`)
- `CatchBlockException` — wraps exceptions from catch block processing (`src/Exceptions/CatchBlockException.cs`)
- `PolicyResultHandlingException` — wraps exceptions from result handlers (`src/Exceptions/PolicyResultHandlingException.cs`)
- `InconsistencyPolicyException` — policy configuration errors

**ThrowHelper pattern:**
- `ThrowHelper.ThrowIfNotImplemented()` used for processor type validation:

```csharp
private void ThrowIfProcessorIsNotDefault(out DefaultRetryProcessor proc)
{
    ThrowHelper.ThrowIfNotImplemented(RetryProcessor, out proc);
}
```

## Partial Class Usage

**Pattern:**
- Large policy/processor classes are split across multiple files using `partial class`
- Each file focuses on a specific concern, named with dot-separated suffixes:
  - `src/Retry/RetryPolicy.cs` — main class with Handle methods
  - `src/Retry/RetryPolicy.WithInnerErrorProcessorOf.cs` — inner error processor registration
  - `src/Retry/RetryPolicy.WithTypedErrorProcessor.cs` — typed error processor registration
  - `src/Retry/RetryPolicyCustomErrorSaverRegistration.cs` — custom error saver
  - `src/Fallback/FallbackPolicy.WithInnerErrorProcessorOf.cs` — inner error processor
  - `src/Fallback/FallbackPolicy.WithTypedErrorProcessor.cs` — typed error processor
  - `src/PolicyProcessor.cs` — base class
  - `src/ExceptionFilter/PolicyProcessor.ExceptionFilter.cs` — filter nested class

## Decorator / Wrapper Pattern for Error Filtering

**Pattern for IncludeError/ExcludeError:**
- Extension methods that wrap the policy's processor's `ErrorFilter`:

```csharp
public RetryPolicy ExcludeError<TException>(Func<TException, bool> func = null) where TException : Exception
    => this.ExcludeError<RetryPolicy, TException>(func);
```

- Generic extension methods on `IWithErrorFilter<T>` handle the actual filter registration
- This keeps the concrete policy classes thin while reusing filter logic

---

*Convention analysis: 2026-06-13*

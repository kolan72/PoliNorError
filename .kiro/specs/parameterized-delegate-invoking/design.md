# Design Document: Parameterized Delegate Invoking

## Overview

This feature extends the `DelegateInvoking` static class with new extension methods that support two distinct parameter-passing patterns already implemented in the underlying policy classes:

1. **Parameterized delegates**: Extension methods for `Action<TParam>` and `Func<TParam, T>` delegates where the parameter is passed directly to the delegate
2. **Context-dependent execution**: Extension methods for non-parameterized delegates (`Action`, `Func<T>`) with a `TErrorContext` parameter that is made available to error processors but not passed to the delegate

The design maintains consistency with existing API patterns while providing convenient access to the full capabilities of `SimplePolicy`, `RetryPolicy`, and `FallbackPolicyBase`.

### Design Goals

- **API Consistency**: Follow the same naming conventions, parameter ordering, and return types as existing non-parameterized extension methods
- **Type Safety**: Leverage C#'s generic type system to ensure compile-time correctness
- **Minimal Duplication**: Reuse existing policy creation helpers and Handle/HandleAsync methods
- **Discoverability**: Make parameterized and context-dependent patterns easily discoverable through IntelliSense
- **Completeness**: Cover all policy types (Simple, Retry, Infinite Retry, Fallback) for both sync and async execution

## Architecture

### Component Structure

The implementation follows the existing partial class architecture:

```
DelegateInvoking (partial class)
├── DelegateInvoking.cs                    // Action methods (non-returning)
├── DelegateInvoking.T.cs                  // Func<T> methods (returning)
├── DelegateInvoking.WithRetryDelay.cs     // Action with RetryDelay
├── DelegateInvoking.WithRetryDelay.T.cs   // Func<T> with RetryDelay
└── [New files for parameterized variants]
```

### Extension Method Pattern

Each extension method follows this pattern:

1. Accept the delegate as the first parameter (extension method target)
2. Accept the parameter value (TParam or TErrorContext) as the second parameter
3. Accept policy-specific parameters (retryCount, delay, fallback, etc.)
4. Accept optional configuration parameters (ErrorProcessorParam, configureAwait, etc.)
5. Accept CancellationToken as the final parameter
6. Create the appropriate policy using existing helper methods
7. Invoke the policy's Handle or HandleAsync method with the delegate and parameter
8. Return the PolicyResult or PolicyResult<T>

### Method Naming Convention

Extension methods follow the existing naming pattern:
- `InvokeWithSimple` - Simple policy
- `InvokeWithRetry` - Retry policy with count
- `InvokeWithWaitAndRetry` - Retry policy with delay
- `InvokeWithRetryInfinite` - Infinite retry without delay
- `InvokeWithWaitAndRetryInfinite` - Infinite retry with delay
- `InvokeWithFallback` - Fallback policy
- Async variants append `Async` suffix

## Components and Interfaces

### File Organization

#### DelegateInvoking.Parameterized.cs
Contains extension methods for parameterized `Action<TParam>` delegates:
- `InvokeWithSimple<TParam>(Action<TParam>, TParam, ...)`
- `InvokeWithRetry<TParam>(Action<TParam>, TParam, int retryCount, ...)`
- `InvokeWithWaitAndRetry<TParam>(Action<TParam>, TParam, int retryCount, TimeSpan delay, ...)`
- `InvokeWithWaitAndRetry<TParam>(Action<TParam>, TParam, int retryCount, Func<int, Exception, TimeSpan>, ...)`
- `InvokeWithRetryInfinite<TParam>(Action<TParam>, TParam, ...)`
- `InvokeWithWaitAndRetryInfinite<TParam>(Action<TParam>, TParam, TimeSpan delay, ...)`
- `InvokeWithWaitAndRetryInfinite<TParam>(Action<TParam>, TParam, Func<int, Exception, TimeSpan>, ...)`
- `InvokeWithFallback<TParam>(Action<TParam>, TParam, Action<TParam> fallback, ...)`

#### DelegateInvoking.Parameterized.T.cs
Contains extension methods for parameterized `Func<TParam, T>` delegates:
- `InvokeWithSimple<TParam, T>(Func<TParam, T>, TParam, ...)`
- `InvokeWithRetry<TParam, T>(Func<TParam, T>, TParam, int retryCount, ...)`
- `InvokeWithWaitAndRetry<TParam, T>(Func<TParam, T>, TParam, int retryCount, TimeSpan delay, ...)`
- `InvokeWithWaitAndRetry<TParam, T>(Func<TParam, T>, TParam, int retryCount, Func<int, Exception, TimeSpan>, ...)`
- `InvokeWithRetryInfinite<TParam, T>(Func<TParam, T>, TParam, ...)`
- `InvokeWithWaitAndRetryInfinite<TParam, T>(Func<TParam, T>, TParam, TimeSpan delay, ...)`
- `InvokeWithWaitAndRetryInfinite<TParam, T>(Func<TParam, T>, TParam, Func<int, Exception, TimeSpan>, ...)`
- `InvokeWithFallback<TParam, T>(Func<TParam, T>, TParam, Func<TParam, T> fallback, ...)`

#### DelegateInvoking.Parameterized.Async.cs
Contains extension methods for parameterized async `Func<TParam, CancellationToken, Task>` delegates:
- `InvokeWithSimpleAsync<TParam>(Func<TParam, CancellationToken, Task>, TParam, ...)`
- `InvokeWithRetryAsync<TParam>(Func<TParam, CancellationToken, Task>, TParam, int retryCount, ...)`
- `InvokeWithWaitAndRetryAsync<TParam>(Func<TParam, CancellationToken, Task>, TParam, int retryCount, TimeSpan delay, ...)`
- `InvokeWithWaitAndRetryAsync<TParam>(Func<TParam, CancellationToken, Task>, TParam, int retryCount, Func<int, Exception, TimeSpan>, ...)`
- `InvokeWithRetryInfiniteAsync<TParam>(Func<TParam, CancellationToken, Task>, TParam, ...)`
- `InvokeWithWaitAndRetryInfiniteAsync<TParam>(Func<TParam, CancellationToken, Task>, TParam, TimeSpan delay, ...)`
- `InvokeWithWaitAndRetryInfiniteAsync<TParam>(Func<TParam, CancellationToken, Task>, TParam, Func<int, Exception, TimeSpan>, ...)`
- `InvokeWithFallbackAsync<TParam>(Func<TParam, CancellationToken, Task>, TParam, Func<TParam, CancellationToken, Task> fallback, ...)`

#### DelegateInvoking.Parameterized.Async.T.cs
Contains extension methods for parameterized async `Func<TParam, CancellationToken, Task<T>>` delegates:
- `InvokeWithSimpleAsync<TParam, T>(Func<TParam, CancellationToken, Task<T>>, TParam, ...)`
- `InvokeWithRetryAsync<TParam, T>(Func<TParam, CancellationToken, Task<T>>, TParam, int retryCount, ...)`
- `InvokeWithWaitAndRetryAsync<TParam, T>(Func<TParam, CancellationToken, Task<T>>, TParam, int retryCount, TimeSpan delay, ...)`
- `InvokeWithWaitAndRetryAsync<TParam, T>(Func<TParam, CancellationToken, Task<T>>, TParam, int retryCount, Func<int, Exception, TimeSpan>, ...)`
- `InvokeWithRetryInfiniteAsync<TParam, T>(Func<TParam, CancellationToken, Task<T>>, TParam, ...)`
- `InvokeWithWaitAndRetryInfiniteAsync<TParam, T>(Func<TParam, CancellationToken, Task<T>>, TParam, TimeSpan delay, ...)`
- `InvokeWithWaitAndRetryInfiniteAsync<TParam, T>(Func<TParam, CancellationToken, Task<T>>, TParam, Func<int, Exception, TimeSpan>, ...)`
- `InvokeWithFallbackAsync<TParam, T>(Func<TParam, CancellationToken, Task<T>>, TParam, Func<TParam, CancellationToken, Task<T>> fallback, ...)`

#### DelegateInvoking.ErrorContext.cs
Contains extension methods for context-dependent `Action` delegates:
- `InvokeWithSimple<TErrorContext>(Action, TErrorContext, ...)`
- `InvokeWithRetry<TErrorContext>(Action, TErrorContext, int retryCount, ...)`
- `InvokeWithWaitAndRetry<TErrorContext>(Action, TErrorContext, int retryCount, TimeSpan delay, ...)`
- `InvokeWithWaitAndRetry<TErrorContext>(Action, TErrorContext, int retryCount, Func<int, Exception, TimeSpan>, ...)`
- `InvokeWithRetryInfinite<TErrorContext>(Action, TErrorContext, ...)`
- `InvokeWithWaitAndRetryInfinite<TErrorContext>(Action, TErrorContext, TimeSpan delay, ...)`
- `InvokeWithWaitAndRetryInfinite<TErrorContext>(Action, TErrorContext, Func<int, Exception, TimeSpan>, ...)`
- `InvokeWithFallback<TErrorContext>(Action, TErrorContext, Action<CancellationToken> fallback, ...)`

#### DelegateInvoking.ErrorContext.T.cs
Contains extension methods for context-dependent `Func<T>` delegates:
- `InvokeWithSimple<TErrorContext, T>(Func<T>, TErrorContext, ...)`
- `InvokeWithRetry<TErrorContext, T>(Func<T>, TErrorContext, int retryCount, ...)`
- `InvokeWithWaitAndRetry<TErrorContext, T>(Func<T>, TErrorContext, int retryCount, TimeSpan delay, ...)`
- `InvokeWithWaitAndRetry<TErrorContext, T>(Func<T>, TErrorContext, int retryCount, Func<int, Exception, TimeSpan>, ...)`
- `InvokeWithRetryInfinite<TErrorContext, T>(Func<T>, TErrorContext, ...)`
- `InvokeWithWaitAndRetryInfinite<TErrorContext, T>(Func<T>, TErrorContext, TimeSpan delay, ...)`
- `InvokeWithWaitAndRetryInfinite<TErrorContext, T>(Func<T>, TErrorContext, Func<int, Exception, TimeSpan>, ...)`
- `InvokeWithFallback<TErrorContext, T>(Func<T>, TErrorContext, Func<CancellationToken, T> fallback, ...)`

#### DelegateInvoking.ErrorContext.Async.cs
Contains extension methods for context-dependent async `Func<CancellationToken, Task>` delegates:
- `InvokeWithSimpleAsync<TErrorContext>(Func<CancellationToken, Task>, TErrorContext, ...)`
- `InvokeWithRetryAsync<TErrorContext>(Func<CancellationToken, Task>, TErrorContext, int retryCount, ...)`
- `InvokeWithWaitAndRetryAsync<TErrorContext>(Func<CancellationToken, Task>, TErrorContext, int retryCount, TimeSpan delay, ...)`
- `InvokeWithWaitAndRetryAsync<TErrorContext>(Func<CancellationToken, Task>, TErrorContext, int retryCount, Func<int, Exception, TimeSpan>, ...)`
- `InvokeWithRetryInfiniteAsync<TErrorContext>(Func<CancellationToken, Task>, TErrorContext, ...)`
- `InvokeWithWaitAndRetryInfiniteAsync<TErrorContext>(Func<CancellationToken, Task>, TErrorContext, TimeSpan delay, ...)`
- `InvokeWithWaitAndRetryInfiniteAsync<TErrorContext>(Func<CancellationToken, Task>, TErrorContext, Func<int, Exception, TimeSpan>, ...)`
- `InvokeWithFallbackAsync<TErrorContext>(Func<CancellationToken, Task>, TErrorContext, Func<CancellationToken, Task> fallback, ...)`

#### DelegateInvoking.ErrorContext.Async.T.cs
Contains extension methods for context-dependent async `Func<CancellationToken, Task<T>>` delegates:
- `InvokeWithSimpleAsync<TErrorContext, T>(Func<CancellationToken, Task<T>>, TErrorContext, ...)`
- `InvokeWithRetryAsync<TErrorContext, T>(Func<CancellationToken, Task<T>>, TErrorContext, int retryCount, ...)`
- `InvokeWithWaitAndRetryAsync<TErrorContext, T>(Func<CancellationToken, Task<T>>, TErrorContext, int retryCount, TimeSpan delay, ...)`
- `InvokeWithWaitAndRetryAsync<TErrorContext, T>(Func<CancellationToken, Task<T>>, TErrorContext, int retryCount, Func<int, Exception, TimeSpan>, ...)`
- `InvokeWithRetryInfiniteAsync<TErrorContext, T>(Func<CancellationToken, Task<T>>, TErrorContext, ...)`
- `InvokeWithWaitAndRetryInfiniteAsync<TErrorContext, T>(Func<CancellationToken, Task<T>>, TErrorContext, TimeSpan delay, ...)`
- `InvokeWithWaitAndRetryInfiniteAsync<TErrorContext, T>(Func<CancellationToken, Task<T>>, TErrorContext, Func<int, Exception, TimeSpan>, ...)`
- `InvokeWithFallbackAsync<TErrorContext, T>(Func<CancellationToken, Task<T>>, TErrorContext, Func<CancellationToken, Task<T>> fallback, ...)`

### Method Overload Strategy

Each extension method family provides multiple overloads to support different use cases:

1. **Minimal overload**: Only required parameters (delegate, param, policy-specific params, token)
2. **ErrorProcessorParam overload**: Adds ErrorProcessorParam for custom error processing
3. **ConfigureAwait overload** (async only): Adds bool configureAwait parameter
4. **CatchBlockFilter overload** (Simple policy only): Adds CatchBlockFilter parameter
5. **CatchBlockHandler overload** (Simple policy only): Accepts CatchBlockHandler instead of separate parameters
6. **CancellationType overload** (Fallback policy only): Adds CancellationType parameter

### Type Parameter Conventions

- `TParam`: The parameter type for parameterized delegates (passed to the delegate)
- `TErrorContext`: The error context type for context-dependent execution (passed to error processors, not to the delegate)
- `T`: The return type for Func delegates

## Data Models

### Delegate Signatures

#### Parameterized Delegates
```csharp
// Synchronous
Action<TParam>
Func<TParam, T>

// Asynchronous
Func<TParam, CancellationToken, Task>
Func<TParam, CancellationToken, Task<T>>
```

#### Context-Dependent Delegates
```csharp
// Synchronous (non-parameterized delegates with error context)
Action
Func<T>

// Asynchronous (non-parameterized delegates with error context)
Func<CancellationToken, Task>
Func<CancellationToken, Task<T>>
```

### Fallback Delegate Signatures

For parameterized fallback methods, the fallback delegate must have the same signature as the primary delegate:

```csharp
// Parameterized fallback
Action<TParam> fallback
Func<TParam, T> fallback
Func<TParam, CancellationToken, Task> fallback
Func<TParam, CancellationToken, Task<T>> fallback
```

For context-dependent fallback methods, the fallback delegate follows the existing pattern:

```csharp
// Context-dependent fallback (existing pattern)
Action<CancellationToken> fallback
Func<CancellationToken, T> fallback
Func<CancellationToken, Task> fallback
Func<CancellationToken, Task<T>> fallback
```

### Return Types

All extension methods return the same types as their non-parameterized counterparts:
- `PolicyResult` for Action delegates
- `PolicyResult<T>` for Func<T> delegates

## Error Handling

### Error Context Availability

For context-dependent execution methods:
- The `TErrorContext` parameter is passed to the policy's `Handle<TErrorContext>` or `HandleAsync<TErrorContext>` method
- Error processors can access the context through their error handling callbacks
- The context is NOT passed to the delegate being executed
- This allows error processors to have additional context without changing the delegate signature

### Error Processing Flow

1. Extension method receives delegate and parameters
2. Policy is created using existing helper methods (ToRetryPolicy, ToSimplePolicy, ToFallbackPolicy)
3. Policy's Handle or HandleAsync method is invoked with the delegate and parameter
4. If an error occurs:
   - For parameterized delegates: Error processors receive the exception and can access policy configuration
   - For context-dependent execution: Error processors receive the exception and the TErrorContext parameter
5. Policy executes its error handling logic (retry, fallback, or simple error processing)
6. PolicyResult is returned with success/failure status and any captured errors

### Exception Propagation

Exception handling follows the existing patterns:
- Exceptions are caught by the policy's error processor
- The PolicyResult captures all exceptions in its Errors collection
- The IsFailed property indicates whether the operation ultimately failed
- Exceptions are not re-thrown unless the policy is configured to do so

## Testing Strategy

### Unit Testing Approach

The testing strategy uses **example-based unit tests** rather than property-based testing because:

1. **API Surface Testing**: We are testing that extension methods correctly delegate to existing policy Handle methods, not testing complex algorithmic properties
2. **Integration Points**: The focus is on verifying that parameters are passed correctly through the call chain
3. **Existing Policy Coverage**: The underlying policy classes already have comprehensive tests; we only need to verify the extension method layer

### Test Coverage Requirements

Each extension method requires tests for:

1. **Basic Invocation**: Verify the method can be called and returns a PolicyResult
2. **Parameter Passing**: Verify the parameter is correctly passed to the delegate (for parameterized) or to error processors (for context-dependent)
3. **Policy Creation**: Verify the correct policy type is created with the correct configuration
4. **Error Handling**: Verify errors are captured in the PolicyResult
5. **Cancellation**: Verify CancellationToken is respected
6. **Async Behavior**: Verify async methods properly await and return results

### Test Organization

Tests should be organized by:
- Policy type (Simple, Retry, Fallback)
- Delegate type (Action, Func, async variants)
- Parameter pattern (parameterized vs context-dependent)

Example test structure:
```
Tests/
├── DelegateInvoking.Parameterized.Tests.cs
├── DelegateInvoking.Parameterized.T.Tests.cs
├── DelegateInvoking.Parameterized.Async.Tests.cs
├── DelegateInvoking.Parameterized.Async.T.Tests.cs
├── DelegateInvoking.ErrorContext.Tests.cs
├── DelegateInvoking.ErrorContext.T.Tests.cs
├── DelegateInvoking.ErrorContext.Async.Tests.cs
└── DelegateInvoking.ErrorContext.Async.T.Tests.cs
```

### Example Test Cases

#### Parameterized Delegate Test
```csharp
[Test]
public void InvokeWithSimple_ActionWithParam_PassesParameterToDelegate()
{
    // Arrange
    int receivedParam = 0;
    Action<int> action = (param) => receivedParam = param;
    int expectedParam = 42;

    // Act
    var result = action.InvokeWithSimple(expectedParam);

    // Assert
    Assert.That(result.IsSuccess, Is.True);
    Assert.That(receivedParam, Is.EqualTo(expectedParam));
}
```

#### Context-Dependent Execution Test
```csharp
[Test]
public void InvokeWithRetry_WithErrorContext_PassesContextToErrorProcessor()
{
    // Arrange
    string capturedContext = null;
    Action action = () => throw new Exception("Test error");
    string errorContext = "TestContext";
    var errorProcessor = new ErrorProcessorParam((ex, ctx) => {
        capturedContext = ctx as string;
    });

    // Act
    var result = action.InvokeWithRetry(errorContext, 1, errorProcessor);

    // Assert
    Assert.That(result.IsFailed, Is.True);
    Assert.That(capturedContext, Is.EqualTo(errorContext));
}
```

#### Retry with Parameter Test
```csharp
[Test]
public void InvokeWithRetry_FuncWithParam_RetriesWithSameParameter()
{
    // Arrange
    int attemptCount = 0;
    int receivedParam = 0;
    Func<int, string> func = (param) => {
        receivedParam = param;
        attemptCount++;
        if (attemptCount < 2) throw new Exception("Retry");
        return "Success";
    };
    int expectedParam = 42;

    // Act
    var result = func.InvokeWithRetry(expectedParam, 2);

    // Assert
    Assert.That(result.IsSuccess, Is.True);
    Assert.That(result.Result, Is.EqualTo("Success"));
    Assert.That(attemptCount, Is.EqualTo(2));
    Assert.That(receivedParam, Is.EqualTo(expectedParam));
}
```

#### Fallback with Parameter Test
```csharp
[Test]
public void InvokeWithFallback_ActionWithParam_UsesFallbackOnError()
{
    // Arrange
    bool fallbackCalled = false;
    int fallbackParam = 0;
    Action<int> action = (param) => throw new Exception("Primary failed");
    Action<int> fallback = (param) => {
        fallbackCalled = true;
        fallbackParam = param;
    };
    int expectedParam = 42;

    // Act
    var result = action.InvokeWithFallback(expectedParam, fallback);

    // Assert
    Assert.That(result.IsSuccess, Is.True);
    Assert.That(fallbackCalled, Is.True);
    Assert.That(fallbackParam, Is.EqualTo(expectedParam));
}
```

### Test Execution

- All tests should be runnable with the existing test framework (NUnit based on the codebase)
- Tests should be independent and not rely on shared state
- Tests should use mocking where appropriate to isolate the extension method layer from policy implementation details
- Async tests should properly handle Task completion and cancellation

## Implementation Notes

### Code Reuse

The implementation maximizes code reuse by:
1. Using existing policy creation helper methods (ToRetryPolicy, ToSimplePolicy, ToFallbackPolicy)
2. Delegating to existing Handle and HandleAsync methods on policy classes
3. Following the same overload chaining pattern as existing extension methods

### Performance Considerations

- Extension methods add minimal overhead (single method call)
- No additional allocations beyond what the underlying policy methods already perform
- Generic type parameters are resolved at compile time

### Backward Compatibility

This feature is purely additive:
- No existing APIs are modified
- No breaking changes to existing code
- New extension methods are discoverable through IntelliSense but don't interfere with existing usage patterns

### Future Extensibility

The design supports future extensions:
- Additional policy types can follow the same pattern
- New parameter patterns can be added without breaking existing code
- The partial class structure allows for easy addition of new file groups

# Requirements Document

## Introduction

This feature adds parameterized delegate invoking extension methods to the DelegateInvoking class. The policy classes (SimplePolicy, RetryPolicy, FallbackPolicyBase) already support two distinct patterns for passing parameters:

1. **Parameterized delegates**: Handle and HandleAsync methods that accept Action<TParam> and Func<TParam, T> delegates along with a parameter value that is passed to the delegate itself
2. **Context-dependent execution**: Handle<TErrorContext> and HandleAsync<TErrorContext> methods that accept non-parameterized delegates (Action, Func<T>) along with an error context parameter that is made available to error processors but not passed to the delegate

However, there are no corresponding convenience extension methods in the DelegateInvoking class to invoke these methods directly.

This feature will add extension methods that allow developers to invoke both parameterized delegates and context-dependent execution with Simple, Retry, and Fallback policies using a fluent API, matching the existing pattern for non-parameterized delegates.

## Glossary

- **DelegateInvoking**: A static partial class containing extension methods for invoking delegates with resilience policies
- **SimplePolicy**: A policy that executes delegates with basic error handling
- **RetryPolicy**: A policy that retries delegate execution on failure
- **FallbackPolicy**: A policy that provides fallback behavior when delegate execution fails
- **Parameterized_Delegate**: A delegate that accepts one or more parameters (Action<TParam> or Func<TParam, T>)
- **Error_Context**: A parameter passed to policy Handle methods that provides contextual information to error processors without being passed to the delegate itself
- **Extension_Method**: A static method that extends the functionality of a type without modifying it
- **Policy_Handle_Method**: The Handle or HandleAsync method on a policy class that executes a delegate

## Requirements

### Requirement 1: Add Parameterized Simple Policy Extension Methods

**User Story:** As a developer, I want to invoke parameterized delegates with SimplePolicy, so that I can apply simple error handling to delegates that require parameters.

#### Acceptance Criteria

1. THE DelegateInvoking SHALL provide an InvokeWithSimple extension method for Action<TParam> delegates
2. THE DelegateInvoking SHALL provide an InvokeWithSimple extension method for Func<TParam, T> delegates
3. THE DelegateInvoking SHALL provide an InvokeWithSimpleAsync extension method for Func<TParam, CancellationToken, Task> delegates
4. THE DelegateInvoking SHALL provide an InvokeWithSimpleAsync extension method for Func<TParam, CancellationToken, Task<T>> delegates
5. WHEN a parameterized Simple extension method is called, THE DelegateInvoking SHALL create a SimplePolicy and invoke its corresponding Handle or HandleAsync method with the delegate and parameter
6. THE parameterized Simple extension methods SHALL accept optional ErrorProcessorParam, CatchBlockFilter, and CatchBlockHandler parameters consistent with existing non-parameterized methods
7. THE async parameterized Simple extension methods SHALL accept an optional configureAwait parameter

### Requirement 2: Add Parameterized Retry Policy Extension Methods

**User Story:** As a developer, I want to invoke parameterized delegates with RetryPolicy, so that I can apply retry logic to delegates that require parameters.

#### Acceptance Criteria

1. THE DelegateInvoking SHALL provide InvokeWithRetry extension methods for Action<TParam> delegates
2. THE DelegateInvoking SHALL provide InvokeWithRetry extension methods for Func<TParam, T> delegates
3. THE DelegateInvoking SHALL provide InvokeWithRetryAsync extension methods for Func<TParam, CancellationToken, Task> delegates
4. THE DelegateInvoking SHALL provide InvokeWithRetryAsync extension methods for Func<TParam, CancellationToken, Task<T>> delegates
5. THE DelegateInvoking SHALL provide InvokeWithWaitAndRetry extension methods for Action<TParam> and Func<TParam, T> delegates with TimeSpan delay parameter
6. THE DelegateInvoking SHALL provide InvokeWithWaitAndRetry extension methods for Action<TParam> and Func<TParam, T> delegates with Func<int, Exception, TimeSpan> retry function parameter
7. THE DelegateInvoking SHALL provide InvokeWithWaitAndRetryAsync extension methods for Func<TParam, CancellationToken, Task> and Func<TParam, CancellationToken, Task<T>> delegates with TimeSpan delay parameter
8. THE DelegateInvoking SHALL provide InvokeWithWaitAndRetryAsync extension methods for Func<TParam, CancellationToken, Task> and Func<TParam, CancellationToken, Task<T>> delegates with Func<int, Exception, TimeSpan> retry function parameter
9. WHEN a parameterized Retry extension method is called, THE DelegateInvoking SHALL create a RetryPolicy and invoke its corresponding Handle or HandleAsync method with the delegate and parameter
10. THE parameterized Retry extension methods SHALL accept optional retryCount, ErrorProcessorParam, failedIfSaveErrorThrows, and RetryErrorSaverParam parameters consistent with existing non-parameterized methods
11. THE async parameterized Retry extension methods SHALL accept an optional configureAwait parameter

### Requirement 3: Add Parameterized Infinite Retry Policy Extension Methods

**User Story:** As a developer, I want to invoke parameterized delegates with infinite retry policies, so that I can apply infinite retry logic to delegates that require parameters.

#### Acceptance Criteria

1. THE DelegateInvoking SHALL provide InvokeWithRetryInfinite extension methods for Action<TParam> and Func<TParam, T> delegates
2. THE DelegateInvoking SHALL provide InvokeWithRetryInfiniteAsync extension methods for Func<TParam, CancellationToken, Task> and Func<TParam, CancellationToken, Task<T>> delegates
3. THE DelegateInvoking SHALL provide InvokeWithWaitAndRetryInfinite extension methods for Action<TParam> and Func<TParam, T> delegates with TimeSpan delay parameter
4. THE DelegateInvoking SHALL provide InvokeWithWaitAndRetryInfinite extension methods for Action<TParam> and Func<TParam, T> delegates with Func<int, Exception, TimeSpan> retry function parameter
5. THE DelegateInvoking SHALL provide InvokeWithWaitAndRetryInfiniteAsync extension methods for Func<TParam, CancellationToken, Task> and Func<TParam, CancellationToken, Task<T>> delegates with TimeSpan delay parameter
6. THE DelegateInvoking SHALL provide InvokeWithWaitAndRetryInfiniteAsync extension methods for Func<TParam, CancellationToken, Task> and Func<TParam, CancellationToken, Task<T>> delegates with Func<int, Exception, TimeSpan> retry function parameter
7. WHEN a parameterized infinite Retry extension method is called, THE DelegateInvoking SHALL create an infinite RetryPolicy and invoke its corresponding Handle or HandleAsync method with the delegate and parameter
8. THE parameterized infinite Retry extension methods SHALL accept optional ErrorProcessorParam, failedIfSaveErrorThrows, and RetryErrorSaverParam parameters consistent with existing non-parameterized methods
9. THE async parameterized infinite Retry extension methods SHALL accept an optional configureAwait parameter

### Requirement 4: Add Parameterized Fallback Policy Extension Methods

**User Story:** As a developer, I want to invoke parameterized delegates with FallbackPolicy, so that I can provide fallback behavior for delegates that require parameters.

#### Acceptance Criteria

1. THE DelegateInvoking SHALL provide InvokeWithFallback extension methods for Action<TParam> delegates with Action<TParam> fallback parameter
2. THE DelegateInvoking SHALL provide InvokeWithFallback extension methods for Func<TParam, T> delegates with Func<TParam, T> fallback parameter
3. THE DelegateInvoking SHALL provide InvokeWithFallbackAsync extension methods for Func<TParam, CancellationToken, Task> delegates with Func<TParam, CancellationToken, Task> fallback parameter
4. THE DelegateInvoking SHALL provide InvokeWithFallbackAsync extension methods for Func<TParam, CancellationToken, Task<T>> delegates with Func<TParam, CancellationToken, Task<T>> fallback parameter
5. WHEN a parameterized Fallback extension method is called, THE DelegateInvoking SHALL create a FallbackPolicy and invoke its corresponding Handle or HandleAsync method with the delegate and parameter
6. THE parameterized Fallback extension methods SHALL accept optional ErrorProcessorParam and CancellationType parameters consistent with existing non-parameterized methods
7. THE async parameterized Fallback extension methods SHALL accept an optional configureAwait parameter

### Requirement 5: Maintain Consistency with Existing API Patterns

**User Story:** As a developer, I want the new parameterized and context-dependent extension methods to follow the same patterns as existing methods, so that the API is consistent and predictable.

#### Acceptance Criteria

1. THE parameterized and context-dependent extension methods SHALL follow the same naming convention as existing non-parameterized methods (InvokeWith[PolicyType])
2. THE parameterized and context-dependent extension methods SHALL accept the same optional parameters as their non-parameterized counterparts in the same order
3. THE parameterized and context-dependent extension methods SHALL return the same PolicyResult or PolicyResult<T> types as their non-parameterized counterparts
4. THE parameterized and context-dependent extension methods SHALL be placed in the same DelegateInvoking partial class files as their non-parameterized counterparts (DelegateInvoking.cs for Action methods, DelegateInvoking.T.cs for Func<T> methods)
5. THE parameterized and context-dependent extension methods SHALL use the same policy creation helper methods (ToRetryPolicy, ToSimplePolicy, ToFallbackPolicy) as existing methods
6. FOR ALL parameterized extension methods, calling the method SHALL produce equivalent behavior to manually creating the policy and calling its Handle or HandleAsync method with the same parameters
7. FOR ALL context-dependent extension methods, calling the method SHALL produce equivalent behavior to manually creating the policy and calling its Handle<TErrorContext> or HandleAsync<TErrorContext> method with the same parameters

### Requirement 6: Add Context-Dependent Execution Extension Methods

**User Story:** As a developer, I want to invoke non-parameterized delegates with an error context parameter, so that I can pass contextual information to error processors without modifying the delegate signature.

#### Acceptance Criteria

1. THE DelegateInvoking SHALL provide InvokeWithSimple extension methods for Action delegates that accept a TErrorContext parameter
2. THE DelegateInvoking SHALL provide InvokeWithSimple extension methods for Func<T> delegates that accept a TErrorContext parameter
3. THE DelegateInvoking SHALL provide InvokeWithSimpleAsync extension methods for Func<CancellationToken, Task> delegates that accept a TErrorContext parameter
4. THE DelegateInvoking SHALL provide InvokeWithSimpleAsync extension methods for Func<CancellationToken, Task<T>> delegates that accept a TErrorContext parameter
5. THE DelegateInvoking SHALL provide InvokeWithRetry extension methods for Action and Func<T> delegates that accept a TErrorContext parameter
6. THE DelegateInvoking SHALL provide InvokeWithRetryAsync extension methods for Func<CancellationToken, Task> and Func<CancellationToken, Task<T>> delegates that accept a TErrorContext parameter
7. THE DelegateInvoking SHALL provide InvokeWithWaitAndRetry extension methods for Action and Func<T> delegates that accept a TErrorContext parameter with TimeSpan delay
8. THE DelegateInvoking SHALL provide InvokeWithWaitAndRetry extension methods for Action and Func<T> delegates that accept a TErrorContext parameter with Func<int, Exception, TimeSpan> retry function
9. THE DelegateInvoking SHALL provide InvokeWithWaitAndRetryAsync extension methods for Func<CancellationToken, Task> and Func<CancellationToken, Task<T>> delegates that accept a TErrorContext parameter with TimeSpan delay
10. THE DelegateInvoking SHALL provide InvokeWithWaitAndRetryAsync extension methods for Func<CancellationToken, Task> and Func<CancellationToken, Task<T>> delegates that accept a TErrorContext parameter with Func<int, Exception, TimeSpan> retry function
11. THE DelegateInvoking SHALL provide InvokeWithRetryInfinite extension methods for Action and Func<T> delegates that accept a TErrorContext parameter
12. THE DelegateInvoking SHALL provide InvokeWithRetryInfiniteAsync extension methods for Func<CancellationToken, Task> and Func<CancellationToken, Task<T>> delegates that accept a TErrorContext parameter
13. THE DelegateInvoking SHALL provide InvokeWithWaitAndRetryInfinite extension methods for Action and Func<T> delegates that accept a TErrorContext parameter with TimeSpan delay
14. THE DelegateInvoking SHALL provide InvokeWithWaitAndRetryInfinite extension methods for Action and Func<T> delegates that accept a TErrorContext parameter with Func<int, Exception, TimeSpan> retry function
15. THE DelegateInvoking SHALL provide InvokeWithWaitAndRetryInfiniteAsync extension methods for Func<CancellationToken, Task> and Func<CancellationToken, Task<T>> delegates that accept a TErrorContext parameter with TimeSpan delay
16. THE DelegateInvoking SHALL provide InvokeWithWaitAndRetryInfiniteAsync extension methods for Func<CancellationToken, Task> and Func<CancellationToken, Task<T>> delegates that accept a TErrorContext parameter with Func<int, Exception, TimeSpan> retry function
17. THE DelegateInvoking SHALL provide InvokeWithFallback extension methods for Action delegates that accept a TErrorContext parameter with Action<CancellationToken> fallback
18. THE DelegateInvoking SHALL provide InvokeWithFallback extension methods for Func<T> delegates that accept a TErrorContext parameter with Func<CancellationToken, T> fallback
19. THE DelegateInvoking SHALL provide InvokeWithFallbackAsync extension methods for Func<CancellationToken, Task> delegates that accept a TErrorContext parameter with Func<CancellationToken, Task> fallback
20. THE DelegateInvoking SHALL provide InvokeWithFallbackAsync extension methods for Func<CancellationToken, Task<T>> delegates that accept a TErrorContext parameter with Func<CancellationToken, Task<T>> fallback
21. WHEN a context-dependent extension method is called, THE DelegateInvoking SHALL invoke the policy's Handle<TErrorContext> or HandleAsync<TErrorContext> method with the non-parameterized delegate and the error context parameter
22. THE error context parameter SHALL be available to error processors but SHALL NOT be passed to the delegate being executed
23. THE context-dependent extension methods SHALL accept the same optional parameters as their non-context counterparts (ErrorProcessorParam, CatchBlockFilter, CatchBlockHandler, configureAwait, etc.)

### Requirement 7: Support Method Overloading for Parameter Variations

**User Story:** As a developer, I want multiple overloads for each parameterized extension method, so that I can use the most convenient signature for my use case.

#### Acceptance Criteria

1. THE DelegateInvoking SHALL provide overloads with minimal required parameters (delegate, param, token)
2. THE DelegateInvoking SHALL provide overloads with ErrorProcessorParam for custom error processing
3. THE DelegateInvoking SHALL provide overloads with configureAwait for async methods
4. THE DelegateInvoking SHALL provide overloads with CancellationType for fallback methods
5. WHEN multiple overloads exist for the same method name, THE DelegateInvoking SHALL ensure each overload has a unique parameter signature to avoid ambiguity

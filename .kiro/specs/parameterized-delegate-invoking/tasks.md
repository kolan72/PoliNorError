# Implementation Plan: Parameterized Delegate Invoking

## Overview

This implementation adds extension methods to the `DelegateInvoking` class to support two distinct parameter-passing patterns:

1. **Parameterized delegates**: Extension methods for `Action<TParam>` and `Func<TParam, T>` delegates where the parameter is passed directly to the delegate
2. **Context-dependent execution**: Extension methods for non-parameterized delegates (`Action`, `Func<T>`) with a `TErrorContext` parameter that is made available to error processors but not passed to the delegate

The implementation will create 8 new partial class files following the existing architecture pattern, with each file containing extension methods for a specific delegate signature pattern. All methods will reuse existing policy creation helpers and delegate to existing `Handle` and `HandleAsync` methods on policy classes.

## Tasks

- [x] 1. Implement parameterized Action<TParam> extension methods
  - [x] 1.1 Create DelegateInvoking.Parameterized.cs file with extension methods for Action<TParam> delegates
    - Implement `InvokeWithSimple<TParam>(Action<TParam>, TParam, ...)` with overloads for ErrorProcessorParam, CatchBlockFilter, and CatchBlockHandler
    - Implement `InvokeWithRetry<TParam>(Action<TParam>, TParam, int retryCount, ...)` with overloads for ErrorProcessorParam
    - Implement `InvokeWithWaitAndRetry<TParam>(Action<TParam>, TParam, int retryCount, TimeSpan delay, ...)` with overloads for ErrorProcessorParam
    - Implement `InvokeWithWaitAndRetry<TParam>(Action<TParam>, TParam, int retryCount, Func<int, Exception, TimeSpan>, ...)` with overloads for ErrorProcessorParam
    - Implement `InvokeWithRetryInfinite<TParam>(Action<TParam>, TParam, ...)` with overloads for ErrorProcessorParam
    - Implement `InvokeWithWaitAndRetryInfinite<TParam>(Action<TParam>, TParam, TimeSpan delay, ...)` with overloads for ErrorProcessorParam
    - Implement `InvokeWithWaitAndRetryInfinite<TParam>(Action<TParam>, TParam, Func<int, Exception, TimeSpan>, ...)` with overloads for ErrorProcessorParam
    - Implement `InvokeWithFallback<TParam>(Action<TParam>, TParam, Action<TParam> fallback, ...)` with overloads for ErrorProcessorParam and CancellationType
    - Each method should create the appropriate policy using existing helper methods and invoke its Handle method with the delegate and parameter
    - _Requirements: 1.1, 1.2, 1.5, 1.6, 2.1, 2.9, 2.10, 3.1, 3.7, 3.8, 4.1, 4.5, 4.6, 5.1, 5.2, 5.3, 5.5, 5.6_

  - [ ]* 1.2 Write unit tests for parameterized Action<TParam> extension methods
    - Test basic invocation and parameter passing for each policy type
    - Test error handling and PolicyResult capture
    - Test that parameters are correctly passed through to the delegate
    - Test retry behavior with same parameter value
    - Test fallback behavior with parameter passing
    - _Requirements: 1.1, 1.2, 1.5, 2.1, 2.9, 3.1, 3.7, 4.1, 4.5_

- [x] 2. Implement parameterized Func<TParam, T> extension methods
  - [x] 2.1 Create DelegateInvoking.Parameterized.T.cs file with extension methods for Func<TParam, T> delegates
    - Implement `InvokeWithSimple<TParam, T>(Func<TParam, T>, TParam, ...)` with overloads for ErrorProcessorParam, CatchBlockFilter, and CatchBlockHandler
    - Implement `InvokeWithRetry<TParam, T>(Func<TParam, T>, TParam, int retryCount, ...)` with overloads for ErrorProcessorParam
    - Implement `InvokeWithWaitAndRetry<TParam, T>(Func<TParam, T>, TParam, int retryCount, TimeSpan delay, ...)` with overloads for ErrorProcessorParam
    - Implement `InvokeWithWaitAndRetry<TParam, T>(Func<TParam, T>, TParam, int retryCount, Func<int, Exception, TimeSpan>, ...)` with overloads for ErrorProcessorParam
    - Implement `InvokeWithRetryInfinite<TParam, T>(Func<TParam, T>, TParam, ...)` with overloads for ErrorProcessorParam
    - Implement `InvokeWithWaitAndRetryInfinite<TParam, T>(Func<TParam, T>, TParam, TimeSpan delay, ...)` with overloads for ErrorProcessorParam
    - Implement `InvokeWithWaitAndRetryInfinite<TParam, T>(Func<TParam, T>, TParam, Func<int, Exception, TimeSpan>, ...)` with overloads for ErrorProcessorParam
    - Implement `InvokeWithFallback<TParam, T>(Func<TParam, T>, TParam, Func<TParam, T> fallback, ...)` with overloads for ErrorProcessorParam and CancellationType
    - Each method should return PolicyResult<T> and delegate to existing policy Handle methods
    - _Requirements: 1.2, 1.5, 1.6, 2.2, 2.9, 2.10, 3.2, 3.7, 3.8, 4.2, 4.5, 4.6, 5.1, 5.2, 5.3, 5.5, 5.6_

  - [ ]* 2.2 Write unit tests for parameterized Func<TParam, T> extension methods
    - Test basic invocation, parameter passing, and return value capture
    - Test error handling and PolicyResult<T> capture
    - Test retry behavior with same parameter and result handling
    - Test fallback behavior with parameter passing and result handling
    - _Requirements: 1.2, 1.5, 2.2, 2.9, 3.2, 3.7, 4.2, 4.5_

- [x] 3. Checkpoint - Ensure synchronous parameterized tests pass
  - Ensure all tests pass, ask the user if questions arise.

- [x] 4. Implement parameterized async Func<TParam, CancellationToken, Task> extension methods
  - [x] 4.1 Create DelegateInvoking.Parameterized.Async.cs file with extension methods for async non-returning delegates
    - Implement `InvokeWithSimpleAsync<TParam>(Func<TParam, CancellationToken, Task>, TParam, ...)` with overloads for ErrorProcessorParam, configureAwait, CatchBlockFilter, and CatchBlockHandler
    - Implement `InvokeWithRetryAsync<TParam>(Func<TParam, CancellationToken, Task>, TParam, int retryCount, ...)` with overloads for ErrorProcessorParam and configureAwait
    - Implement `InvokeWithWaitAndRetryAsync<TParam>(Func<TParam, CancellationToken, Task>, TParam, int retryCount, TimeSpan delay, ...)` with overloads for ErrorProcessorParam and configureAwait
    - Implement `InvokeWithWaitAndRetryAsync<TParam>(Func<TParam, CancellationToken, Task>, TParam, int retryCount, Func<int, Exception, TimeSpan>, ...)` with overloads for ErrorProcessorParam and configureAwait
    - Implement `InvokeWithRetryInfiniteAsync<TParam>(Func<TParam, CancellationToken, Task>, TParam, ...)` with overloads for ErrorProcessorParam and configureAwait
    - Implement `InvokeWithWaitAndRetryInfiniteAsync<TParam>(Func<TParam, CancellationToken, Task>, TParam, TimeSpan delay, ...)` with overloads for ErrorProcessorParam and configureAwait
    - Implement `InvokeWithWaitAndRetryInfiniteAsync<TParam>(Func<TParam, CancellationToken, Task>, TParam, Func<int, Exception, TimeSpan>, ...)` with overloads for ErrorProcessorParam and configureAwait
    - Implement `InvokeWithFallbackAsync<TParam>(Func<TParam, CancellationToken, Task>, TParam, Func<TParam, CancellationToken, Task> fallback, ...)` with overloads for ErrorProcessorParam, configureAwait, and CancellationType
    - Each method should return Task<PolicyResult> and delegate to existing policy HandleAsync methods
    - _Requirements: 1.3, 1.5, 1.6, 1.7, 2.3, 2.7, 2.8, 2.9, 2.11, 3.3, 3.5, 3.6, 3.7, 3.9, 4.3, 4.5, 4.6, 4.7, 5.1, 5.2, 5.3, 5.5, 5.6_

  - [ ]* 4.2 Write unit tests for parameterized async non-returning extension methods
    - Test basic async invocation and parameter passing
    - Test cancellation token handling
    - Test configureAwait behavior
    - Test async retry and fallback behavior
    - _Requirements: 1.3, 1.5, 1.7, 2.3, 2.7, 2.8, 2.11, 3.3, 3.5, 3.6, 3.9, 4.3, 4.5, 4.7_

- [x] 5. Implement parameterized async Func<TParam, CancellationToken, Task<T>> extension methods
  - [x] 5.1 Create DelegateInvoking.Parameterized.Async.T.cs file with extension methods for async returning delegates
    - Implement `InvokeWithSimpleAsync<TParam, T>(Func<TParam, CancellationToken, Task<T>>, TParam, ...)` with overloads for ErrorProcessorParam, configureAwait, CatchBlockFilter, and CatchBlockHandler
    - Implement `InvokeWithRetryAsync<TParam, T>(Func<TParam, CancellationToken, Task<T>>, TParam, int retryCount, ...)` with overloads for ErrorProcessorParam and configureAwait
    - Implement `InvokeWithWaitAndRetryAsync<TParam, T>(Func<TParam, CancellationToken, Task<T>>, TParam, int retryCount, TimeSpan delay, ...)` with overloads for ErrorProcessorParam and configureAwait
    - Implement `InvokeWithWaitAndRetryAsync<TParam, T>(Func<TParam, CancellationToken, Task<T>>, TParam, int retryCount, Func<int, Exception, TimeSpan>, ...)` with overloads for ErrorProcessorParam and configureAwait
    - Implement `InvokeWithRetryInfiniteAsync<TParam, T>(Func<TParam, CancellationToken, Task<T>>, TParam, ...)` with overloads for ErrorProcessorParam and configureAwait
    - Implement `InvokeWithWaitAndRetryInfiniteAsync<TParam, T>(Func<TParam, CancellationToken, Task<T>>, TParam, TimeSpan delay, ...)` with overloads for ErrorProcessorParam and configureAwait
    - Implement `InvokeWithWaitAndRetryInfiniteAsync<TParam, T>(Func<TParam, CancellationToken, Task<T>>, TParam, Func<int, Exception, TimeSpan>, ...)` with overloads for ErrorProcessorParam and configureAwait
    - Implement `InvokeWithFallbackAsync<TParam, T>(Func<TParam, CancellationToken, Task<T>>, TParam, Func<TParam, CancellationToken, Task<T>> fallback, ...)` with overloads for ErrorProcessorParam, configureAwait, and CancellationType
    - Each method should return Task<PolicyResult<T>> and delegate to existing policy HandleAsync methods
    - _Requirements: 1.4, 1.5, 1.6, 1.7, 2.4, 2.8, 2.9, 2.11, 3.4, 3.6, 3.7, 3.9, 4.4, 4.5, 4.6, 4.7, 5.1, 5.2, 5.3, 5.5, 5.6_

  - [ ]* 5.2 Write unit tests for parameterized async returning extension methods
    - Test basic async invocation, parameter passing, and return value capture
    - Test cancellation token handling
    - Test configureAwait behavior
    - Test async retry and fallback behavior with result handling
    - _Requirements: 1.4, 1.5, 1.7, 2.4, 2.8, 2.11, 3.4, 3.6, 3.9, 4.4, 4.5, 4.7_

- [x] 6. Checkpoint - Ensure all parameterized tests pass
  - Ensure all tests pass, ask the user if questions arise.

- [x] 7. Implement context-dependent Action extension methods
  - [x] 7.1 Create DelegateInvoking.ErrorContext.cs file with extension methods for Action delegates with TErrorContext
    - Implement `InvokeWithSimple<TErrorContext>(Action, TErrorContext, ...)` with overloads for ErrorProcessorParam, CatchBlockFilter, and CatchBlockHandler
    - Implement `InvokeWithRetry<TErrorContext>(Action, TErrorContext, int retryCount, ...)` with overloads for ErrorProcessorParam
    - Implement `InvokeWithWaitAndRetry<TErrorContext>(Action, TErrorContext, int retryCount, TimeSpan delay, ...)` with overloads for ErrorProcessorParam
    - Implement `InvokeWithWaitAndRetry<TErrorContext>(Action, TErrorContext, int retryCount, Func<int, Exception, TimeSpan>, ...)` with overloads for ErrorProcessorParam
    - Implement `InvokeWithRetryInfinite<TErrorContext>(Action, TErrorContext, ...)` with overloads for ErrorProcessorParam
    - Implement `InvokeWithWaitAndRetryInfinite<TErrorContext>(Action, TErrorContext, TimeSpan delay, ...)` with overloads for ErrorProcessorParam
    - Implement `InvokeWithWaitAndRetryInfinite<TErrorContext>(Action, TErrorContext, Func<int, Exception, TimeSpan>, ...)` with overloads for ErrorProcessorParam
    - Implement `InvokeWithFallback<TErrorContext>(Action, TErrorContext, Action<CancellationToken> fallback, ...)` with overloads for ErrorProcessorParam and CancellationType
    - Each method should invoke the policy's Handle<TErrorContext> method with the non-parameterized delegate and error context
    - _Requirements: 6.1, 6.4, 6.5, 6.7, 6.8, 6.11, 6.13, 6.14, 6.17, 6.21, 6.22, 6.23, 5.1, 5.2, 5.3, 5.5, 5.7_

  - [ ]* 7.2 Write unit tests for context-dependent Action extension methods
    - Test that error context is passed to error processors but not to the delegate
    - Test basic invocation without parameter passing to delegate
    - Test error handling with context availability
    - Test retry and fallback behavior with error context
    - _Requirements: 6.1, 6.4, 6.5, 6.21, 6.22, 6.23_

- [x] 8. Implement context-dependent Func<T> extension methods
  - [x] 8.1 Create DelegateInvoking.ErrorContext.T.cs file with extension methods for Func<T> delegates with TErrorContext
    - Implement `InvokeWithSimple<TErrorContext, T>(Func<T>, TErrorContext, ...)` with overloads for ErrorProcessorParam, CatchBlockFilter, and CatchBlockHandler
    - Implement `InvokeWithRetry<TErrorContext, T>(Func<T>, TErrorContext, int retryCount, ...)` with overloads for ErrorProcessorParam
    - Implement `InvokeWithWaitAndRetry<TErrorContext, T>(Func<T>, TErrorContext, int retryCount, TimeSpan delay, ...)` with overloads for ErrorProcessorParam
    - Implement `InvokeWithWaitAndRetry<TErrorContext, T>(Func<T>, TErrorContext, int retryCount, Func<int, Exception, TimeSpan>, ...)` with overloads for ErrorProcessorParam
    - Implement `InvokeWithRetryInfinite<TErrorContext, T>(Func<T>, TErrorContext, ...)` with overloads for ErrorProcessorParam
    - Implement `InvokeWithWaitAndRetryInfinite<TErrorContext, T>(Func<T>, TErrorContext, TimeSpan delay, ...)` with overloads for ErrorProcessorParam
    - Implement `InvokeWithWaitAndRetryInfinite<TErrorContext, T>(Func<T>, TErrorContext, Func<int, Exception, TimeSpan>, ...)` with overloads for ErrorProcessorParam
    - Implement `InvokeWithFallback<TErrorContext, T>(Func<T>, TErrorContext, Func<CancellationToken, T> fallback, ...)` with overloads for ErrorProcessorParam and CancellationType
    - Each method should return PolicyResult<T> and invoke the policy's Handle<TErrorContext> method
    - _Requirements: 6.2, 6.4, 6.5, 6.7, 6.8, 6.12, 6.13, 6.14, 6.18, 6.21, 6.22, 6.23, 5.1, 5.2, 5.3, 5.5, 5.7_

  - [ ]* 8.2 Write unit tests for context-dependent Func<T> extension methods
    - Test that error context is passed to error processors but not to the delegate
    - Test basic invocation and return value capture without parameter passing to delegate
    - Test error handling with context availability
    - Test retry and fallback behavior with error context and result handling
    - _Requirements: 6.2, 6.4, 6.5, 6.21, 6.22, 6.23_

- [x] 9. Checkpoint - Ensure synchronous context-dependent tests pass
  - Ensure all tests pass, ask the user if questions arise.

- [x] 10. Implement context-dependent async Func<CancellationToken, Task> extension methods
  - [x] 10.1 Create DelegateInvoking.ErrorContext.Async.cs file with extension methods for async non-returning delegates with TErrorContext
    - Implement `InvokeWithSimpleAsync<TErrorContext>(Func<CancellationToken, Task>, TErrorContext, ...)` with overloads for ErrorProcessorParam, configureAwait, CatchBlockFilter, and CatchBlockHandler
    - Implement `InvokeWithRetryAsync<TErrorContext>(Func<CancellationToken, Task>, TErrorContext, int retryCount, ...)` with overloads for ErrorProcessorParam and configureAwait
    - Implement `InvokeWithWaitAndRetryAsync<TErrorContext>(Func<CancellationToken, Task>, TErrorContext, int retryCount, TimeSpan delay, ...)` with overloads for ErrorProcessorParam and configureAwait
    - Implement `InvokeWithWaitAndRetryAsync<TErrorContext>(Func<CancellationToken, Task>, TErrorContext, int retryCount, Func<int, Exception, TimeSpan>, ...)` with overloads for ErrorProcessorParam and configureAwait
    - Implement `InvokeWithRetryInfiniteAsync<TErrorContext>(Func<CancellationToken, Task>, TErrorContext, ...)` with overloads for ErrorProcessorParam and configureAwait
    - Implement `InvokeWithWaitAndRetryInfiniteAsync<TErrorContext>(Func<CancellationToken, Task>, TErrorContext, TimeSpan delay, ...)` with overloads for ErrorProcessorParam and configureAwait
    - Implement `InvokeWithWaitAndRetryInfiniteAsync<TErrorContext>(Func<CancellationToken, Task>, TErrorContext, Func<int, Exception, TimeSpan>, ...)` with overloads for ErrorProcessorParam and configureAwait
    - Implement `InvokeWithFallbackAsync<TErrorContext>(Func<CancellationToken, Task>, TErrorContext, Func<CancellationToken, Task> fallback, ...)` with overloads for ErrorProcessorParam, configureAwait, and CancellationType
    - Each method should return Task<PolicyResult> and invoke the policy's HandleAsync<TErrorContext> method
    - _Requirements: 6.3, 6.4, 6.6, 6.9, 6.10, 6.12, 6.15, 6.16, 6.19, 6.21, 6.22, 6.23, 5.1, 5.2, 5.3, 5.5, 5.7_

  - [ ]* 10.2 Write unit tests for context-dependent async non-returning extension methods
    - Test that error context is passed to error processors but not to the delegate
    - Test basic async invocation without parameter passing to delegate
    - Test cancellation token handling
    - Test configureAwait behavior
    - Test async retry and fallback behavior with error context
    - _Requirements: 6.3, 6.4, 6.6, 6.21, 6.22, 6.23_

- [x] 11. Implement context-dependent async Func<CancellationToken, Task<T>> extension methods
  - [x] 11.1 Create DelegateInvoking.ErrorContext.Async.T.cs file with extension methods for async returning delegates with TErrorContext
    - Implement `InvokeWithSimpleAsync<TErrorContext, T>(Func<CancellationToken, Task<T>>, TErrorContext, ...)` with overloads for ErrorProcessorParam, configureAwait, CatchBlockFilter, and CatchBlockHandler
    - Implement `InvokeWithRetryAsync<TErrorContext, T>(Func<CancellationToken, Task<T>>, TErrorContext, int retryCount, ...)` with overloads for ErrorProcessorParam and configureAwait
    - Implement `InvokeWithWaitAndRetryAsync<TErrorContext, T>(Func<CancellationToken, Task<T>>, TErrorContext, int retryCount, TimeSpan delay, ...)` with overloads for ErrorProcessorParam and configureAwait
    - Implement `InvokeWithWaitAndRetryAsync<TErrorContext, T>(Func<CancellationToken, Task<T>>, TErrorContext, int retryCount, Func<int, Exception, TimeSpan>, ...)` with overloads for ErrorProcessorParam and configureAwait
    - Implement `InvokeWithRetryInfiniteAsync<TErrorContext, T>(Func<CancellationToken, Task<T>>, TErrorContext, ...)` with overloads for ErrorProcessorParam and configureAwait
    - Implement `InvokeWithWaitAndRetryInfiniteAsync<TErrorContext, T>(Func<CancellationToken, Task<T>>, TErrorContext, TimeSpan delay, ...)` with overloads for ErrorProcessorParam and configureAwait
    - Implement `InvokeWithWaitAndRetryInfiniteAsync<TErrorContext, T>(Func<CancellationToken, Task<T>>, TErrorContext, Func<int, Exception, TimeSpan>, ...)` with overloads for ErrorProcessorParam and configureAwait
    - Implement `InvokeWithFallbackAsync<TErrorContext, T>(Func<CancellationToken, Task<T>>, TErrorContext, Func<CancellationToken, Task<T>> fallback, ...)` with overloads for ErrorProcessorParam, configureAwait, and CancellationType
    - Each method should return Task<PolicyResult<T>> and invoke the policy's HandleAsync<TErrorContext> method
    - _Requirements: 6.4, 6.4, 6.6, 6.10, 6.12, 6.16, 6.20, 6.21, 6.22, 6.23, 5.1, 5.2, 5.3, 5.5, 5.7_

  - [ ]* 11.2 Write unit tests for context-dependent async returning extension methods
    - Test that error context is passed to error processors but not to the delegate
    - Test basic async invocation and return value capture without parameter passing to delegate
    - Test cancellation token handling
    - Test configureAwait behavior
    - Test async retry and fallback behavior with error context and result handling
    - _Requirements: 6.4, 6.4, 6.6, 6.21, 6.22, 6.23_

- [x] 12. Final checkpoint - Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- Tasks marked with `*` are optional and can be skipped for faster MVP
- Each task references specific requirements for traceability
- Checkpoints ensure incremental validation
- All extension methods follow existing API patterns and naming conventions
- Implementation reuses existing policy creation helpers and Handle/HandleAsync methods
- The design uses C# as the implementation language
- No property-based tests are included as this feature is testing API surface delegation rather than complex algorithmic properties

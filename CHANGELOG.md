## 2.24.0

- Introduced the `RetryPolicy.ThenFallback` method that implements the retry-then-fallback pattern.
- Introduced the PolicyResult.PolicyCanceledError property, which stores the `OperationCanceledException` when cancellation occurs.
- Fix issue #150.
- Introduced `TimeSeriesRetryDelay` class 
- Introduced the `TimeSeriesRetryDelayOptions` class and added `TimeSeries` to the `RetryDelayType` enum.
- Made `DefaultFallbackProcessor` implement the `ICanAddErrorFilter<DefaultFallbackProcessor>` interface.
- Made `FallbackPolicyBase` implement the `ICanAddErrorFilter<FallbackPolicyBase>` interface.
- Made `FallbackPolicy` implement the `ICanAddErrorFilter<FallbackPolicy>` interface.
- Made `FallbackPolicyWithAsyncFunc` implement the `ICanAddErrorFilter<FallbackPolicyWithAsyncFunc>` interface.
- Made `FallbackPolicyWithAction` implement the `ICanAddErrorFilter<FallbackPolicyWithAction>` interface.
- Made `SimplePolicyProcessor` implement the `ICanAddErrorFilter<SimplePolicyProcessor>` interface.
- Made `SimplePolicy` implement the `ICanAddErrorFilter<SimplePolicy>` interface.
- Introduced the `FallbackFuncsProvider.ToFallbackPolicy` method.
- Prevent `NullReferenceException` when a `FallbackPolicyBase`- derived class is initialized by `FallbackFuncsProvider`.
- Introduced the `BulkErrorProcessor.WithErrorContextProcessorOf<TErrorContext>(Func<Exception, ProcessingErrorInfo<TErrorContext>, CancellationToken, Task>)` method.
- Introduced the `BulkErrorProcessor.WithErrorContextProcessorOf<TErrorContext>(Action<Exception, ProcessingErrorInfo<TErrorContext>, CancellationToken>)` method.	
- Introduced the `BulkErrorProcessor.WithErrorContextProcessorOf<TErrorContext>(Action<Exception, ProcessingErrorInfo<TErrorContext>>)` extension method and its overloads.
- Introduced the `BulkErrorProcessor.WithErrorContextProcessorOf<TErrorContext>(Func<Exception, ProcessingErrorInfo<TErrorContext>, Task>)` extension method and its overloads.
- Introduced the `BulkErrorProcessor.WithErrorContextProcessor<TErrorContext>(DefaultErrorProcessor<TErrorContext)` method.
- Introduced the `GetAttemptCount()` extensions method for `ProcessingErrorInfo`.
- Introduced a `DelayErrorProcessor` constructor that accepts a `RetryDelay` parameter.
- Add an internal extension method `GetCancellationException()` for `AggregateException`.
- Refactor `AddIncludedErrorFilterForAll<TException>` extension methods for `IEnumerable<IPolicyBase>`.
- Refactor `AddExcludedErrorFilterForAll<TException>` extension methods for `IEnumerable<IPolicyBase>`.
- Handle null comparison safely in `ErrorSetItem`
- Introduced the `IBulkErrorProcessor.WithDelayBetweenRetries(TimeSpan)` extension method.
- Prevent `NullReferenceException` when `ProcessingErrorContext` parameter is `null` in `BulkErrorProcessor.Process(Async)` methods.
- Refactor internal extension method `ExceptionFilter.AddIncludedErrorSet`.
- Refactor internal extension method `ExceptionFilter.AddExcludedErrorSet`.
- Refactor how the error context processor is added to the policy processor in the `ErrorProcessorRegistration` class.
- Added internal `IPolicyResultHandlerCollection` interface.
- Deprecate the `BulkProcessStatus` enum.
- Edit 'RetryPolicy' README chapter.
- Edit 'Key Concepts' README Chapter.
- Add 'Tips and Tricks' README Chapter.


## 2.23.0

- Introduced the `RetryPolicy.Handle<TErrorContext>(Action, TErrorContext, CancellationToken)` method.
- Introduced the `RetryPolicy.Handle<TParam>(Action<TParam>, TParam, CancellationToken)` method.
- Introduced the `RetryPolicy.Handle<TErrorContext, T>(Func<T>, TErrorContext, CancellationToken)` method.
- Introduced the `RetryPolicy.Handle<TParam, T>(Func<TParam, T>, TParam, CancellationToken)` method.
- Introduced the `RetryPolicy.HandleAsync<TErrorContext>(Func<CancellationToken, Task>, TErrorContext, ... ,CancellationToken)` method overloads.
- Introduced the `RetryPolicy.HandleAsync<TParam>(Func<TParam, CancellationToken, Task>, TParam, ... ,CancellationToken)` method overloads.
- Introduced the `RetryPolicy.HandleAsync<TErrorContext, T>(Func<CancellationToken, Task<T>>, TErrorContext, ...,CancellationToken)` method overloads.
- Introduced the `RetryPolicy.HandleAsync<TParam, T>(Func<TParam, CancellationToken, Task<T>>, TParam, ... ,CancellationToken)` method overloads.
- Introduced the `IRetryExecutionInfo` interface, which is implemented by `RetryProcessingErrorInfo` and `RetryProcessingErrorInfo<TParam>`.
- Introduced the `GetRetryCount()` extensions method for `ProcessingErrorInfo`.
- Introduced the `RetryPolicy.WithErrorContextProcessor<TErrorContext>(DefaultErrorProcessor<TErrorContext>)` method.
- Introduced the `RetryPolicy.WithErrorContextProcessorOf<TErrorContext>(Action<Exception, ProcessingErrorInfo<TErrorContext>>)` method and its overloads.
- Introduced the `RetryPolicy.WithErrorContextProcessorOf<TErrorContext>(Action<Exception, ProcessingErrorInfo<TErrorContext>, CancellationToken>)` method.
- Introduced the `RetryPolicy.WithErrorContextProcessorOf<TErrorContext>(Func<Exception, ProcessingErrorInfo<TErrorContext>, Task>)` and its overloads.
- Introduced the `RetryPolicy.WithErrorContextProcessorOf<TErrorContext>(Func<Exception, ProcessingErrorInfo<TErrorContext>, CancellationToken, Task>)` method.
- Added an optional bool excludeLastPolicy parameter (default: `false`) to the `PolicyCollection.AddPolicyResultHandlerForAll(Func<PolicyResult, CancellationToken, Task>, ...)` method.
- Added an optional bool excludeLastPolicy parameter (default: `false`) to the `PolicyCollection.AddPolicyResultHandlerForAll(Func<PolicyResult, Task>, CancellationType, ...)` overloads.
- Added an optional bool excludeLastPolicy parameter (default: `false`) to the `PolicyCollection.AddPolicyResultHandlerForAll<T>(Func<PolicyResult<T>, Task>, CancellationType, ...)` overloads.
- Added an optional bool excludeLastPolicy parameter (default: `false`) to the `PolicyCollection.AddPolicyResultHandlerForAll<T>(Action<PolicyResult<T>>, CancellationType, ...)` overloads.
- Added an optional bool excludeLastPolicy parameter (default: `false`) to the `PolicyCollection.AddPolicyResultHandlerForAll<T>(Func<PolicyResult<T>, CancellationToken, Task>, ...)` method.
- Added an optional bool excludeLastPolicy parameter (default: `false`) to the `PolicyCollection.AddPolicyResultHandlerForAll<T>(Action<PolicyResult<T>, CancellationToken>, ...)` method.
- Added an optional bool excludeLastPolicy parameter (default: `false`) to the `PolicyCollection.AddPolicyResultHandlerForAll(Action<PolicyResult, CancellationToken>, ...)` method.
- Add an internal `DelayErrorProcessor` constructor overload that accepts an `IDelayProvider`.
- Introduced the `RetryDelay(Func<int, TimeSpan> delayValueProvider)` constructor.
- Introduced the `RetryDelay(RetryDelayOptions)` constructor.
- Added the protected `RetryDelay.DelayValueProvider` property and deprecated the `InnerDelay` and `InnerDelayValueProvider` properties.
- Added the `ICanAddErrorFilter<T>` interface and implemented it in `RetryPolicy` and `DefaultRetryProcessor`.
- Introduced the `IBulkErrorProcessor.WithDelayBetweenRetries(Func<int,Exception, TimeSpan>)` extension method.
- Introduced the `TryCatchBuilder.AddCatchBlock(Func<IEmptyCatchBlockFilter, NonEmptyCatchBlockFilter>, IBulkErrorProcessor)` method and deprecated the `TryCatchBuilder.AddCatchBlock(Func<IEmptyCatchBlockFilter, NonEmptyCatchBlockFilter>, Action<IBulkErrorProcessor>)` method.
- Added the `ExceptionFilter.AppendFilter(ExceptionFilter)` method.
- DRY refactoring to add a policy result handler to `PolicyCollection`.
- Refactor `Policy.HandlePolicyResult(Async)` methods.
- Update `System.Memory` to version 4.0.5.0 in 'PoliNorError.Tests'. 
- Bump `System.Runtime.CompilerServices.Unsafe` and `System.Threading.Tasks.Extensions` in 'PoliNorError.Tests'. 
- Update 'RetryPolicy' README Chapter.
- Update NuGet package README.


## 2.22.0

- Introduced overloads of these methods, with and without the `RetryDelay` parameter:  
  - `DefaultRetryProcessor.RetryWithErrorContextAsync<TErrorContext>(Action, TErrorContext, ..., ..., CancellationToken)`  
  - `DefaultRetryProcessor.Retry<TParam>(Action<TParam>, TParam, RetryCountInfo, ..., CancellationToken)`  
  - `DefaultRetryProcessor.RetryInfiniteWithErrorContext<TErrorContext>(Action, TErrorContext, CancellationToken)`  
  - `DefaultRetryProcessor.RetryInfinite<TParam>(Action<TParam>, TParam, ..., CancellationToken)`  
  - `DefaultRetryProcessor.RetryWithErrorContextAsync<TErrorContext>(Func<CancellationToken, Task>, TErrorContext param, ..., ..., configureAwait, token)`  
  - `DefaultRetryProcessor.RetryAsync<TParam>(Func<TParam, CancellationToken, Task>, TParam, ..., ..., bool, CancellationToken)`  
  - `DefaultRetryProcessor.RetryInfiniteWithErrorContextAsync<TErrorContext>(Func<CancellationToken, Task>, TErrorContext, ..., bool, CancellationToken)`  
  - `DefaultRetryProcessor.RetryInfiniteAsync<TParam>(Func<TParam, CancellationToken, Task>, TParam, bool, CancellationToken)`  
  - `DefaultRetryProcessor.RetryWithErrorContext<TErrorContext, T>(Func<T> func, TErrorContext param, …, CancellationToken)`  
  - `DefaultRetryProcessor.Retry<TParam, T>(Func<TParam, T>, TParam, ..., CancellationToken)`  
  - `DefaultRetryProcessor.RetryInfiniteWithErrorContext<TErrorContext, T>(Func<T>, TErrorContext, CancellationToken)`  
  - `DefaultRetryProcessor.RetryInfinite<TParam, T>(Func<TParam, T>, TParam, CancellationToken)`  
  - `DefaultRetryProcessor.RetryWithErrorContextAsync<TErrorContext, T>(Func<CancellationToken, Task<T>>, TErrorContext, ..., bool, CancellationToken)`  
  - `DefaultRetryProcessor.RetryAsync<TParam, T>(Func<TParam, CancellationToken, Task<T>, TParam, ..., bool, CancellationToken)`  
  - `DefaultRetryProcessor.RetryInfiniteWithErrorContextAsync<TErrorContext, T>(Func<CancellationToken, Task<T>>, TErrorContext, ..., bool, CancellationToken)`
  - `DefaultRetryProcessor.RetryInfiniteAsync<TParam, T>(Func<TParam, CancellationToken, Task<T>>, TParam, ..., bool, CancellationToken)`  
- Introduced the `DefaultRetryProcessor.WithErrorContextProcessorOf<TErrorContext>(Action<Exception, ProcessingErrorInfo<TErrorContext>, CancellationToken>)` method.  
- Introduced the `DefaultRetryProcessor.WithErrorContextProcessor<TErrorContext>(DefaultErrorProcessor<TErrorContext>)` method.  
- Introduced the `DefaultRetryProcessor.WithErrorContextProcessorOf<TErrorContext>(Func<Exception, ProcessingErrorInfo<TErrorContext>, CancellationToken, Task>)` method.  
- Introduced the `DefaultRetryProcessor.WithErrorContextProcessorOf<TErrorContext>(Func<Exception, ProcessingErrorInfo<TErrorContext>, Task>)` method overloads.  
- Refactor `DelayIfNeed` and `DelayIfNeedAsync` methods in `DefaultRetryProcessor`.  
- Introduced the `TryCatchBuilder.AddCatchBlock(Func<IEmptyCatchBlockFilter, NonEmptyCatchBlockFilter>, Action<IBulkErrorProcessor)` method.
- Introduced the `RetryProcessingErrorInfo<TParam>` class along with related internal classes.  
- Applied DRY refactoring to `DefaultRetryProcessor` – added a lazily initialized private `DelayProvider` property.  
- Applied DRY refactoring to `ExceptionFilter`-dependent classes by adding the `AddIncludedErrorFilter<TException>` and `AddExcludedErrorFilter<TException>` methods.
- Applied DRY refactoring to `ExceptionFilter`- dependent classes by adding the `AddIncludedInnerErrorFilter<TException>` and `AddExcludedInnerErrorFilter<TException>` methods.  
- Bump Castle.Core from 5.1.1 to 5.2.1.  
- Bump System.Runtime.CompilerServices.Unsafe and System.Threading.Tasks.Extensions.  
- Bump System.Buffers from 4.6.0 to 4.6.1  
- Update the 'Key Concepts' chapter in the README.
- Added a 'Key Concepts' section to the README.


## 2.21.0

- Introduced the `FallbackPolicyBase.Handle<TErrorContext>(Action, TErrorContext, CancellationToken)` and `FallbackPolicyBase.Handle<TParam>(Action<TParam>, TParam, CancellationToken)` methods.
- Introduced the `FallbackPolicyBase.Handle<TParam, T>(Func<TParam, T>, TParam, CancellationToken)` and `FallbackPolicyBase.Handle<TErrorContext, T>(Func<T> func, TErrorContext param, CancellationToken)` methods.
- Introduced overloads for the `FallbackPolicyBase.HandleAsync<TParam>` and `FallbackPolicyBase.HandleAsync<TErrorContext>` methods.
- Introduced overloads for the `FallbackPolicyBase.HandleAsync<TParam, T>` and `FallbackPolicyBase.HandleAsync<TErrorContext, T>` methods.
- Introduced the `WithErrorContextProcessor<TErrorContext>` and `WithErrorContextProcessorOf<TErrorContext>` method overloads for `FallbackPolicy`, `FallbackPolicyBase`, `FallbackPolicyWithAction`, and `FallbackPolicyWithAsyncFunc` policies.
- Introduced the `DefaultFallbackProcessor.Fallback<TErrorContext>(Action, TErrorContext, Action<CancellationToken>, CancellationToken)` method.
- Introduced the `DefaultFallbackProcessor.Fallback<TParam>(Action<TParam>, TParam, Action<CancellationToken>)` method.
- Introduced the `DefaultFallbackProcessor.Fallback<TErrorContext, T>(Func<T>, TErrorContext, Func<CancellationToken, T>)` method.
- Introduced the `DefaultFallbackProcessor.Fallback<TParam, T>(Func<TParam, T>, TParam, Func<CancellationToken, T>, CancellationToken)` method.
- Introduced the `DefaultFallbackProcessor.FallbackAsync<TErrorContext>(Func<CancellationToken, Task>, TErrorContext, Func<CancellationToken, Task>, bool, CancellationToken)` method.
- Introduced the `DefaultFallbackProcessor.FallbackAsync<TParam>(Func<TParam, CancellationToken, Task>, TParam, Func<CancellationToken, Task>, bool, CancellationToken)` method.
- Introduced the `DefaultFallbackProcessor.FallbackAsync<TErrorContext, T>(Func<CancellationToken, Task<T>>, TErrorContext, Func<CancellationToken, Task<T>>, bool, CancellationToken)` method.
- Introduced the `DefaultFallbackProcessor.FallbackAsync<TParam, T>(Func<TParam, CancellationToken, Task<T>>, TParam, Func<CancellationToken, Task<T>>, bool, CancellationToken)` method.
- Introduced the `WithErrorContextProcessor<TErrorContext>(DefaultErrorProcessor<TErrorContext>)` method in `DefaultFallbackProcessor`.
- Introduced the `DefaultFallbackProcessor.WithErrorContextProcessorOf<TErrorContext>` method overloads.
- `DefaultRetryProcessor` refactoring - make the field representing the retry policy rule static and simplify `RetryErrorContext` creation.
- Apply the 'check reference equality instead' Rider rule to type comparisons in the `DefaultRetryProcessor.RetryInternal<T>` method.
- Update NuGet README.
- Updated 'Usage' in README for 2.20.0 changes.
- Update 'TryCatch' README Chapter.
- Update the Error processors chapter in the README.
- Add diagrams to README.
- Bump the nunit group with 7 updates.


## 2.20.0

- Introduce the `DefaultErrorProcessor<TParam>` , `ProcessingErrorContext<TParam>` and `ProcessingErrorInfo<TParam>` classes.
- Introduce the `SimplePolicy.Handle<TErrorContext>(Action, TErrorContext, CancellationToken)` method.
- Introduce the `SimplePolicy.Handle<TParam>(Action<TParam>, TParam, CancellationToken)` method.
- Introduce the `Handle<TParam, T>(Func<TParam, T>, TParam, CancellationToken)` and `Handle<TErrorContext, T>(Func<T> func, TErrorContext param, CancellationToken)` methods of `SimplePolicy`.
- Introduce overloads for the `SimplePolicy.HandleAsync<TParam>` and `SimplePolicy.HandleAsync<TErrorContext>` methods.
- Introduce overloads for the `SimplePolicy.HandleAsync<TParam, T>` and `SimplePolicy.HandleAsync<TErrorContext, T>` methods.
- Introduce the `SimplePolicy.WithErrorContextProcessor<TErrorContext>` and `SimplePolicy.WithErrorContextProcessorOf<TErrorContext>` method overloads.
- Introduce the `SimplePolicyProcessor.Execute<TParam>(Action)` method.
- Introduce the `SimplePolicyProcessor.Execute<TParam>(Action<TParam>, TParam, token)` method.
- Introduce the `SimplePolicyProcessor.Execute<TErrorContext, T>(Func<T>, TErrorContext, CancellationToken)` method.
- Introduce the `SimplePolicyProcessor.Execute<TParam, T>(Func<TParam, T>, TParam, CancellationToken)` method.
- Introduce the `SimplePolicyProcessor.ExecuteAsync<TErrorContext>(Func<CancellationToken, Task>, TErrorContext, bool, CancellationToken)` method.
- Introduce the `SimplePolicyProcessor.ExecuteAsync<TParam>(Func<TParam, CancellationToken, Task>, TParam, bool, CancellationToken)` method.
- Introduce the `SimplePolicyProcessor.ExecuteAsync<TErrorContext, T>(Func<CancellationToken, Task<T>>, TErrorContext, bool, CancellationToken)` method.
- Introduce the `SimplePolicyProcessor.ExecuteAsync<TParam, T>(Func<TParam, CancellationToken, Task<T>>, TParam, bool, CancellationToken)` method.
- Introduce the `SimplePolicyProcessor.WithErrorContextProcessor<TErrorContext>`  and `SimplePolicyProcessor.WithErrorContextProcessorOf<TErrorContext>`  method overloads.
- Introduce the `Func<CancellationToken, Task<T>>.InvokeWithTryCatchAsync` extension method.
- Introduce `InvokeWithTryCatchAsync` extension methods overloads for the `Func<CancellationToken, Task<T>>`, `Func<CancellationToken, Task>` delegates.
- Introduce the `Func<CancellationToken, Task>.InvokeWithTryCatchAsync` extension method.
- Introduce the `Func<T>.InvokeWithTryCatch` extension method.
- Introduce the `Action.InvokeWithTryCatch` extension method.
- Introduce `TryCatchBuilder.AddCatchBlock` method overloads with `NonEmptyCatchBlockFilter`, `IBulkErrorProcessor` parameters.
- Introduce the `TryCatchBuilder.AddCatchBlock` method overload with the `NonEmptyCatchBlockFilter` parameter.
- Introduce the `TryCatchBuilder.AddCatchBlock` method overload with the `IBulkErrorProcessor` parameter.
- Introduce `TryCatchBuilder.CreateFrom` method overload with `NonEmptyCatchBlockFilter`, `IBulkErrorProcessor` parameters.
- Introduce the `TryCatchBuilder.CreateFrom` method overload with the `NonEmptyCatchBlockFilter` parameter.
- Introduce the `TryCatchBuilder.CreateFrom` method overload with the `IBulkErrorProcessor` parameter.
- Introduce shorthand `TryCatchBuilder.CreateAndBuild` method without parameters.
- Introduce shorthand `TryCatchBuilder.CreateAndBuild` method with the `IBulkErrorProcessor` parameter
- Introduce shorthand `TryCatchBuilder.CreateAndBuild` method with the `Func<Exception, Task>` parameter.
- Introduce shorthand `TryCatchBuilder.CreateAndBuild` method with the `Action<Exception>` parameter.
- Deprecate the `BulkErrorProcessor(PolicyAlias)` constructor.
- Made `PolicyProcessor` constructors that use the `PolicyAlias` parameter and the `_isPolicyAliasSet` field obsolete.
- Call the `ConfigureAwait` method in the `ErrorProcessorBase.ProcessAsync` method with the `configAwait` parameter passed.
- Add the internal class `EmptyErrorContext<TParam>`.
- Add the `Policy.HasPolicyWrapperFactory` internal property.
- Add the Rider-related .gitignore.
- Edit 'Policy wrap' README Chapter.
- Update 'RetryPolicy' README Chapter and example in README for NuGet package.
- Bump the nunit group with 7 updates.
- Remove old references to NUnit-related nuget packages in the PoliNorError.Tests project file.


## 2.19.15

- Introduce `RetryPolicy.WithWait(DelayErrorProcessor)` method.
- `DefaultRetryProcessor` refactoring - add field representing retry policy rule.
- Add the `RetryContext.IsZeroRetry` property and use it to set the `PolicyResult.NoError` property to true.
- Slightly improved performance for the `RetryPolicy` custom error saver by passing the `ErrorContext<RetryContext>` instead of `int` `tryCount` arg in the `DefaultRetryProcessor.SaveError(Async)` methods.
- Deprecate `ProcessingErrorContext.CurrentRetryCount` property.
- Introduce the `Policy.WrapPolicyCollection` extension method.
- Argument exception guard clause for the `PolicyCollection.WrapUp` method with the `ThrowOnWrappedCollectionFailed.None` value for the `throwOnWrappedCollectionFailed` parameter.
- Argument exception guard clause for the `Policy.WrapPolicyCollection` method with the `ThrowOnWrappedCollectionFailed.None` value for the `throwOnWrappedCollectionFailed` parameter.
- Made the `Policy` class constructor `protected` (previously `private protected`).
- Made the `cancellationToken` parameter optional in `PolicyDelegate(<T>).HandleAsync` methods.
- Introduce `PolicyResultHandlerFailedException.Result` property.
- Introduce `PolicyResultHandlerFailedException<T>` exception.
- Directly return Task instead of await when converting async uncancelable generic fallback delegate to cancelable with `CancellationType.Precancelable` argument.
- New tests for the `PolicyCollection.WrapUp` method with the `ThrowOnWrappedCollectionFailed.CollectionError` parameter.
- New tests for a wrapped `SimplePolicy` that wraps another policy. 
- Move the tests for the `RetryDelay` classes to a separate folder. 
- Add doc comments to the public members of the `PolicyBuilding` class. 
- Put the nuget README in the file.
- Update 'RetryPolicy' README Chapter.
- Correct retry terminology in README.
- Correct 'Error processors' README Chapters.
- Bump NSubstitute from 5.1.0 to 5.3.0.
- Bump System.Numerics.Vectors from 4.5.0 to 4.6.0.
- Bump System.Buffers from 4.5.1 to 4.6.0.
- Bump System.Runtime.CompilerServices.Unsafe from 6.0.0 to 6.1.0.


## 2.19.11

- Introduce `SlopeFactor` for `LinearRetryDelay`.
- Slightly improved performance for `RetryPolicy` by removing the overhead of using the `int` `tryCount` argument in the `DefaultRetryProcessor.DelayIfNeedAsync(Async)` methods.  
- Introduce `PolicyCollection.WithRetry`, `PolicyCollection.WithInfiniteRetry` methods with `RetryDelay` parameter.
- Introduce `PolicyDelegateCollection.WithRetry`, `PolicyDelegateCollection.WithInfiniteRetry` methods with `RetryDelay` parameter.
- Introduce `PolicyDelegateCollection<T>.WithRetry`, `PolicyDelegateCollection<T>.WithInfiniteRetry` methods with `RetryDelay` parameter.
- Introduce `RetryPolicy.SetPolicyResultFailedIf` method overloads with `Action<PolicyResult<T>>` handler parameter for `RetryPolicy`, `SimplePolicy` and `Fallback` policies.  
- Move null guards before getting a fallback delegate in `FallbackPolicyBase`.  
- Remove deprecated suppression for `PolicyResultHandlingException`.
- Add doc comments to the public members of the `PolicyDelegate` and `PolicyDelegate<T>` classes.
- Add doc comments to the public members of the `PolicyDelegateResult` and `PolicyDelegateResult<T>` classes.
- Add doc comments to the public members of the `PolicyResult` and `PolicyResult<T>` classes, and to the `PolicyResultFailedReason` enum.
- Update 'Calling Func and Action delegates in a resilient manner' README Chapter.
- Update 'RetryPolicy' README Chapter.


## 2.19.8

- Introduce `Policy` error filtering extension methods in the `PoliNorError.Extensions.PolicyErrorFiltering` namespace.
- Fix issue #126 by using the `IDelayProvider.BackoffBackoffSafely(Async)` extension methods.
- Introduce `Action.InvokeWithRetryDelay` extension methods.
- Introduce `Action.InvokeWithRetryDelayInfinite`  extension methods.
- Introduce `Func<CancellationToken, Task>.InvokeWithRetryDelayAsync` extension methods.
- Introduce `Func<CancellationToken, Task>.InvokeWithRetryDelayInfiniteAsync` extension methods.
- Introduce `Func<T>.InvokeWithRetryDelay` extension methods.
- Introduce `Func<T>.InvokeWithRetryDelayInfinite` extension methods.
- Introduce `Func<CancellationToken, Task<T>>.InvokeWithRetryDelayAsync` extension methods.
- Introduce `Func<CancellationToken, Task<T>>.InvokeWithRetryDelayInfiniteAsync` extension methods.
- Add early return in `RetryProcessor.Retry(Async)(<T>)` methods if token is already canceled.
- DRY Refactoring of the use of `RetryDelay` in `DefaultRetryProcessor` methods.
- Refactor catch block exception handling in `DefaultRetryProcessor.Retry(Async)<T>` methods.
- Add 'Try', 'Catch' tags to nuget package.
- Update 'RetryPolicy' README Chapter.
- Add doc comments to `CatchBlockExceptionSource`.
- Bump NUnit from 4.1.0 to 4.2.1.
- Bump NUnit from  4.2.1  to  4.2.2.


## 2.19.5

- Introduce jittering for `RetryDelay` subclasses, adapted from Polly.
- Introduce `RetryDelayOptions.MaxDelay` property.
- Introduce `Create` static methods for `ConstantRetryDelay`, `LinearRetryDelay`, `ExponentialRetryDelay` classes.
- Bump NUnit3TestAdapter from 4.5.0 to 4.6.0 in the nunit group.


## 2.19.0

- Add the `RetryDelay` class and its subclasses(`ConstantRetryDelay`, `LinearRetryDelay`, `ExponentialRetryDelay`).  
- Change `RetryPolicy` ctor signature to accept the `RetryDelay` parameter.  
- Add the `RetryDelay` parameter to the `RetryPolicy.InfiniteRetries` method overloads.  
- Modify `RetryPolicy.Handle(Async)<T>` methods to use the `RetryDelay`.  
- Add `DefaultRetryProcessor.Retry` method overloads and `RetryInfinite` method to handle `Action` with `RetryDelay` parameter.  
- Add `DefaultRetryProcessor.Retry<T>` method overloads and `RetryInfinite<T>` method to handle `Func<T>` with `RetryDelay` parameter.  
- Add `DefaultRetryProcessor.RetryAsync` method overloads and `RetryInfiniteAsync` method to handle `Func<CancellationToken, Task>` with `RetryDelay` parameter.  
- Add `DefaultRetryProcessor.RetryAsync<T>` method overloads and `RetryInfiniteAsync<T>` method to handle `Func<CancellationToken, Task<T>>` with `RetryDelay` parameter.  
- Add internal `DelayProvider` class.  
- Refactoring to use the `DelayProvider` class in the `DelayErrorProcessor` class.  
- Introduce `RetryProcessingErrorInfo` class, add `ProcessingErrorInfo.CurrentContext` property, deprecate `ProcessingErrorInfo.CurrentRetryCount` property.  
- DRY refactoring and new tests for `RetryPolicy.WithWait` method overloads.  
- Add doc comments to `IRetryProcessor` class.  
- Add doc comments to `RetryCountInfoOptions` class and `RetryCountInfo` struct.  
- Update 'RetryPolicy' README Chapter.  
- Update 'TryCatch' README Chapter.  


## 2.18.14

- Introduce the `NonEmptyCatchBlockFilter.ExcludeErrorSet`, `NonEmptyCatchBlockFilter.IncludeErrorSet` methods.
- Introduce the `CatchBlockHandlerFactory.FilterExceptionsByIncluding(IErrorSet)`, `CatchBlockHandlerFactory.FilterExceptionsByExcluding(IErrorSet)` methods.
- DRY refactoring of the method that gets the `IsError` property of the `TryCatchResult` and `TryCatchResult<T>` classes.
- Remove redundant `IErrorsAggregator<T>`, `IErrorsToStringAggregator` interfaces, `DefaultErrorsToStringAggregator` class,  internal `ProcessingErrorContext` constructor.
- Remove unnecessary value assignment (IDE0059) in tests.
- Add doc comments to `IErrorSet` interface and `ErrorSet` class.


## 2.18.11

- For `CatchBlockFilter`, `NonEmptyCatchBlockFilter` classes. add `ExcludeError`, `IncludeError` fluent methods overloads for inner exception types.
- For `NonEmptyCatchBlockFilter` class, add `CreateByIncluding`, `CreateByExcluding` fluent methods overloads with possibility to add inner exception types.
- Introduce `NonEmptyCatchBlockFilter.CreateByIncluding(IErrorSet)`,  `NonEmptyCatchBlockFilter.CreateByExcluding(IErrorSet) static methods`.
- For `FallbackFuncsProvider` class, add protected `SetFallbackAction`, `SetAsyncFallbackFunc` methods with `CancellationType` param.
- Directly return `Task` instead of await when converting async uncancelable non-generic delegate to precancelable when registering `PolicyResult` handler with `CancellationType.Precancelable` argument.
- Directly return `Task` instead of await when converting async uncancelable non generic fallback delegate to cancelable with `CancellationType.Precancelable` argument.
- Throw a `NotImplementedException` exception in the `PolicyResult.SetErrors` protected method.
- DRY refactoring for `IncludeError`, `ExcludeError` methods of `CatchBlockFilter`, `NonEmptyCatchBlockFilter` classes.
- Update 'FallbackPolicy' README Chapter.
- Update 'Error filters' README Chapter.
- Update 'TryCatch' README Chapter.


## 2.18.4

- Introduce the `FromInnerError`, `WithInnerError` methods of the `ErrorSet` class.
- Introduce `TryCatchResult(<T>).IsSuccess` property.
- Directly return `Task` instead of await in asynchronous error processing scenarios with the `CancellationType.Precancelable` argument.
- Use `Task.GetAwaiter().GetResult()` instead of the `Task.Wait` method in sync-over-async error processing scenarios with the `CancellationType.Precancelable` argument.
- Remove the obsolete `ProcessingErrorContext.FromRetry` method.
- Add README main content.
- Update 'PolicyResult' README Chapter.


## 2.18.0

- Introduce `IncludeErrorSet(IErrorSet)`, `ExcludeErrorSet(IErrorSet)` methods for `PolicyCollection`.
- Introduce `IncludeErrorSet(IErrorSet)` and `ExcludeErrorSet(IErrorSet)` extension methods for the `PolicyDelegateCollection(<T>)` classes.
- Introduce the `TryCatchBase` class and the `ITryCatch<T>` interface for dependency injection scenarios.
- Introduce `ITryCatch.HasCatchBlockForAll` property.
- Dispose of `CancellationTokenSource` objects in tests where it was absent.
- Add code coverage badge.
- Update 'Error filters' README Chapter.
- Update 'TryCatch' README Chapter.


## 2.17.0

- Introduce `IErrorSet` interface and `ErrorSet` class.
- Introduce `IncludeErrorSet(IErrorSet)`, `ExcludeErrorSet(IErrorSet)` extension methods for policy processors.
- Introduce `IncludeErrorSet(IErrorSet)`, `ExcludeErrorSet(IErrorSet)` extension methods for library policies (`RetryPolicy`, `SimplePolicy` and `FallbackPolicy`).
- Add the `ITryCatch.ExecuteAsync` extension methods with the `configureAwait` = false parameter.
- Introduce `TryCatchResultBase.ExceptionHandlerIndex` property.
- Improve conversion of `PolicyResult(<T>)` class to `TryCatchResult(<T>)` class.
- Add  'TryCatch' README Chapter.


## 2.16.21

- `TryCatch/TryCatchBuilder` classes now support more than two `CatchBlockHandler`s.


## 2.16.20

- Introduce the `TryCatch` class, which implements the `ITryCatch` interface with methods for executing sync or async, generic or non-generic delegates that return `TryCatchResult(<T>)` class (no more than two `CatchBlockHandler` supported so far).
- Introduce the `ITryCatchBuilder` interface and `TryCatchBuilder` class.
- Add `NonEmptyCatchBlockFilter.ToCatchBlockHandler` and `CatchBlockForAllHandler.ToTryCatch` methods.
- Add `ToTryCatchBuilder` and `ToTryCatch` extension methods to the `CatchBlockFilteredHandler` class.
- Add CODE_COVERAGE.md.
- Update 'Calling Func and Action delegates in a resilient manner' README Chapter.


## 2.16.16

- Hotfix introducing new `CatchBlockHandlerFactory` class to prevent inconsistent creation of `CatchBlockHandler` subclasses.  


## 2.16.15

- Introduce the `CatchBlockHandler` class and the `CatchBlockFilteredHandler` and `CatchBlockForAllHandler` subclasses.
- Introduce new `InvokeWithSimple(Async)` extension methods with a `CatchBlockHandler` parameter for non-generic delegates.
- Introduce new `InvokeWithSimple(Async)<T>` extension methods with a `CatchBlockHandler` parameter for generic delegates.
- Introduce the `NonEmptyCatchBlockFilter` class.
- Add the `RetryProcessingErrorInfo` and `RetryProcessingErrorContext` classes to process exceptions by a retry processor in a more object-oriented way.
- Add `CatchBlockFilter.Empty()` static method.
- Upgrade tests to NUnit.4.1.0.
- Update 'Calling Func and Action delegates in a resilient manner' README Chapter.
- Update 'SimplePolicy' README Chapter.


## 2.16.9

- Add `CatchBlockFilter` class and use it in the `SimplePolicyProcessor` class.
- Introduce new `Action.InvokeWithSimple` extension method with an `CatchBlockFilter` parameter.
- Introduce new `Func<CancellationToken, Task>.InvokeWithSimpleAsync` extension method with an `CatchBlockFilter` parameter.
- Introduce new `Func<T>.InvokeWithSimple` extension method with an `CatchBlockFilter` parameter.
- Introduce new `Func<CancellationToken, Task<T>>.InvokeWithSimpleAsync<T>` extension methods with an `CatchBlockFilter` parameter.
- Delegates, when included as part of `PolicyDelegate` in a collection, are handled error-free even if a policy rethrows an exception.
- Set the `PolicyResult.ErrorFilterUnsatisfied` property to `true` when a delegate is handled as part of a `PolicyDelegate` by the `PolicyDelegateCollection(T)` and an exception is rethrown because the error filter is not satisfied.
- Force the non-generic async fallback delegate converted from `Func<Task>` to throw `OperationCanceledException` if cancellation has already occurred.
- Rename the incorrect filename PolicyProcessorTests.cs to ExceptionFilterTests.cs.


## 2.16.1

- Introduce `IncludeInnerError<TInnerException>` and `ExcludeInnerError<TInnerException>` methods for `PolicyCollection`.
- Introduce `IncludeInnerError<TInnerException>` and `ExcludeInnerError<TInnerException>` methods for the `PolicyDelegateCollection`  and `PolicyDelegateCollection<T>` classes.
- Introduce `FallbackFuncsProvider` class.
- New constructors for `FallbackPolicy` classes that accept the `FallbackFuncsProvider` parameter.
- Introduce `PolicyCollection.WithFallback(FallbackFuncsProvider)` method.
- Add `PolicyCollection.WithFallback` overloaded methods with `onlyGenericFallbackForGenericDelegate` parameter.
- Refactor the constructors of `FallbackPolicy` classes to accept the new `onlyGenericFallbackForGenericDelegate` parameter.
- Improved performance for `FallbackPolicy`, since no more `Expression`s are used to store generic fallback functions.
- Made `SimplePolicyProcessor` and `SimplePolicy` rethrow exception if error filter is unsatisfied.
- Minimize the number of calls to the `Expression.Compile` method in the `PolicyProcessor.ExceptionFilter` class.
- Fix issue #93.
- Introduce `PolicyResult.FailedHandlerIndex` property.
- Made `SimplePolicyProcessor` class sealed.
- Update 'Error processors' README Chapter.
- Add doc comments to `IBulkErrorProcessor`, `IFallbackProcessor` interfaces and `FallbackPolicy` class.


## 2.15.0

- Introduce `SetPolicyResultFailedIf(<T>)` methods for the `PolicyCollection` and the `PolicyDelegateCollection(<T>)` classes.
- Introduce `IncludeInnerError<TInnerException>` and `ExcludeInnerError<TInnerException>` methods for policy processors and library policies(`RetryPolicy`, `SimplePolicy` and `FallbackPolicy`).
- Cross-synchronisation support for invoking a non-generic fallback delegate when a generic one is not set.
- Reduce allocations by using only a single instance of the `RetryErrorContext` class in Retry processing.
- Add `Apply<T>` extension method to `Action<T>` delegate.
- Correct the doc comments for the `SetPolicyResultFailedIfInner` methods of the library policies.
- Update 'PolicyResult handlers' README Chapter.


## 2.14.0

- Introduce `WithInnerErrorProcessorOf<TException>` overloaded methods for policy processor interfaces, `BulkErrorProcessor`, library policies(`RetryPolicy`, `SimplePolicy` and `FallbackPolicy`), `PolicyDelegateCollection(<T>)` and `PolicyCollection`.
- Introduce `SetPolicyResultFailedIf(<T>)` methods for library policies(`RetryPolicy`, `SimplePolicy` and `FallbackPolicy`).


## 2.12.1

- Introduce `IncludeErrorSet<TException1, TException2>` and `ExcludeErrorSet<TException1, TException2>` extension methods for the `PolicyDelegateCollection(<T>)` classes.
- Introduce `PolicyResult(<T>).CriticalError` property.
- Introduce `PolicyResultHandlingException.HandlerIndex` property.
- Fix issue (#83): the `PolicyResult` handler index in the collection should be correct when adding generic and non generic handlers consecutively.
- Upgrade tests to Nunit 4.0.1.
- Update 'Error filters' README Chapter.
- Edit 'PolicyResult handlers' README Chapter.
- Edit 'PolicyDelegateCollection' README Chapter.
- Correct 'Policy wrap' README Chapter example.


## 2.11.1

- Introduce `IncludeErrorSet<TException1, TException2>` methods for policy processors, library policies(`RetryPolicy`, `SimplePolicy` and `FallbackPolicy`) and `PolicyCollection`.
- Introduce `ExcludeErrorSet<TException1, TException2>` methods for policy processors, library policies(`RetryPolicy`, `SimplePolicy` and `FallbackPolicy`) and `PolicyCollection`.
- Slightly improve performance by using the equality operator instead of the `Equals` method for comparing types in generic error filters.
- Add new 'PolicyDelegateCollectionResult' README chapter.
- Update examples in the 'Policy wrap' README chapter.
- Add docs for the `ExcludeError`, `IncludeError` extension methods in the  `RetryProcessorErrorFiltering`, `FallbackProcessorErrorFiltering`, `SimplePolicyProcessorErrorFiltering` classes.


## 2.10.0

- Introduce `PolicyCollection.ExcludeErrorForLast`, `PolicyCollection.IncludeErrorForLast` extension methods.
- Introduce `PolicyDelegateCollection(<T>).ExcludeErrorForLast`, `PolicyDelegateCollection<T>.IncludeErrorForLast` extension methods.
- Introduce `PolicyDelegateCollectionResult(<T>).IsCanceled` property.
- The condition that the`PolicyDelegateCollectionResult(<T>).IsSuccess`  property be true was reinforced by the condition that the `IsFailed` and `IsCanceled` properties be both equal false.
- The condition that the property `PolicyDelegateCollectionResult<T>.Result` not be equal to `default` was strengthened by the condition that the `IsSuccess` property be true.
- 'The 'PolicyDelegateCollection' chapter in the README was rewritten.


## 2.9.1

- Introduce `PolicyDelegateCollection(<T>).WithErrorProcessorOf` and `PolicyDelegateCollection(<T>).WithErrorProcessor` extension methods.
- Introduce `PolicyCollection.WithErrorProcessorOf` and `PolicyCollection.WithErrorProcessor` extension methods.
- Fix issue (#61): the handling of a `PolicyDelegateCollection` should fail fast when the collection was obtained from a `PolicyCollection` and the delegate is null.
- Fix the oversight related to cancellation in the `PolicyDelegatesHandler.HandleAllBySyncType` method.
- DRY refactoring for extension methods adding filters to a `IPolicyDelegateCollection`.
 

## 2.8.1

- Support a fluent interface for the `IBulkErrorProcessor` interface.
- Introduce default constructor for the `BulkErrorProcessor` class.
- Introduce `PolicyResult.IsPolicySuccess` property.
- Fix issue (#50) with the `PolicyResult(<T>).WrappedPolicyResults` property being empty when a wrapped `PolicyCollection(<T>)` didn't handle delegate.
- Reduce allocations by using only a single instance of  the`EmptyErrorContext` class when processing Simple and Fallback policies.
- Add the marker `ICanAddErrorProcessor` interface.


## 2.6.1

- Introduce `PolicyCollection.WrapUp`method.
- Introduce `Policy.ResetWrap` method.
- Fix issue (#43) when the `PolicyResult.SetFailed` method is not called in a policy result handler due to previous cancellation.
- Add `PolicyDelegateCollection(<T>).WithThrowOnLastFailed` extensions methods with `Func<IEnumerable<PolicyDelegateResult(<T>)>, Exception>` as a parameter.
- The `PolicyDelegateCollectionResult`'s `IsFailed` and `IsSuccess` properties are set once in the constructor now.
- Add `PolicyDelegateResult(<T>).IsCanceled` and `PolicyDelegateResult(<T>).Errors` properties.
- Remove redundant `PolicyDelegateCollectionException` - related internal classes.


## 2.4.0

- Add `IPolicyDelegateCollection(<T>).AddPolicyResultHandlerForLast` and  `PolicyCollection.AddPolicyResultHandlerForLast` methods.
- Add `PolicyDelegateResult(<T>).IsFailed`, `PolicyDelegateCollectionResult(<T>).IsFailed` properties.
- Add `PolicyDelegateResult(<T>).IsSuccess`, `PolicyDelegateCollectionResult(<T>).IsSuccess` properties. 
- Add `IPolicyBase.WrapUp` extension method.
- Changes that are non-breaking in the signature of `PolicyCollection.HandleDelegate(Async)(<T>)`  methods and add documentation.
- `PolicyDelegateResult(<T>)` classes' constructors were made internal.
- `PolicyDelegateCollectionResult(<T>)` classes' constructors were internal.
- Get rid of 'Moq' in tests.


## 2.0.5

- Reduce parameter passing overhead for policy processors.
- Fix two issues related to cancellation when handling the `PolicyDelegateCollection(<T>)` (#4, #11).
- Fix bug that caused a generic policy result handler to throw an exception when the delegate return type was different than the type of handler(#5). 
- Fix the bug with an unhandled  exception if the error filter throws(#19) and add new enum member `CatchBlockExceptionSource.ErrorFilter`.
- Some  methods of `IPolicyDelegateCollection (<T>)` are now extensions methods.
- DRY refactoring for policies and policy processors.


## 2.0.0

- Rename the `ErrorProcessorDelegate` class to `ErrorProcessorParam`.
- Add the `RetryErrorSaverParam` class and use it in the `DelegateInvoking(T)` and `PolicyDelegateCollectionRegistrar` classes for extensions methods.
- Rename the `PolicyDelegateBase.UseSync` property to `SyncType`.
- Support not cancelable `PolicyResult` handlers for policies, `PolicyCollection` and `PolicyDelegateCollection(<T>)` classes.
- Add new `WithErrorProcessorOf` extensions methods to `IPolicyProcessor` and `IPolicyBase` interfaces.
- The existed `DefaultErrorProcessor` class was renamed to `BasicErrorProcessor`, the new `DefaultErrorProcessor`was added.
- Rename the `ConvertToCancelableFuncType` enum to `CancellationType`.
- Some classes for extensions methods was renamed, splitted or dropped.


## 2.0.0-rc5  

- Add new `UseCustomErrorSaver` method to the `IRetryProcessor` interface.
- Add `IRetryProcessor.UseCustomErrorSaverOf(...)` extensions methods.
- Add `RetryPolicy.UseCustomErrorSaverOf` extensions methods.
- Refactor `IPolicyProcessor.WithErrorProcessorOf<T>(...)` overloaded methods.
- Rename `CatchBlockException.ProcessException` property to `ProcessingException`.
- Add `CatchBlockException.ExceptionSource` property.
- Change `IPolicyDelegateCollectionHandler<T>.HandleAsync` methods signature.
- Rename `ProcessErrorInfo` class to `ProcessingErrorInfo`.
- Change the method signature of the interfaces `IErrorProcessor` and `IBulkErrorProcessor`.
- Add `PolicyResult.PolicyName` property.
- Add `PolicyResult<T>.WrappedPolicyResults` property.
- Made `GetResults()` method from `PolicyDelegateCollectionException.ErrorResults` property.


## 2.0.0-rc4  

- Made `PolicyResult(<T>)` static creation methods internal.
- Add new `PolicyResult.ErrorsNotUsed` property.
- Extract the handling of `PolicyDelegateCollection(<T>)` into the new interfaces `IPolicyDelegateCollectionHandler(<T>)`.
- Add `IPolicyProcessor.WithErrorProcessor` generic extension method.
- Add `IPolicyDelegateCollection(<T>).HandleAll(Async)` extensions methods.
- Add `PolicyCollection.BuildCollectionHandlerFor` methods and `PolicyCollection.HandleDelegate(Async)` extensions methods.
- Refactor the `CatchBlockProcessErrorInfo` class and rename it to `ProcessErrorInfo`.
- Rename `InvokeParams` class to `ErrorProcessorDelegate`.


## 2.0.0-rc3  

- Introduce `PolicyResult.UnprocessedError` property.
- Remove `PolicyDelegateCollectionBase<T>.LastPolicyDelegate` property.
- Made `IPolicyBase.ToPolicyDelegate` extension methods without delegate param internal.
- Made `IHandlerRunnerBase` interface and its inheritors internal.
- Change the accessibility of all `IPolicyBaseExtensions` error filter related extension methods to internal.
- Change the accessibility of all `IPolicyProcessorExtensions` error filter related extension methods to internal.
- Drop redundant `ICanAddPolicyResultHandler<T>` interface.


## 2.0.0-rc2  

- Introduce `PolicyCollectin` class.
- More robust `PolicyDelegateCollection(<T>)` creation.
- All `PolicyDelegateCollection(<T>)` creation methods are named `Create`.
- Drop `PolicyDelegateCollection(<T>).SetCommonDelegate` method.
- New public `RetryPolicy.RetryInfo` property.
- Made `PolicyResult<T>.SetResult` method internal.
- Add `PolicyResult.FailedReason` property.
- Add  `AddPolicyResultHandler<T>`, `AddPolicyResultHandlerForAll<T>` methods for handling `PolicyResult<T>`.
- Add `PolicyDelegateCollectionResult(<T>).LastPolicyResult` property.
- Drop `PolicyDelegateCollectionResult.LastFailedError` property.


## 2.0.0-rc  

- Add `SimplePolicy`-related methods to extensions methods for delegates.
- `IPolicyProcessor` now has an `ErrorFilter` property. Methods that add filters were removed.
- Fix `PolicyDelegateCollectionExtensions.WithFallback` method signature
- Change the name of the `PolicyResult.IsOk` property to `NoError`.


## 2.0.0-alpha  

- Introduce `SimplePolicy` and `SimplePolicyProcessor` classes
- Drop the  `PolicyDelegateCollection(<T>).WithCommonResultErrorsHandler` method
- Alter the name of the policy `WithPolicyResultHandler` method to `AddPolicyResultHandler`
- Change the name of the `PolicyDelegateCollection(<T>).WithCommonDelegate` methods to `SetCommonDelegate`
- Policy `ForError` methods were renamed to `IncludeError`
- Rename `PolicyDelegateCollection(<T>).WithCommonResultHandler` to `AddPolicyResultHandlerForAll`
- Rename `PolicyDelegateCollection(<T>).ForError` methods to `IncludeErrorForAll`
- `PolicyDelegateCollection(<T>).ExcludeError` methods was renamed to `ExcludeErrorForAll`
- Get rid of `RetryErrorProcessor`, `FallbackErrorProcessor` classes.
- Rename the `PolicyHandledResult` and `PolicyHandledInfo` classes (now `PolicyDelegateResult` and `PolicyDelegateInfo` respectively) and the related classes.
- The name of the `PolicyDelegateCollection(T>)` creation methods is started with the `Create` prefix.


## 1.0.4  

- Reduce the number of public classes and methods that are made internal.

## 1.0.3

- Fix not correct IntelliSense order for RetryPolicy ctors (Issue #1)
- Fix bug with empty HandleResultErrors (Issue #2)
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
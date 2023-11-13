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
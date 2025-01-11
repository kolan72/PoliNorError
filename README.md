# PoliNorError  
[![nuget](https://img.shields.io/nuget/v/PoliNorError)](https://www.nuget.org/packages/PoliNorError/)
[![badge_shieldsio_linecoverage_brightgreen](https://github.com/kolan72/PoliNorError/assets/6088210/632190cb-f61a-4f0c-b2ae-c4cec5e0395d)](https://github.com/kolan72/PoliNorError/blob/main/CODE_COVERAGE.md)

![alt text](https://github.com/kolan72/kolan72.github.io/blob/master/images/PoliNorError6.png?raw=true)  

PoliNorError is a library that provides error handling capabilities through Retry and Fallback policies. The library has a specific focus on handling potential exceptions within the catch block and offers various configuration options.
Heavily inspired by  [Polly](https://github.com/App-vNext/Polly).

---
- [Key Features](#key-features)
- [Usage](#usage)
- [PolicyResult](#policyresult)
- [Error processors](#error-processors)
- [Error filters](#error-filters)
- [PolicyResult handlers](#policyresult-handlers)
- [RetryPolicy](#retrypolicy)
- [FallbackPolicy](#fallbackpolicy)
- [SimplePolicy](#simplepolicy)
- [PolicyDelegate](#policydelegate)
- [PolicyDelegateCollection](#policydelegatecollection)
- [PolicyDelegateCollectionResult](#policydelegatecollectionresult)
- [PolicyCollection](#policycollection)
- [Policy wrap](#policy-wrap)
- [TryCatch](#trycatch-since-version-21621)
- [Calling Func and Action delegates in a resilient manner](#calling-func-and-action-delegates-in-a-resilient-manner)
- [Usage recommendations](#usage-recommendations)
- [Nuances of using the library](#nuances-of-using-the-library)
---

## Key Features
- Implements two commonly used resiliency patterns - Retry and Fallback.
- Also provides `SimplePolicy` for simple handling.
- Put emphasize on error handling within the catch block.
- Extensibility: error handling within the catch block can be extended by error processors.
- Simplicity: one policy type for sync and async, and a generic and not generic delegate.
- Composability: policies and delegates can be composed into a single `PolicyDelegateCollection`.
- Flexible filters can be set for errors that should be handled.
- A policy or a collection of policies can be wrapped by another.
- Func and Action delegates can be called in a resilient manner.
- Catches exceptions not only thrown by a delegate, but also in error filters, error processors, and `PolicyResult` handlers.
- Convenient API with methods with minimum of optional parameters.
- High Test Coverage.
- Targets .NET Standard 2.0+

## Usage
The term *handling delegate* refers to the process of handling errors that occur when a delegate is executed. 
The types of delegates that can be handled include:
- `Action`
- `Func<T>`
- `Func<CancellationToken, Task>`
- `Func<CancellationToken, Task<T>>`

Handling delegate is performed through the use of policy processors, which are classes that implement policy-specific interfaces inherited from the `IPolicyProcessor` interface. Policy processors implicitly determine *policy inner rules* (further for simplicity referred to as *policy rules*) - built-in behavioral features that determine whether or not a policy can handle exception. For example, the policy rule for the Retry is that it can handle exceptions only until the number of permitted retries does not exceed.  

Within the catch block, the policy processor can contain [error processors](#error-processors) that can extra handle exceptions.  

But before error processing can start, [error filters](#error-filters) need to be satisfied.  

So, the process of handling a delegate consists of checking error filters, running error processors and applying policy rules.  

A policy is a wrapper for the policy processor that adapts it to the `IPolicyBase` interface with `Handle` and `HandleAsync` methods for handling aforementioned delegates.  
The policy can have [`PolicyResult` handlers](#policyresult-handlers) that handle the `PolicyResult` after the policy processor has finished.  

![Set up for handledelegate](/src/docs/diagrams/set-up-for-handle-delegate.png)

Below are some examples of how policies and policy processors are used.
For retries using default retry policy processor:
```csharp
var result = RetryProcessor
			.CreateDefault()
			.WithWait(TimeSpan.FromMilliseconds(300))
			.Retry(() => DoSomethingRepeatedly()), 5);
```
With [`RetryPolicy`](#retrypolicy), more complex case:
```csharp
var result = await new RetryPolicy(5)
	                 .ExcludeError<DbEntityValidationException>()
			 .WithWait((currentRetry) => TimeSpan.FromSeconds(Math.Pow(2, currentRetry)))
			 .AddPolicyResultHandler<int>((pr) =>
							{ 
								if (pr.IsCanceled) 
									logger.Error("The operation was canceled.");
							}
						 )
			.HandleAsync(async (ct) => await dbContext.SaveChangesAsync(ct), token);
```
For fallback using default fallback policy processor:
```csharp
var result = FallbackProcessor
                           .CreateDefault()
                           .IncludeError<ObjectDisposedException>()
                           .WithErrorProcessorOf((ex) => logger.Error(ex.Message))
                           .Fallback(someDisposableObj.SomeMethod, 
                                    (_) => new SomeFallbackClass().SomeFallbackMethod());
```
With [`FallbackPolicy`](#fallbackpolicy):
```csharp
var result = new FallbackPolicy()
                             .IncludeError<ObjectNotFoundException>()
                             .WithFallbackFunc<Email>(() => UserManager.GetGuestEmail())
                             .Handle(() => UserManager.GetUserEmail(userId));
```
The results of handling are stored in the [PolicyResult](#policyresult) class.  

A policy can be combined with a delegate in the [`PolicyDelegate`](#policydelegate) class. The `PolicyDelegate` object, in turn, can be added to the [`PolicyDelegateCollection`](#policydelegatecollection). In this case, each delegate will be handled according to its policy.  

You can also create a [`PolicyCollection`](#policycollection) (appeared in _version_ 2.0.0-rc2) for handling single delegate.  

The classes `PolicyResult`, `PolicyDelegate`, `PolicyDelegateCollection` and some other handling-related classes have corresponding generic versions.

### PolicyResult
Handling begins when an exception occurs during the execution of the delegate. At first, exception will be stored in the `Errors` property (for retry-related classes, this is by default and can be customized).  
Later on, the policy processor will try to process error and populate the other `PolicyResult` properties.  

The most crucial property is the `IsFailed` property. If it equals `true`, the delegate was not able to be handled.  
It can happen due to these reasons:
-   The delegate  to handle is null.
-   The exception cannot be handled due to policy rules.
-   The error filter conditions are not satisfied (the  `ErrorFilterUnsatisfied`  property will also be set to `true`).
-   A critical exception has occurred within the catch block, specifically related to the saving exception for  `RetryPolicy`  or calling the fallback delegate for  `FallbackPolicy` (the  `IsCritical`  property of the  `CatchBlockException`  object will also be set to  `true`).
-   An exception has occurred when applying an error filter. In this case, the exception is also treated as critical and handling is interrupted.
 -  The cancellation occurs after the first call of the handling delegate, but before the execution flow enters in the `PolicyResult` handler.
 -  If delegate is handled by policy within collection and exception is unhandled (see [`SimplePolicy`](#simplepolicy) that can rethrow exception).
 -  If the handling result cannot be accepted as a success, and a policy is in use, you can set `IsFailed` to true in a `PolicyResult` handler by using the `SetFailed` method (or `SetPolicyResultFailedIf(<T>)(Func<PolicyResult(<T>), bool> predicate)` policy method since _version_ 2.14.0).  
 
To find out why `IsFailed` was set to true, there is a property called `FailedReason`. It equals: 
- `PolicyResultFailedReason.DelegateIsNull` and `PolicyResultFailedReason.PolicyResultHandlerFailed` for the first and last cases, respectively, 
- `PolicyResultFailedReason.UnhandledError` when a policy (re-)throws an exception (since _version_ 2.16.9), 
- and `PolicyResultFailedReason.PolicyProcessorFailed` for the others.  

Having `IsFailed` true, you can check the `UnprocessedError` property (appeared in version 2.0.0-rc3) to see if there was an exception that was not handled properly within the catch block.

The `IsSuccess`property indicates success of the handling. If it is true, it means that not only `IsFailed` equals false, but also `IsCanceled`, indicating that no cancellation occurred during handling.  
The `NoError` property makes sure that there were no exceptions at all when calling the handling delegate.  
The `IsPolicySuccess` property (since _version_ 2.8.1) indicates that, despite errors during the handling, the policy handled the delegate successfully.  

You might wonder why there are so many success-related properties. See [Nuances of using the library](#nuances-of-using-the-library)  for a detailed answer.

If an exception occurs within the catch block, it will be stored in the  `CatchBlockErrors`  property that is collection of the `CatchBlockException`  objects. For a critical exception, as mentioned above, the `CatchBlockException.IsCritical` property will be equal to true.  
The `CriticalError` property represents a critical exception itself or wrapped in the `AggregateException` (since _version_ 2.12.1).  

For generic `Func` delegates, a return value will be stored in the `Result` property if the handling was successful (except for `SimplePolicy`, where `Result` can remain the default value even on success) or there were no errors at all.  

### Error processors
In the common case, an error processor is a class that implements the `IErrorProcessor` interface. For instance, the class `DelayErrorProcessor` specifies amount of delay time before continuing with the handling process.  

You can add your object with the implementation  of `IErrorProcessor`  to a policy or policy processor by using the `WithErrorProcessor` method.  
But the easiest way to add an error processor is to use the `WithErrorProcessorOf` method overloads with this list of asynchronous:

- `Func<Exception, Task>`
- `Func<Exception, CancellationToken, Task>`
- `Func<Exception, ProcessingErrorInfo, Task>`
- `Func<Exception, ProcessingErrorInfo, CancellationToken, Task>`

or synchronous  delegates

- `Action<Exception>`
- `Action<Exception, CancellationToken>`
- `Action<Exception, ProcessingErrorInfo>`
- `Action<Exception, ProcessingErrorInfo, CancellationToken>`

, or a pair of delegates from both lists if you plan to use a policy in sync and async handling scenarios.  
The last two delegates have the `ProcessingErrorInfo` argument. This type contains a policy alias and it's subtype may also contain the current context of the policy processor, such as the current retry for the `RetryPolicy`.  

You can also add an error processor for `InnerException` using the `WithInnerErrorProcessorOf` method overloads (since _version_ 2.14.0), for example:
```csharp
var result = new SimplePolicy()
	.WithInnerErrorProcessorOf<NullReferenceException>
			((_) =>
			//This line is only printed to the Console when a NullReferenceException is thrown.
			Console.WriteLine("Null!"))
	.Handle(() => Task.Run(CanThrowNullOrOtherException).Wait());
```

Note that the error processor is added to the *whole* policy or policy processor, so its `Process` or `ProcessAsync` method will be called depending on the execution type of the policy handling method. If an error processor was created by a delegate of a particular execution type, the library can utilize sync-over-async or `Task` creation to obtain its counterpart.  

Error processors are handled one by one by the `BulkErrorProcessor` class. To customize this behavior, you could implement the `IBulkErrorProcessor` interface and use one of the policy or policy processor class constructors.  

To have a common error processor set for more than one policy, create a common `BulkErrorProcessor`(supporting a fluent interface since _version_ 2.8.1) and inject it as a parameter into the each policy constructor:
```csharp
var bulkErrorProcessor = new BulkErrorProcessor().WithErrorProcessorOf(logger.Error);
//These two policies have the same error processor set.
var simplePolicy = new SimplePolicy(bulkErrorProcessor);
var fallbackPolicy = new FallbackPolicy(bulkErrorProcessor).WithFallbackAction(DoSomething);
```

Note that if cancellation occurs during `BulkErrorProcessor` execution, delegate handling will be interrupted, and the `IsFailed` and `IsCanceled` properties of the `PolicyResult` will be set to true.

### Error filters
If no filter is set, the delegate will try to be handled with any exception.  
You can specify an error filter for the policy or the policy processor by using `IncludeError<TException>` and `ExcludeError<TException>` methods overloads:
```csharp
//Using generic methods:
 var result = new FallbackPolicy()
                                .IncludeError<SqlException>() 
                                .ExcludeError<SqlException>(ex => ex.Number == 1205)
                                .WithFallbackAction(() => cmd2.ExecuteNonQuery())
                                .Handle(() => cmd1.ExecuteNonQuery());

//...or non-generic methods that accept Expression<Func<Exception, bool>> as an argument:
 var result = new FallbackPolicy()
                                .IncludeError(ex => ex.Source == "MySource") 
                                .WithFallbackAction(DoSomething)
                                .Handle(DoSomethingThatThrowsExceptionWithSource);

```
An exception is permitted for processing if any of the conditions specified by `IncludeError` are satisfied and all conditions specified by `ExcludeError` are unsatisfied.  

There are no limitations on the number of filter conditions for both types.  

If you want to add a filtering condition based on two types of exceptions, you can use `IncludeErrorSet<TException1, TException2>` and `ExcludeErrorSet<TException1, TException2>` shorthand methods (since _version_ 2.11.1):
```csharp
var result = new RetryPolicy(1)
				.ExcludeErrorSet<FileNotFoundException, DirectoryNotFoundException>()
				.Handle(() => File.Copy(filePath, newFilePath));
```
To set an error filter based on a set of exception types, you can also use the `IncludeErrorSet`/`ExcludeErrorSet` methods, which accept the argument of the interface type `IErrorSet` (since _version_ 2.17.0).  
The library has the `ErrorSet` class that implements this interface, and the previous example can be overwritten:
```csharp
var excludeErrorSet = ErrorSet
		.FromError<FileNotFoundException>()
		.WithError<DirectoryNotFoundException>();

var result = new RetryPolicy(1)
		.ExcludeErrorSet(excludeErrorSet)
		.Handle(() => File.Copy(filePath, newFilePath));
```

You can also filter exceptions by their `InnerException` property using these methods (since _version_ 2.15.0):  

- `IncludeInnerError<TInnerException>`  
- `ExcludeInnerError<TInnerException>`  

For example, there is a service that uses `HttpClient`, and we want to fallback response only if `HttpRequestException` has an inner exception of type `SocketException` or `WebException`:  
```csharp
var policyResult = await new FallbackPolicy()
				.WithFallbackFunc<SomeResponse>((_) => new FallbackResponse())
				.IncludeInnerError<WebException>()
				.IncludeInnerError<SocketException>()
				.WithErrorProcessorOf((ex) => logger.Error(ex))
				.AddPolicyResultHandler<SomeResponse>((pr) => {
					if (pr.IsPolicySuccess)
						//The line will be printed here
						//only if HttpRequestException will have
						//SocketException or WebException inner exception type
						Console.WriteLine("Fallback data: " + pr.Result.SomeData.ToString());
				})
				.HandleAsync(serviceThatUseHttpClient.GetSomethingAsync);

```

Since _version_ 2.18.4, this example could be rewritten using `ErrorSet`, which is constructed from `HttpRequestException` exception and inner `WebException` and `SocketException` exceptions:  
```csharp
var errorSet = ErrorSet
				.FromError<HttpRequestException>()
				.WithInnerError<WebException>()
				.WithInnerError<SocketException>();

var policyResult = await new FallbackPolicy()
				.WithFallbackFunc<SomeResponse>((_) => new FallbackResponse())
				.IncludeErrorSet(errorSet)
				.WithErrorProcessorOf((ex) => logger.Error(ex))
				.AddPolicyResultHandler<SomeResponse>((pr) =>
				{
				...
				})
				.HandleAsync(serviceThatUseHttpClient.GetSomethingAsync);
```

If filter conditions are unsatisfied, error handling break and set both the `IsFailed` and `ErrorFilterUnsatisfied` properies to `true`.

### PolicyResult handlers
To handle a `PolicyResult(<T>)` object after a policy processor has handled a delegate, add a `PolicyResult` handler using the `AddPolicyResultHandler` or `AddPolicyResultHandler<T>` methods for non-generic and generic delegates, respectively.  
The full list of delegates that can be handlers and accepted as arguments for these methods is as follows:

- `Action<PolicyResult>`
- `Action<PolicyResult, CancellationToken>`
- `Func<PolicyResult, Task>`
- `Func<PolicyResult, CancellationToken, Task>`
- `Action<PolicyResult<T>>`
- `Action<PolicyResult<T>, CancellationToken>`
- `Func<PolicyResult<T>, Task>`
- `Func<PolicyResult<T>, CancellationToken, Task>`

Note that when handling a generic delegate, all generic handlers that do not match the return type of the delegate and non-generic handlers are ignored.  
Similarly, when handling a non-generic delegate, only non-generic handlers will be executed.  

For example:
```csharp
var result = await new RetryPolicy(5)
                            .AddPolicyResultHandler<int>((pr) => { if (pr.NoError) logger.Info("There were no errors.");})
                            .HandleAsync(async (ct) => await dbContext.SaveChangesAsync(ct), token);
```
In the `PolicyResult` handler, it is possible to set the `IsFailed` property to true by using `PolicyResult.SetFailed()` method.  
For the same purpose, use the `SetPolicyResultFailedIf(<T>)(Func<PolicyResult(<T>), bool> predicate)` policy method, which adds a special handler that sets `PolicyResult(<T>).IsFailed` to `true` only if the executed predicate returns `true` (since _version_ 2.14.0).  
It may be helpful if for some reason the `PolicyResult` object, as a result of handling, can't be accepted as a success and may require additional work, see [`PolicyDelegateCollection`](#policydelegatecollection) for details.  

Exceptions in a `PolicyResult` handler are allowed without affecting other `PolicyResult` properties.  The `PolicyResult.PolicyResultHandlingErrors` property is a collection of `PolicyResultHandlingException` exceptions. This exception has the `InnerException` property with the exception that occurred and the `HandlerIndex` property with the handler index that caused the exception in the handlers collection (since _version_ 2.12.1).  

If a cancellation occurs at the stage of the `PolicyResult` handling, the process of running the `PolicyResult` handlers will not be interrupted.  

Methods that add handlers to collections (see below) are as follows:  

- `AddPolicyResultHandlerForLast(<T>)` -  adds `PolicyResult(<T>)` handler to the last (newly added) element of collection  
- `AddPolicyResultHandlerForAll(<T>)` -   adds `PolicyResult(<T>)` handler for all elements that have already been added to collection  

These methods take the same arguments as the `AddPolicyResultHandler` methods above.  

### RetryPolicy
The policy rule for the `RetryPolicy` is that it can handle exceptions only until the number of permitted retries does not exceed, so it is the most crucial parameter and is set in policy constructor.  

Note that retries start from 0. In some cases it may be more appropriate to use the term *attempt*, which means running a delegate and always starts at 1. I.e. on the time scale:

```
attempts	:	1		2		...			n  
retries		:	0		1		...			n-1
```

For a `RetryPolicy`, you can get the current attempt in error processor:
```csharp
var retryResult = new RetryPolicy(2)
			.WithErrorProcessorOf((Exception ex, ProcessingErrorInfo pi) =>
				logger.LogError(ex, 
				"Policy processed exception on {Attempt} attempt:", 
				((RetryProcessingErrorInfo)pi).RetryCount + 1))
			.Handle(ActionThatCanThrow);
```
where `RetryProcessingErrorInfo` is the subclass of `ProcessingErrorInfo`.  
Note that within the `RetryPolicy`, the `RetryProcessingErrorInfo` class is always used instead of the base `ProcessingErrorInfo` class, so the direct cast is always successful.  

You can also specify the delay time before next retry with `WithWait(TimeSpan)` method, or use one of the overloads with Func, returning TimeSpan, for example:
```csharp
            var policy = new RetryPolicy(5)
                                    .WithWait((currentRetry, ex) =>
                                    {
                                        logger.Error(ex.Message);
                                        return TimeSpan.FromSeconds(Math.Pow(2, currentRetry));
                                    });
```
To retry infinitely, until it succeeds, use the`InfiniteRetries` method:
```csharp
            var possibleResult = await RetryPolicy
                                    .InfiniteRetries()
                                    .WithWait(TimeSpan.FromSeconds(1))
                                    .HandleAsync(async (ct) => 
                                                    await DoSomethingNearlyImpossibleAsync(ct), token);
```
These methods create the `DelayErrorProcessor` object behind the scenes.  
The `WithWait` method also has overload that accept the `DelayErrorProcessor` argument. This method allows you to customize the delay behavior by inheriting from the  `DelayErrorProcessor` class.  

Since _version_ 2.19.0, there is an error processor independent way to set the delay between retries - by using the `RetryDelay` class or its subclasses.  
Simply create a `RetryPolicy` using one of the constructors with the `RetryDelay` parameter, or call one of the `DefaultRetryProcessor.Retry(Async)` method overloads that accepts one.  
There are three ways to create a `RetryDelay`, represented by the `RetryDelayType` parameter:  

- `RetryDelayType.Constant` with corresponding `ConstantRetryDelay` subclass configured by `ConstantRetryDelayOptions`. For `baseDelay` = 200ms the time delay will be 200ms, 200ms, 200ms.  
- `RetryDelayType.Linear` with corresponding `LinearRetryDelay` subclass configured by `LinearRetryDelayOptions`. For `baseDelay` = 200ms and  default `SlopeFactor` = 1.0 (since _version_ 2.19.11) the time delay will be 200ms, 400ms, 600ms.  
- `RetryDelayType.Exponential` with corresponding `ExponentialRetryDelay` subclass configured by `ExponentialRetryDelayOptions`. For `baseDelay` = 200ms and  default `ExponentialFactor` = 2.0 the time delay will be 200ms, 400ms, 800ms.  

Since _version_ 2.19.5, the `ConstantRetryDelayOptions`, `LinearRetryDelayOptions`, and `ExponentialRetryDelayOptions` classes also have `MaxDelay` and `UseJitter` properties in their `RetryDelayOptions` base class:

- `MaxDelay` - the delay will not exceed this value, regardless of the number of retries. The default is `TimeSpan.MaxValue`.
- `UseJitter` - indicates if jitter is used. The default is `false`.  

You can also create `ConstantRetryDelay`, `LinearRetryDelay`, `ExponentialRetryDelay` classes using the `Create` static methods.

Using `RetryDelay` is a more accurate alternative to the above approach with `DelayErrorProcessor`. Note that unlike `DelayErrorProcessor`, the `RetryDelay` parameter allows you to configure only one delay for a retry policy or processor.  

Since _version_ 2.19.8 you can use the `RetryDelay` class or its subclasses [to call Func and Action delegates in a resilient manner](#calling-func-and-action-delegates-in-a-resilient-manner).  

For huge numbers of retries, memory-related exceptions, such as `OutOfMemoryException`, may occur while saving handling exceptions in the `PolicyResult.Errors` property. This exception will be handled, wrapped up in a `CatchBlockException`, and saved in the `PolicyResult.CatchBlockErrors`.  

If you want to interrupt the handling process after that, create a Retry policy or processor with the `failedIfSaveErrorThrow` parameter set to true. In this case, the `CatchBlockException.IsCritical` property will be set to true, as well as the `PolicyResult.IsFailed` property.  

Moreover, you can also customize error saving by calling the `UseCustomErrorSaver` or `UseCustomErrorSaverOf` methods to save errors elsewhere.  
These methods have the `IErrorProcessor` type or delegates that take an exception argument as a parameter:  
```csharp
var retryPolicy = new RetryPolicy(1)
			.UseCustomErrorSaverOf(async (ex, ct) => await db.SaveErrorAsync(ex, ct));
...
await retryPolicy.HandleAsync(DoSomethingAsync);
...
```
Note that unlike error processors, you can only have one custom error saver for a retry policy or processor.  
When error saving is customized, the `PolicyResult.ErrorsNotUsed` property will be set to true.  

For testing purposes there is a `RetryPolicy` constructor that has `Action<RetryCountInfoOptions>` parameter.  
`RetryPolicy` can be customized of your implementation of `IRetryProcessor` interface.  

### FallbackPolicy
The policy rule for the `FallbackPolicy` is that it can't handle error when the fallback delegate throws an exception.  
If it happens, this exception will be wrapped up in a `CatchBlockException` exception with the `IsCritical` property set to true, which will be saved in the `CatchBlockErrors` property of the `PolicyResult` object.  Additionally, the `PolicyResult.IsFailed` property will be set to true.  

You can setup this policy for different return types:
```csharp
  var userFallbackPolicy = new FallbackPolicy()
                                        .WithFallbackFunc<User>(() => UserManager.GetGuestUser())
                                        .WithFallbackFunc<Email>(() => UserManager.GetGuestEmail());
```
And use it for handling delegates:
```csharp
    //Somewhere in your code:
    //resultUser will contain guest user in the Result property if any error occurs 
    var resultUser = userFallbackPolicy.Handle(() => UserManager.GetUser(userName));
     
    //resultEmail will contain guest email in the Result property if any error occurs 
    var resultEmail = userFallbackPolicy.Handle(() => UserManager.GetUserEmail(userId));                                   
```
The whole list of methods, accepting fallback delegate as an argument:

-    `WithFallbackAction`
-    `WithAsyncFallbackFunc`
-    `WithFallbackFunc<T>`
-    `WithAsyncFallbackFunc<T>`

`FallbackPolicy` has constructor that accepts optional `onlyGenericFallbackForGenericDelegate`  parameter that equals false by default (since _version_ 2.16.1). This parameter is intended for use when a generic delegate is handled by a `FallbackPolicy` with no generic fallback delegate set.  
If it's `false`, the policy will attempt to find a registered non-generic fallback delegate, convert to (a-)synchronous counterpart if necessary, and execute it. A non-generic delegate of the same synchronous type has priority, and if it is found, no conversion takes place. Whether a non-generic fallback delegate was found and executed or not, the default value of type is returned.  
If `onlyGenericFallbackForGenericDelegate` is true, trivial `Func`s that return defaults will be called.  

With non-generic handling and only generic delegates present, no generic delegate is called. Instead, trivial functions are called, returning void or `Task`, possibly already obtained from counterpart.  

Note that error processors for fallback policies run *before* calling fallback delegate. This lets you cancel before calling the fallback delegate if you need to, but if you want to get fallback faster, don't add long-running error processors.  
`FallbackPolicy` can be customized of your implementation of `IFallbackProcessor` interface.  

### SimplePolicy
The `SimplePolicy` is a policy without rules. If an exception occurs, the `SimplePolicyProcessor` just stores it in the `Errors` collection and, if the error filters match, runs error processors. With policy result handlers, it can be helpful when a specific reaction to the result of handling is needed.  
For example, you could create a policy for copying or reading a file with a warning on the `FileNotFoundException` and logging an error for the other exceptions:  
```csharp
var fileNotFoundPolicy = new SimplePolicy()
						.IncludeError<FileNotFoundException>()
						.WithErrorProcessorOf((ex) => logger.Warning(ex.Message))
						.AddPolicyResultHandler((pr) =>
						{
							if (pr.IsFailed)
							{
								logger.Error(pr.UnprocessedError?.Message);
							}
						}
						)
						.AddPolicyResultHandler<string>((pr) =>
						{
							if (pr.NoError)
							{
								logger.Info("Result: {text}", pr.Result);
							}
							if (pr.IsFailed)
							{
								logger.Error(pr.UnprocessedError?.Message);
							}
						}
						);

//A warning  will be reported in the log if the FileNotFoundException occurs,
//and an error message  for all other exceptions.
var copyResult = fileNotFoundPolicy
				    .Handle(() => File.Copy(source, dest));

var readAllTextResult = fileNotFoundPolicy
				    .Handle(() => File.ReadAllText(source));
```

Note that for `SimplePolicy`  the `PolicyResult.IsSuccess` property will always be true if an exception satisfies the filters and no cancellation occurs.  
Therefore, when handling generic delegates, it's better to check the `NoError` property instead of the `IsSuccess` property to get the `PolicyResult.Result`.  
Note also, that the `SimplePolicy` can be helpful for exiting from the `PolicyDelegateCollection` handling soon, see [`PolicyDelegateCollection`](#policydelegatecollection) for details.  

`SimplePolicy` and its processor has constructor that accepts optional `rethrowIfErrorFilterUnsatisfied ` parameter that equals `false` by default (since _version_ 2.16.1).  
If it is `true`, the exception will be rethrown if the error filter is unsatisfied, and you can use a hybrid approach to handle exceptions:
```csharp
var sp = new SimplePolicy(true)
		.ExcludeError<ExceptionToHandleInCatchBlock>()
		.WithErrorProcessorOf((ex) => 
			Console.WriteLine(
			"The exception is handled by SimplePolicy: " + ex.Message));
try
{
	sp.Handle(CanThrowExceptionForCatchBlockOrOther);
}
catch (Exception ex)
{
	//We are only here with the `ExceptionToHandleInCatchBlock` exception.
	logger.Error(ex);
}
```

### PolicyDelegate
A `PolicyDelegate` just pack delegate with a policy into a single object.
You can create `PolicyDelegate` object from policy and delegate:
```csharp
var policyDelegate = new RetryPolicy(1).ToPolicyDelegate(() => query.GetData());
```
For  handling delegate by the `PolicyDelegate` simply call `Handle` or `HandleAsync` method:
```csharp
var result = policyDelegate.Handle();
```

### PolicyDelegateCollection
The `PolicyDelegateCollection(T)` class is a collection of a `PolicyDelegate(<T>)` that uses `List<PolicyDelegate(<T>)>` as an inner storage and implements the `IEnumerable<PolicyDelegate(<T>)>` interface.  

For handling collection just call  `HandleAll` or `HandleAllAsync` method.  
Handling `PolicyDelegateCollection(<T>)` is merely calling the `Handle(Async(<T>))` method for each element in the collection one by one in a sync or async manner, while the current policy `IsFailed` equals true and no cancellation occurs.  
```
PolicyDelegate_1_handling —— Failed ——> PolicyDelegate_2_handling —— Failed ——> ..
  |                                     |
  | —— Success_Or_Canceled ——> Exit     | —— Success_Or_Canceled ——> Exit

```
In a certain sense, it is remarkably similar to wrapping current Policy1 with a `Fallback` policy that uses the appropriate `PolicyDelegate2.Handle(Async)(<T>)` method as a fallback delegate, handling Delegate1, but storing all handling results in a flat collection of the `PolicyDelegateResult` object.  

You can create `PolicyDelegateCollection(<T>)` by using `Create` method.  
There are two different approaches for adding a `PolicyDelegate` to the collection.  

The first one is to use an existing policy or `PolicyDelegate` and these methods:
- `WithPolicy` (create the `PolicyDelegate` with a policy but without a delegate)
- `WithPolicyDelagate`
- `WithPolicyAndDelegate`
- `AndDelegate` (set delegate to last `PolicyDelegate` object in the collection)

The second approach is to use specific policy-related shorthand extensions methods and their overloads:
- `WithRetry`
- `WithWaitAndRetry`
- `WithInfiniteRetry`
- `WithWaitAndInfiniteRetry`
- `WithFallback`
- `WithSimple`

For the first approach example, if you wish to remove large folders from your disk when there is less than 40Gb of free space, and notify about it, create two policies and combine them in the `PolicyDelegateCollection` for further handling:
```csharp
            var checkFreeSpacePolicy = new SimplePolicy()
						//Or simply (since _version_ 2.14.0):
						//.SetPolicyResultFailedIf<long>((pr) => pr.NoError && pr.Result < DesiredFreeSpaceInBytes)
						.AddPolicyResultHandler<long>((pr) =>
						{
							if (pr.NoError && pr.Result < DesiredFreeSpaceInBytes)
							{
								//If free space is not enough we pass handling to the next PolicyDelegate in the collection:
								pr.SetFailed();
							}
						})
						.AddPolicyResultHandler<long>((pr) =>
						{
							if(pr.NoError && !pr.IsFailed)
								logger.Info("Free space is ok");
						})
						.WithErrorProcessorOf((Exception ex, ProcessingErrorInfo pei) =>
						{
							logger.Error(ex,
								"{policy} successfully handled exception.",
								pei.PolicyKind.ToString()+ "Policy");
						});

            var deleteFoldersPolicyDelegate = new SimplePolicy()
						.WithErrorProcessorOf((ex) => logger.Error(ex.Message))
						.AddPolicyResultHandler<long>((pr) =>
						{
							if (pr.NoError)
							{
								logger.Info("Total available space: {freeSpace} bytes", pr.Result);
							}
						}).ToPolicyDelegate(() => 
						{
						DeleteUselessLargeFolders();
						return GetFreeSpace();
						});

			var freeSpaceResult = PolicyDelegateCollection<long>
						.Create()
						.WithPolicyAndDelegate(checkFreeSpacePolicy, GetFreeSpace)
						.WithPolicyDelegate(deleteFoldersPolicyDelegate)
						.HandleAll();


//Somewhere in your code:
private void DeleteUselessLargeFolders()
{
	//Delete folders here...
}

private long GetFreeSpace() => new DriveInfo("D:").TotalFreeSpace;

private long DesiredFreeSpaceInBytes => 40 * (1024L * 1024L * 1024L);

```
Note that the `PolicyResult` handlers are executed in the order they are added, so add a `PolicyResult` handler that changes the `IsFailed` property before handlers that check it.  

For the second  approach, the recommended one for library policies, the previous example can be overwritten:  
```csharp
var freeSpaceResult = PolicyDelegateCollection<long>.Create()
							.WithSimple()
							.AndDelegate(GetFreeSpace)
							.AddPolicyResultHandlerForLast((pr) =>
							{
								if (pr.NoError && pr.Result < DesiredFreeSpaceInBytes)
								{
									//If free space is not enough we pass handling to the next PolicyDelegate in the collection:
									pr.SetFailed();
								}
							})
							.AddPolicyResultHandlerForLast((pr) =>
							{
								if (pr.NoError && !pr.IsFailed)
									logger.Info("Free space is ok");
							})
							//We can use this method for the collection since version 2.9.1
							.WithErrorProcessorOf((ex, pei) =>
							{
								logger.Error(ex,
								"{policy} successfully handled exception.",
								pei.PolicyKind.ToString() + "Policy");
							})
							.WithSimple((ErrorProcessorParam)logger.Error)
							.AndDelegate(() =>
							{
								DeleteLargeFolders();
								return GetFreeSpace();
						  	})
							.AddPolicyResultHandlerForLast((pr) =>
							{
								if (pr.NoError)
								{
									logger.Info($"Total available space: {pr.Result} bytes");
								}
							})
							.HandleAll();
```
In this example, note that when an exception occurs on getting free space, we exit from further handling due to `SimplePolicy` with an error message in the log.  
Note also how the `AddPolicyResultHandlerForLast<T>` method can be used for the `PolicyDelegateCollection<T>`.  

To set a common `PolicyResult(<T>)` handler for all items already added to the collection, use the `AddPolicyResultHandlerForAll(<T>)` method:  
```csharp
var result = PolicyDelegateCollection<IConnection>.Create()
                        .WithWaitAndRetry(3, TimeSpan.FromSeconds(2))
                        .AndDelegate(() => connectionFactory1.CreateConnection())
                        .WithRetry(5)
                        .AndDelegate(() => connectionFactory2.CreateConnection())
                        .AddPolicyResultHandlerForAll((pr) =>
						{
							//There is no need to add logging to each policy result handler -
							//the code below will log all errors
							//that occur during handling by two policies.
							foreach (var err in pr.Errors)
							{
								logger.Error(err.Message);
							}
						}
                        .HandleAll();
```

You can use `ExcludeErrorForAll` and `IncludeErrorForAll` methods to set filters on the entire collection:
```csharp
var result = PolicyDelegateCollection<int>.Create()
					.WithRetry(5)
					.AndDelegate(() => cmd1.ExecuteNonQuery())
					.WithFallback(() => (Int32)cmd3.ExecuteScalar())
					.AndDelegate(() => cmd2.ExecuteNonQuery())
					.ExcludeErrorForAll<SqlException>((ex) => ex.Number == 1205)
					.HandleAll();
``` 

You can also use the `BuildCollectionHandler()` method to get the `IPolicyDelegateCollectionHandler(T)` interface that has `Handle` and `HandleAsync` methods and pass it somewhere as a dependency injection parameter for further handling.  

Be careful when adding an existing `Policy` or `PoliyDelegate` to a collection, see [Nuances of using the library](#nuances-of-using-the-library) for details.

### PolicyDelegateCollectionResult
The results of handling a `PolicyDelegateCollection(<T>)` are stored in `PolicyDelegateCollectionResult(<T>)` class that implements `IEnumerable<PolicyDelegateResult(<T>)>` interface. The `PolicyDelegateResult(<T>)` class is a wrapper around `PolicyResult` that additionally contains `MethodInfo` of the delegate in the `PolicyMethodInfo` property.  
Properties of the `PolicyDelegateCollectionResult(<T>)` class:  

- `PolicyDelegateResults` - the collection of the results of the `PolicyDelegate(<T>)`s handling. The enumerator of this collection is used to implement the `IEnumerable<PolicyDelegateResult(<T>)>` interface.
- `IsFailed` - the `IsFailed` property value of the last element  of the `PolicyDelegateResults`  or `true` if the whole `PolicyDelegateCollection(<T>)` collection was not handled properly (for example, a common delegate for handling by `PolicyCollection` is null - see below).
- `IsCanceled` - the `IsCanceled` property value of the last element  of the `PolicyDelegateResults` or `true` if the cancellation occurred between the handling elements of the `PolicyDelegateCollection`.
- `IsSuccess` - the `IsSuccess` property value of the last element  of the `PolicyDelegateResults` provided both the `IsFailed` and `IsCanceled` properties are false.
- `PolicyDelegatesUnused` - the part of the `PolicyDelegateCollection(<T>)` collection that was not handled.  
- `LastPolicyResult` - the `Result` property value of the last element  of the `PolicyDelegateResults` or `null` if the results collection is empty.
- `LastPolicyResultFailedReason` - the `FailedReason` property value of the last element  of the `PolicyDelegateResults` or `PolicyResultFailedReason.DelegateIsNull` if a common delegate for handling by`PolicyCollection` is null - see below.
- `Result` - the `LastPolicyResult.Result` property value if the `IsSuccess` property is true and the `LastPolicyResult` property is not `null`, or `default` (for the generic version only).

When handling an empty `PolicyDelegateCollection(<T>)` collection, the `IsFailed`, `IsCanceled` and `IsSuccess` properties will be equal to `false` and  `LastPolicyResultFailedReason` will be equal to `PolicyResultFailedReason.None`.  

If the handling of the `PolicyDelegateCollection(<T>)` fails, with help the `WithThrowOnLastFailed` method it is possible to throw the `PolicyDelegateCollectionException(<T>)` exception instead of returning the `PolicyDelegateCollectionResult(<T>)`.  
This exception contains `InnerExceptions` property with exceptions added from `Errors` properties of all `PolicyResult`s. For the `PolicyDelegateCollectionException<T>` object, you can also obtain all `PolicyResult.Result`s by using `GetResults()` method.  

### PolicyCollection
Sometimes one delegate needs to be handled by many policies, and this can be done easily with the `PolicyCollection` class.  

If, for instance, you'd like to read a file that's currently being used by another process, you could try two retries and, if the error persists, copy the file to the temporary folder and access it from there:
```csharp
	var result = PolicyCollection.Create()
		.WithRetry(2) 
		.WithFallback(() =>
		{
			var newFilePath = Path.Combine(Path.GetTempPath(), Path.GetFileName(filePath));
			File.Copy(filePath, newFilePath, true);

			return File.ReadAllLines(newFilePath);
		})
		.AddPolicyResultHandlerForAll<string[]>(pr => 
			pr.Errors
				.ToList()
				.ForEach(ex => logger.Error(ex.Message)))

		//You can call `BuildCollectionHandlerFor(..)` method to build a collection handler
		//that has a `IPolicyDelegateCollectionHandler<string[]>` type
		//that can be passed somewhere for further handling
		//or handle delegate in place:
		.HandleDelegate(() => File.ReadAllLines(filePath));

	if (result.LastPolicyResult.IsSuccess) /*Or simply if(result.IsSuccess) since 2.4.0 version*/
	{
		result.Result.ToList().ForEach(l => Console.WriteLine(l));
	}
```

Furthermore, with the `PolicyCollection` :

- You can call the `BuildCollectionHandlerFor(commonDelegate)`  method to obtain the `IPolicyDelegateCollectionHandler(<T>)` interface and pass it somewhere as a dependency injection parameter.  
- If you want to create a `PolicyDelegateCollection` based on the collection of policies you just created, for example, to handle other delegates, you can call the `ToPolicyDelegateCollection(commonDelegate)` method.  Each element of the new collection will consist of a common delegate `commonDelegate` and one of the policies that have been added to `PolicyCollection` object.  

The `PolicyCollection` class has the same options for filtering errors and adding `PolicyResult` handlers as the `PolicyDelegateCollection` class.  

Be careful when adding an existing `Policy` to a collection, see [Nuances of using the library](#nuances-of-using-the-library) for details.

### Policy wrap
For wrap policy by other policy use `WrapPolicy` method, for example:
```csharp
	    var wrapppedPolicy = new RetryPolicy(3).ExcludeError<BrokerUnreachableException>();
            var fallBackPolicy = new FallbackPolicy()
                                            .WithFallbackFunc(() => connectionFactory2.CreateConnection());
            fallBackPolicy.WrapPolicy(wrapppedPolicy);
            var polResult = fallBackPolicy.Handle(() => connectionFactory1.CreateConnection());
```
Alternately, you could use the bottom-up approach and, after configuring the policy that will be wrapped, switch to the wrapper policy by using the `WrapUp` method (since _version_ 2.4.0):  
```csharp
var wrapperPolicyResult = await new RetryPolicy(2)
						.WithErrorProcessorOf(logger.Error)
						//We wrap up the current RetryPolicy by FallbackPolicy
						.WrapUp(new FallbackPolicy()
							.WithAsyncFallbackFunc(SendAlarmEmailAsync))
						//and switch to the last one here,
						.OuterPolicy
						//where we can further configure it 
						.WithPolicyName("WrapperPolicy")
						 //before handling a delegate
						.HandleAsync(async(_) => await SendEmailAsync("someuser@somedomain.com"));
```
![Policy WrapUp flow](/src/docs/diagrams/policy_wrapup_flow.png)

Behind the scenes wrapped policy's `Handle(Async)(<T>)`  method will be called as a handling delegate with throwing its `PolicyResult.UnprocessedError` if it fails or `PolicyResultHandlerFailedException` exception  if the `IsFailed` property was set to true in a `PolicyResult` handler.

Results of handling a wrapped policy are stored in the `WrappedPolicyResults` property of the wrapper `PolicyResult`.  

The example in the  [`PolicyCollection`](#policycollection) chapter could be rewritten to use a different approach by using the `WrapUp` method as well:  
```csharp
var policyResult = new RetryPolicy(2)
		.WrapUp(new FallbackPolicy())
		.OuterPolicy
		.WithFallbackFunc(() =>
		{
			var newFilePath = Path.Combine(Path.GetTempPath(),
										 Path.GetFileName(filePath));
			File.Copy(filePath, newFilePath, true);

			return File.ReadAllLines(newFilePath);
		})
		.AddPolicyResultHandler<string[]>((pr) => {
			if (!pr.NoError && pr.IsSuccess) /*Or simply if(pr.IsPolicySuccess) since 2.8.1 version*/
				Console.WriteLine("The file was copied into the Temp directory");
		})
		.Handle(() => File.ReadAllLines(filePath));

policyResult
	//Use this property to get exceptions that occur when RetryPolicy tries to handle the delegate.
	.WrappedPolicyResults
	.SelectMany(pd => pd.Result.Errors)
	.ToList()
	.ForEach(ex => logger.Error(ex.Message));

if (policyResult.IsSuccess)
{
	policyResult.Result.ToList().ForEach(l => Console.WriteLine(l));
}
```
You can wrap up a `PolicyCollection` itself using the `WrapUp` method as well (this example for _version_ 2.10.0).
```csharp
var outerPolicyResult = PolicyCollection
	.Create()
	//It is a 'stop' policy, that halts handling next policy delegate if the file is not found.
	.WithSimple()
	.IncludeErrorForLast<FileNotFoundException>()
	//Warning message in the log if the file is not found.
	.WithErrorProcessorOf((ex) => logger.Warning(ex.Message))
	.AddPolicyResultHandlerForLast<string[]>((pr) =>
	{
		//It is the handler for SimplePolicy, so check the NoError property
		//to ensure that there were no exceptions during handling.
		if (pr.NoError)
		{
			PrintResultInConsole(pr);
		}
		else if (pr.IsPolicySuccess)
		{
			Console.WriteLine($"This exception was caught by {pr.PolicyName}." +
				$"The exception type is not suitable for retries, exit from handling.");
		}
		else if (pr.IsFailed)
		{
			Console.WriteLine($"{pr.PolicyName} can't handle this exception, handling continues...");
		}
	})
	//If the file exists, we will try to read it twice using RetryPolicy:
	.WithRetry(2)
	//All failed policies exceptions will be logged here.		
	.AddPolicyResultHandlerForAll<string[]>(pr =>
	{
		if (pr.IsFailed)
		{
			logger.Error($"Exceptions after {pr.PolicyName}:");
			pr.Errors.ToList().ForEach(ex => logger.Error(ex.Message));
		}
	})		
	//Wrap up the PolicyCollection by FallbackPolicy
	.WrapUp(new FallbackPolicy())
	.OuterPolicy
	.WithFallbackFunc(() =>
	{
		var newFilePath = Path.Combine(Path.GetTempPath(),
									 Path.GetFileName(filePath));
		File.Copy(filePath, newFilePath, true);

		return File.ReadAllLines(newFilePath);
	})
	.AddPolicyResultHandler<string[]>((pr) => {
		if(pr.IsSuccess)
		{
			if (pr.IsPolicySuccess)
			{
				Console.WriteLine("The file was copied into the Temp directory");
			}
			//Note that if the file was successfully read
			//during the retry policy handling,
			//its lines will also be printed here.
			PrintResultInConsole(pr);
		}
	})
	.Handle(() => File.ReadAllLines(filePath));

private static void PrintResultInConsole(PolicyResult<string[]> pr) => pr.Result?.ToList().ForEach(l => Console.WriteLine(l));
```
The `PolicyCollection.WrapUp` method has an optional parameter of type `ThrowOnWrappedCollectionFailed`, that by default is set to `ThrowOnWrappedCollectionFailed.LastError`  with behind the scenes throwing `PolicyResult.UnprocessedError` of failed policy (usually the last one in the `PolicyCollection`).  

You can use `ThrowOnWrappedCollectionFailed.CollectionError` if you want to deal with all the exceptions that happen when `PolicyCollection` handles delegate. In this case, the `PolicyDelegateCollectionException(<T>)` will be thrown as a result of failed handling of wrapped `PolicyCollection`.  

For example, there is a service that should not be used if there are multiple `TimeoutExceptions` within a certain time period.  
Your strategy may be to repeat a certain number of retries with a second interval, then with the half-minute interval, and then repeat this set of retries after an hour.
But only if the maximum number of `TimeoutException`s were not exceeded:
```csharp
var result = await PolicyCollection.Create()
		.WithWaitAndRetry(7, TimeSpan.FromSeconds(1), (ErrorProcessorParam)logger.Error)
		.WithWaitAndRetry(3, TimeSpan.FromSeconds(30), (ErrorProcessorParam)logger.Error)
		//Use `ThrowOnWrappedCollectionFailed.CollectionError`
		.WrapUp(new RetryPolicy(1), ThrowOnWrappedCollectionFailed.CollectionError)
		.OuterPolicy
		//Limit the number of `TimeoutException`s.
		.ExcludeError<PolicyDelegateCollectionException>((pe) 
			=> pe.InnerExceptions.OfType<TimeoutException>().Count() > 8)
		.WithWait(TimeSpan.FromHours(1))
		.AddPolicyResultHandler((pr) =>
		{
			if (pr.ErrorFilterUnsatisfied)
				logger.Warning("The tries were interrupted because the maximum number of TimeoutExceptions was exceeded.");
		})
		.HandleAsync(async (ct) => await service.DoSomethingAsync(ct), token);
```
Note that if the `FailedReason` property of the last `PolicyResult` is `PolicyResultFailedReason.PolicyResultHandlerFailed`, a `PolicyResultHandlerFailedException` is thrown for both the `ThrowOnWrappedCollectionFailed.LastError` and `ThrowOnWrappedCollectionFailed.CollectionError` settings.  

You can reset a policy to its original state (without wrapped policy or collection inside) by using the `Policy.ResetWrap` method.

### TryCatch (since _version_ 2.16.21)
`SimplePolicy`, rethrowing exceptions, and wrapping can be used to mimic the functionality of the try-catch block.  
To create `TryCatch`, first create the `TryCatchBuilder` class from:
- a `CatchBlockFilteredHandler` - adding more `CatchBlockHandler`s is allowed. (see more about `CatchBlockHandler`s in [Calling Func and Action delegates in a resilient manner](#calling-func-and-action-delegates-in-a-resilient-manner)).
- a `CatchBlockForAllHandler` - no other handlers can be added if you create `TryCatchBuilder` from this handler or add it later - similar to the last catch block `catch (Exception ex) ` that adds to the try-catch block.  

When all needed catchblock handlers are added, just call `Build` method, and get `ITryCatch` interface (we can see the number of added `CatchBlockHandler`s in the `ITryCatch.CatchBlockCount` property) with methods that execute aforementioned delegates and return `TryCatchResult(<T>)` object:
```csharp
var result = TryCatchBuilder
		.CreateFrom(
			CatchBlockHandlerFactory.FilterExceptionsBy(
				NonEmptyCatchBlockFilter
					.CreateByIncluding<DirectoryNotFoundException>())
				.WithErrorProcessorOf((ex) => logger.Error(ex)))
		.AddCatchBlock(
			CatchBlockHandlerFactory.FilterExceptionsBy(
				NonEmptyCatchBlockFilter
					.CreateByIncluding<FileNotFoundException>())
				.WithErrorProcessorOf((ex) => logger.Warning(ex)))
		.AddCatchBlock(
			//Catch and process all other exceptions
			CatchBlockHandlerFactory.ForAllExceptions()
				.WithErrorProcessorOf((ex) => Console.WriteLine(ex)))
		.Build()
		//We get ITryCatch after calling the Build method
		.Execute(() => File.ReadLines(filePath).ToList());
```

![TryCatch flow](/src/docs/diagrams/try-catch-flow.png)

The `TryCatchResult(<T>)` class is very similar to the well-known *Result* pattern, but also has 
- the `IsCanceled` property, which indicates whether the execution was cancelled.  
- the `ExceptionHandlerIndex` property, which represents the index of the `CatchBlockHandler` that handled an exception (since _version_ 2.17.0).  

Note that `TryCatch` will not catch all exceptions guaranteed until you add the last `CatchBlockForAllHandler`.  
As with `SimplePolicy`, you can also use a hybrid approach and wrap the executing delegate of `TryCatch` in the usual try/catch block.  

There are shorthand methods to create `TryCatch`/`TryCatchBuilder` directly from `CatchBlockHandler`s:
```csharp
var result = NonEmptyCatchBlockFilter.CreateByIncluding<FileNotFoundException>()
		.ToCatchBlockHandler()
		.WithErrorProcessorOf((ex) => logger.Error(ex))
		//Or convert to ITryCatch directly for just one handler and execute delegate:
		//.ToTryCatch().Execute(...)
		.ToTryCatchBuilder()
		.AddCatchBlock(
			CatchBlockHandlerFactory.FilterExceptionsBy(
				NonEmptyCatchBlockFilter.CreateByIncluding<DirectoryNotFoundException>())
				.WithErrorProcessorOf((ex) => logger.Warning(ex)))
		.AddCatchBlock(
			//Catch and process all other exceptions
			CatchBlockHandlerFactory.ForAllExceptions()
			.WithErrorProcessorOf((ex) => Console.WriteLine(ex)))
		.Build()
		.Execute(() => File.ReadLines(filePath).ToList());
```
To mimic just a catch block with no exception filter, convert handler to `TryCatch` directly:
```csharp
var result = CatchBlockHandlerFactory.ForAllExceptions()
			.WithErrorProcessorOf((ex) => Console.WriteLine(ex))
			.ToTryCatch()
			.Execute(() => File.ReadLines(filePath).ToList());
```
Since _version_ 2.18.14 you can create `CatchBlockFilteredHandler` from `ErrorSet`:
```csharp
var fatalErrorSet = ErrorSet
			.FromError<OutOfMemoryException>()
			.WithError<NotImplementedException>()
			.WithError<DivideByZeroException>();

var fatalTryCatch = CatchBlockHandlerFactory.FilterExceptionsByIncluding(fatalErrorSet)
			.WithErrorProcessorOf((ex) => Console.WriteLine(ex))
			.ToTryCatch();
...
//Somewhere in your code:			
//If a fatal exception is thrown, it will be stored in the tryCatchResult.Error property.
var tryCatchResult = fatalTryCatch.Execute(DoSomethingThatMayThrowFatalEror);
```
You can use `ITryCatch` as a service in DI (since _version_ 2.18.0).  
For example, to handle `DirectoryNotFoundException` or `FileNotFoundException` exceptions that might be thrown when reading a file, create a class named `ReadFileTryCatch` that inherits from the `TryCathBase` class and implements the `ITryCatch<ReadFileTryCatch>` interface:
```csharp
public class ReadFileTryCatch : TryCatchBase,  ITryCatch<ReadFileTryCatch>
{
	public ReadFileTryCatch(ILogger logger)
	{
		TryCatch = TryCatchBuilder
			.CreateFrom(
				CatchBlockHandlerFactory.FilterExceptionsBy(
					NonEmptyCatchBlockFilter
					.CreateByIncluding<DirectoryNotFoundException>())
			.WithErrorProcessorOf((ex) => logger.Error(ex, "Directory not found.")))
			.AddCatchBlock(
				CatchBlockHandlerFactory.FilterExceptionsBy(
					NonEmptyCatchBlockFilter
					.CreateByIncluding<FileNotFoundException>())
			.WithErrorProcessorOf((ex) => logger.Warning(ex, "File not found.")))
			.Build();
	}
}
```
Then register `ReadFileTryCatch` as a transient service and use it in some other service (this example for _version_ 2.18.4):
```csharp
//In Program.cs
...
	services.AddTransient<ITryCatch<ReadFileTryCatch>, ReadFileTryCatch>();
...
//In SomeService.cs
public class SomeService
{
	private readonly ITryCatch<ReadFileTryCatch> _tryCatch;
	public SomeService(ITryCatch<ReadFileTryCatch> tryCatch) => _tryCatch = tryCatch;

	public async Task DoWhateverAsync(CancellationToken token)
	{
		//If any `DirectoryNotFoundException' or `FileNotFoundException' exceptions
		//are thrown, we will handle them here
		var tryCatchResult = await _tryCatch.ExecuteAsync((ct) => File.ReadAllLinesAsync(filePath, ct), token)
			.ConfigureAwait(false);
		if (tryCatchResult.IsSuccess)
		{
			//Do something with the lines in tryCatchResult.Result
			...
		}
	}
}
```

`TryCatch` related classes placed in the `PoliNorError.TryCatch` namespace.

### Calling Func and Action delegates in a resilient manner
There are delegate extension methods that allow aforementioned delegates to be called in a resilient manner.  
Each method calls corresponding policy method behind the scenes.  

Complete list of extension methods - for generic and non-generic delegates:

- `InvokeWithRetry(Async)`
- `InvokeWithWaitAndRetry(Async)`
- `InvokeWithRetryInfinite(Async)`
- `InvokeWithWaitAndRetryInfinite(Async)`
- `InvokeWithRetryDelay(Async)` (since _version_ 2.19.8)
- `InvokeWithRetryDelayInfinite(Async)` (since _version_ 2.19.8)
- `InvokeWithFallback(Async)`
- `InvokeWithSimple(Async)`

These methods have parameters that a library policy is usually configured to use when explicitly created.  
`PolicyResult` handlers are not supported.  
Error filtering supported only for `InvokeWithSimple(Async)` methods (since _version_ 2.16.9) using the `CatchBlockFilter` class, for example:

```csharp
	Action action = () => File.Copy(filePath, newFilePath);
	var catchBlockFilter = new CatchBlockFilter().ExcludeError<FileNotFoundException>();
	
	//The policyResult.IsFailed property is true only when the FileNotFoundException occurs.
	var policyResult = action.InvokeWithSimple(catchBlockFilter, (ErrorProcessorParam)logger.Error);
```
The  `InvokeWithSimple(Async)` method also has overloads that allow you to add not only error filters to the catch block, but also error processors using the `CatchBlockHandler` class (since _version_ 2.16.16):

```csharp
	Action action = () => File.Copy(filePath, newFilePath);

	//Construct a catch block handler with a non-empty catch block filter and few error processors:
	var catchBlockHandler = CatchBlockHandlerFactory.FilterExceptionsBy(
		NonEmptyCatchBlockFilter.CreateByIncluding<FileNotFoundException>())
		.WithErrorProcessorOf((ex) => logger.Error(ex))
		.WithErrorProcessorOf((ex) =>
			Console.WriteLine(((FileNotFoundException)ex).FileName + " is not found."));

	//If file is not found, messages are printed to log and Console:
	var policyResult = action.InvokeWithSimple(catchBlockHandler);
```
In the example above, we created a `CatchBlockHandler` subclass object by using the `CatchBlockHandlerFactory` class. This class is a factory for `CatchBlockHandler` objects and has two static methods:

- `FilterExceptionsBy(NonEmptyCatchBlockFilter)` - creates a `CatchBlockFilteredHandler` object based on the `NonEmptyCatchBlockFilter` class. The last class contains exception filtering conditions (including the `FileNotFoundException` exception type in the example). The created object mimics try-catch block's catch clause *with* exception filter.
- `ForAllExceptions` -  creates a `CatchBlockForAllHandler` object that does not filter any exceptions.  The created object mimics try-catch block's catch clause *without* exception filter.

After creating the `CatchBlockHandler` object, you can use its the `WithErrorProcessor(Of)` methods to add error processors for exception handling, which mimics the code that runs inside the catch clause of the try-catch block (or leave it as is, which mimics swallowing exceptions).


For other `InvokeWith...` methods, only one error processor is supported and can be set using a parameter of type `ErrorProcessorParam`.  
This helper class helps to reduce the number of invoking method overloads, for example:  

```csharp
	Action action = () => SendEmail("someuser@somedomain.com");

	//For the error processor created from the BasicErrorProcessor class
	action.InvokeWithRetry(2,						
							new BasicErrorProcessor(logger.Error)
							);

	//For the error processor created from the Action<Exception> delegate:
	action.InvokeWithRetry(2,
							//Or (ErrorProcessorParam)logger.Error
							ErrorProcessorParam.From(logger.Error)
							);

	//For the error processor created from the FuncException, Task> delegate: 
	action.InvokeWithRetry(2,
							//Or (ErrorProcessorParam)errorSaver.SaveChangesAsync
							ErrorProcessorParam.From(errorSaver.SaveChangesAsync)
							);
```

### Usage recommendations
For simple use cases, you could use policy processors. If your case involves more complexity and requires wrapping other policy or handling of policy results, consider using a suitable policy or packing policy with a delegate in the `PolicyDelegate` object.
In certain scenarios, for example, where a large number of retries are required, the `PolicyDelegateCollection` or `PolicyCollection` may be useful.

### Nuances of using the library
All default policy processor classes that implement `IPolicyProcessor`  will handle the `OperationCanceledException` exception in a policy-specific way if the token is different from the one passed as argument. Otherwise, only the `PolicyResult`s properties `IsFailed` and `IsCanceled` will be set to `true` and handling will exit.  

Some library methods accept delegate argument that are not cancelable, but can still be canceled. Such methods also have an extra `CancellationType` argument type that shows how cancellation will be performed.
The default value of `CancellationType` as a method argument is the `Precancelable`, means that the delegate will not be executed if the token has already been canceled. If its equals `Cancelable`, a new task that supports cancellation will be used.  

Note that the methods named `...ForAll(...)` of the `PolicyCollection` and `PolicyDelegateCollection` classes set the common filter or handler  only for elements that have already been added to the collection, not for items that will be added later.  

Calling collections methods that add a filter or handler to a policy if the collection is empty will have no effect.  

Note that collections methods such as `AddPolicyResultHandler..` or `IncludeErrorForAll` change existing policies, so you should create a policy for collection in-place or avoid using it elsewhere besides collection. For adding library policies to a collection the recommended approach is to use `With...`*`PolicyName`* shorthand methods.

For a very large retry count the `OutOfMemoryException` exception may occur. You can set "n-times infinite" handling by creating `PolicyDelegateCollection` from `RetryPolicy` with max no-error retry count defined by experiment:

```csharp
var result = await PolicyDelegateCollection
                                    .Create(new RetryPolicy(maxOfNoErrorRetries), funcForRetry, nTimeInfinite)
                                    .HandleAllAsync(token);
```
In theory, it can support up to maxOfNoErrorRetries * int.MaxValue maximum number of retries.
Please don't forget about enabling `gcAllowVeryLargeObjects` setting in the config file.  

To check if a delegate was handled successfully use these `PolicyResult` success-related properties, especially if you want to get a `PolicyResult.Result` for a generic one:  

-   `NoError`- should be used to ensure that there were no exceptions during handling, especially when getting `SimplePolicy`'s `PolicyResult.Result`.
-   `IsSuccess`- no matter how the success was gotten, there may have been no error at all (`NoError` = `true`) or the policy handled the delegate successfully.
-   `IsPolicySuccess`- at least one exception occurred (`NoError` = `false`), the policy came into play and handled the delegate successfully. For example, you can use it in a `PolicyResult` handler to write some policy-specific information into a log.

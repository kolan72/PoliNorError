# PoliNorError  
[![nuget](https://img.shields.io/nuget/v/PoliNorError)](https://www.nuget.org/packages/PoliNorError/)

![alt text](https://github.com/kolan72/kolan72.github.io/blob/master/images/PoliNorError.png?raw=true)  

PoliNorError is a library that provides error handling capabilities through Retry and Fallback policies. The library has a specific focus on handling potential exceptions within the catch block and offers various configuration options.
Heavily inspired by  [Polly](https://github.com/App-vNext/Polly).

## Key Features
- Implements two commonly used resiliency patterns - Retry and Fallback.
- Also provides `SimplePolicy` for simple handling.
- Put emphasize on error handling within the catch block.
- Extensibility: error handling within the catch block can be extended by error processors.
- Simplicity: one policy type for sync and async, and a generic and not generic delegate.
- Composability: policies and delegates can be composed into a single `PolicyDelegateCollection`.
- Flexible filters can be set for errors that should be handled.
- Policy can be wrapped by other policy.
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
			 .WithWait((retryAttempt) => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))
			 .WithPolicyResultHandler((pr) =>
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
-   A critical error has occurred within the catch block, specifically related to the saving error for  `RetryPolicy`  or calling the fallback delegate for  `FallbackPolicy` (the  `IsCritical`  property of the  `CatchBlockException`  object will also be set to  `true`).
 -  The cancellation occurs after the first call of the handling delegate, but before the execution flow enters in the `PolicyResult` handler.
 -  If the handling result cannot be accepted as a success, and a policy is in use, you can set `IsFailed` to true in a `PolicyResult` handler.  
 
To find out why `IsFailed` was set to true, there is a property called `FailedReason`. It equals `PolicyResultFailedReason.DelegateIsNull` and `PolicyResultFailedReason.PolicyResultHandlerFailed` for the first and last cases, respectively, and `PolicyResultFailedReason.PolicyProcessorFailed` for the others.  

Having `IsFailed` true, you can check the `UnprocessedError` property (appeared in version 2.0.0-rc3) to see if there was an exception that was not handled properly within the catch block.

The `IsSuccess`property indicates success of the handling. If it is true, it means that not only `IsFailed` equals false, but also `IsCanceled`, indicating that no cancellation occurred during handling.  
The `NoError` property makes sure that there were no exceptions at all when calling the handling delegate.  

If an error occurs within the catch block, it will be stored in the  `CatchBlockErrors`  property that is collection of the `CatchBlockException`  objects.  

For generic `Func` delegates, a return value will be stored in the `Result` property if the handling was successful or there were no errors at all.  

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
The last two delegates have the `ProcessingErrorInfo` argument. This type contains a policy alias and may also contain the current context of the policy processor, such as the current retry for the `RetryPolicy`.

Note that the error processor is added to the *whole* policy or policy processor, so its `Process` or `ProcessAsync` method will be called depending on the execution type of the policy handling method. If an error processor was created by a delegate of a particular execution type, the library can utilize sync-over-async or `Task` creation to obtain its counterpart.  

Error processors are handled one by one by the `BulkErrorProcessor` class. To customize this behavior, you could implement the `IBulkErrorProcessor` interface and use one of the policy or policy processor class constructors.  

Note that if cancellation occurs during `BulkErrorProcessor` execution, delegate handling will be interrupted, and the `IsFailed` and `IsCanceled` properties of the `PolicyResult` will be set to true.

### Error filters
If no filter is set, the delegate will try to be handled with any exception.  
You can specify error filter for policy or policy processor:
```csharp
 var result = new FallbackPolicy()
                                .IncludeError<SqlException>() 
                                .ExcludeError<SqlException>(ex => ex.Number == 1205)
                                .WithFallbackAction(() => cmd2.ExecuteNonQuery())
                                .Handle(() => cmd1.ExecuteNonQuery());
```
An exception is permitted for processing if any of the conditions specified by `IncludeError` are satisfied and all conditions specified by `ExcludeError` are unsatisfied.  
There are no limitations on the number of filter conditions for both types. 
If filter conditions are unsatisfied, error handling break and set both the `IsFailed` and `ErrorFilterUnsatisfied` properies to `true`.

### PolicyResult handlers

When you handle delegate by policy, you can add `PolicyResult` handlers to it using `AddPolicyResultHandler` method overloads.  
The full list of delegates that can be accepted as arguments for these methods:

- `Action<PolicyResult>`
- `Action<PolicyResult, CancellationToken>`
- `Func<PolicyResult, Task>`
- `Func<PolicyResult, CancellationToken, Task>`
- `Action<PolicyResult<T>>` (appeared in _version_ 2.0.0-rc2)
- `Action<PolicyResult<T>, CancellationToken>` (appeared in _version_ 2.0.0-rc2)
- `Func<PolicyResult<T>, Task>` (appeared in _version_ 2.0.0-rc2)
- `Func<PolicyResult<T>, CancellationToken, Task>` (appeared in _version_ 2.0.0-rc2)

The generic and non-generic `PolicyResult` handlers will only handle the generic and non-generic delegate, respectively.  
For example:
```csharp
var result = await new RetryPolicy(5)
                            .AddPolicyResultHandler<int>((pr) => { if (pr.NoError) logger.Info("There were no errors.");})
                            .HandleAsync(async (ct) => await dbContext.SaveChangesAsync(ct), token);
```
In the `PolicyResult` handler, it is possible to set the `IsFailed` property to true by using `PolicyResult.SetFailed()` method. It may be helpful if for some reason the `PolicyResult` object, as a result of handling, can't be accepted as a success and may require additional work.  
For example, if you wish to remove large folders from your disk when there is less than 40Gb of free space, and notify about it, create two policies and combine them in the `PolicyDelegateCollection` for further handling.:
```csharp
            var checkFreeSpacePolicy = new SimplePolicy()
						.AddPolicyResultHandler<long>((pr) =>
						{
							if (pr.NoError)
							{
								if (pr.Result < 40000000000)
									//If free space is not enough we pass handling 
									//to the next PolicyDelegate in the collection:
									pr.SetFailed();
								else
									logger.Info("Free space is ok");
							}
						});

            var freeSpaceAfterPolicy = new SimplePolicy()
						.WithErrorProcessorOf((ex) => logger.Error(ex.Message))
						.AddPolicyResultHandler<long>((pr) =>
						{
							if (pr.NoError)
							{
								logger.Info("Total available space: {freeSpace} bytes", pr.Result);
							}
						});


			var freeSpaceResult = PolicyDelegateCollection<long>
						.Create()
						.WithPolicyAndDelegate(checkFreeSpacePolicy, GetFreeSpace)
						.WithPolicyAndDelegate(freeSpaceAfterPolicy, () =>
						{
							DeleteUselessLargeFolders();
							return GetFreeSpace(); 
						})
						.HandleAll();


//Somewhere in your code:
private void DeleteUselessLargeFolders()
{
	//Delete folders here...
}

private long GetFreeSpace() => new DriveInfo("D:").TotalFreeSpace;
```

Exceptions in a `PolicyResult` handler are allowed and stored in `PolicyResultHandlingErrors` property without affecting other `PolicyResult` properties.  
If a cancellation occurs at the stage of the `PolicyResult` handling, the process of running the `PolicyResult` handlers will not be interrupted.  

### RetryPolicy
The policy rule for the `RetryPolicy` is that it can handle exceptions only until the number of permitted retries does not exceed, so it is the most crucial parameter and is set in policy constructor.  
You can also specify the delay time before next retry with `WithWait(TimeSpan)` method, or use one of the overloads with Func, returning TimeSpan, for example:
```csharp
            var policy = new RetryPolicy(5)
                                    .WithWait((retryAttempt, ex) =>
                                    {
                                        logger.Error(ex.Message);
                                        return TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
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

For huge numbers of retries, memory-related exceptions, such as `OutOfMemoryException`, may occur while saving handling exceptions in the `PolicyResult.Errors` property. This exception will be handled, wrapped up in a `CatchBlockException`, and saved in the `PolicyResult.CatchBlockErrors`.  

If you want to interrupt the handling process after that, create a Retry policy or processor with the `failedIfSaveErrorThrow` parameter set to true. In this case, the `CatchBlockException.IsCritical` property will be set to true, as well as the `PolicyResult.IsFailed` property.  

Moreover, you can also customize error saving by calling the `UseCustomErrorSaver` or `UseCustomErrorSaverOf` methods to save errors elsewhere.  
These methods have the `IErrorProcessor` type or delegates that take an exception argument as a parameter.  
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

If you try to handle a generic func delegate without a corresponding fallback delegate being set, the default value of type will be returned.  
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
Behind the scenes wrapped policy's `Handle(Async)(<T>)`  method will be called as a handling delegate with throwing `PolicyResult.UnprocessedError` or `PolicyResultHandlerFailedException` exception if the `SetFailed` method was called in a `PolicyResult` handler.

Results of handling a wrapped policy are stored in the `WrappedPolicyResults` property of the wrapper `PolicyResult`.


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

You can create `PolicyDelegateCollection(<T>)` by using `Create` method.  
With these methods
- `WithPolicy` (create the `PolicyDelegate` with a policy but without a delegate)
- `WithPolicyDelagate`
- `WithPolicyAndDelegate`
- `AndDelegate` (set delegate to last `PolicyDelegate` object in the collection)

 or specific policy-related extensions methods and their overloads:
- `WithRetry`
- `WithWaitAndRetry`
- `WithInfiniteRetry`
- `WithWaitAndInfiniteRetry`
- `WithFallback`
- `WithSimple` (appeared in _version_ 2.0.0-alpha)

you can further construct a collection in a fluent manner and call `HandleAll` or `HandleAllAsync` method.  

Handling `PolicyDelegateCollection(<T>)` is merely calling the `Handle(Async(<T>))` method for each element in the collection one by one, while the current policy `IsFailed` equals true and no cancellation occurs.  
```
PolicyDelegate_1_handling —— Failed ——> PolicyDelegate_2_handling —— Failed ——> ..
  |                                     |
  | —— Success_Or_Canceled ——> Exit     | —— Success_Or_Canceled ——> Exit

```
Handling is smart - it checks the synchronicity type of all delegates in collection and calls the appropriate method behind the scenes, which calls delegates in sync or async manner.  
You can also use the `BuildCollectionHandler()` method to obtain the `IPolicyDelegateCollectionHandler(T)` interface with the aforementioned methods and pass it somewhere as a dependency injection parameter.  

You can establish a `PolicyResult` handler for the entire collection by using the `AddPolicyResultHandlerForAll` method and for the last element by using the `AddPolicyResultHandlerForLast`(since _version_ 2.4.0) method.  
These methods require the same delegates types as `PolicyResult` handlers for policy.  

This is an example of how to add retry policies and delegates with common `PolicyResult` handler(example for _version_ 2.0.0-rc2 version):
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
The example in the  [`PolicyResult` handlers](#policyresult-handlers) chapter could be rewritten to use a different approach by using the `AddPolicyResultHandlerForLast` method:
```csharp
var freeSpaceResult = PolicyDelegateCollection<long>.Create()
						  .WithSimple()
						  .AndDelegate(GetFreeSpace)
						  .AddPolicyResultHandlerForLast((pr) =>
						  {
							  if (pr.NoError)
							  {
								  if (pr.Result < 40000000000)
									  //If free space is not enough we pass handling to the next PolicyDelegate in the collection:
									  pr.SetFailed();
								  else
									  logger.Info("Free space is ok");
							  }
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

Results of handling are stored in `PolicyDelegateCollectionResult(<T>)` that implements `IEnumerable<PolicyDelegateResult(<T>)>` interface. The `PolicyDelegateResult(<T>)` class is a wrapper around `PolicyResult` that additionally contains `MethodInfo` of the delegate.  

The `PolicyDelegatesUnused` property contains a collection of policydelegates that were not handled due to the reasons described above.  

It is possible to throw an exception if handling of the last element in the collection fails. This can be done with the `WithThrowOnLastFailed` method, which throws a special `PolicyDelegateCollectionException` exception for non-generic collection and  `PolicyDelegateCollectionException<T>` for generic one.   
This exception contains `InnerExceptions` property with exceptions added from `Errors` properties of all `PolicyResult`s. For the `PolicyDelegateCollectionException<T>` object, you can also obtain all `PolicyResult.Result`s by using `GetResults()` method.


### PolicyCollection
Sometimes one delegate needs to be handled by many policies, and this can be done easily with the `PolicyCollection` class.  

If, for instance, you'd like to read a file that's currently being used by another process, you could try two attempts and, if the error persists, copy the file to the temporary folder and access it from there:
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


### Calling Func and Action delegates in a resilient manner
There are delegate extension methods that allow aforementioned delegates to be called in a resilient manner.  
Each method calls corresponding policy method behind the scenes.  
These methods have parameters that the policy is usually configured by, excluding error filters and `PolicyResult` handlers.  

Only one error processor is supported and can be set up by a parameter of type `ErrorProcessorParam`.  
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

Full list of extensions methods names:

- `InvokeWithRetry(Async)`
- `InvokeWithWaitAndRetry(Async)`
- `InvokeWithRetryInfinite(Async)`
- `InvokeWithWaitAndRetryInfinite(Async)`
- `InvokeWithFallback(Async)`
- `InvokeWithSimple(Async)` (appeared in _version_ 2.0.0-alpha)


### Usage recommendations
For simple use cases, you could use policy processors. If your case involves more complexity and requires wrapping other policy or handling of policy results, consider using a suitable policy or packing policy with a delegate in the `PolicyDelegate` object.
In certain scenarios, for example, where a large number of retries are required, the `PolicyDelegateCollection` or `PolicyCollection` may be useful.

### Nuances of using the library
All default policy processor classes that implement `IPolicyProcessor`  will handle the `OperationCanceledException` exception in a policy-specific way if the token is different from the one passed as argument. Otherwise, only the `PolicyResult`s properties `IsFailed` and `IsCanceled` will be set to `true` and handling will exit.  

Some library methods accept delegate argument that are not cancelable, but can still be canceled. Such methods also have an extra `CancellationType` argument type that shows how cancellation will be performed.
The default value of `CancellationType` as a method argument is the `Precancelable`, means that the delegate will not be executed if the token has already been canceled. If its equals `Cancelable`, a new task that supports cancellation will be used.  

Note the methods `PolicyDelegateCollection....ForAll(commonDelegate)` of `PolicyCollection` and `PolicyDelegateCollection`  classes set the common delegate only for items that have already been added to the collection, not for items that will be added later.  

For a very large retry count the `OutOfMemoryException` exception may occur. You can set "n-times infinite" handling by creating `PolicyDelegateCollection` from `RetryPolicy` with max no-error retry count defined by experiment:

```csharp
var result = await PolicyDelegateCollection
                                    .Create(new RetryPolicy(maxOfNoErrorRetries), funcForRetry, nTimeInfinite)
                                    .HandleAllAsync(token);
```
In theory, it can support up to maxOfNoErrorRetries * int.MaxValue maximum number of retries.
Please don't forget about enabling `gcAllowVeryLargeObjects` setting in the config file.

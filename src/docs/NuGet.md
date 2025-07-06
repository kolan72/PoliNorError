PoliNorError is a library that provides error handling capabilities through Retry and Fallback policies or processors.  
Also provides SimplePolicy and TryCatch mimicking.  
The library can also handle potential exceptions within the catch block and offers various configuration options.

```csharp
var result = 
	new RetryPolicy(2)

	.IncludeError<SomeException>()

	.WithErrorProcessorOf((Exception ex, ProcessingErrorInfo pi) =>
		logger.LogError(ex, 
		"Policy processed exception on {Attempt} attempt:", 
		pi.GetRetryCount() + 1))

	.AddPolicyResultHandler((PolicyResult pr) =>
		logger.LogWarning(
		"{Errors} exceptions were thrown.",
		pr.Errors.Count()))

	.Handle(ActionThatCanThrowSomeException);


var simplePolicyResult = new SimplePolicy()

	.WithErrorContextProcessorOf<int>((ex, pi) =>
		loggerTest.LogError(ex,
		"Delegate call with parameter value {Param} failed with an exception.", 
		pi.Param))
		
	//If the random value is zero,
	//the previous line of code
	//logs an error with {Param} set to zero.
	.Handle((i) => 5 / i, random);


var retryPolicyResult = await new RetryPolicy(5)

	.WithErrorContextProcessorOf<string>((ex, pi) =>
	 	logger.LogError(ex,
			"Failed to connect to {Uri} on attempt {Attempt}.",
			pi.Param,
			pi.GetAttemptCount()))

	//In case of an exception,
	//the previous line logs an error
	//with {Uri} set to "users"
	//and {Attempt} set to the current attempt
	.HandleAsync(async (uri, ct)
		=> await _httpClient.GetAsync(uri, ct), "users", token);


//Implementation of the Retry-Then-Fallback pattern:

// Create a retry policy that will attempt the operation up to 3 times.
var fallbackResult = new RetryPolicy(3)

	// Exclude DivideByZeroException from being retried.
	.ExcludeError<DivideByZeroException>()

	// Switch to a fallback policy that wraps up the current retry policy.
	.ThenFallback()

	// Configure the fallback policy using a function that returns int.MaxValue.
	.WithFallbackFunc(() => int.MaxValue)

	// The fallback policy exclusively handles DivideByZeroException.
	.IncludeError<DivideByZeroException>()

	// Log an error message when fallback is triggered
	.WithErrorProcessorOf(_ => logger.LogError("Fallback to int.MaxValue"))

	// If the random value is zero,
	// the fallback is triggered,
	// "Fallback to int.MaxValue" is logged,
	// and fallbackResult.Result is set to int.MaxValue.
	.Handle(() => 5 / random);

```

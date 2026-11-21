using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using PoliNorError.Extensions.PolicyErrorFiltering;
using PoliNorError.Extensions.PolicyResultHandling;

namespace PoliNorError
{
	/// <summary>
	/// A gate policy that allows the delegate to execute only while an <see cref="IRateLimiter"/> grants a permit.
	/// </summary>
	/// <remarks>
	/// <para>
	/// When the gate denies execution the returned result is failed with
	/// <see cref="PolicyResultFailedReason.ExecutionDenied"/>. Inspect the outcome with
	/// <see cref="RateLimitPolicyResultExtensions.IsExecutionDenied"/> and
	/// <see cref="RateLimitPolicyResultExtensions.GetRetryAfter"/>, or observe it in registered result handlers;
	/// the rejection is recorded as a <see cref="RateLimitException"/>.
	/// </para>
	/// <para>
	/// Error filters and error processors apply only to exceptions thrown by the <b>delegate</b> on the permitted path.
	/// A rate-limiting rejection bypasses them, just like <see cref="NoDelegateException"/>.
	/// Delegate exceptions processed inside this policy are tagged with <see cref="PolicyAlias.Simple"/>
	/// because the inner simple processor owns error processing.
	/// </para>
	/// <para>
	/// On the permitted path a delegate exception that matches the configured error filter is handled by the
	/// inner simple processor (recorded in <see cref="PolicyResult.Errors"/>, error processors ran) and the
	/// returned result is <b>failed</b> with <see cref="PolicyResultFailedReason.PolicyProcessorFailed"/> -
	/// not canceled and not <see cref="PolicyResultFailedReason.ExecutionDenied"/>. Sequential chains
	/// (<see cref="PolicyCollection"/>, <c>PolicyDelegateCollection</c>) therefore continue with the next
	/// element, and when this policy is used as a wrapped inner policy the wrapper rethrows the original
	/// exception to the outer policy. Because of that rethrow, configure an <see cref="IBulkErrorProcessor"/>
	/// on one side of a wrap only (this policy or the outer policy), otherwise one exception is processed twice.
	/// </para>
	/// <para>
	/// Composition: use this policy as an <b>outer wrapper</b> (<c>inner.Then(rateLimit)</c>,
	/// <c>collection.Then(rateLimit)</c>) so that a rejection prevents the wrapped delegate from running.
	/// As an element of any sequential chain (<see cref="PolicyCollection"/> and <c>PolicyDelegateCollection</c>)
	/// a rejection also stops the chain: subsequent elements are skipped and the gated delegate is not run.
	/// </para>
	/// </remarks>
	public sealed class RateLimitPolicy : Policy, IPolicyBase, IWithErrorFilter<RateLimitPolicy>
	{
		private readonly ISimplePolicyProcessor _simpleProcessor;
		private readonly IRateLimiter _rateLimiter;

		/// <summary>
		/// Initializes a new instance of the <see cref="RateLimitPolicy"/> class.
		/// </summary>
		/// <param name="rateLimiter">The limiter that gates delegate execution.</param>
		/// <exception cref="ArgumentNullException"><paramref name="rateLimiter"/> is <see langword="null"/>.</exception>
		public RateLimitPolicy(IRateLimiter rateLimiter)
			: this(SimplePolicyProcessor.CreateFailingOnError(), rateLimiter)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RateLimitPolicy"/> class with a custom error processor.
		/// </summary>
		/// <param name="rateLimiter">The limiter that gates delegate execution.</param>
		/// <param name="processor"><see cref="IBulkErrorProcessor"/> applied to exceptions thrown by the delegate.</param>
		/// <param name="rethrowIfErrorFilterUnsatisfied">Specifies whether an exception is rethrown if the error filter is unsatisfied.</param>
		/// <exception cref="ArgumentNullException"><paramref name="rateLimiter"/> is <see langword="null"/>.</exception>
		public RateLimitPolicy(IRateLimiter rateLimiter, IBulkErrorProcessor processor, bool rethrowIfErrorFilterUnsatisfied = false)
			: this(SimplePolicyProcessor.CreateFailingOnError(processor, rethrowIfErrorFilterUnsatisfied), rateLimiter)
		{
		}

		private RateLimitPolicy(ISimplePolicyProcessor processor, IRateLimiter rateLimiter) : base(processor)
		{
			_simpleProcessor = processor ?? throw new ArgumentNullException(nameof(processor));
			_rateLimiter = rateLimiter ?? throw new ArgumentNullException(nameof(rateLimiter));
		}

		/// <summary>
		/// Attempts to acquire a permit and, when granted, executes the <paramref name="action"/> synchronously.
		/// </summary>
		/// <param name="action">The action to execute when the gate permits it.</param>
		/// <param name="token">The cancellation token to monitor for cancellation requests.</param>
		/// <returns><see cref="PolicyResult"/>; a rejection is failed with <see cref="PolicyResultFailedReason.ExecutionDenied"/>; a filter-matched delegate exception is handled and failed with <see cref="PolicyResultFailedReason.PolicyProcessorFailed"/>.</returns>
		public PolicyResult Handle(Action action, CancellationToken token = default)
		{
			if (action == null)
			{
				return NoDelegateResult(token);
			}

			if (token.IsCancellationRequested)
			{
				var canceledResult = _simpleProcessor.Execute(action, token);
				canceledResult.SetPolicyName(PolicyName);
				return HandlePolicyResult(canceledResult, token);
			}

			RateLimitResult acquire;
			try
			{
				acquire = _rateLimiter.TryAcquire(token);
			}
			catch (OperationCanceledException) when (token.IsCancellationRequested)
			{
				var canceledResult = PolicyResult.ForSync();
				canceledResult.SetCanceledEarly();
				canceledResult.SetPolicyName(PolicyName);
				return HandlePolicyResult(canceledResult, token);
			}
			catch (Exception ex)
			{
				var failedResult = PolicyResult.ForSync();
				failedResult.AddError(ex);
				failedResult.SetFailedInner(PolicyResultFailedReason.PolicyProcessorFailed);
				failedResult.SetPolicyName(PolicyName);
				return HandlePolicyResult(failedResult, token);
			}

			if (!acquire.IsPermitted)
			{
				var rejectedResult = PolicyResult.ForSync();
				rejectedResult.AddError(new RateLimitException(acquire.RetryAfter));
				rejectedResult.SetFailedInner(PolicyResultFailedReason.ExecutionDenied);
				rejectedResult.SetPolicyName(PolicyName);
				return HandlePolicyResult(rejectedResult, token);
			}

			var (Act, Wrapper) = WrapDelegateIfNeed(action, token);
			var result = _simpleProcessor.Execute(Act, token)
						  .SetWrappedPolicyResults(Wrapper, PolicyName);

			return HandlePolicyResult(result, token);
		}

		/// <summary>
		/// Attempts to acquire a permit and, when granted, executes the <paramref name="func"/> synchronously.
		/// </summary>
		/// <typeparam name="T">The type of the return value of <paramref name="func"/>.</typeparam>
		/// <param name="func">The function to execute when the gate permits it.</param>
		/// <param name="token">The cancellation token to monitor for cancellation requests.</param>
		/// <returns><see cref="PolicyResult{T}"/>; a rejection is failed with <see cref="PolicyResultFailedReason.ExecutionDenied"/> and leaves <see cref="PolicyResult{T}.Result"/> at its default value; a filter-matched delegate exception is handled and failed with <see cref="PolicyResultFailedReason.PolicyProcessorFailed"/>.</returns>
		public PolicyResult<T> Handle<T>(Func<T> func, CancellationToken token = default)
		{
			if (func == null)
			{
				return NoDelegateResult<T>(token);
			}

			if (token.IsCancellationRequested)
			{
				var canceledResult = _simpleProcessor.Execute(func, token);
				canceledResult.SetPolicyName(PolicyName);
				return HandlePolicyResult(canceledResult, token);
			}

			RateLimitResult acquire;
			try
			{
				acquire = _rateLimiter.TryAcquire(token);
			}
			catch (OperationCanceledException) when (token.IsCancellationRequested)
			{
				var canceledResult = PolicyResult<T>.ForSync();
				canceledResult.SetCanceledEarly();
				canceledResult.SetPolicyName(PolicyName);
				return HandlePolicyResult(canceledResult, token);
			}
			catch (Exception ex)
			{
				var failedResult = PolicyResult<T>.ForSync();
				failedResult.AddError(ex);
				failedResult.SetFailedInner(PolicyResultFailedReason.PolicyProcessorFailed);
				failedResult.SetPolicyName(PolicyName);
				return HandlePolicyResult(failedResult, token);
			}

			if (!acquire.IsPermitted)
			{
				var rejectedResult = PolicyResult<T>.ForSync();
				rejectedResult.AddError(new RateLimitException(acquire.RetryAfter));
				rejectedResult.SetFailedInner(PolicyResultFailedReason.ExecutionDenied);
				rejectedResult.SetPolicyName(PolicyName);
				return HandlePolicyResult(rejectedResult, token);
			}

			var (Fn, Wrapper) = WrapDelegateIfNeed(func, token);
			var result = _simpleProcessor.Execute(Fn, token)
						  .SetWrappedPolicyResults(Wrapper, PolicyName);

			return HandlePolicyResult(result, token);
		}

		/// <summary>
		/// Attempts to acquire a permit asynchronously and, when granted, executes the <paramref name="func"/> delegate.
		/// </summary>
		/// <param name="func">The asynchronous action to execute when the gate permits it.</param>
		/// <param name="configureAwait">Whether to configure await for the async operation.</param>
		/// <param name="token">The cancellation token to monitor for cancellation requests.</param>
		/// <returns>A task that yields a <see cref="PolicyResult"/>; a rejection is failed with <see cref="PolicyResultFailedReason.ExecutionDenied"/>; a filter-matched delegate exception is handled and failed with <see cref="PolicyResultFailedReason.PolicyProcessorFailed"/>.</returns>
		public async Task<PolicyResult> HandleAsync(Func<CancellationToken, Task> func, bool configureAwait = false, CancellationToken token = default)
		{
			if (func == null)
			{
				return await NoDelegateResultAsync(configureAwait, token).ConfigureAwait(configureAwait);
			}

			if (token.IsCancellationRequested)
			{
				var canceledResult = await _simpleProcessor.ExecuteAsync(func, configureAwait, token).ConfigureAwait(configureAwait);
				canceledResult.SetPolicyName(PolicyName);
				return await HandlePolicyResultAsync(canceledResult, configureAwait, token).ConfigureAwait(configureAwait);
			}

			RateLimitResult acquire;
			try
			{
				acquire = await _rateLimiter.TryAcquireAsync(token).ConfigureAwait(configureAwait);
			}
			catch (OperationCanceledException) when (token.IsCancellationRequested)
			{
				var canceledResult = PolicyResult.InitByConfigureAwait(configureAwait);
				canceledResult.SetCanceledEarly();
				canceledResult.SetPolicyName(PolicyName);
				return await HandlePolicyResultAsync(canceledResult, configureAwait, token).ConfigureAwait(configureAwait);
			}
			catch (Exception ex)
			{
				var failedResult = PolicyResult.InitByConfigureAwait(configureAwait);
				failedResult.AddError(ex);
				failedResult.SetFailedInner(PolicyResultFailedReason.PolicyProcessorFailed);
				failedResult.SetPolicyName(PolicyName);
				return await HandlePolicyResultAsync(failedResult, configureAwait, token).ConfigureAwait(configureAwait);
			}

			if (!acquire.IsPermitted)
			{
				var rejectedResult = PolicyResult.InitByConfigureAwait(configureAwait);
				rejectedResult.AddError(new RateLimitException(acquire.RetryAfter));
				rejectedResult.SetFailedInner(PolicyResultFailedReason.ExecutionDenied);
				rejectedResult.SetPolicyName(PolicyName);
				return await HandlePolicyResultAsync(rejectedResult, configureAwait, token).ConfigureAwait(configureAwait);
			}

			var (Fn, Wrapper) = WrapDelegateIfNeed(func, token, configureAwait);
			var result = (await _simpleProcessor.ExecuteAsync(Fn, configureAwait, token).ConfigureAwait(configureAwait))
								.SetWrappedPolicyResults(Wrapper, PolicyName);

			return await HandlePolicyResultAsync(result, configureAwait, token).ConfigureAwait(configureAwait);
		}

		/// <summary>
		/// Attempts to acquire a permit asynchronously and, when granted, executes the <paramref name="func"/> delegate.
		/// </summary>
		/// <typeparam name="T">The type of the return value of <paramref name="func"/>.</typeparam>
		/// <param name="func">The asynchronous function to execute when the gate permits it.</param>
		/// <param name="configureAwait">Whether to configure await for the async operation.</param>
		/// <param name="token">The cancellation token to monitor for cancellation requests.</param>
		/// <returns>A task that yields a <see cref="PolicyResult{T}"/>; a rejection is failed with <see cref="PolicyResultFailedReason.ExecutionDenied"/> and leaves <see cref="PolicyResult{T}.Result"/> at its default value; a filter-matched delegate exception is handled and failed with <see cref="PolicyResultFailedReason.PolicyProcessorFailed"/>.</returns>
		public async Task<PolicyResult<T>> HandleAsync<T>(Func<CancellationToken, Task<T>> func, bool configureAwait = false, CancellationToken token = default)
		{
			if (func == null)
			{
				return await NoDelegateResultAsync<T>(configureAwait, token).ConfigureAwait(configureAwait);
			}

			if (token.IsCancellationRequested)
			{
				var canceledResult = await _simpleProcessor.ExecuteAsync(func, configureAwait, token).ConfigureAwait(configureAwait);
				canceledResult.SetPolicyName(PolicyName);
				return await HandlePolicyResultAsync(canceledResult, configureAwait, token).ConfigureAwait(configureAwait);
			}

			RateLimitResult acquire;
			try
			{
				acquire = await _rateLimiter.TryAcquireAsync(token).ConfigureAwait(configureAwait);
			}
			catch (OperationCanceledException) when (token.IsCancellationRequested)
			{
				var canceledResult = PolicyResult<T>.InitByConfigureAwait(configureAwait);
				canceledResult.SetCanceledEarly();
				canceledResult.SetPolicyName(PolicyName);
				return await HandlePolicyResultAsync(canceledResult, configureAwait, token).ConfigureAwait(configureAwait);
			}
			catch (Exception ex)
			{
				var failedResult = PolicyResult<T>.InitByConfigureAwait(configureAwait);
				failedResult.AddError(ex);
				failedResult.SetFailedInner(PolicyResultFailedReason.PolicyProcessorFailed);
				failedResult.SetPolicyName(PolicyName);
				return await HandlePolicyResultAsync(failedResult, configureAwait, token).ConfigureAwait(configureAwait);
			}

			if (!acquire.IsPermitted)
			{
				var rejectedResult = PolicyResult<T>.InitByConfigureAwait(configureAwait);
				rejectedResult.AddError(new RateLimitException(acquire.RetryAfter));
				rejectedResult.SetFailedInner(PolicyResultFailedReason.ExecutionDenied);
				rejectedResult.SetPolicyName(PolicyName);
				return await HandlePolicyResultAsync(rejectedResult, configureAwait, token).ConfigureAwait(configureAwait);
			}

			var (Fn, Wrapper) = WrapDelegateIfNeed(func, token, configureAwait);
			var result = (await _simpleProcessor.ExecuteAsync(Fn, configureAwait, token).ConfigureAwait(configureAwait))
								.SetWrappedPolicyResults(Wrapper, PolicyName);

			return await HandlePolicyResultAsync(result, configureAwait, token).ConfigureAwait(configureAwait);
		}

		/// <summary>
		/// Includes a specific exception type with an optional predicate for filtering.
		/// </summary>
		/// <typeparam name="TException">The type of exception to include.</typeparam>
		/// <param name="func">Optional predicate function to filter exceptions.</param>
		/// <returns>The current RateLimitPolicy instance for method chaining.</returns>
		public RateLimitPolicy IncludeError<TException>(Func<TException, bool> func = null) where TException : Exception => this.IncludeError<RateLimitPolicy, TException>(func);

		/// <summary>
		/// Includes exceptions based on a LINQ expression predicate.
		/// </summary>
		/// <param name="expression">The expression predicate to filter exceptions.</param>
		/// <returns>The current RateLimitPolicy instance for method chaining.</returns>
		public RateLimitPolicy IncludeError(Expression<Func<Exception, bool>> expression) => this.IncludeError<RateLimitPolicy>(expression);

		/// <summary>
		/// Excludes a specific exception type with an optional predicate for filtering.
		/// </summary>
		/// <typeparam name="TException">The type of exception to exclude.</typeparam>
		/// <param name="func">Optional predicate function to filter exceptions.</param>
		/// <returns>The current RateLimitPolicy instance for method chaining.</returns>
		public RateLimitPolicy ExcludeError<TException>(Func<TException, bool> func = null) where TException : Exception => this.ExcludeError<RateLimitPolicy, TException>(func);

		/// <summary>
		/// Excludes exceptions based on a LINQ expression predicate.
		/// </summary>
		/// <param name="expression">The expression predicate to filter exceptions.</param>
		/// <returns>The current RateLimitPolicy instance for method chaining.</returns>
		public RateLimitPolicy ExcludeError(Expression<Func<Exception, bool>> expression) => this.ExcludeError<RateLimitPolicy>(expression);

		/// <summary>
		/// Adds a synchronous action handler for policy results.
		/// </summary>
		/// <param name="action">The action to execute when a policy result is available.</param>
		/// <returns>The current RateLimitPolicy instance for method chaining.</returns>
		public RateLimitPolicy AddPolicyResultHandler(Action<PolicyResult> action)
		{
			return this.AddHandlerForPolicyResult(action);
		}

		/// <summary>
		/// Adds a synchronous action handler for policy results with cancellation type conversion.
		/// </summary>
		/// <param name="action">The action to execute when a policy result is available.</param>
		/// <param name="convertType">The cancellation type conversion to apply.</param>
		/// <returns>The current RateLimitPolicy instance for method chaining.</returns>
		public RateLimitPolicy AddPolicyResultHandler(Action<PolicyResult> action, CancellationType convertType)
		{
			return this.AddHandlerForPolicyResult(action, convertType);
		}

		/// <summary>
		/// Adds a synchronous action handler for policy results with cancellation token support.
		/// </summary>
		/// <param name="action">The action to execute when a policy result is available, with cancellation token.</param>
		/// <returns>The current RateLimitPolicy instance for method chaining.</returns>
		public RateLimitPolicy AddPolicyResultHandler(Action<PolicyResult, CancellationToken> action)
		{
			return this.AddHandlerForPolicyResult(action);
		}

		/// <summary>
		/// Adds an asynchronous function handler for policy results.
		/// </summary>
		/// <param name="func">The function to execute when a policy result is available.</param>
		/// <returns>The current RateLimitPolicy instance for method chaining.</returns>
		public RateLimitPolicy AddPolicyResultHandler(Func<PolicyResult, Task> func)
		{
			return this.AddHandlerForPolicyResult(func);
		}

		/// <summary>
		/// Adds an asynchronous function handler for policy results with cancellation type conversion.
		/// </summary>
		/// <param name="func">The function to execute when a policy result is available.</param>
		/// <param name="convertType">The cancellation type conversion to apply.</param>
		/// <returns>The current RateLimitPolicy instance for method chaining.</returns>
		public RateLimitPolicy AddPolicyResultHandler(Func<PolicyResult, Task> func, CancellationType convertType)
		{
			return this.AddHandlerForPolicyResult(func, convertType);
		}

		/// <summary>
		/// Adds an asynchronous function handler for policy results with cancellation token support.
		/// </summary>
		/// <param name="func">The function to execute when a policy result is available, with cancellation token.</param>
		/// <returns>The current RateLimitPolicy instance for method chaining.</returns>
		public RateLimitPolicy AddPolicyResultHandler(Func<PolicyResult, CancellationToken, Task> func)
		{
			return this.AddHandlerForPolicyResult(func);
		}

		/// <summary>
		/// Adds a synchronous action handler for typed policy results.
		/// </summary>
		/// <typeparam name="T">The type of the policy result value.</typeparam>
		/// <param name="action">The action to execute when a typed policy result is available.</param>
		/// <returns>The current RateLimitPolicy instance for method chaining.</returns>
		public RateLimitPolicy AddPolicyResultHandler<T>(Action<PolicyResult<T>> action)
		{
			return this.AddHandlerForPolicyResult(action);
		}

		/// <summary>
		/// Adds a synchronous action handler for typed policy results with cancellation type conversion.
		/// </summary>
		/// <typeparam name="T">The type of the policy result value.</typeparam>
		/// <param name="action">The action to execute when a typed policy result is available.</param>
		/// <param name="convertType">The cancellation type conversion to apply.</param>
		/// <returns>The current RateLimitPolicy instance for method chaining.</returns>
		public RateLimitPolicy AddPolicyResultHandler<T>(Action<PolicyResult<T>> action, CancellationType convertType)
		{
			return this.AddHandlerForPolicyResult(action, convertType);
		}

		/// <summary>
		/// Adds a synchronous action handler for typed policy results with cancellation token support.
		/// </summary>
		/// <typeparam name="T">The type of the policy result value.</typeparam>
		/// <param name="action">The action to execute when a typed policy result is available, with cancellation token.</param>
		/// <returns>The current RateLimitPolicy instance for method chaining.</returns>
		public RateLimitPolicy AddPolicyResultHandler<T>(Action<PolicyResult<T>, CancellationToken> action)
		{
			return this.AddHandlerForPolicyResult(action);
		}

		/// <summary>
		/// Adds an asynchronous function handler for typed policy results.
		/// </summary>
		/// <typeparam name="T">The type of the policy result value.</typeparam>
		/// <param name="func">The function to execute when a typed policy result is available.</param>
		/// <returns>The current RateLimitPolicy instance for method chaining.</returns>
		public RateLimitPolicy AddPolicyResultHandler<T>(Func<PolicyResult<T>, Task> func)
		{
			return this.AddHandlerForPolicyResult(func);
		}

		/// <summary>
		/// Adds an asynchronous function handler for typed policy results with cancellation type conversion.
		/// </summary>
		/// <typeparam name="T">The type of the policy result value.</typeparam>
		/// <param name="func">The function to execute when a typed policy result is available.</param>
		/// <param name="convertType">The cancellation type conversion to apply.</param>
		/// <returns>The current RateLimitPolicy instance for method chaining.</returns>
		public RateLimitPolicy AddPolicyResultHandler<T>(Func<PolicyResult<T>, Task> func, CancellationType convertType)
		{
			return this.AddHandlerForPolicyResult(func, convertType);
		}

		/// <summary>
		/// Adds an asynchronous function handler for typed policy results with cancellation token support.
		/// </summary>
		/// <typeparam name="T">The type of the policy result value.</typeparam>
		/// <param name="func">The function to execute when a typed policy result is available, with cancellation token.</param>
		/// <returns>The current RateLimitPolicy instance for method chaining.</returns>
		public RateLimitPolicy AddPolicyResultHandler<T>(Func<PolicyResult<T>, CancellationToken, Task> func)
		{
			return this.AddHandlerForPolicyResult(func);
		}
	}
}

using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using PoliNorError.Extensions.PolicyErrorFiltering;
using PoliNorError.Extensions.PolicyResultHandling;

namespace PoliNorError
{
	/// <summary>
	/// A simple policy that can handle delegates.
	/// </summary>
	public sealed partial class SimplePolicy : Policy, IPolicyBase, IWithErrorFilter<SimplePolicy>, IWithInnerErrorFilter<SimplePolicy>, ICanAddErrorFilter<SimplePolicy>
	{
		private readonly ISimplePolicyProcessor _simpleProcessor;

		///<inheritdoc cref = "SimplePolicy(IBulkErrorProcessor,bool)"/>
		public SimplePolicy(bool rethrowIfErrorFilterUnsatisfied = false) : this(null, rethrowIfErrorFilterUnsatisfied) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="SimplePolicy"/>.
		/// </summary>
		/// <param name="processor"><see cref="IBulkErrorProcessor"/></param>
		/// <param name="rethrowIfErrorFilterUnsatisfied">Specifies whether an exception is rethrown if the error filter is unsatisfied.</param>
		public SimplePolicy(IBulkErrorProcessor processor, bool rethrowIfErrorFilterUnsatisfied = false) : this(new SimplePolicyProcessor(processor, rethrowIfErrorFilterUnsatisfied)) { }

		internal SimplePolicy(CatchBlockFilter catchBlockFilter, IBulkErrorProcessor processor = null, bool rethrowIfErrorFilterUnsatisfied = false) : this(new SimplePolicyProcessor(catchBlockFilter, processor, rethrowIfErrorFilterUnsatisfied)) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="SimplePolicy"/>.
		/// </summary>
		/// <param name="processor"><see cref="ISimplePolicyProcessor"/></param>
		public SimplePolicy(ISimplePolicyProcessor processor) : base(processor) => _simpleProcessor = processor;

		/// <summary>
		/// Executes an action synchronously using the simple policy.
		/// </summary>
		/// <param name="action">The action to execute.</param>
		/// <param name="token">The cancellation token to monitor for cancellation requests.</param>
		public PolicyResult Handle(Action action, CancellationToken token = default)
		{
			var (Act, Wrapper) = WrapDelegateIfNeed(action, token);
			if (Act == null && Wrapper != null)
			{
				return new PolicyResult().WithNoDelegateExceptionAndPolicyNameFrom(this);
			}

			var retryResult = _simpleProcessor.Execute(Act, token)
							  .SetWrappedPolicyResults(Wrapper)
							  .SetPolicyName(PolicyName);

			HandlePolicyResult(retryResult, token);
			return retryResult;
		}

		/// <summary>
		/// Executes an action synchronously with an error context parameter using the simple policy.
		/// </summary>
		/// <typeparam name="TErrorContext">The type of the error context parameter.</typeparam>
		/// <param name="action">The action to execute.</param>
		/// <param name="param">The error context parameter.</param>
		/// <param name="token">The cancellation token to monitor for cancellation requests.</param>
		public PolicyResult Handle<TErrorContext>(Action action, TErrorContext param, CancellationToken token = default)
		{
			var (Act, Wrapper) = WrapDelegateIfNeed(action, token);
			if (Act == null && Wrapper != null)
			{
				return new PolicyResult().WithNoDelegateExceptionAndPolicyNameFrom(this);
			}

			ThrowIfProcessorIsNotDefault(out SimplePolicyProcessor processor);

			var retryResult = processor.Execute(Act, param, token)
							  .SetWrappedPolicyResults(Wrapper)
							  .SetPolicyName(PolicyName);

			HandlePolicyResult(retryResult, token);
			return retryResult;
		}

		/// <summary>
		/// Executes an action with a parameter synchronously using the simple policy.
		/// </summary>
		/// <typeparam name="TParam">The type of the parameter.</typeparam>
		/// <param name="action">The action to execute.</param>
		/// <param name="param">The parameter to pass to the action.</param>
		/// <param name="token">The cancellation token to monitor for cancellation requests.</param>
		public PolicyResult Handle<TParam>(Action<TParam> action, TParam param, CancellationToken token = default)
		{
			if (HasPolicyWrapperFactory)
			{
				return Handle(action.Apply(param), token);
			}
			else
			{
				ThrowIfProcessorIsNotDefault(out SimplePolicyProcessor processor);

				var result = processor.Execute(action, param, token)
								  .SetPolicyName(PolicyName);
				HandlePolicyResult(result, token);
				return result;
			}
		}

		/// <summary>
		/// Executes a function synchronously and returns a result using the simple policy.
		/// </summary>
		/// <typeparam name="T">The type of the result returned by the function.</typeparam>
		/// <param name="func">The function to execute.</param>
		/// <param name="token">The cancellation token to monitor for cancellation requests.</param>
		public PolicyResult<T> Handle<T>(Func<T> func, CancellationToken token = default)
		{
			var (Fn, Wrapper) = WrapDelegateIfNeed(func, token);
			if (Fn == null && Wrapper != null)
			{
				return new PolicyResult<T>().WithNoDelegateExceptionAndPolicyNameFrom(this);
			}

			var retryResult = _simpleProcessor.Execute(Fn, token)
							  .SetWrappedPolicyResults(Wrapper)
							  .SetPolicyName(PolicyName);

			HandlePolicyResult(retryResult, token);
			return retryResult;
		}

		/// <summary>
		/// Executes a function with a parameter synchronously and returns a result using the simple policy.
		/// </summary>
		/// <typeparam name="TParam">The type of the parameter.</typeparam>
		/// <typeparam name="T">The type of the result returned by the function.</typeparam>
		/// <param name="func">The function to execute.</param>
		/// <param name="param">The parameter to pass to the function.</param>
		/// <param name="token">The cancellation token to monitor for cancellation requests.</param>
		public PolicyResult<T> Handle<TParam, T>(Func<TParam, T> func, TParam param, CancellationToken token = default)
		{
			if (HasPolicyWrapperFactory)
			{
				return Handle(func.Apply(param), token);
			}
			else
			{
				ThrowIfProcessorIsNotDefault(out SimplePolicyProcessor processor);

				var result = processor.Execute(func, param, token)
								  .SetPolicyName(PolicyName);
				HandlePolicyResult(result, token);
				return result;
			}
		}

		/// <summary>
		/// Executes a function synchronously with an error context parameter and returns a result using the simple policy.
		/// </summary>
		/// <typeparam name="TErrorContext">The type of the error context parameter.</typeparam>
		/// <typeparam name="T">The type of the result returned by the function.</typeparam>
		/// <param name="func">The function to execute.</param>
		/// <param name="param">The error context parameter.</param>
		/// <param name="token">The cancellation token to monitor for cancellation requests.</param>
		public PolicyResult<T> Handle<TErrorContext, T>(Func<T> func, TErrorContext param, CancellationToken token = default)
		{
			var (Fn, Wrapper) = WrapDelegateIfNeed(func, token);
			if (Fn == null && Wrapper != null)
			{
				return new PolicyResult<T>().WithNoDelegateExceptionAndPolicyNameFrom(this);
			}
			ThrowIfProcessorIsNotDefault(out SimplePolicyProcessor processor);

			var retryResult = processor.Execute(Fn, param, token)
							  .SetWrappedPolicyResults(Wrapper)
							  .SetPolicyName(PolicyName);

			HandlePolicyResult(retryResult, token);
			return retryResult;
		}

		/// <summary>
		/// Executes an action asynchronously using the simple policy.
		/// </summary>
		/// <param name="func">The asynchronous action to execute.</param>
		/// <param name="configureAwait">Whether to configure await for the async operation.</param>
		/// <param name="token">The cancellation token to monitor for cancellation requests.</param>
		public async Task<PolicyResult> HandleAsync(Func<CancellationToken, Task> func, bool configureAwait = false, CancellationToken token = default)
		{
			var (Fn, Wrapper) = WrapDelegateIfNeed(func, token, configureAwait);
			if (Fn == null && Wrapper != null)
			{
				return new PolicyResult().WithNoDelegateExceptionAndPolicyNameFrom(this);
			}

			var retryResult = (await _simpleProcessor.ExecuteAsync(Fn, configureAwait, token).ConfigureAwait(configureAwait))
									.SetWrappedPolicyResults(Wrapper)
									.SetPolicyName(PolicyName);

			await HandlePolicyResultAsync(retryResult, configureAwait, token).ConfigureAwait(configureAwait);
			return retryResult;
		}

		/// <summary>
		/// Executes an action asynchronously with an error context parameter using the simple policy.
		/// </summary>
		/// <typeparam name="TErrorContext">The type of the error context parameter.</typeparam>
		/// <param name="func">The asynchronous action to execute.</param>
		/// <param name="param">The error context parameter.</param>
		/// <param name="token">The cancellation token to monitor for cancellation requests.</param>
		public Task<PolicyResult> HandleAsync<TErrorContext>(Func<CancellationToken, Task> func, TErrorContext param, CancellationToken token)
		{
			return HandleAsync(func, param, false, token);
		}

		/// <summary>
		/// Executes an action asynchronously with an error context parameter using the simple policy.
		/// </summary>
		/// <typeparam name="TErrorContext">The type of the error context parameter.</typeparam>
		/// <param name="func">The asynchronous action to execute.</param>
		/// <param name="param">The error context parameter.</param>
		/// <param name="configureAwait">Whether to configure await for the async operation.</param>
		/// <param name="token">The cancellation token to monitor for cancellation requests.</param>
		public async Task<PolicyResult> HandleAsync<TErrorContext>(Func<CancellationToken, Task> func, TErrorContext param, bool configureAwait, CancellationToken token)
		{
			var (Fn, Wrapper) = WrapDelegateIfNeed(func, token, configureAwait);
			if (Fn == null && Wrapper != null)
			{
				return new PolicyResult().WithNoDelegateExceptionAndPolicyNameFrom(this);
			}

			ThrowIfProcessorIsNotDefault(out SimplePolicyProcessor processor);

			var retryResult = (await processor.ExecuteAsync(Fn, param, configureAwait, token).ConfigureAwait(configureAwait))
									.SetWrappedPolicyResults(Wrapper)
									.SetPolicyName(PolicyName);

			await HandlePolicyResultAsync(retryResult, configureAwait, token).ConfigureAwait(configureAwait);
			return retryResult;
		}

		/// <summary>
		/// Executes an action with a parameter asynchronously using the simple policy.
		/// </summary>
		/// <typeparam name="TParam">The type of the parameter.</typeparam>
		/// <param name="func">The asynchronous action to execute.</param>
		/// <param name="param">The parameter to pass to the action.</param>
		/// <param name="token">The cancellation token to monitor for cancellation requests.</param>
		public Task<PolicyResult> HandleAsync<TParam>(Func<TParam, CancellationToken, Task> func, TParam param, CancellationToken token)
		{
			return HandleAsync(func, param, false, token);
		}

		/// <summary>
		/// Executes an action with a parameter asynchronously using the simple policy.
		/// </summary>
		/// <typeparam name="TParam">The type of the parameter.</typeparam>
		/// <param name="func">The asynchronous action to execute.</param>
		/// <param name="param">The parameter to pass to the action.</param>
		/// <param name="configureAwait">Whether to configure await for the async operation.</param>
		/// <param name="token">The cancellation token to monitor for cancellation requests.</param>
		public async Task<PolicyResult> HandleAsync<TParam>(Func<TParam, CancellationToken, Task> func, TParam param, bool configureAwait, CancellationToken token)
		{
			if (HasPolicyWrapperFactory)
			{
				return await HandleAsync(func.Apply(param), configureAwait, token).ConfigureAwait(configureAwait);
			}
			else
			{
				ThrowIfProcessorIsNotDefault(out SimplePolicyProcessor processor);

				var result = (await processor.ExecuteAsync(func, param, configureAwait, token).ConfigureAwait(configureAwait))
								  .SetPolicyName(PolicyName);
				HandlePolicyResult(result, token);
				return result;
			}
		}

		/// <summary>
		/// Executes a function asynchronously and returns a result using the simple policy.
		/// </summary>
		/// <typeparam name="T">The type of the result returned by the function.</typeparam>
		/// <param name="func">The asynchronous function to execute.</param>
		/// <param name="configureAwait">Whether to configure await for the async operation.</param>
		/// <param name="token">The cancellation token to monitor for cancellation requests.</param>
		public async Task<PolicyResult<T>> HandleAsync<T>(Func<CancellationToken, Task<T>> func, bool configureAwait = false, CancellationToken token = default)
		{
			var (Fn, Wrapper) = WrapDelegateIfNeed(func, token, configureAwait);
			if (Fn == null && Wrapper != null)
			{
				return new PolicyResult<T>().WithNoDelegateExceptionAndPolicyNameFrom(this);
			}

			var retryResult = (await _simpleProcessor.ExecuteAsync(Fn, configureAwait, token).ConfigureAwait(configureAwait))
									.SetWrappedPolicyResults(Wrapper)
									.SetPolicyName(PolicyName);

			await HandlePolicyResultAsync(retryResult, configureAwait, token).ConfigureAwait(configureAwait);
			return retryResult;
		}

		/// <summary>
		/// Asynchronously executes a delegate with a parameter and returns a policy result.
		/// </summary>
		/// <typeparam name="TParam">The type of the parameter passed to the delegate.</typeparam>
		/// <typeparam name="T">The type of the result returned by the delegate.</typeparam>
		/// <param name="func">The delegate to execute asynchronously.</param>
		/// <param name="param">The parameter to pass to the delegate.</param>
		/// <param name="token">Cancellation token to cancel the operation.</param>
		public Task<PolicyResult<T>> HandleAsync<TParam, T>(Func<TParam, CancellationToken, Task<T>> func, TParam param, CancellationToken token)
		{
			return HandleAsync(func, param, false, token);
		}

		/// <summary>
		/// Asynchronously executes a delegate with a parameter and returns a policy result.
		/// </summary>
		/// <typeparam name="TParam">The type of the parameter passed to the delegate.</typeparam>
		/// <typeparam name="T">The type of the result returned by the delegate.</typeparam>
		/// <param name="func">The delegate to execute asynchronously.</param>
		/// <param name="param">The parameter to pass to the delegate.</param>
		/// <param name="configureAwait">Whether to configure await for the async operation.</param>
		/// <param name="token">Cancellation token to cancel the operation.</param>
		public async Task<PolicyResult<T>> HandleAsync<TParam, T>(Func<TParam, CancellationToken, Task<T>> func, TParam param, bool configureAwait, CancellationToken token)
		{
			if (HasPolicyWrapperFactory)
			{
				return await HandleAsync(func.Apply(param), configureAwait, token).ConfigureAwait(configureAwait);
			}
			else
			{
				ThrowIfProcessorIsNotDefault(out SimplePolicyProcessor processor);

				var result = (await processor.ExecuteAsync(func, param, configureAwait, token).ConfigureAwait(configureAwait))
								  .SetPolicyName(PolicyName);
				HandlePolicyResult(result, token);
				return result;
			}
		}

		/// <summary>
		/// Asynchronously executes a delegate with an error context parameter and returns a policy result.
		/// </summary>
		/// <typeparam name="TErrorContext">The type of the error context parameter.</typeparam>
		/// <typeparam name="T">The type of the result returned by the delegate.</typeparam>
		/// <param name="func">The delegate to execute asynchronously.</param>
		/// <param name="param">The error context parameter to pass to the delegate.</param>
		/// <param name="token">Cancellation token to cancel the operation.</param>
		/// <returns>A task representing the asynchronous operation that yields a policy result.</returns>
		public Task<PolicyResult<T>> HandleAsync<TErrorContext, T>(Func<CancellationToken, Task<T>> func, TErrorContext param, CancellationToken token)
		{
			return HandleAsync(func, param, false, token);
		}

		/// <summary>
		/// Asynchronously executes a delegate with an error context parameter and returns a policy result.
		/// </summary>
		/// <typeparam name="TErrorContext">The type of the error context parameter.</typeparam>
		/// <typeparam name="T">The type of the result returned by the delegate.</typeparam>
		/// <param name="func">The delegate to execute asynchronously.</param>
		/// <param name="param">The error context parameter to pass to the delegate.</param>
		/// <param name="configureAwait">Whether to configure await for the async operation.</param>
		/// <param name="token">Cancellation token to cancel the operation.</param>
		/// <returns>A task representing the asynchronous operation that yields a policy result.</returns>
		public async Task<PolicyResult<T>> HandleAsync<TErrorContext, T>(Func<CancellationToken, Task<T>> func, TErrorContext param, bool configureAwait, CancellationToken token)
		{
			var (Fn, Wrapper) = WrapDelegateIfNeed(func, token, configureAwait);
			if (Fn == null && Wrapper != null)
			{
				return new PolicyResult<T>().WithNoDelegateExceptionAndPolicyNameFrom(this);
			}

			ThrowIfProcessorIsNotDefault(out SimplePolicyProcessor processor);

			var retryResult = (await processor.ExecuteAsync(Fn, param, configureAwait, token).ConfigureAwait(configureAwait))
									.SetWrappedPolicyResults(Wrapper)
									.SetPolicyName(PolicyName);

			await HandlePolicyResultAsync(retryResult, configureAwait, token).ConfigureAwait(configureAwait);
			return retryResult;
		}

		/// <summary>
		/// Includes a specific exception type with an optional predicate for filtering.
		/// </summary>
		/// <typeparam name="TException">The type of exception to include.</typeparam>
		/// <param name="func">Optional predicate function to filter exceptions.</param>
		/// <returns>The current SimplePolicy instance for method chaining.</returns>
		public SimplePolicy IncludeError<TException>(Func<TException, bool> func = null) where TException : Exception => this.IncludeError<SimplePolicy, TException>(func);

		/// <summary>
		/// Includes exceptions based on a LINQ expression predicate.
		/// </summary>
		/// <param name="expression">The expression predicate to filter exceptions.</param>
		/// <returns>The current SimplePolicy instance for method chaining.</returns>
		public SimplePolicy IncludeError(Expression<Func<Exception, bool>> expression) => this.IncludeError<SimplePolicy>(expression);

		/// <summary>
		/// Includes a set of two specific exception types.
		/// </summary>
		/// <typeparam name="TException1">The first type of exception to include.</typeparam>
		/// <typeparam name="TException2">The second type of exception to include.</typeparam>
		/// <returns>The current SimplePolicy instance for method chaining.</returns>
		public SimplePolicy IncludeErrorSet<TException1, TException2>() where TException1 : Exception where TException2 : Exception => this.IncludeErrorSet<SimplePolicy, TException1, TException2>();

		/// <summary>
		/// Includes exceptions based on an error set.
		/// </summary>
		/// <param name="errorSet">The error set containing exceptions to include.</param>
		/// <returns>The current SimplePolicy instance for method chaining.</returns>
		public SimplePolicy IncludeErrorSet(IErrorSet errorSet) => this.IncludeErrorSet<SimplePolicy>(errorSet);

		/// <summary>
		/// Specifies the type- and optionally <paramref name="predicate"/> predicate-based filter condition for the inner exception of a handling exception to be included in the handling by the Simple policy.
		/// </summary>
		/// <typeparam name="TInnerException">A type of an inner exception.</typeparam>
		/// <param name="predicate">A predicate that an inner exception should satisfy.</param>
		/// <returns></returns>
		public SimplePolicy IncludeInnerError<TInnerException>(Func<TInnerException, bool> predicate = null) where TInnerException : Exception => this.IncludeInnerError<SimplePolicy, TInnerException>(predicate);

		/// <summary>
		/// Specifies the type- and optionally <paramref name="predicate"/> predicate-based filter condition for the inner exception of a handling exception to be excluded from the handling by the Simple policy.
		/// </summary>
		/// <typeparam name="TInnerException">A type of an inner exception.</typeparam>
		/// <param name="predicate">A predicate that an inner exception should satisfy.</param>
		/// <returns></returns>
		public SimplePolicy ExcludeInnerError<TInnerException>(Func<TInnerException, bool> predicate = null) where TInnerException : Exception => this.ExcludeInnerError<SimplePolicy, TInnerException>(predicate);

		/// <summary>
		/// Excludes a specific exception type with an optional predicate for filtering.
		/// </summary>
		/// <typeparam name="TException">The type of exception to exclude.</typeparam>
		/// <param name="func">Optional predicate function to filter exceptions.</param>
		/// <returns>The current SimplePolicy instance for method chaining.</returns>
		public SimplePolicy ExcludeError<TException>(Func<TException, bool> func = null) where TException : Exception => this.ExcludeError<SimplePolicy, TException>(func);

		/// <summary>
		/// Excludes exceptions based on a LINQ expression predicate.
		/// </summary>
		/// <param name="expression">The expression predicate to filter exceptions.</param>
		/// <returns>The current SimplePolicy instance for method chaining.</returns>
		public SimplePolicy ExcludeError(Expression<Func<Exception, bool>> expression) => this.ExcludeError<SimplePolicy>(expression);

		/// <summary>
		/// Excludes a set of two specific exception types.
		/// </summary>
		/// <typeparam name="TException1">The first type of exception to exclude.</typeparam>
		/// <typeparam name="TException2">The second type of exception to exclude.</typeparam>
		/// <returns>The current SimplePolicy instance for method chaining.</returns>
		public SimplePolicy ExcludeErrorSet<TException1, TException2>() where TException1 : Exception where TException2 : Exception => this.ExcludeErrorSet<SimplePolicy, TException1, TException2>();

		/// <summary>
		/// Excludes exceptions based on an error set.
		/// </summary>
		/// <param name="errorSet">The error set containing exceptions to exclude.</param>
		/// <returns>The current SimplePolicy instance for method chaining.</returns>
		public SimplePolicy ExcludeErrorSet(IErrorSet errorSet) => this.ExcludeErrorSet<SimplePolicy>(errorSet);

		/// <summary>
		/// Adds a synchronous action handler for policy results.
		/// </summary>
		/// <param name="action">The action to execute when a policy result is available.</param>
		/// <returns>The current SimplePolicy instance for method chaining.</returns>
		public SimplePolicy AddPolicyResultHandler(Action<PolicyResult> action)
		{
			return this.AddHandlerForPolicyResult(action);
		}

		/// <summary>
		/// Adds a synchronous action handler for policy results with cancellation type conversion.
		/// </summary>
		/// <param name="action">The action to execute when a policy result is available.</param>
		/// <param name="convertType">The cancellation type conversion to apply.</param>
		/// <returns>The current SimplePolicy instance for method chaining.</returns>
		public SimplePolicy AddPolicyResultHandler(Action<PolicyResult> action, CancellationType convertType)
		{
			return this.AddHandlerForPolicyResult(action, convertType);
		}

		/// <summary>
		/// Adds a synchronous action handler for policy results with cancellation token support.
		/// </summary>
		/// <param name="action">The action to execute when a policy result is available, with cancellation token.</param>
		/// <returns>The current SimplePolicy instance for method chaining.</returns>
		public SimplePolicy AddPolicyResultHandler(Action<PolicyResult, CancellationToken> action)
		{
			return this.AddHandlerForPolicyResult(action);
		}

		/// <summary>
		/// Adds an asynchronous function handler for policy results.
		/// </summary>
		/// <param name="func">The function to execute when a policy result is available.</param>
		/// <returns>The current SimplePolicy instance for method chaining.</returns>
		public SimplePolicy AddPolicyResultHandler(Func<PolicyResult, Task> func)
		{
			return this.AddHandlerForPolicyResult(func);
		}

		/// <summary>
		/// Adds an asynchronous function handler for policy results with cancellation type conversion.
		/// </summary>
		/// <param name="func">The function to execute when a policy result is available.</param>
		/// <param name="convertType">The cancellation type conversion to apply.</param>
		/// <returns>The current SimplePolicy instance for method chaining.</returns>
		public SimplePolicy AddPolicyResultHandler(Func<PolicyResult, Task> func, CancellationType convertType)
		{
			return this.AddHandlerForPolicyResult(func, convertType);
		}

		/// <summary>
		/// Adds an asynchronous function handler for policy results with cancellation token support.
		/// </summary>
		/// <param name="func">The function to execute when a policy result is available, with cancellation token.</param>
		/// <returns>The current SimplePolicy instance for method chaining.</returns>
		public SimplePolicy AddPolicyResultHandler(Func<PolicyResult, CancellationToken, Task> func)
		{
			return this.AddHandlerForPolicyResult(func);
		}

		/// <summary>
		/// Adds a synchronous action handler for typed policy results.
		/// </summary>
		/// <typeparam name="T">The type of the policy result value.</typeparam>
		/// <param name="action">The action to execute when a typed policy result is available.</param>
		/// <returns>The current SimplePolicy instance for method chaining.</returns>
		public SimplePolicy AddPolicyResultHandler<T>(Action<PolicyResult<T>> action)
		{
			return this.AddHandlerForPolicyResult(action);
		}

		/// <summary>
		/// Adds a synchronous action handler for typed policy results with cancellation type conversion.
		/// </summary>
		/// <typeparam name="T">The type of the policy result value.</typeparam>
		/// <param name="action">The action to execute when a typed policy result is available.</param>
		/// <param name="convertType">The cancellation type conversion to apply.</param>
		/// <returns>The current SimplePolicy instance for method chaining.</returns>
		public SimplePolicy AddPolicyResultHandler<T>(Action<PolicyResult<T>> action, CancellationType convertType)
		{
			return this.AddHandlerForPolicyResult(action, convertType);
		}

		/// <summary>
		/// Adds a synchronous action handler for typed policy results with cancellation token support.
		/// </summary>
		/// <typeparam name="T">The type of the policy result value.</typeparam>
		/// <param name="action">The action to execute when a typed policy result is available, with cancellation token.</param>
		/// <returns>The current SimplePolicy instance for method chaining.</returns>
		public SimplePolicy AddPolicyResultHandler<T>(Action<PolicyResult<T>, CancellationToken> action)
		{
			return this.AddHandlerForPolicyResult(action);
		}

		/// <summary>
		/// Adds an asynchronous function handler for typed policy results.
		/// </summary>
		/// <typeparam name="T">The type of the policy result value.</typeparam>
		/// <param name="func">The function to execute when a typed policy result is available.</param>
		/// <returns>The current SimplePolicy instance for method chaining.</returns>
		public SimplePolicy AddPolicyResultHandler<T>(Func<PolicyResult<T>, Task> func)
		{
			return this.AddHandlerForPolicyResult(func);
		}

		/// <summary>
		/// Adds an asynchronous function handler for typed policy results with cancellation type conversion.
		/// </summary>
		/// <typeparam name="T">The type of the policy result value.</typeparam>
		/// <param name="func">The function to execute when a typed policy result is available.</param>
		/// <param name="convertType">The cancellation type conversion to apply.</param>
		/// <returns>The current SimplePolicy instance for method chaining.</returns>
		public SimplePolicy AddPolicyResultHandler<T>(Func<PolicyResult<T>, Task> func, CancellationType convertType)
		{
			return this.AddHandlerForPolicyResult(func, convertType);
		}

		/// <summary>
		/// Adds an asynchronous function handler for typed policy results with cancellation token support.
		/// </summary>
		/// <typeparam name="T">The type of the policy result value.</typeparam>
		/// <param name="func">The function to execute when a typed policy result is available, with cancellation token.</param>
		/// <returns>The current SimplePolicy instance for method chaining.</returns>
		public SimplePolicy AddPolicyResultHandler<T>(Func<PolicyResult<T>, CancellationToken, Task> func)
		{
			return this.AddHandlerForPolicyResult(func);
		}

		/// <summary>
		/// Sets a policy result to failed state based on a predicate condition.
		/// </summary>
		/// <param name="predicate">The predicate function to determine if the policy result should be marked as failed.</param>
		/// <returns>The current SimplePolicy instance for method chaining.</returns>
		///<inheritdoc cref = "PolicyResultHandlerRegistration.SetPolicyResultFailedIfInner{SimplePolicy}"/>
		public SimplePolicy SetPolicyResultFailedIf(Func<PolicyResult, bool> predicate)
		{
			return this.SetPolicyResultFailedIfInner(predicate);
		}

		/// <summary>
		/// Sets a typed policy result to failed state based on a predicate condition.
		/// </summary>
		/// <typeparam name="T">The type of the policy result value.</typeparam>
		/// <param name="predicate">The predicate function to determine if the policy result should be marked as failed.</param>
		/// <returns>The current SimplePolicy instance for method chaining.</returns>
		///<inheritdoc cref = "PolicyResultHandlerRegistration.SetPolicyResultFailedIfInner{SimplePolicy, T}"/>
		public SimplePolicy SetPolicyResultFailedIf<T>(Func<PolicyResult<T>, bool> predicate)
		{
			return this.SetPolicyResultFailedIfInner(predicate);
		}

		/// <summary>
		/// Sets a typed policy result to failed state based on a predicate condition and executes a handler when set.
		/// </summary>
		/// <typeparam name="T">The type of the policy result value.</typeparam>
		/// <param name="predicate">The predicate function to determine if the policy result should be marked as failed.</param>
		/// <param name="onSetPolicyResultFailed">The action to execute when the policy result is set to failed state.</param>
		/// <returns>The current SimplePolicy instance for method chaining.</returns>
		///<inheritdoc cref = "PolicyResultHandlerRegistration.SetPolicyResultFailedWithHandlerIfInner{SimplePolicy, T}"/>
		public SimplePolicy SetPolicyResultFailedIf<T>(Func<PolicyResult<T>, bool> predicate, Action<PolicyResult<T>> onSetPolicyResultFailed)
		{
			return this.SetPolicyResultFailedWithHandlerIfInner(predicate, onSetPolicyResultFailed);
		}

		/// <summary>
		/// Adds a synchronous action processor for error context handling.
		/// </summary>
		/// <typeparam name="TErrorContext">The type of the error context.</typeparam>
		/// <param name="actionProcessor">The action processor to handle exceptions with error context.</param>
		/// <returns>The current SimplePolicy instance for method chaining.</returns>
		public SimplePolicy WithErrorContextProcessorOf<TErrorContext>(Action<Exception, ProcessingErrorInfo<TErrorContext>> actionProcessor)
		{
			ThrowIfProcessorIsNotDefault(out SimplePolicyProcessor _);
			return this.WithErrorContextProcessorOf<SimplePolicy, TErrorContext>(actionProcessor);
		}

		/// <summary>
		/// Adds a synchronous action processor for error context handling with cancellation type conversion.
		/// </summary>
		/// <typeparam name="TErrorContext">The type of the error context.</typeparam>
		/// <param name="actionProcessor">The action processor to handle exceptions with error context.</param>
		/// <param name="cancellationType">The cancellation type conversion to apply.</param>
		/// <returns>The current SimplePolicy instance for method chaining.</returns>
		public SimplePolicy WithErrorContextProcessorOf<TErrorContext>(Action<Exception, ProcessingErrorInfo<TErrorContext>> actionProcessor, CancellationType cancellationType)
		{
			ThrowIfProcessorIsNotDefault(out SimplePolicyProcessor _);
			return this.WithErrorContextProcessorOf<SimplePolicy, TErrorContext>(actionProcessor, cancellationType);
		}

		/// <summary>
		/// Adds a synchronous action processor for error context handling with cancellation token support.
		/// </summary>
		/// <typeparam name="TErrorContext">The type of the error context.</typeparam>
		/// <param name="actionProcessor">The action processor to handle exceptions with error context, with cancellation token.</param>
		/// <returns>The current SimplePolicy instance for method chaining.</returns>
		public SimplePolicy WithErrorContextProcessorOf<TErrorContext>(Action<Exception, ProcessingErrorInfo<TErrorContext>, CancellationToken> actionProcessor)
		{
			ThrowIfProcessorIsNotDefault(out SimplePolicyProcessor _);
			return this.WithErrorContextProcessorOf<SimplePolicy, TErrorContext>(actionProcessor);
		}

		/// <summary>
		/// Adds an asynchronous function processor for error context handling.
		/// </summary>
		/// <typeparam name="TErrorContext">The type of the error context.</typeparam>
		/// <param name="funcProcessor">The function processor to handle exceptions with error context.</param>
		/// <returns>The current SimplePolicy instance for method chaining.</returns>
		public SimplePolicy WithErrorContextProcessorOf<TErrorContext>(Func<Exception, ProcessingErrorInfo<TErrorContext>, Task> funcProcessor)
		{
			ThrowIfProcessorIsNotDefault(out SimplePolicyProcessor _);
			return this.WithErrorContextProcessorOf<SimplePolicy, TErrorContext>(funcProcessor);
		}

		/// <summary>
		/// Adds an asynchronous function processor for error context handling with cancellation type conversion.
		/// </summary>
		/// <typeparam name="TErrorContext">The type of the error context.</typeparam>
		/// <param name="funcProcessor">The function processor to handle exceptions with error context.</param>
		/// <param name="cancellationType">The cancellation type conversion to apply.</param>
		/// <returns>The current SimplePolicy instance for method chaining.</returns>
		public SimplePolicy WithErrorContextProcessorOf<TErrorContext>(Func<Exception, ProcessingErrorInfo<TErrorContext>, Task> funcProcessor, CancellationType cancellationType)
		{
			ThrowIfProcessorIsNotDefault(out SimplePolicyProcessor _);
			return this.WithErrorContextProcessorOf<SimplePolicy, TErrorContext>(funcProcessor, cancellationType);
		}

		/// <summary>
		/// Adds an asynchronous function processor for error context handling with cancellation token support.
		/// </summary>
		/// <typeparam name="TErrorContext">The type of the error context.</typeparam>
		/// <param name="funcProcessor">The function processor to handle exceptions with error context, with cancellation token.</param>
		/// <returns>The current SimplePolicy instance for method chaining.</returns>
		public SimplePolicy WithErrorContextProcessorOf<TErrorContext>(Func<Exception, ProcessingErrorInfo<TErrorContext>, CancellationToken, Task> funcProcessor)
		{
			ThrowIfProcessorIsNotDefault(out SimplePolicyProcessor _);
			return this.WithErrorContextProcessorOf<SimplePolicy, TErrorContext>(funcProcessor);
		}

		/// <summary>
		/// Adds a default error processor for error context handling.
		/// </summary>
		/// <typeparam name="TErrorContext">The type of the error context.</typeparam>
		/// <param name="errorProcessor">The default error processor to handle exceptions with error context.</param>
		/// <returns>The current SimplePolicy instance for method chaining.</returns>
		public SimplePolicy WithErrorContextProcessor<TErrorContext>(DefaultErrorProcessor<TErrorContext> errorProcessor)
		{
			ThrowIfProcessorIsNotDefault(out SimplePolicyProcessor _);
			return this.WithErrorContextProcessor<SimplePolicy, TErrorContext>(errorProcessor);
		}

		private void ThrowIfProcessorIsNotDefault(out SimplePolicyProcessor proc)
		{
			ThrowHelper.ThrowIfNotImplemented(_simpleProcessor, out proc);
		}

		/// <summary>
		/// Adds a non-empty catch block filter to the policy processor.
		/// </summary>
		/// <param name="filter">The non-empty catch block filter to add.</param>
		/// <returns>The current SimplePolicy instance for method chaining.</returns>
		///<inheritdoc cref = "ICanAddErrorFilter{SimplePolicy}.AddErrorFilter(NonEmptyCatchBlockFilter)"/>
		public SimplePolicy AddErrorFilter(NonEmptyCatchBlockFilter filter)
		{
			PolicyProcessor.AddNonEmptyCatchBlockFilter(filter);
			return this;
		}

		/// <summary>
		/// Adds a non-empty catch block filter factory to the policy processor.
		/// </summary>
		/// <param name="filterFactory">The factory function that creates a non-empty catch block filter.</param>
		/// <returns>The current SimplePolicy instance for method chaining.</returns>
		///<inheritdoc cref = "ICanAddErrorFilter{SimplePolicy}.AddErrorFilter(Func{IEmptyCatchBlockFilter, NonEmptyCatchBlockFilter})"/>
		public SimplePolicy AddErrorFilter(Func<IEmptyCatchBlockFilter, NonEmptyCatchBlockFilter> filterFactory)
		{
			PolicyProcessor.AddNonEmptyCatchBlockFilter(filterFactory);
			return this;
		}

		/// <summary>
		/// Wraps this policy with a <see cref="FallbackPolicy"/> and returns the resulting fallback policy.
		/// </summary>
		/// <param name="onlyGenericFallbackForGenericDelegate">Specifies that only the generic fallback delegates, if any are added, will be called to handle the generic delegates.</param>
		public FallbackPolicy ThenFallback(bool onlyGenericFallbackForGenericDelegate = false)
		{
			return this.WrapUp(new FallbackPolicy(onlyGenericFallbackForGenericDelegate)).OuterPolicy;
		}
	}
}

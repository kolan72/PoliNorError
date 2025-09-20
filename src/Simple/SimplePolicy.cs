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

		public Task<PolicyResult> HandleAsync<TErrorContext>(Func<CancellationToken, Task> func, TErrorContext param, CancellationToken token)
		{
			return HandleAsync(func, param, false, token);
		}

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

		public Task<PolicyResult> HandleAsync<TParam>(Func<TParam, CancellationToken, Task> func, TParam param, CancellationToken token)
		{
			return HandleAsync(func, param, false, token);
		}

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

		public Task<PolicyResult<T>> HandleAsync<TParam, T>(Func<TParam, CancellationToken, Task<T>> func, TParam param, CancellationToken token)
		{
			return HandleAsync(func, param, false, token);
		}

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

		public Task<PolicyResult<T>> HandleAsync<TErrorContext, T>(Func<CancellationToken, Task<T>> func, TErrorContext param, CancellationToken token)
		{
			return HandleAsync(func, param, false, token);
		}

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

		public SimplePolicy IncludeError<TException>(Func<TException, bool> func = null) where TException : Exception => this.IncludeError<SimplePolicy, TException>(func);

		public SimplePolicy IncludeError(Expression<Func<Exception, bool>> expression) => this.IncludeError<SimplePolicy>(expression);

		public SimplePolicy IncludeErrorSet<TException1, TException2>() where TException1 : Exception where TException2 : Exception => this.IncludeErrorSet<SimplePolicy, TException1, TException2>();

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

		public SimplePolicy ExcludeError<TException>(Func<TException, bool> func = null) where TException : Exception => this.ExcludeError<SimplePolicy, TException>(func);

		public SimplePolicy ExcludeError(Expression<Func<Exception, bool>> expression) => this.ExcludeError<SimplePolicy>(expression);

		public SimplePolicy ExcludeErrorSet<TException1, TException2>() where TException1 : Exception where TException2 : Exception => this.ExcludeErrorSet<SimplePolicy, TException1, TException2>();

		public SimplePolicy ExcludeErrorSet(IErrorSet errorSet) => this.ExcludeErrorSet<SimplePolicy>(errorSet);

		public SimplePolicy AddPolicyResultHandler(Action<PolicyResult> action)
		{
			return this.AddHandlerForPolicyResult(action);
		}

		public SimplePolicy AddPolicyResultHandler(Action<PolicyResult> action, CancellationType convertType)
		{
			return this.AddHandlerForPolicyResult(action, convertType);
		}

		public SimplePolicy AddPolicyResultHandler(Action<PolicyResult, CancellationToken> action)
		{
			return this.AddHandlerForPolicyResult(action);
		}

		public SimplePolicy AddPolicyResultHandler(Func<PolicyResult, Task> func)
		{
			return this.AddHandlerForPolicyResult(func);
		}

		public SimplePolicy AddPolicyResultHandler(Func<PolicyResult, Task> func, CancellationType convertType)
		{
			return this.AddHandlerForPolicyResult(func, convertType);
		}

		public SimplePolicy AddPolicyResultHandler(Func<PolicyResult, CancellationToken, Task> func)
		{
			return this.AddHandlerForPolicyResult(func);
		}

		public SimplePolicy AddPolicyResultHandler<T>(Action<PolicyResult<T>> action)
		{
			return this.AddHandlerForPolicyResult(action);
		}

		public SimplePolicy AddPolicyResultHandler<T>(Action<PolicyResult<T>> action, CancellationType convertType)
		{
			return this.AddHandlerForPolicyResult(action, convertType);
		}

		public SimplePolicy AddPolicyResultHandler<T>(Action<PolicyResult<T>, CancellationToken> action)
		{
			return this.AddHandlerForPolicyResult(action);
		}

		public SimplePolicy AddPolicyResultHandler<T>(Func<PolicyResult<T>, Task> func)
		{
			return this.AddHandlerForPolicyResult(func);
		}

		public SimplePolicy AddPolicyResultHandler<T>(Func<PolicyResult<T>, Task> func, CancellationType convertType)
		{
			return this.AddHandlerForPolicyResult(func, convertType);
		}

		public SimplePolicy AddPolicyResultHandler<T>(Func<PolicyResult<T>, CancellationToken, Task> func)
		{
			return this.AddHandlerForPolicyResult(func);
		}

		///<inheritdoc cref = "PolicyResultHandlerRegistration.SetPolicyResultFailedIfInner{SimplePolicy}"/>
		public SimplePolicy SetPolicyResultFailedIf(Func<PolicyResult, bool> predicate)
		{
			return this.SetPolicyResultFailedIfInner(predicate);
		}

		///<inheritdoc cref = "PolicyResultHandlerRegistration.SetPolicyResultFailedIfInner{SimplePolicy, T}"/>
		public SimplePolicy SetPolicyResultFailedIf<T>(Func<PolicyResult<T>, bool> predicate)
		{
			return this.SetPolicyResultFailedIfInner(predicate);
		}

		///<inheritdoc cref = "PolicyResultHandlerRegistration.SetPolicyResultFailedWithHandlerIfInner{SimplePolicy, T}"/>
		public SimplePolicy SetPolicyResultFailedIf<T>(Func<PolicyResult<T>, bool> predicate, Action<PolicyResult<T>> onSetPolicyResultFailed)
		{
			return this.SetPolicyResultFailedWithHandlerIfInner(predicate, onSetPolicyResultFailed);
		}

		public SimplePolicy WithErrorContextProcessorOf<TErrorContext>(Action<Exception, ProcessingErrorInfo<TErrorContext>> actionProcessor)
		{
			ThrowIfProcessorIsNotDefault(out SimplePolicyProcessor _);
			return this.WithErrorContextProcessorOf<SimplePolicy, TErrorContext>(actionProcessor);
		}

		public SimplePolicy WithErrorContextProcessorOf<TErrorContext>(Action<Exception, ProcessingErrorInfo<TErrorContext>> actionProcessor, CancellationType cancellationType)
		{
			ThrowIfProcessorIsNotDefault(out SimplePolicyProcessor _);
			return this.WithErrorContextProcessorOf<SimplePolicy, TErrorContext>(actionProcessor, cancellationType);
		}

		public SimplePolicy WithErrorContextProcessorOf<TErrorContext>(Action<Exception, ProcessingErrorInfo<TErrorContext>, CancellationToken> actionProcessor)
		{
			ThrowIfProcessorIsNotDefault(out SimplePolicyProcessor _);
			return this.WithErrorContextProcessorOf<SimplePolicy, TErrorContext>(actionProcessor);
		}

		public SimplePolicy WithErrorContextProcessorOf<TErrorContext>(Func<Exception, ProcessingErrorInfo<TErrorContext>, Task> funcProcessor)
		{
			ThrowIfProcessorIsNotDefault(out SimplePolicyProcessor _);
			return this.WithErrorContextProcessorOf<SimplePolicy, TErrorContext>(funcProcessor);
		}

		public SimplePolicy WithErrorContextProcessorOf<TErrorContext>(Func<Exception, ProcessingErrorInfo<TErrorContext>, Task> funcProcessor, CancellationType cancellationType)
		{
			ThrowIfProcessorIsNotDefault(out SimplePolicyProcessor _);
			return this.WithErrorContextProcessorOf<SimplePolicy, TErrorContext>(funcProcessor, cancellationType);
		}

		public SimplePolicy WithErrorContextProcessorOf<TErrorContext>(Func<Exception, ProcessingErrorInfo<TErrorContext>, CancellationToken, Task> funcProcessor)
		{
			ThrowIfProcessorIsNotDefault(out SimplePolicyProcessor _);
			return this.WithErrorContextProcessorOf<SimplePolicy, TErrorContext>(funcProcessor);
		}

		public SimplePolicy WithErrorContextProcessor<TErrorContext>(DefaultErrorProcessor<TErrorContext> errorProcessor)
		{
			ThrowIfProcessorIsNotDefault(out SimplePolicyProcessor _);
			return this.WithErrorContextProcessor<SimplePolicy, TErrorContext>(errorProcessor);
		}

		private void ThrowIfProcessorIsNotDefault(out SimplePolicyProcessor proc)
		{
			ThrowHelper.ThrowIfNotImplemented(_simpleProcessor, out proc);
		}

		///<inheritdoc cref = "ICanAddErrorFilter{SimplePolicy}.AddErrorFilter(NonEmptyCatchBlockFilter)"/>
		public SimplePolicy AddErrorFilter(NonEmptyCatchBlockFilter filter)
		{
			PolicyProcessor.AddNonEmptyCatchBlockFilter(filter);
			return this;
		}

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

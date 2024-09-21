using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using PoliNorError.Extensions.PolicyErrorFiltering;

namespace PoliNorError
{
	/// <summary>
	/// A simple policy that can handle delegates.
	/// </summary>
	public sealed partial class SimplePolicy : Policy, IPolicyBase, IWithErrorFilter<SimplePolicy>, IWithInnerErrorFilter<SimplePolicy>
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
			return this.AddPolicyResultHandlerInner(action);
		}

		public SimplePolicy AddPolicyResultHandler(Action<PolicyResult> action, CancellationType convertType)
		{
			return this.AddPolicyResultHandlerInner(action, convertType);
		}

		public SimplePolicy AddPolicyResultHandler(Action<PolicyResult, CancellationToken> action)
		{
			return this.AddPolicyResultHandlerInner(action);
		}

		public SimplePolicy AddPolicyResultHandler(Func<PolicyResult, Task> func)
		{
			return this.AddPolicyResultHandlerInner(func);
		}

		public SimplePolicy AddPolicyResultHandler(Func<PolicyResult, Task> func, CancellationType convertType)
		{
			return this.AddPolicyResultHandlerInner(func, convertType);
		}

		public SimplePolicy AddPolicyResultHandler(Func<PolicyResult, CancellationToken, Task> func)
		{
			return this.AddPolicyResultHandlerInner(func);
		}

		public SimplePolicy AddPolicyResultHandler<T>(Action<PolicyResult<T>> action)
		{
			return this.AddPolicyResultHandlerInner(action);
		}

		public SimplePolicy AddPolicyResultHandler<T>(Action<PolicyResult<T>> action, CancellationType convertType)
		{
			return this.AddPolicyResultHandlerInner(action, convertType);
		}

		public SimplePolicy AddPolicyResultHandler<T>(Action<PolicyResult<T>, CancellationToken> action)
		{
			return this.AddPolicyResultHandlerInner(action);
		}

		public SimplePolicy AddPolicyResultHandler<T>(Func<PolicyResult<T>, Task> func)
		{
			return this.AddPolicyResultHandlerInner(func);
		}

		public SimplePolicy AddPolicyResultHandler<T>(Func<PolicyResult<T>, Task> func, CancellationType convertType)
		{
			return this.AddPolicyResultHandlerInner(func, convertType);
		}

		public SimplePolicy AddPolicyResultHandler<T>(Func<PolicyResult<T>, CancellationToken, Task> func)
		{
			return this.AddPolicyResultHandlerInner(func);
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
	}
}

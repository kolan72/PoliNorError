using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public sealed class SimplePolicy : Policy, IPolicyBase, IWithErrorFilter<SimplePolicy>
	{
		private readonly ISimplePolicyProcessor _simpleProcessor;

		public SimplePolicy(IBulkErrorProcessor processor = null) : this(new SimplePolicyProcessor(processor)) { }

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

		public SimplePolicy ExcludeError<TException>(Func<TException, bool> func = null) where TException : Exception => this.ExcludeError<SimplePolicy, TException>(func);

		public SimplePolicy ExcludeError(Expression<Func<Exception, bool>> expression) => this.ExcludeError<SimplePolicy>(expression);

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
	}
}

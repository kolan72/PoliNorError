using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public sealed class SimplePolicy : HandleErrorPolicyBase, IPolicyBase, IWithErrorFilter<SimplePolicy>
	{
		private readonly ISimplePolicyProcessor _simpleProcessor;

		public SimplePolicy(IBulkErrorProcessor processor = null) : this(new SimplePolicyProcessor(processor ?? new BulkErrorProcessor())) { }

		public SimplePolicy(ISimplePolicyProcessor processor) : base(processor) => _simpleProcessor = processor;

		public PolicyResult Handle(Action action, CancellationToken token = default)
		{
			PolicyResult retryResult = null;

			if (_wrappedPolicy == null)
			{
				retryResult = _simpleProcessor.Execute(action, token);
			}
			else
			{
				if (action == null)
					return PolicyResult.ForSync().SetFailedWithError(new NoDelegateException(this));

				var wrapper = new PolicyWrapper(_wrappedPolicy, action, token);
				Action actionWrapped = wrapper.Handle;

				retryResult = _simpleProcessor.Execute(actionWrapped, token);
				retryResult.WrappedPolicyResults = wrapper.PolicyDelegateResults;
			}
			HandlePolicyResult(retryResult, token);
			return retryResult;
		}

		public PolicyResult<T> Handle<T>(Func<T> func, CancellationToken token = default)
		{
			PolicyResult<T> retryResult = null;

			if (_wrappedPolicy == null)
			{
				retryResult = _simpleProcessor.Execute(func, token);
			}
			else
			{
				if (func == null)
					return PolicyResult<T>.ForSync().SetFailedWithError(new NoDelegateException(this));

				var wrapper = new PolicyWrapper<T>(_wrappedPolicy, func, token);
				Func<T> funcWrapped = wrapper.Handle;

				retryResult = _simpleProcessor.Execute(funcWrapped, token);
				retryResult.WrappedPolicyResults = wrapper.PolicyResults.Select(pr => pr.ToPolicyDelegateResult());
			}
			HandlePolicyResult(retryResult, token);
			return retryResult;
		}

		public async Task<PolicyResult> HandleAsync(Func<CancellationToken, Task> func, bool configureAwait = false, CancellationToken token = default)
		{
			PolicyResult retryResult = null;

			if (_wrappedPolicy == null)
			{
				retryResult = await _simpleProcessor.ExecuteAsync(func, configureAwait, token).ConfigureAwait(configureAwait);
			}
			else
			{
				if (func == null)
					return PolicyResult.ForNotSync().SetFailedWithError(new NoDelegateException(this));

				var wrapper = new PolicyWrapper(_wrappedPolicy, func, token, configureAwait);
				Func<CancellationToken, Task> funcWrapped = wrapper.HandleAsync;

				retryResult = await _simpleProcessor.ExecuteAsync(funcWrapped, configureAwait, token).ConfigureAwait(configureAwait);
				retryResult.WrappedPolicyResults = wrapper.PolicyDelegateResults;
			}
			await HandlePolicyResultAsync(retryResult, configureAwait, token).ConfigureAwait(configureAwait);
			return retryResult;
		}

		public async Task<PolicyResult<T>> HandleAsync<T>(Func<CancellationToken, Task<T>> func, bool configureAwait = false, CancellationToken token = default)
		{
			PolicyResult<T> retryResult = null;
			if (_wrappedPolicy == null)
			{
				retryResult = await _simpleProcessor.ExecuteAsync(func, configureAwait, token).ConfigureAwait(configureAwait);
			}
			else
			{
				if (func == null)
					return PolicyResult<T>.ForNotSync().SetFailedWithError(new NoDelegateException(this));
				var wrapper = new PolicyWrapper<T>(_wrappedPolicy, func, token, configureAwait);
				Func<CancellationToken, Task<T>> funcWrapped = wrapper.HandleAsync;

				retryResult = await _simpleProcessor.ExecuteAsync(funcWrapped, configureAwait, token).ConfigureAwait(configureAwait);
				retryResult.WrappedPolicyResults = wrapper.PolicyResults.Select(pr => pr.ToPolicyDelegateResult());
			}
			await HandlePolicyResultAsync(retryResult, configureAwait, token).ConfigureAwait(configureAwait);
			return retryResult;
		}

		public SimplePolicy IncludeError<TException>(Func<TException, bool> func = null) where TException : Exception => this.IncludeError<SimplePolicy, TException>(func);

		public SimplePolicy IncludeError(Expression<Func<Exception, bool>> expression) => this.IncludeError<SimplePolicy>(expression);

		public SimplePolicy ExcludeError<TException>(Func<TException, bool> func = null) where TException : Exception => this.ExcludeError<SimplePolicy, TException>(func);

		public SimplePolicy ExcludeError(Expression<Func<Exception, bool>> expression) => this.ExcludeError<SimplePolicy>(expression);

		public SimplePolicy AddPolicyResultHandler(Action<PolicyResult> action, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return this.AddPolicyResultHandlerInner(action, convertType);
		}

		public SimplePolicy AddPolicyResultHandler(Action<PolicyResult, CancellationToken> action)
		{
			return this.AddPolicyResultHandlerInner(action);
		}

		public SimplePolicy AddPolicyResultHandler(Func<PolicyResult, Task> func, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return this.AddPolicyResultHandlerInner(func, convertType);
		}

		public SimplePolicy AddPolicyResultHandler(Func<PolicyResult, CancellationToken, Task> func)
		{
			return this.AddPolicyResultHandlerInner(func);
		}

		public SimplePolicy AddPolicyResultHandler<T>(Action<PolicyResult<T>> action, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return this.AddPolicyResultHandlerInner(action, convertType);
		}

		public SimplePolicy AddPolicyResultHandler<T>(Action<PolicyResult<T>, CancellationToken> action)
		{
			return this.AddPolicyResultHandlerInner(action);
		}

		public SimplePolicy AddPolicyResultHandler<T>(Func<PolicyResult<T>, Task> func, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return this.AddPolicyResultHandlerInner(func, convertType);
		}

		public SimplePolicy AddPolicyResultHandler<T>(Func<PolicyResult<T>, CancellationToken, Task> func)
		{
			return this.AddPolicyResultHandlerInner(func);
		}
	}
}

using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	internal sealed class SimplePolicy : HandleErrorPolicyBase, IPolicyBase
	{
		private readonly ISimplePolicyProcessor _simpleProcessor;

		public SimplePolicy(IBulkErrorProcessor processor = null) : this(new SimplePolicyProcessor(processor ?? new BulkErrorProcessor())) { }

		public SimplePolicy(ISimplePolicyProcessor processor) : base(processor) => _simpleProcessor = processor;

		public PolicyResult Handle(Action action, CancellationToken token = default)
		{
			if (action == null)
				return PolicyResult.ForSync().SetFailedWithError(new NoDelegateException(this));

			PolicyResult retryResult = null;

			if (_wrappedPolicy == null)
			{
				retryResult = _simpleProcessor.Execute(action, token);
			}
			else
			{
				var wrapper = new PolicyWrapper(_wrappedPolicy, action, token);
				Action actionWrapped = wrapper.Handle;

				retryResult = _simpleProcessor.Execute(actionWrapped, token);
				retryResult.WrappedPolicyResults = wrapper.PolicyResults;
			}
			HandlePolicyResult(retryResult, token);
			return retryResult;
		}

		public PolicyResult<T> Handle<T>(Func<T> func, CancellationToken token = default) => throw new NotImplementedException();
		public Task<PolicyResult> HandleAsync(Func<CancellationToken, Task> func, bool configureAwait = false, CancellationToken token = default) => throw new NotImplementedException();
		public Task<PolicyResult<T>> HandleAsync<T>(Func<CancellationToken, Task<T>> func, bool configureAwait = false, CancellationToken token = default) => throw new NotImplementedException();
	}
}

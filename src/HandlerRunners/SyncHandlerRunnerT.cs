using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	internal sealed class SyncHandlerRunnerT : HandlerRunnerBase, IHandlerRunnerT
	{
		private readonly Action<PolicyResult, object, CancellationToken> _act;

		public override bool SyncRun => true;

		public static SyncHandlerRunnerT Create<T>(Action<PolicyResult<T>, CancellationToken> act, int num)
		{
			void actArg(PolicyResult pr, object _, CancellationToken ct) => act((PolicyResult<T>)pr, ct);
			return new SyncHandlerRunnerT(actArg, num);
		}

		private SyncHandlerRunnerT(Action<PolicyResult, object, CancellationToken> act, int num) : base(num)
		{
			_act = act;
		}

		public void Run<T>(PolicyResult<T> policyResult, CancellationToken token = default)
		{
			if (token.IsCancellationRequested)
				return;

			bool wasNotFailed = false;
			if (!policyResult.IsFailed)
				wasNotFailed = true;

			_act(policyResult, policyResult.Result, token);

			if (wasNotFailed && policyResult.IsFailed)
				policyResult.FailedReason = PolicyResultFailedReason.PolicyResultHandlerFailed;
		}

		public Task RunAsync<T>(PolicyResult<T> policyResult, CancellationToken token = default)
		{
			throw new NotImplementedException();
		}
	}
}

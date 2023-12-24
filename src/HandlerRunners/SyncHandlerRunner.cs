using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	internal class SyncHandlerRunner : HandlerRunnerBase, IHandlerRunner
	{
		private readonly Action<PolicyResult, CancellationToken> _act;

		public SyncHandlerRunner(Action<PolicyResult, CancellationToken> act, int num) : base(num)
		{
			_act = act;
		}

		public SyncHandlerRunner(Action<PolicyResult, CancellationToken> act)
		{
			_act = act;
		}

		public override bool SyncRun => true;

		public void Run(PolicyResult policyResult, CancellationToken token = default)
		{
			bool wasNotFailed = false;
			if (!policyResult.IsFailed)
				wasNotFailed = true;
			_act(policyResult, token);
			if (wasNotFailed && policyResult.IsFailed)
				policyResult.FailedReason = PolicyResultFailedReason.PolicyResultHandlerFailed;
		}

		public Task RunAsync(PolicyResult policyResult, CancellationToken token = default) => throw new NotImplementedException();
	}
}

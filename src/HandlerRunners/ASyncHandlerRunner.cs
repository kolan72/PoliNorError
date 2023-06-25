using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	internal class ASyncHandlerRunner : HandlerRunnerBase, IHandlerRunner
	{
		private readonly Func<PolicyResult, CancellationToken, Task> _func;

		public ASyncHandlerRunner(Func<PolicyResult, CancellationToken, Task> func, int num) : base(num)
		{
			_func = func;
		}

		public override bool SyncRun => false;

		public async Task RunAsync(PolicyResult policyResult, CancellationToken token = default)
		{
			if (token.IsCancellationRequested)
				return;

			bool wasNotFailed = false;
			if (!policyResult.IsFailed)
				wasNotFailed = true;
			await _func(policyResult, token);
			if (wasNotFailed && policyResult.IsFailed)
				policyResult.FailedReason = PolicyResultFailedReason.PolicyResultHandlerFailed;
		}

		public void Run(PolicyResult policyResult, CancellationToken token = default) => throw new NotImplementedException();
	}
}

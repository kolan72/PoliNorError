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

		public override bool UseSync => true;

		public void Run(PolicyResult policyResult, CancellationToken token = default)
		{
			if (token.IsCancellationRequested)
				return;
			_act(policyResult, token);
		}

		public Task RunAsync(PolicyResult policyResult, CancellationToken token = default) => throw new NotImplementedException();
	}
}

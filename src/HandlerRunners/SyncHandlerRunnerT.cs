using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	internal sealed class SyncHandlerRunnerT : HandlerRunnerBase, IHandlerRunnerT
	{
		private readonly SyncHandlerRunner _syncHandlerRunnerInner;
		public override bool SyncRun => true;

		public static SyncHandlerRunnerT Create<T>(Action<PolicyResult<T>, CancellationToken> act, int num)
		{
			void actArg(PolicyResult pr, CancellationToken ct) => act((PolicyResult<T>)pr, ct);
			return new SyncHandlerRunnerT(new SyncHandlerRunner(actArg, num), num);
		}

		private SyncHandlerRunnerT(SyncHandlerRunner syncHandlerRunner, int num) : base(num)
		{
			_syncHandlerRunnerInner = syncHandlerRunner;
		}

		public void Run<T>(PolicyResult<T> policyResult, CancellationToken token = default)
		{
			_syncHandlerRunnerInner.Run(policyResult, token);
		}

		public Task RunAsync<T>(PolicyResult<T> policyResult, CancellationToken token = default)
		{
			throw new NotImplementedException();
		}
	}
}

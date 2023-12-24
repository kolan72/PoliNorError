using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	internal sealed class SyncHandlerRunnerT : HandlerRunnerBase, IHandlerRunnerT
	{
		private readonly SyncHandlerRunner _syncHandlerRunnerInner;
		public override bool SyncRun => true;

		private readonly Type _type;

		public static SyncHandlerRunnerT Create<T>(Action<PolicyResult<T>, CancellationToken> act, int num)
		{
			void actArg(PolicyResult pr, CancellationToken ct) => act((PolicyResult<T>)pr, ct);
			return new SyncHandlerRunnerT(new SyncHandlerRunner(actArg, num), num, typeof(T));
		}

		public static SyncHandlerRunnerT Create<T>(Action<PolicyResult<T>, CancellationToken> act)
		{
			void actArg(PolicyResult pr, CancellationToken ct) => act((PolicyResult<T>)pr, ct);
			return new SyncHandlerRunnerT(new SyncHandlerRunner(actArg), typeof(T));
		}

		private SyncHandlerRunnerT(SyncHandlerRunner syncHandlerRunner, int num, Type type) : base(num)
		{
			_syncHandlerRunnerInner = syncHandlerRunner;
			_type = type;
		}

		private SyncHandlerRunnerT(SyncHandlerRunner syncHandlerRunner, Type type)
		{
			_syncHandlerRunnerInner = syncHandlerRunner;
			_type = type;
		}

		public void Run<T>(PolicyResult<T> policyResult, CancellationToken token = default)
		{
			if (typeof(T) != _type)
				return;
			_syncHandlerRunnerInner.Run(policyResult, token);
		}

		public Task RunAsync<T>(PolicyResult<T> policyResult, CancellationToken token = default)
		{
			throw new NotImplementedException();
		}
	}
}
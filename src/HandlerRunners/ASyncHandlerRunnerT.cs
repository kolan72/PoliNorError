using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	internal sealed class ASyncHandlerRunnerT : HandlerRunnerBase, IHandlerRunnerT
	{
		private readonly ASyncHandlerRunner _asyncHandlerRunnerInner;

		public override bool SyncRun => false;

		public static ASyncHandlerRunnerT Create<T>(Func<PolicyResult<T>, CancellationToken, Task> func, int num)
		{
			Task funcArg(PolicyResult pr, CancellationToken ct) => func((PolicyResult<T>)pr, ct);
			return new ASyncHandlerRunnerT(new ASyncHandlerRunner(funcArg, num), num);
		}

		private ASyncHandlerRunnerT(ASyncHandlerRunner asyncHandlerRunnerInner, int num) : base(num)
		{
			_asyncHandlerRunnerInner = asyncHandlerRunnerInner;
		}

		public Task RunAsync<T>(PolicyResult<T> policyResult, CancellationToken token = default)
		{
			return _asyncHandlerRunnerInner.RunAsync(policyResult, token);
		}

		public void Run<T>(PolicyResult<T> policyResult, CancellationToken token = default) => throw new NotImplementedException();
	}
}

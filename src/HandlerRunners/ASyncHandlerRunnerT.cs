using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	internal sealed class ASyncHandlerRunnerT : HandlerRunnerBase, IHandlerRunnerT
	{
		private readonly ASyncHandlerRunner _asyncHandlerRunnerInner;

		public override bool SyncRun => false;

		private readonly Type _type;

		public static ASyncHandlerRunnerT Create<T>(Func<PolicyResult<T>, CancellationToken, Task> func, int num)
		{
			Task funcArg(PolicyResult pr, CancellationToken ct) => func((PolicyResult<T>)pr, ct);
			return new ASyncHandlerRunnerT(new ASyncHandlerRunner(funcArg, num), num, typeof(T));
		}

		public static ASyncHandlerRunnerT Create<T>(Func<PolicyResult<T>, CancellationToken, Task> func)
		{
			Task funcArg(PolicyResult pr, CancellationToken ct) => func((PolicyResult<T>)pr, ct);
			return new ASyncHandlerRunnerT(new ASyncHandlerRunner(funcArg), typeof(T));
		}

		private ASyncHandlerRunnerT(ASyncHandlerRunner asyncHandlerRunnerInner, int num, Type type) : base(num)
		{
			_asyncHandlerRunnerInner = asyncHandlerRunnerInner;
			_type = type;
		}

		private ASyncHandlerRunnerT(ASyncHandlerRunner asyncHandlerRunnerInner, Type type)
		{
			_asyncHandlerRunnerInner = asyncHandlerRunnerInner;
			_type = type;
		}

		public Task RunAsync<T>(PolicyResult<T> policyResult, CancellationToken token = default)
		{
			if (typeof(T) != _type)
				return Task.CompletedTask;
			return _asyncHandlerRunnerInner.RunAsync(policyResult, token);
		}

		public void Run<T>(PolicyResult<T> policyResult, CancellationToken token = default) => throw new NotImplementedException();
	}
}

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

		public override bool UseSync => false;

		public async Task RunAsync(PolicyResult policyResult, CancellationToken token = default)
		{
			if (token.IsCancellationRequested)
				return;

			await _func(policyResult, token);
		}

		public void Run(PolicyResult policyResult, CancellationToken token = default) => throw new NotImplementedException();
	}
}

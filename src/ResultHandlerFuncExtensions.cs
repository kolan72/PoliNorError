using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	internal static class ResultHandlerFuncExtensions
	{
		public static Action<PolicyResult, CancellationToken> ToPolicyResultHandlerAction(this Action<IEnumerable<Exception>, CancellationToken> action)
		{
			return (pr, ct) => action(pr.Errors, ct);
		}

		public static Func<PolicyResult, CancellationToken, Task> ToPolicyResultHandlerAsyncFunc(this Func<IEnumerable<Exception>, CancellationToken, Task> func)
		{
			return (pr, ct) => func(pr.Errors, ct);
		}
	}
}

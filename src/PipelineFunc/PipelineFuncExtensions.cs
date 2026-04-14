using System;
using System.Threading;

namespace PoliNorError
{
	internal static class PipelineFuncExtensions
	{
		internal static Func<T, CancellationToken, PipelineResult<U>> Bind<T, M, U>(this Func<T, CancellationToken, PipelineResult<M>> func, Func<M, CancellationToken, PipelineResult<U>> funcNext)
		{
			return (t, ct) =>
			{
				var result = func(t, ct);
				if (result.IsFailed)
				{
					return PipelineResult<U>.Failure(result.FailedPolicyResult);
				}

				var resultNext = funcNext(result.SucceededPolicyResult.Result, ct);

				if (resultNext.IsFailed)
				{
					return PipelineResult<U>.Failure(resultNext.FailedPolicyResult);
				}
				return PipelineResult<U>.Success(resultNext.SucceededPolicyResult);
			};
		}
	}
}

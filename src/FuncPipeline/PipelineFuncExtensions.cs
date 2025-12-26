using System;
using System.Threading;

namespace PoliNorError
{
	internal static class PipelineFuncExtensions
	{
		internal static Func<T, CancellationToken, PipelineResult<U>> ToHandledByPolicy<T, U>(this Func<T, U> func, SimplePolicy policy)
		{
			return (t, ct) =>
			{
				var res = policy.Handle(func, t, ct);

				if (ct.IsCancellationRequested)
				{
					return PipelineResult<U>.Failure(res, true);
				}

				if (!res.NoError || res.IsCanceled)
				{
					return PipelineResult<U>.Failure(res);
				}
				return PipelineResult<U>.Success(res);
			};
		}
	}
}

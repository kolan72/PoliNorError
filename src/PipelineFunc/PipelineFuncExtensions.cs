using System;
using System.Threading;

namespace PoliNorError
{
	/// <summary>
	/// Extension methods for pipeline function composition.
	/// </summary>
	internal static class PipelineFuncExtensions
	{
		/// <summary>
		/// Binds two pipeline functions together, creating a composed function.
		/// </summary>
		/// <typeparam name="T">The input type.</typeparam>
		/// <typeparam name="M">The intermediate type.</typeparam>
		/// <typeparam name="U">The output type.</typeparam>
		/// <param name="func">The first function in the composition.</param>
		/// <param name="funcNext">The second function in the composition.</param>
		/// <returns>A composed function that executes both functions in sequence.</returns>
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

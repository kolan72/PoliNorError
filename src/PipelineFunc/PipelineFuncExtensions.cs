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

		/// <summary>
		/// Wraps a synchronous function with a resilience policy, producing a pipeline-compatible delegate.
		/// </summary>
		/// <typeparam name="TIn">The input type accepted by the function.</typeparam>
		/// <typeparam name="TOut">The output type returned by the function.</typeparam>
		/// <param name="func">The synchronous function to be executed under the policy.</param>
		/// <param name="policy">The resilience policy (<see cref="RetryPolicy"/>, <see cref="FallbackPolicy"/>, or <see cref="SimplePolicy"/>) that governs execution of <paramref name="func"/>.</param>
		/// <returns>
		/// A <c>Func&lt;TIn, CancellationToken, PipelineResult&lt;TOut&gt;&gt;</c> that executes <paramref name="func"/>
		/// through <paramref name="policy"/> and returns a <see cref="PipelineResult{TOut}"/> representing
		/// success, failure, or cancellation of the operation.
		/// </returns>
		internal static Func<TIn, CancellationToken, PipelineResult<TOut>> ToPipelineFunc<TIn, TOut>(this Func<TIn, TOut> func, IPolicyBase policy)
		{
			Func<TIn, CancellationToken, PolicyResult<TOut>> handler;
			bool isSimplePolicy;
			switch (policy)
			{
				case RetryPolicy rp:
					handler = (t, ct) => rp.Handle(func, t, ct);
					isSimplePolicy = false;
					break;
				case FallbackPolicy fp:
					handler = (t, ct) => fp.Handle(func, t, ct);
					isSimplePolicy = false;
					break;
				case SimplePolicy sp:
					handler = (t, ct) => sp.Handle(func, t, ct);
					isSimplePolicy = true;
					break;
				default:
					var notSupportedError = new NotSupportedException(
						$"Pipelines support only {nameof(RetryPolicy)}, {nameof(FallbackPolicy)} and {nameof(SimplePolicy)}. Got: {policy?.GetType().Name ?? "null"}.");
					return (_, __) => PipelineResult<TOut>.Failure(new PolicyResult().SetFailedWithError(notSupportedError, PolicyResultFailedReason.UnsupportedPolicy));
			}

			return (t, ct) =>
			{
				var res = handler(t, ct);
				if ((isSimplePolicy ? !res.NoError : res.IsFailed) || res.IsCanceled)
				{
					return PipelineResult<TOut>.Failure(res);
				}
				return PipelineResult<TOut>.Success(res);
			};
		}
	}
}

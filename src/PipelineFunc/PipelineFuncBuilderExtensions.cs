using System;
using System.Threading;

namespace PoliNorError
{
	public static class PipelineFuncBuilderExtensions
	{
		/// <summary>
		/// Adds a function to the pipeline that transforms the output with a simple (non-retry) policy.
		/// </summary>
		/// <typeparam name="TIn">The input type for the pipeline.</typeparam>
		/// <typeparam name="TOut">The current output type of the pipeline.</typeparam>
		/// <typeparam name="TNext">The type of the next step's output.</typeparam>
		/// <param name="pipelineFuncBuilder">The pipeline function builder.</param>
		/// <param name="fNext">The function to add to the pipeline.</param>
		/// <returns>A step builder for the next pipeline stage.</returns>
		public static IPipelineFuncStepBuilder<TIn, TOut, TNext> AddFunc<TIn, TOut, TNext>(
			this IPipelineFuncBuilder<TIn, TOut> pipelineFuncBuilder,
			Func<TOut, TNext> fNext)
		{
			return pipelineFuncBuilder.AddFunc(fNext, new SimplePolicy());
		}

		/// <summary>
		/// Adds a function to the pipeline that transforms the output with a retry policy.
		/// </summary>
		/// <typeparam name="TIn">The input type for the pipeline.</typeparam>
		/// <typeparam name="TOut">The current output type of the pipeline.</typeparam>
		/// <typeparam name="TNext">The type of the next step's output.</typeparam>
		/// <param name="pipelineFuncBuilder">The pipeline function builder.</param>
		/// <param name="fNext">The function to add to the pipeline.</param>
		/// <param name="retryCount">The maximum number of retry attempts.</param>
		/// <param name="retryDelay">The delay between retries. If null, no delay is used.</param>
		/// <returns>A step builder for the next pipeline stage.</returns>
		public static IPipelineFuncStepBuilder<TIn, TOut, TNext> AddFuncWithRetry<TIn, TOut, TNext>(
			this IPipelineFuncBuilder<TIn, TOut> pipelineFuncBuilder,
			Func<TOut, TNext> fNext,
			int retryCount,
			RetryDelay retryDelay = null)
		{
			var retryPolicy = new RetryPolicy(retryCount, retryDelay: retryDelay);
			return pipelineFuncBuilder.AddFunc(fNext, retryPolicy);
		}

		/// <summary>
		/// Adds a function to the pipeline that transforms the output with an infinite retry policy.
		/// </summary>
		/// <typeparam name="TIn">The input type for the pipeline.</typeparam>
		/// <typeparam name="TOut">The current output type of the pipeline.</typeparam>
		/// <typeparam name="TNext">The type of the next step's output.</typeparam>
		/// <param name="pipelineFuncBuilder">The pipeline function builder.</param>
		/// <param name="fNext">The function to add to the pipeline.</param>
		/// <param name="retryDelay">The delay between retries. If null, no delay is used.</param>
		/// <returns>A step builder for the next pipeline stage.</returns>
		public static IPipelineFuncStepBuilder<TIn, TOut, TNext> AddFuncWithInfiniteRetry<TIn, TOut, TNext>(
			this IPipelineFuncBuilder<TIn, TOut> pipelineFuncBuilder,
			Func<TOut, TNext> fNext,
			RetryDelay retryDelay = null)
		{
			var retryPolicy = RetryPolicy.InfiniteRetries(retryDelay: retryDelay);
			return pipelineFuncBuilder.AddFunc(fNext, retryPolicy);
		}

		/// <summary>
		/// Adds a function to the pipeline that transforms the output with a fallback policy using a synchronous fallback function.
		/// </summary>
		/// <typeparam name="TIn">The input type for the pipeline.</typeparam>
		/// <typeparam name="TOut">The current output type of the pipeline.</typeparam>
		/// <typeparam name="TNext">The type of the next step's output.</typeparam>
		/// <param name="pipelineFuncBuilder">The pipeline function builder.</param>
		/// <param name="fNext">The function to add to the pipeline.</param>
		/// <param name="fallbackFunc">The fallback function to execute when the primary function fails.</param>
		/// <returns>A step builder for the next pipeline stage.</returns>
		public static IPipelineFuncStepBuilder<TIn, TOut, TNext> AddFuncWithFallback<TIn, TOut, TNext>(
			this IPipelineFuncBuilder<TIn, TOut> pipelineFuncBuilder,
			Func<TOut, TNext> fNext,
			Func<TNext> fallbackFunc)
		{
			var fallbackPolicy = new FallbackPolicy()
				.WithFallbackFunc(fallbackFunc);
			return pipelineFuncBuilder.AddFunc(fNext, fallbackPolicy);
		}

		/// <summary>
		/// Adds a function to the pipeline that transforms the output with a fallback policy using a synchronous fallback function with cancellation support.
		/// </summary>
		/// <typeparam name="TIn">The input type for the pipeline.</typeparam>
		/// <typeparam name="TOut">The current output type of the pipeline.</typeparam>
		/// <typeparam name="TNext">The type of the next step's output.</typeparam>
		/// <param name="pipelineFuncBuilder">The pipeline function builder.</param>
		/// <param name="fNext">The function to add to the pipeline.</param>
		/// <param name="fallbackFunc">The fallback function to execute when the primary function fails.</param>
		/// <returns>A step builder for the next pipeline stage.</returns>
		public static IPipelineFuncStepBuilder<TIn, TOut, TNext> AddFuncWithFallback<TIn, TOut, TNext>(
			this IPipelineFuncBuilder<TIn, TOut> pipelineFuncBuilder,
			Func<TOut, TNext> fNext,
			Func<CancellationToken, TNext> fallbackFunc)
		{
			var fallbackPolicy = new FallbackPolicy()
				.WithFallbackFunc(fallbackFunc);
			return pipelineFuncBuilder.AddFunc(fNext, fallbackPolicy);
		}
	}
}

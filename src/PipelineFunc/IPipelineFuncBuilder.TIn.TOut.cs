using System;
using System.Threading;

namespace PoliNorError
{
	/// <summary>
	/// Builder interface for constructing pipeline functions.
	/// </summary>
	/// <typeparam name="TIn">The input type for the pipeline.</typeparam>
	/// <typeparam name="TOut">The output type for the pipeline.</typeparam>
	public interface IPipelineFuncBuilder<TIn, TOut>
	{
		/// <summary>
		/// Adds a function to the pipeline that transforms the output using the default SimplePolicy.
		/// </summary>
		/// <typeparam name="TNext">The type of the next step's output.</typeparam>
		/// <param name="fNext">The function to add to the pipeline.</param>
		/// <returns>A step builder for the next pipeline stage.</returns>
		IPipelineFuncStepBuilder<TIn, TOut, TNext> AddFunc<TNext>(Func<TOut, TNext> fNext);

		/// <summary>
		/// Adds a function to the pipeline that transforms the output using a specified policy.
		/// </summary>
		/// <typeparam name="TNext">The type of the next step's output.</typeparam>
		/// <param name="fNext">The function to add to the pipeline.</param>
		/// <param name="policy">The policy to use for error handling. If null, a SimplePolicy will be created.</param>
		/// <returns>A step builder for the next pipeline stage.</returns>
		IPipelineFuncStepBuilder<TIn, TOut, TNext> AddFunc<TNext>(Func<TOut, TNext> fNext, IPolicyBase policy);

		/// <summary>
		/// Adds a function to the pipeline with retry policy.
		/// </summary>
		/// <typeparam name="TNext">The type of the next step's output.</typeparam>
		/// <param name="fNext">The function to add to the pipeline.</param>
		/// <param name="retryCount">The number of retry attempts.</param>
		/// <param name="retryDelay">Optional retry delay configuration.</param>
		/// <returns>A step builder for the next pipeline stage.</returns>
		IPipelineFuncStepBuilder<TIn, TOut, TNext> AddFuncWithRetry<TNext>(
			Func<TOut, TNext> fNext,
			int retryCount,
			RetryDelay retryDelay = null);

		/// <summary>
		/// Adds a function to the pipeline with infinite retry policy.
		/// </summary>
		/// <typeparam name="TNext">The type of the next step's output.</typeparam>
		/// <param name="fNext">The function to add to the pipeline.</param>
		/// <param name="retryDelay">Optional retry delay configuration.</param>
		/// <returns>A step builder for the next pipeline stage.</returns>
		IPipelineFuncStepBuilder<TIn, TOut, TNext> AddFuncWithInfiniteRetry<TNext>(
			Func<TOut, TNext> fNext,
			RetryDelay retryDelay = null);

		/// <summary>
		/// Adds a function to the pipeline with fallback policy.
		/// </summary>
		/// <typeparam name="TNext">The type of the next step's output.</typeparam>
		/// <param name="fNext">The function to add to the pipeline.</param>
		/// <param name="fallbackFunc">The fallback function to execute if the main function fails.</param>
		/// <returns>A step builder for the next pipeline stage.</returns>
		IPipelineFuncStepBuilder<TIn, TOut, TNext> AddFuncWithFallback<TNext>(
			Func<TOut, TNext> fNext,
			Func<TNext> fallbackFunc);

		/// <summary>
		/// Adds a function to the pipeline with fallback policy that accepts a cancellation token.
		/// </summary>
		/// <typeparam name="TNext">The type of the next step's output.</typeparam>
		/// <param name="fNext">The function to add to the pipeline.</param>
		/// <param name="fallbackFunc">The fallback function to execute if the main function fails.</param>
		/// <returns>A step builder for the next pipeline stage.</returns>
		IPipelineFuncStepBuilder<TIn, TOut, TNext> AddFuncWithFallback<TNext>(
			Func<TOut, TNext> fNext,
			Func<CancellationToken, TNext> fallbackFunc);

		/// <summary>
		/// Builds the pipeline function.
		/// </summary>
		/// <returns>A function that executes the complete pipeline.</returns>
		Func<TIn, CancellationToken, PipelineResult<TOut>> Build();
	}
}

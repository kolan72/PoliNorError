using System;
using System.Threading;

namespace PoliNorError
{
	/// <summary>
	/// Static factory class for creating pipeline function builders.
	/// </summary>
	public static class PipelineFuncBuilder
	{
		/// <summary>
		/// Starts a new pipeline with the specified function using the default SimplePolicy.
		/// </summary>
		/// <typeparam name="TIn">The input type for the pipeline.</typeparam>
		/// <typeparam name="TOut">The output type for the first step.</typeparam>
		/// <param name="func">The initial function for the pipeline.</param>
		/// <param name="policyName">An optional name for the policy. If null, the policy uses its type name.</param>
		/// <returns>A pipeline builder for constructing the pipeline.</returns>
		public static PipelineFuncBuilder<TIn, TIn, TOut> StartWith<TIn, TOut>(Func<TIn, TOut> func, string policyName = null)
		{
			var delegateHolder = new PipelineDelegateHolder<TIn, TOut>(func, policyName);
			return new PipelineFuncBuilder<TIn, TIn, TOut>(delegateHolder);
		}

		/// <summary>
		/// Starts a new pipeline with the specified function using a specified policy.
		/// </summary>
		/// <typeparam name="TIn">The input type for the pipeline.</typeparam>
		/// <typeparam name="TOut">The output type for the first step.</typeparam>
		/// <param name="func">The initial function for the pipeline.</param>
		/// <param name="policy">The policy to use for error handling. If null, a SimplePolicy will be created.</param>
		/// <returns>A pipeline builder for constructing the pipeline.</returns>
		public static PipelineFuncBuilder<TIn, TIn, TOut> StartWith<TIn, TOut>(Func<TIn, TOut> func, IPolicyBase policy)
		{
			var delegateHolder = new PipelineDelegateHolder<TIn, TOut>(func, policy);
			return new PipelineFuncBuilder<TIn, TIn, TOut>(delegateHolder);
		}

		/// <summary>
		/// Starts a new pipeline with the specified function using a retry policy.
		/// </summary>
		/// <typeparam name="TIn">The input type for the pipeline.</typeparam>
		/// <typeparam name="TOut">The output type for the first step.</typeparam>
		/// <param name="func">The initial function for the pipeline.</param>
		/// <param name="retryCount">The number of retry attempts.</param>
		/// <param name="retryDelay">Optional retry delay configuration.</param>
		/// <param name="policyName">An optional name for the policy. If null, the policy uses its type name.</param>
		/// <returns>A pipeline builder for constructing the pipeline.</returns>
		public static PipelineFuncBuilder<TIn, TIn, TOut> StartWithRetry<TIn, TOut>(
			Func<TIn, TOut> func,
			int retryCount,
			RetryDelay retryDelay = null,
			string policyName = null)
		{
			var retryPolicy = new RetryPolicy(retryCount, retryDelay: retryDelay);
			return StartWith(func, policyName is null ? retryPolicy : retryPolicy.WithPolicyName(policyName));
		}

		/// <summary>
		/// Starts a new pipeline with the specified function using an infinite retry policy.
		/// </summary>
		/// <typeparam name="TIn">The input type for the pipeline.</typeparam>
		/// <typeparam name="TOut">The output type for the first step.</typeparam>
		/// <param name="func">The initial function for the pipeline.</param>
		/// <param name="retryDelay">Optional retry delay configuration.</param>
		/// <param name="policyName">An optional name for the policy. If null, the policy uses its type name.</param>
		/// <returns>A pipeline builder for constructing the pipeline.</returns>
		public static PipelineFuncBuilder<TIn, TIn, TOut> StartWithInfiniteRetry<TIn, TOut>(
			Func<TIn, TOut> func,
			RetryDelay retryDelay = null,
			string policyName = null)
		{
			var retryPolicy = RetryPolicy.InfiniteRetries(retryDelay: retryDelay);
			return StartWith(func, policyName is null ? retryPolicy : retryPolicy.WithPolicyName(policyName));
		}

		/// <summary>
		/// Starts a new pipeline with the specified function using a fallback policy.
		/// </summary>
		/// <typeparam name="TIn">The input type for the pipeline.</typeparam>
		/// <typeparam name="TOut">The output type for the first step.</typeparam>
		/// <param name="func">The initial function for the pipeline.</param>
		/// <param name="fallbackFunc">The fallback function to execute if the main function fails.</param>
		/// <param name="policyName">An optional name for the policy. If null, the policy uses its type name.</param>
		/// <returns>A pipeline builder for constructing the pipeline.</returns>
		public static PipelineFuncBuilder<TIn, TIn, TOut> StartWithFallback<TIn, TOut>(
			Func<TIn, TOut> func,
			Func<TOut> fallbackFunc,
			string policyName = null)
		{
			var fallbackPolicy = new FallbackPolicy()
				.WithFallbackFunc(fallbackFunc);
			return StartWith(func, policyName is null ? fallbackPolicy : fallbackPolicy.WithPolicyName(policyName));
		}

		/// <summary>
		/// Starts a new pipeline with the specified function using a fallback policy that accepts a cancellation token.
		/// </summary>
		/// <typeparam name="TIn">The input type for the pipeline.</typeparam>
		/// <typeparam name="TOut">The output type for the first step.</typeparam>
		/// <param name="func">The initial function for the pipeline.</param>
		/// <param name="fallbackFunc">The fallback function to execute if the main function fails.</param>
		/// <param name="policyName">An optional name for the policy. If null, the policy uses its type name.</param>
		/// <returns>A pipeline builder for constructing the pipeline.</returns>
		public static PipelineFuncBuilder<TIn, TIn, TOut> StartWithFallback<TIn, TOut>(
			Func<TIn, TOut> func,
			Func<CancellationToken, TOut> fallbackFunc,
			string policyName = null)
		{
			var fallbackPolicy = new FallbackPolicy()
				.WithFallbackFunc(fallbackFunc);
			return StartWith(func, policyName is null ? fallbackPolicy : fallbackPolicy.WithPolicyName(policyName));
		}
	}
}

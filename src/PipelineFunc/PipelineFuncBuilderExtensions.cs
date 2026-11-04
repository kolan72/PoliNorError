using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace PoliNorError
{
	public static class PipelineFuncBuilderExtensions
	{
		/// <summary>
		/// Materializes the pipeline into an immutable <see cref="PipelineFunc{TIn, TOut}"/>.
		/// The pipeline name is taken from <paramref name="pipelineName"/> when it is supplied,
		/// otherwise from the builder's underlying step (the leaf policy's <see cref="IPolicyBase.PolicyName"/>).
		/// Purely additive: <see cref="IPipelineFuncBuilder{TIn, TOut}.Build"/> keeps working unchanged.
		/// </summary>
		/// <typeparam name="TIn">The input type of the pipeline.</typeparam>
		/// <typeparam name="TOut">The output type of the pipeline.</typeparam>
		/// <param name="pipelineFuncBuilder">The pipeline function builder.</param>
		/// <param name="pipelineName">An optional name for the resulting <see cref="PipelineFunc{TIn, TOut}"/>.</param>
		/// <returns>The immutable pipeline function.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="pipelineFuncBuilder"/> is <c>null</c>.</exception>
		public static PipelineFunc<TIn, TOut> ToPipelineFunc<TIn, TOut>(
			this IPipelineFuncBuilder<TIn, TOut> pipelineFuncBuilder,
			string pipelineName = null)
		{
			if (pipelineFuncBuilder is null)
			{
				throw new ArgumentNullException(nameof(pipelineFuncBuilder));
			}

			var name = pipelineName;
			if (name is null && pipelineFuncBuilder is IPipelineFuncBuilderInternals internals)
			{
				name = internals.GetPipelineName();
			}

			return new PipelineFunc<TIn, TOut>(pipelineFuncBuilder.Build(), name);
		}

		/// <summary>
		/// Materializes the pipeline into an immutable <see cref="PipelineFunc{TIn, TOut}"/> and registers it
		/// in the <see cref="PipelineFuncRegistry"/> obtained from <paramref name="services"/>.
		/// The pipeline is registered under <paramref name="pipelineName"/> when supplied, otherwise under the
		/// default key (the name derived from the input type), which is what <see cref="PipelineFuncRegistry.Get{TIn,TOut}()"/> looks up.
		/// </summary>
		/// <typeparam name="TIn">The input type of the pipeline.</typeparam>
		/// <typeparam name="TOut">The output type of the pipeline.</typeparam>
		/// <param name="pipelineFuncBuilder">The pipeline function builder.</param>
		/// <param name="services">The service provider holding the <see cref="PipelineFuncRegistry"/> singleton.</param>
		/// <param name="pipelineName">An optional name for the resulting <see cref="PipelineFunc{TIn, TOut}"/> and its registry key.</param>
		/// <returns>The immutable pipeline function.</returns>
		/// <exception cref="ArgumentNullException">Any argument is <c>null</c>.</exception>
		/// <exception cref="InvalidOperationException">No <see cref="PipelineFuncRegistry"/> is registered in <paramref name="services"/>.</exception>
		public static PipelineFunc<TIn, TOut> ToPipelineFunc<TIn, TOut>(
			this IPipelineFuncBuilder<TIn, TOut> pipelineFuncBuilder,
			IServiceProvider services,
			string pipelineName = null)
		{
			if (services is null)
			{
				throw new ArgumentNullException(nameof(services));
			}

			var pipelineFunc = pipelineFuncBuilder.ToPipelineFunc(pipelineName);

			var registry = services.GetService<PipelineFuncRegistry>();
			if (registry is null)
			{
				throw new InvalidOperationException(
					$"No {nameof(PipelineFuncRegistry)} is registered in the service provider. " +
					$"Call {nameof(PipelineFuncServiceCollectionExtensions)}.{nameof(PipelineFuncServiceCollectionExtensions.AddPipelineFunc)} first " +
					$"or register {nameof(PipelineFuncRegistry)} manually.");
			}

			registry.Register(pipelineFunc, pipelineName ?? PipelineFuncRegistry.GetDefaultName(typeof(TIn)));
			return pipelineFunc;
		}

		/// <summary>
		/// Adds a function to the pipeline that transforms the output with a simple (non-retry) policy.
		/// </summary>
		/// <typeparam name="TIn">The input type for the pipeline.</typeparam>
		/// <typeparam name="TOut">The current output type of the pipeline.</typeparam>
		/// <typeparam name="TNext">The type of the next step's output.</typeparam>
		/// <param name="pipelineFuncBuilder">The pipeline function builder.</param>
		/// <param name="fNext">The function to add to the pipeline.</param>
		/// <param name="policyName">An optional name for the policy. If null, the policy uses its type name.</param>
		/// <returns>A step builder for the next pipeline stage.</returns>
		public static IPipelineFuncStepBuilder<TIn, TOut, TNext> AddFunc<TIn, TOut, TNext>(
			this IPipelineFuncBuilder<TIn, TOut> pipelineFuncBuilder,
			Func<TOut, TNext> fNext,
			string policyName = null)
		{
			return pipelineFuncBuilder.AddFunc(fNext, policyName is null ? new SimplePolicy() : new SimplePolicy().WithPolicyName(policyName));
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
		/// <param name="policyName">An optional name for the policy. If null, the policy uses its type name.</param>
		/// <returns>A step builder for the next pipeline stage.</returns>
		public static IPipelineFuncStepBuilder<TIn, TOut, TNext> AddFuncWithRetry<TIn, TOut, TNext>(
			this IPipelineFuncBuilder<TIn, TOut> pipelineFuncBuilder,
			Func<TOut, TNext> fNext,
			int retryCount,
			RetryDelay retryDelay = null,
			string policyName = null)
		{
			var retryPolicy = new RetryPolicy(retryCount, retryDelay: retryDelay);
			return pipelineFuncBuilder.AddFunc(fNext, policyName is null ? retryPolicy : retryPolicy.WithPolicyName(policyName));
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
		/// <param name="policyName">An optional name for the policy. If null, the policy uses its type name.</param>
		/// <returns>A step builder for the next pipeline stage.</returns>
		public static IPipelineFuncStepBuilder<TIn, TOut, TNext> AddFuncWithInfiniteRetry<TIn, TOut, TNext>(
			this IPipelineFuncBuilder<TIn, TOut> pipelineFuncBuilder,
			Func<TOut, TNext> fNext,
			RetryDelay retryDelay = null,
			string policyName = null)
		{
			var retryPolicy = RetryPolicy.InfiniteRetries(retryDelay: retryDelay);
			return pipelineFuncBuilder.AddFunc(fNext, policyName is null ? retryPolicy : retryPolicy.WithPolicyName(policyName));
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
		/// <param name="policyName">An optional name for the policy. If null, the policy uses its type name.</param>
		/// <returns>A step builder for the next pipeline stage.</returns>
		public static IPipelineFuncStepBuilder<TIn, TOut, TNext> AddFuncWithFallback<TIn, TOut, TNext>(
			this IPipelineFuncBuilder<TIn, TOut> pipelineFuncBuilder,
			Func<TOut, TNext> fNext,
			Func<TNext> fallbackFunc,
			string policyName = null)
		{
			var fallbackPolicy = new FallbackPolicy()
				.WithFallbackFunc(fallbackFunc);
			return pipelineFuncBuilder.AddFunc(fNext, policyName is null ? fallbackPolicy : fallbackPolicy.WithPolicyName(policyName));
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
		/// <param name="policyName">An optional name for the policy. If null, the policy uses its type name.</param>
		/// <returns>A step builder for the next pipeline stage.</returns>
		public static IPipelineFuncStepBuilder<TIn, TOut, TNext> AddFuncWithFallback<TIn, TOut, TNext>(
			this IPipelineFuncBuilder<TIn, TOut> pipelineFuncBuilder,
			Func<TOut, TNext> fNext,
			Func<CancellationToken, TNext> fallbackFunc,
			string policyName = null)
		{
			var fallbackPolicy = new FallbackPolicy()
				.WithFallbackFunc(fallbackFunc);
			return pipelineFuncBuilder.AddFunc(fNext, policyName is null ? fallbackPolicy : fallbackPolicy.WithPolicyName(policyName));
		}
	}
}

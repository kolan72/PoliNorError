using System;
using System.Threading;

namespace PoliNorError
{
	public static class PipelineFuncBuilderExtensions
	{
		public static IPipelineFuncStepBuilder<TIn, TOut, TNext> AddFunc<TIn, TOut, TNext>(
			this IPipelineFuncBuilder<TIn, TOut> pipelineFuncBuilder,
			Func<TOut, TNext> fNext)
		{
			return pipelineFuncBuilder.AddFunc(fNext, new SimplePolicy());
		}

		public static IPipelineFuncStepBuilder<TIn, TOut, TNext> AddFuncWithRetry<TIn, TOut, TNext>(
			this IPipelineFuncBuilder<TIn, TOut> pipelineFuncBuilder,
			Func<TOut, TNext> fNext,
			int retryCount,
			RetryDelay retryDelay = null)
		{
			var retryPolicy = new RetryPolicy(retryCount, retryDelay: retryDelay);
			return pipelineFuncBuilder.AddFunc(fNext, retryPolicy);
		}

		public static IPipelineFuncStepBuilder<TIn, TOut, TNext> AddFuncWithInfiniteRetry<TIn, TOut, TNext>(
			this IPipelineFuncBuilder<TIn, TOut> pipelineFuncBuilder,
			Func<TOut, TNext> fNext,
			RetryDelay retryDelay = null)
		{
			var retryPolicy = RetryPolicy.InfiniteRetries(retryDelay: retryDelay);
			return pipelineFuncBuilder.AddFunc(fNext, retryPolicy);
		}

		public static IPipelineFuncStepBuilder<TIn, TOut, TNext> AddFuncWithFallback<TIn, TOut, TNext>(
			this IPipelineFuncBuilder<TIn, TOut> pipelineFuncBuilder,
			Func<TOut, TNext> fNext,
			Func<TNext> fallbackFunc)
		{
			var fallbackPolicy = new FallbackPolicy()
				.WithFallbackFunc(fallbackFunc);
			return pipelineFuncBuilder.AddFunc(fNext, fallbackPolicy);
		}

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

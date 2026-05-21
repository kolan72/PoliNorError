using System;
using System.Threading;

namespace PoliNorError
{
	/// <summary>
	/// Concrete implementation of a pipeline function builder.
	/// </summary>
	/// <typeparam name="TIn">The input type for the pipeline.</typeparam>
	/// <typeparam name="TIm">The intermediate type at this step.</typeparam>
	/// <typeparam name="TOut">The output type for the pipeline.</typeparam>
	public class PipelineFuncBuilder<TIn, TIm, TOut> : IPipelineFuncStepBuilder<TIn, TIm, TOut>
	{
		private readonly IPipelineDelegateHolder<TIn, TOut> _delegateHolder;

		/// <summary>
		/// Initializes a new instance of the <see cref="PipelineFuncBuilder{TIn, TIm, TOut}"/> class.
		/// </summary>
		/// <param name="delegateHolder">The delegate holder for managing pipeline functions.</param>
		internal PipelineFuncBuilder(IPipelineDelegateHolder<TIn, TOut> delegateHolder)
		{
			_delegateHolder = delegateHolder;
		}

		/// <summary>
		/// Adds a function to the pipeline that transforms the output using the default SimplePolicy.
		/// </summary>
		/// <typeparam name="TNext">The type of the next step's output.</typeparam>
		/// <param name="fNext">The function to add to the pipeline.</param>
		/// <returns>A step builder for the next pipeline stage.</returns>
		public IPipelineFuncStepBuilder<TIn, TOut, TNext> AddFunc<TNext>(Func<TOut, TNext> fNext)
		{
			return AddFunc(fNext, policy: null);
		}

		/// <summary>
		/// Adds a function to the pipeline that transforms the output using a specified policy.
		/// </summary>
		/// <typeparam name="TNext">The type of the next step's output.</typeparam>
		/// <param name="fNext">The function to add to the pipeline.</param>
		/// <param name="policy">The policy to use for error handling. If null, a SimplePolicy will be created.</param>
		/// <returns>A step builder for the next pipeline stage.</returns>
		public IPipelineFuncStepBuilder<TIn, TOut, TNext> AddFunc<TNext>(Func<TOut, TNext> fNext, IPolicyBase policy)
		{
			var pdh = new PipelineDelegateHolder<TIn, TOut, TNext>(_delegateHolder.GetPipelineDelegate(), fNext, policy);
			return new PipelineFuncBuilder<TIn, TOut, TNext>(pdh);
		}

		/// <summary>
		/// Adds a function to the pipeline with retry policy.
		/// </summary>
		/// <typeparam name="TNext">The type of the next step's output.</typeparam>
		/// <param name="fNext">The function to add to the pipeline.</param>
		/// <param name="retryCount">The number of retry attempts.</param>
		/// <param name="retryDelay">Optional retry delay configuration.</param>
		/// <returns>A step builder for the next pipeline stage.</returns>
		public IPipelineFuncStepBuilder<TIn, TOut, TNext> AddFuncWithRetry<TNext>(
			Func<TOut, TNext> fNext,
			int retryCount,
			RetryDelay retryDelay = null)
		{
			var retryPolicy = new RetryPolicy(retryCount, retryDelay: retryDelay);
			return AddFunc(fNext, retryPolicy);
		}

		/// <summary>
		/// Adds a function to the pipeline with infinite retry policy.
		/// </summary>
		/// <typeparam name="TNext">The type of the next step's output.</typeparam>
		/// <param name="fNext">The function to add to the pipeline.</param>
		/// <param name="retryDelay">Optional retry delay configuration.</param>
		/// <returns>A step builder for the next pipeline stage.</returns>
		public IPipelineFuncStepBuilder<TIn, TOut, TNext> AddFuncWithInfiniteRetry<TNext>(
			Func<TOut, TNext> fNext,
			RetryDelay retryDelay = null)
		{
			var retryPolicy = RetryPolicy.InfiniteRetries(retryDelay: retryDelay);
			return AddFunc(fNext, retryPolicy);
		}

		/// <summary>
		/// Adds a function to the pipeline with fallback policy.
		/// </summary>
		/// <typeparam name="TNext">The type of the next step's output.</typeparam>
		/// <param name="fNext">The function to add to the pipeline.</param>
		/// <param name="fallbackFunc">The fallback function to execute if the main function fails.</param>
		/// <returns>A step builder for the next pipeline stage.</returns>
		public IPipelineFuncStepBuilder<TIn, TOut, TNext> AddFuncWithFallback<TNext>(
			Func<TOut, TNext> fNext,
			Func<TNext> fallbackFunc)
		{
			var fallbackPolicy = new FallbackPolicy()
				.WithFallbackFunc(fallbackFunc);
			return AddFunc(fNext, fallbackPolicy);
		}

		/// <summary>
		/// Adds a function to the pipeline with fallback policy that accepts a cancellation token.
		/// </summary>
		/// <typeparam name="TNext">The type of the next step's output.</typeparam>
		/// <param name="fNext">The function to add to the pipeline.</param>
		/// <param name="fallbackFunc">The fallback function to execute if the main function fails.</param>
		/// <returns>A step builder for the next pipeline stage.</returns>
		public IPipelineFuncStepBuilder<TIn, TOut, TNext> AddFuncWithFallback<TNext>(
			Func<TOut, TNext> fNext,
			Func<CancellationToken, TNext> fallbackFunc)
		{
			var fallbackPolicy = new FallbackPolicy()
				.WithFallbackFunc(fallbackFunc);
			return AddFunc(fNext, fallbackPolicy);
		}

		public IPipelineFuncBuilder<TIn, TOut> OnError(Action<BulkErrorProcessor> configure)
		{
			_delegateHolder.SetConfigure(configure);
			return this;
		}

		public IPipelineFuncBuilder<TIn, TOut> OnError(Action<ContextErrorProcessors<TIm>> configure)
		{
			void action(BulkErrorProcessor bep)
			{
				var processors = new ContextErrorProcessors<TIm>();
				configure(processors);
				foreach(var p in processors)
				{
					bep.AddProcessor(p);
				}
			}
			_delegateHolder.SetConfigure(action);
			return this;
		}

		/// <summary>
		/// Builds the pipeline function.
		/// </summary>
		/// <returns>A function that executes the complete pipeline.</returns>
		public Func<TIn, CancellationToken, PipelineResult<TOut>> Build() => _delegateHolder.GetPipelineDelegate();
	}
}

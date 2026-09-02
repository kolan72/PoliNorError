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

		/// <inheritdoc/>
		public IPipelineFuncBuilder<TIn, TOut> ConfigureErrorProcessors(Action<PipelineErrorProcessors<TIm>> configure)
		{
			void action(BulkErrorProcessor bep)
			{
				var processors = new PipelineErrorProcessors<TIm>();
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
	}
}

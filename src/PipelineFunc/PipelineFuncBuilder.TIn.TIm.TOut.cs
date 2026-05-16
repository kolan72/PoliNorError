using System;
using System.Threading;
using System.Threading.Tasks;

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
		/// Adds a function to the pipeline that transforms the output.
		/// </summary>
		/// <typeparam name="TNext">The type of the next step's output.</typeparam>
		/// <param name="fNext">The function to add to the pipeline.</param>
		/// <returns>A step builder for the next pipeline stage.</returns>
		public IPipelineFuncStepBuilder<TIn, TOut, TNext> AddFunc<TNext>(Func<TOut, TNext> fNext)
		{
			var pdh = new PipelineDelegateHolder<TIn, TOut, TNext>(_delegateHolder.GetPipelineDelegate(), fNext);
			return new PipelineFuncBuilder<TIn, TOut, TNext>(pdh);
		}

		/// <summary>
		/// Configures synchronous error handling for this pipeline step.
		/// </summary>
		/// <param name="actionProcessor">The action to execute when an error occurs.</param>
		/// <returns>The pipeline builder for further configuration.</returns>
		public IPipelineFuncBuilder<TIn, TOut> OnError(Action<Exception, ProcessingErrorInfo<TIm>> actionProcessor)
		{
			void action(BulkErrorProcessor bep) => bep.WithErrorContextProcessorOf(actionProcessor);
			_delegateHolder.SetConfigure(action);
			return this;
		}

		/// <summary>
		/// Configures asynchronous error handling for this pipeline step.
		/// </summary>
		/// <param name="actionProcessor">The async function to execute when an error occurs.</param>
		/// <returns>The pipeline builder for further configuration.</returns>
		public IPipelineFuncBuilder<TIn, TOut> OnError(Func<Exception, ProcessingErrorInfo<TIm>, Task> actionProcessor)
		{
			void action(BulkErrorProcessor bep) => bep.WithErrorContextProcessorOf(actionProcessor);
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

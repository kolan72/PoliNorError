using System;
using System.Threading;

namespace PoliNorError
{
	/// <summary>
	/// Holds and manages a pipeline delegate that chains multiple functions together.
	/// </summary>
	/// <typeparam name="TIn">The input type.</typeparam>
	/// <typeparam name="TIm">The intermediate type.</typeparam>
	/// <typeparam name="TOut">The output type.</typeparam>
	internal class PipelineDelegateHolder<TIn, TIm, TOut> : IPipelineDelegateHolder<TIn, TOut>
	{
		private readonly Func<TIn, CancellationToken, PipelineResult<TIm>> _prevFunc;

		private readonly PipelineDelegateHolder<TIm, TOut> _pipelineDelegate;

		/// <summary>
		/// Initializes a new instance of the <see cref="PipelineDelegateHolder{TIn, TIm, TOut}"/> class.
		/// </summary>
		/// <param name="prevFunc">The previous function in the pipeline.</param>
		/// <param name="fNext">The next function to add to the pipeline.</param>
		public PipelineDelegateHolder(Func<TIn, CancellationToken, PipelineResult<TIm>> prevFunc, Func<TIm, TOut> fNext)
		{
			_prevFunc = prevFunc;
			_pipelineDelegate = new PipelineDelegateHolder<TIm, TOut>(fNext);
		}

		/// <summary>
		/// Gets the pipeline delegate function.
		/// </summary>
		/// <returns>A function that processes input and returns a pipeline result.</returns>
		public Func<TIn, CancellationToken, PipelineResult<TOut>> GetPipelineDelegate()
		{
			return _prevFunc.Bind(_pipelineDelegate.GetPipelineDelegate());
		}

		/// <summary>
		/// Sets the configuration action for error processors.
		/// </summary>
		/// <param name="configureProcessors">The action to configure bulk error processors.</param>
		public void SetConfigure(Action<BulkErrorProcessor> configureProcessors)
			=> _pipelineDelegate.SetConfigure(configureProcessors);
	}
}
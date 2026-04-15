using System;
using System.Threading;

namespace PoliNorError
{
	internal class PipelineDelegateHolder<TIn, TIm, TOut> : IPipelineDelegateHolder<TIn, TOut>
	{
		private readonly Func<TIn, CancellationToken, PipelineResult<TIm>> _prevFunc;

		private readonly PipelineDelegateHolder<TIm, TOut> _pipelineDelegate;

		public PipelineDelegateHolder(Func<TIn, CancellationToken, PipelineResult<TIm>> prevFunc, Func<TIm, TOut> fNext)
		{
			_prevFunc = prevFunc;
			_pipelineDelegate = new PipelineDelegateHolder<TIm, TOut>(fNext);
		}

		public Func<TIn, CancellationToken, PipelineResult<TOut>> GetPipelineDelegate()
		{
			return _prevFunc.Bind(_pipelineDelegate.GetPipelineDelegate());
		}

		public void SetConfigure(Action<BulkErrorProcessor> configureProcessors)
			=> _pipelineDelegate.SetConfigure(configureProcessors);
	}
}
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public class PipelineFuncBuilder<TIn, TIm, TOut> : IPipelineFuncStepBuilder<TIn, TIm, TOut>
	{
		private readonly IPipelineDelegateHolder<TIn, TOut> _delegateHolder;

		internal PipelineFuncBuilder(IPipelineDelegateHolder<TIn, TOut> delegateHolder)
		{
			_delegateHolder = delegateHolder;
		}

		public IPipelineFuncStepBuilder<TIn, TOut, TNext> AddFunc<TNext>(Func<TOut, TNext> fNext)
		{
			var pdh = new PipelineDelegateHolder<TIn, TOut, TNext>(_delegateHolder.GetPipelineDelegate(), fNext);
			return new PipelineFuncBuilder<TIn, TOut, TNext>(pdh);
		}

		public IPipelineFuncBuilder<TIn, TOut> OnError(Action<Exception, ProcessingErrorInfo<TIm>> actionProcessor)
		{
			void action(BulkErrorProcessor bep) => bep.WithErrorContextProcessorOf(actionProcessor);
			_delegateHolder.SetConfigure(action);
			return this;
		}

		public IPipelineFuncBuilder<TIn, TOut> OnError(Func<Exception, ProcessingErrorInfo<TIm>, Task> actionProcessor)
		{
			void action(BulkErrorProcessor bep) => bep.WithErrorContextProcessorOf(actionProcessor);
			_delegateHolder.SetConfigure(action);
			return this;
		}

		public Func<TIn, CancellationToken, PipelineResult<TOut>> Build() => _delegateHolder.GetPipelineDelegate();
	}
}

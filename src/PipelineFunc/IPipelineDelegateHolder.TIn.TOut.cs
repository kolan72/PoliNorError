using System;
using System.Threading;

namespace PoliNorError
{
	internal interface IPipelineDelegateHolder<TIn, TOut>
	{
		Func<TIn, CancellationToken, PipelineResult<TOut>> GetPipelineDelegate();
		void SetConfigure(Action<BulkErrorProcessor> configureProcessors);
	}
}
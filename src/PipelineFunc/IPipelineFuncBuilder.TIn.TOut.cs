using System;
using System.Threading;

namespace PoliNorError
{
	public interface IPipelineFuncBuilder<TIn, TOut>
	{
		IPipelineFuncStepBuilder<TIn, TOut, TNext> AddFunc<TNext>(Func<TOut, TNext> fNext);
		Func<TIn, CancellationToken, PipelineResult<TOut>> Build();
	}
}

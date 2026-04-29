using System;
using System.Threading.Tasks;

namespace PoliNorError
{
	public interface IPipelineWithHandlersBuilder<TIn, TMid, TOut> : IPipelineFuncBuilder<TIn, TOut>
	{
		IPipelineFuncBuilder<TIn, TOut> OnError(Action<Exception, ProcessingErrorInfo<TMid>> actionProcessor);
		IPipelineFuncBuilder<TIn, TOut> OnError(Func<Exception, ProcessingErrorInfo<TMid>, Task> actionProcessor);
	}
}

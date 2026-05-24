using System;

namespace PoliNorError
{
	/// <summary>
	/// Builder interface for a pipeline step with error handling capabilities.
	/// </summary>
	/// <typeparam name="TIn">The input type for the pipeline.</typeparam>
	/// <typeparam name="TMid">The intermediate type at this step.</typeparam>
	/// <typeparam name="TOut">The output type for the pipeline.</typeparam>
	public interface IPipelineFuncStepBuilder<TIn, TMid, TOut> : IPipelineFuncBuilder<TIn, TOut>
	{
		IPipelineFuncBuilder<TIn, TOut> ConfigureErrorProcessors(Action<ContextErrorProcessors<TMid>> configure);
	}
}

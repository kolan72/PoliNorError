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
		/// <summary>
		/// Configures error processors for the current pipeline step.
		/// </summary>
		/// <param name="configure">
		/// An action that configures the collection of error processors for the current pipeline step.
		/// </param>
		/// <returns>
		/// An <see cref="IPipelineFuncBuilder{TIn, TOut}"/> for further pipeline configuration.
		/// </returns>
		IPipelineFuncBuilder<TIn, TOut> ConfigureErrorProcessors(Action<PipelineErrorProcessors<TMid>> configure);
	}
}

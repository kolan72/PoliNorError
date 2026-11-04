using System;
using System.Threading;

namespace PoliNorError
{
	/// <summary>
	/// Internal interface for holding and configuring pipeline delegates.
	/// </summary>
	/// <typeparam name="TIn">The input type for the pipeline.</typeparam>
	/// <typeparam name="TOut">The output type for the pipeline.</typeparam>
	internal interface IPipelineFuncStep<TIn, TOut>
	{
		/// <summary>
		/// Gets the pipeline delegate function.
		/// </summary>
		/// <returns>A function that processes input and returns a pipeline result.</returns>
		Func<TIn, CancellationToken, PipelineResult<TOut>> GetPipelineDelegate();

		/// <summary>
		/// Gets the pipeline delegate function, allowing a pipeline name to disambiguate between
		/// multiple pipelines with the same input/output type pair.
		/// </summary>
		/// <param name="pipelineName">The optional name of the pipeline.</param>
		/// <returns>A function that processes input and returns a pipeline result.</returns>
		Func<TIn, CancellationToken, PipelineResult<TOut>> GetPipelineDelegate(string pipelineName);

		/// <summary>
		/// Sets the configuration action for error processors.
		/// </summary>
		/// <param name="configureProcessors">The action to configure bulk error processors.</param>
		void SetConfigure(Action<BulkErrorProcessor> configureProcessors);

		/// <summary>
		/// Gets the name of this pipeline, derived from the underlying policy's <see cref="IPolicyBase.PolicyName"/>.
		/// </summary>
		string Name { get; }
	}
}

using System;
using System.Threading;

namespace PoliNorError
{
	/// <summary>
	/// Internal interface for holding and configuring pipeline delegates.
	/// </summary>
	/// <typeparam name="TIn">The input type for the pipeline.</typeparam>
	/// <typeparam name="TOut">The output type for the pipeline.</typeparam>
	internal interface IPipelineDelegateHolder<TIn, TOut>
	{
		/// <summary>
		/// Gets the pipeline delegate function.
		/// </summary>
		/// <returns>A function that processes input and returns a pipeline result.</returns>
		Func<TIn, CancellationToken, PipelineResult<TOut>> GetPipelineDelegate();

		/// <summary>
		/// Sets the configuration action for error processors.
		/// </summary>
		/// <param name="configureProcessors">The action to configure bulk error processors.</param>
		void SetConfigure(Action<BulkErrorProcessor> configureProcessors);
	}
}
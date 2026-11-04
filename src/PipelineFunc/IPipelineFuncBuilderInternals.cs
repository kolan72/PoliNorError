using System;

namespace PoliNorError
{
	/// <summary>
	/// Internal, non-generic contract implemented by <see cref="PipelineFuncBuilder{TIn, TIm, TOut}"/>.
	/// It lets extension code reach the underlying step (its name and configuration hook)
	/// through an <see cref="IPipelineFuncBuilder{TIn, TOut}"/> reference without knowing
	/// the builder's intermediate type parameter and without reflection.
	/// </summary>
	internal interface IPipelineFuncBuilderInternals
	{
		/// <summary>
		/// Gets the pipeline name reported by the underlying step
		/// (the leaf policy's <see cref="IPolicyBase.PolicyName"/>).
		/// </summary>
		string GetPipelineName();

		/// <summary>
		/// Applies an error-processor configuration to the underlying step.
		/// </summary>
		/// <param name="configureProcessors">The action to configure bulk error processors.</param>
		void SetConfigure(Action<BulkErrorProcessor> configureProcessors);
	}
}
using System;
using System.Threading;

namespace PoliNorError
{
	/// <summary>
	/// Builder interface for constructing pipeline functions.
	/// </summary>
	/// <typeparam name="TIn">The input type for the pipeline.</typeparam>
	/// <typeparam name="TOut">The output type for the pipeline.</typeparam>
	public interface IPipelineFuncBuilder<TIn, TOut>
	{
		IPipelineFuncStepBuilder<TIn, TOut, TNext> AddFunc<TNext>(Func<TOut, TNext> fNext, IPolicyBase policy);

		/// <summary>
		/// Builds the pipeline function.
		/// </summary>
		/// <returns>A function that executes the complete pipeline.</returns>
		Func<TIn, CancellationToken, PipelineResult<TOut>> Build();
	}
}

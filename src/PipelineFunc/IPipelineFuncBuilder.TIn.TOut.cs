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
		/// <summary>
		/// Adds a function to the pipeline that transforms the output.
		/// </summary>
		/// <typeparam name="TNext">The type of the next step's output.</typeparam>
		/// <param name="fNext">The function to add to the pipeline.</param>
		/// <returns>A step builder for the next pipeline stage.</returns>
		IPipelineFuncStepBuilder<TIn, TOut, TNext> AddFunc<TNext>(Func<TOut, TNext> fNext);

		/// <summary>
		/// Builds the pipeline function.
		/// </summary>
		/// <returns>A function that executes the complete pipeline.</returns>
		Func<TIn, CancellationToken, PipelineResult<TOut>> Build();
	}
}

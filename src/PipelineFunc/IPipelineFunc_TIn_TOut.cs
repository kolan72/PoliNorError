using System;
using System.Threading;

namespace PoliNorError
{
	/// <summary>
	/// Represents an immutable pipeline function that transforms an input of type <typeparamref name="TIn"/>
	/// into an output of type <typeparamref name="TOut"/> wrapped in a <see cref="PipelineResult{TOut}"/>.
	/// </summary>
	/// <typeparam name="TIn">The type of the pipeline's input.</typeparam>
	/// <typeparam name="TOut">The type of the pipeline's output.</typeparam>
	public interface IPipelineFunc<TIn, TOut> : IPipelineFunc
	{
		/// <summary>
		/// Gets the pipeline delegate function.
		/// </summary>
		/// <returns>A function that processes input and returns a pipeline result.</returns>
		Func<TIn, CancellationToken, PipelineResult<TOut>> GetPipelineDelegate();
	}
}

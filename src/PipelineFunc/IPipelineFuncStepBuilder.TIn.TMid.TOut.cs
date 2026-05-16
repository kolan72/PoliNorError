using System;
using System.Threading.Tasks;

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
		/// Configures synchronous error handling for this pipeline step.
		/// </summary>
		/// <param name="actionProcessor">The action to execute when an error occurs.</param>
		/// <returns>The pipeline builder for further configuration.</returns>
		IPipelineFuncBuilder<TIn, TOut> OnError(Action<Exception, ProcessingErrorInfo<TMid>> actionProcessor);

		/// <summary>
		/// Configures asynchronous error handling for this pipeline step.
		/// </summary>
		/// <param name="actionProcessor">The async function to execute when an error occurs.</param>
		/// <returns>The pipeline builder for further configuration.</returns>
		IPipelineFuncBuilder<TIn, TOut> OnError(Func<Exception, ProcessingErrorInfo<TMid>, Task> actionProcessor);
	}
}

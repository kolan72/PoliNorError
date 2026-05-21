using System;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static class PipelineFuncStepBuilderExtensions
	{
		/// <summary>
		/// Configures synchronous error handling for this pipeline step.
		/// </summary>
		/// <param name="stepBuilder">Current step builder</param>
		/// <param name="actionProcessor">The action to execute when an error occurs.</param>
		public static IPipelineFuncBuilder<TIn, TOut> OnError<TIn, TIm, TOut>(this IPipelineFuncStepBuilder<TIn, TIm, TOut> stepBuilder, Action<Exception, ProcessingErrorInfo<TIm>> actionProcessor)
		{
			void action(ContextErrorProcessors<TIm> bep) => bep.Add(actionProcessor);
			return stepBuilder.OnError(action);
		}

		/// <summary>
		/// Configures asynchronous error handling for this pipeline step.
		/// </summary>
		/// <param name="stepBuilder">Current step builder</param>
		/// <param name="actionProcessor">The async function to execute when an error occurs.</param>
		public static IPipelineFuncBuilder<TIn, TOut> OnError<TIn, TIm, TOut>(this IPipelineFuncStepBuilder<TIn, TIm, TOut> stepBuilder, Func<Exception, ProcessingErrorInfo<TIm>, Task> actionProcessor)
		{
			void action(ContextErrorProcessors<TIm> bep) => bep.Add(actionProcessor);
			return stepBuilder.OnError(action);
		}
	}
}

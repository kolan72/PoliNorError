using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static class PipelineFuncStepBuilderExtensions
	{
		/// <summary>
		/// Adds synchronous error processor for this pipeline step.
		/// </summary>
		/// <param name="stepBuilder">Current step builder</param>
		/// <param name="actionProcessor">The action to execute when an error occurs.</param>
		public static IPipelineFuncBuilder<TIn, TOut> OnError<TIn, TIm, TOut>(this IPipelineFuncStepBuilder<TIn, TIm, TOut> stepBuilder, Action<Exception, ProcessingErrorInfo<TIm>> actionProcessor)
		{
			void action(PipelineErrorProcessors<TIm> bep) => bep.Add(actionProcessor);
			return stepBuilder.ConfigureErrorProcessors(action);
		}

		/// <summary>
		/// Adds a synchronous error processor with cancellation support for the current pipeline step.
		/// </summary>
		/// <param name="stepBuilder">Current step builder</param>
		/// <param name="actionProcessor">The action to execute when an error occurs.</param>
		public static IPipelineFuncBuilder<TIn, TOut> OnError<TIn, TIm, TOut>(this IPipelineFuncStepBuilder<TIn, TIm, TOut> stepBuilder, Action<Exception, ProcessingErrorInfo<TIm>, CancellationToken> actionProcessor)
		{
			void action(PipelineErrorProcessors<TIm> bep) => bep.Add(actionProcessor);
			return stepBuilder.ConfigureErrorProcessors(action);
		}

		/// <summary>
		/// Adds asynchronous error processor for this pipeline step.
		/// </summary>
		/// <param name="stepBuilder">Current step builder</param>
		/// <param name="actionProcessor">The async function to execute when an error occurs.</param>
		public static IPipelineFuncBuilder<TIn, TOut> OnError<TIn, TIm, TOut>(this IPipelineFuncStepBuilder<TIn, TIm, TOut> stepBuilder, Func<Exception, ProcessingErrorInfo<TIm>, Task> actionProcessor)
		{
			void action(PipelineErrorProcessors<TIm> bep) => bep.Add(actionProcessor);
			return stepBuilder.ConfigureErrorProcessors(action);
		}
	}
}

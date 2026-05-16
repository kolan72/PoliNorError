using System;

namespace PoliNorError
{
	/// <summary>
	/// Static factory class for creating pipeline function builders.
	/// </summary>
	public static class PipelineFuncBuilder
	{
		/// <summary>
		/// Starts a new pipeline with the specified function.
		/// </summary>
		/// <typeparam name="TIn">The input type for the pipeline.</typeparam>
		/// <typeparam name="TOut">The output type for the first step.</typeparam>
		/// <param name="func">The initial function for the pipeline.</param>
		/// <returns>A pipeline builder for constructing the pipeline.</returns>
		public static PipelineFuncBuilder<TIn, TIn, TOut> StartWith<TIn, TOut>(Func<TIn, TOut> func)
		{
			var delegateHolder = new PipelineDelegateHolder<TIn, TOut>(func);
			return new PipelineFuncBuilder<TIn, TIn, TOut>(delegateHolder);
		}
	}
}

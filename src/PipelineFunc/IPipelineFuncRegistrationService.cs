namespace PoliNorError
{
	/// <summary>
	/// Internal service contract for registering <see cref="PipelineFunc{TIn,TOut}"/> instances.
	/// Exposed publicly only so <see cref="PipelineFuncRegistry"/> can implement it
	/// and be swapped in a host dependency-injection container in the future.
	/// </summary>
	internal interface IPipelineFuncRegistrationService
	{
		/// <summary>
		/// Registers the specified <see cref="PipelineFunc{TIn,TOut}"/>.
		/// </summary>
		/// <typeparam name="TIn">The input type of the pipeline.</typeparam>
		/// <typeparam name="TOut">The output type of the pipeline.</typeparam>
		/// <param name="pipelineFunc">The pipeline to register.</param>
		/// <exception cref="System.ArgumentNullException"><paramref name="pipelineFunc"/> is null.</exception>
		void Register<TIn, TOut>(PipelineFunc<TIn, TOut> pipelineFunc);
	}
}

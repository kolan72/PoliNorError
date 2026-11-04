using System;

namespace PoliNorError
{
	/// <summary>
	/// Internal side-channel registered per pipeline by <see cref="PipelineFuncServiceCollectionExtensions"/>:
	/// when resolution of a pipeline function is requested through
	/// <see cref="PipelineFuncServiceCollectionExtensions.GetPipelineFunc{TIn,TOut}(System.IServiceProvider)"/>,
	/// every registered resolution is invoked so that the <c>IPipelineFunc{TIn,TOut}</c> singletons
	/// are created (and therefore registered in the <see cref="PipelineFuncRegistry"/>).
	/// Each descriptor materializes at most once, no matter how many times it is invoked.
	/// </summary>
	internal sealed class PipelineFuncResolution
	{
		private readonly object _sync = new object();
		private readonly Action<IServiceProvider> _materialize;
		private bool _materialized;

		internal PipelineFuncResolution(Action<IServiceProvider> materialize)
		{
			_materialize = materialize ?? throw new ArgumentNullException(nameof(materialize));
		}

		/// <summary>
		/// Runs the materialization action once; subsequent calls are no-ops.
		/// </summary>
		/// <param name="services">The provider to materialize against.</param>
		internal void Materialize(IServiceProvider services)
		{
			if (_materialized)
			{
				return;
			}

			lock (_sync)
			{
				if (_materialized)
				{
					return;
				}

				_materialize(services);
				_materialized = true;
			}
		}
	}
}
using System;
using System.Threading;

namespace PoliNorError
{
	/// <summary>
	/// Immutable representation of a pipeline function with a descriptive name.
	/// </summary>
	/// <typeparam name="TIn">The type of the pipeline's input.</typeparam>
	/// <typeparam name="TOut">The type of the pipeline's output.</typeparam>
	public sealed class PipelineFunc<TIn, TOut> : IPipelineFunc<TIn, TOut>
	{
		private readonly Func<TIn, CancellationToken, PipelineResult<TOut>> _delegate;

		/// <summary>
		/// Creates a <see cref="PipelineFunc{TIn, TOut}"/> from an already-materialized pipeline delegate
		/// together with the pipeline's name. Intended to be called from within this library only.
		/// </summary>
		/// <param name="pipelineDelegate">The pipeline delegate obtained from a built <see cref="PipelineFuncStep{TIn,TOut}"/>.</param>
		/// <param name="name">The pipeline name, derived from the underlying policy's <see cref="IPolicyBase.PolicyName"/>.</param>
		internal PipelineFunc(Func<TIn, CancellationToken, PipelineResult<TOut>> pipelineDelegate, string name)
		{
			_delegate = pipelineDelegate ?? throw new ArgumentNullException(nameof(pipelineDelegate));
			Name = name;
		}

		/// <inheritdoc/>
		public Func<TIn, CancellationToken, PipelineResult<TOut>> GetPipelineDelegate() => _delegate;

		/// <inheritdoc/>
		public string Name { get; }
	}
}

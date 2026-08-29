using System;
using System.Threading;

namespace PoliNorError
{
	/// <summary>
	/// Holds and manages a pipeline delegate for a single function.
	/// </summary>
	/// <typeparam name="TIn">The input type.</typeparam>
	/// <typeparam name="TOut">The output type.</typeparam>
	internal class PipelineDelegateHolder<TIn, TOut> : IPipelineDelegateHolder<TIn, TOut>
	{
		private readonly Func<TIn, TOut> _func;
		private readonly IPolicyBase _policy;

		/// <summary>
		/// Initializes a new instance of the <see cref="PipelineDelegateHolder{TIn, TOut}"/> class.
		/// </summary>
		/// <param name="func">The function to wrap in the pipeline.</param>
		public PipelineDelegateHolder(Func<TIn, TOut> func) : this(func, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PipelineDelegateHolder{TIn, TOut}"/> class with a specific policy.
		/// </summary>
		/// <param name="func">The function to wrap in the pipeline.</param>
		/// <param name="policy">The policy to use for error handling. If null, a SimplePolicy will be created.</param>
		public PipelineDelegateHolder(Func<TIn, TOut> func, IPolicyBase policy)
		{
			_func = func;
			_policy = policy ?? new SimplePolicy();
		}

		/// <summary>
		/// Sets the configuration action for error processors.
		/// </summary>
		/// <param name="configureProcessors">The action to configure bulk error processors.</param>
		public void SetConfigure(Action<BulkErrorProcessor> configureProcessors)
		{
			var bp = new BulkErrorProcessor();
			configureProcessors?.Invoke(bp);

			// Apply custom error processors to the provided policy
			foreach (var processor in bp)
			{
				_policy.PolicyProcessor.WithErrorProcessor(processor);
			}
		}

		/// <summary>
		/// Gets the pipeline delegate function.
		/// </summary>
		/// <returns>A function that processes input and returns a pipeline result.</returns>
		public Func<TIn, CancellationToken, PipelineResult<TOut>> GetPipelineDelegate()
		{
			return _func.ToPipelineFunc(_policy);
		}
	}
}

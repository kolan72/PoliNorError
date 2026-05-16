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
		private Action<BulkErrorProcessor> _configureProcessors;

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
			_policy = policy;
		}

		/// <summary>
		/// Sets the configuration action for error processors.
		/// </summary>
		/// <param name="configureProcessors">The action to configure bulk error processors.</param>
		public void SetConfigure(Action<BulkErrorProcessor> configureProcessors)
		{
			_configureProcessors = configureProcessors;
		}

		/// <summary>
		/// Gets the pipeline delegate function.
		/// </summary>
		/// <returns>A function that processes input and returns a pipeline result.</returns>
		public Func<TIn, CancellationToken, PipelineResult<TOut>> GetPipelineDelegate()
		{
			return (t, ct) =>
			{
				var bp = new BulkErrorProcessor();
				_configureProcessors?.Invoke(bp);

				var policy = _policy ?? new SimplePolicy();

				// Apply custom error processors to the provided policy
				foreach (var processor in bp)
				{
					policy.PolicyProcessor.WithErrorProcessor(processor);
				}

				PolicyResult<TOut> res = null;
				switch (policy)
				{
					case RetryPolicy rp:
						res = rp.Handle(_func, t, ct);
						break;
					case FallbackPolicy fp:
						res = fp.Handle(_func, t, ct);
						break;
					case SimplePolicy sp:
						res = sp.Handle(_func, t, ct);
						break;
					default:
						res = new PolicyResult<TOut>();
						res.AddError(new NotImplementedException());
						break;
				}

				if (((policy is SimplePolicy) ? !res.NoError : res.IsFailed) || res.IsCanceled)
				{
					return PipelineResult<TOut>.Failure(res);
				}
				return PipelineResult<TOut>.Success(res);
			};
		}
	}
}

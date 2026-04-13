using System;
using System.Threading;

namespace PoliNorError
{
	internal class PipelineDelegateHolder<TIn, TOut> : IPipelineDelegateHolder<TIn, TOut>
	{
		private Action<BulkErrorProcessor> _configureProcessors;

		private readonly Func<TIn, TOut> _func;

		public PipelineDelegateHolder(Func<TIn, TOut> func)
		{
			_func = func;
		}

		public void SetConfigure(Action<BulkErrorProcessor> configureProcessors)
		{
			_configureProcessors = configureProcessors;
		}

		public Func<TIn, CancellationToken, PipelineResult<TOut>> GetPipelineDelegate()
		{
			return (t, ct) =>
			{
				var bp = new BulkErrorProcessor();
				_configureProcessors?.Invoke(bp);

				var policy = new SimplePolicy(bp);
				var res = policy.Handle(_func, t, ct);

				if (!res.NoError || res.IsCanceled)
				{
					return PipelineResult<TOut>.Failure(res);
				}
				return PipelineResult<TOut>.Success(res);
			};
		}
	}
}

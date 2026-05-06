using System;

namespace PoliNorError
{
	public static class PipelineFuncBuilder
	{
		public static PipelineFuncBuilder<TIn, TIn, TOut> StartWith<TIn, TOut>(Func<TIn, TOut> func)
		{
			var delegateHolder = new PipelineDelegateHolder<TIn, TOut>(func);
			return new PipelineFuncBuilder<TIn, TIn, TOut>(delegateHolder);
		}
	}
}

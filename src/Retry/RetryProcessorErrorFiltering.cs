using System;
using System.Linq.Expressions;

namespace PoliNorError
{
	/// <summary>
	///  Provides extension methods to set error filters to IRetryProcessor.
	/// </summary>
	public static class RetryProcessorErrorFiltering
	{
		public static IRetryProcessor IncludeError<TException>(this IRetryProcessor retryProcessor, Func<TException, bool> func = null) where TException : Exception => retryProcessor.IncludeError<IRetryProcessor, TException>(func);

		public static IRetryProcessor IncludeError(this IRetryProcessor retryProcessor, Expression<Func<Exception, bool>> handledErrorFilter) => retryProcessor.IncludeError<IRetryProcessor>(handledErrorFilter);

		public static IRetryProcessor IncludeErrorSet<TException1, TException2>(this IRetryProcessor retryProcessor) where TException1 : Exception where TException2 : Exception
			=> retryProcessor.IncludeErrorSet<IRetryProcessor, TException1, TException2>();

		public static IRetryProcessor ExcludeError<TException>(this IRetryProcessor retryProcessor, Func<TException, bool> func = null) where TException : Exception => retryProcessor.ExcludeError<IRetryProcessor, TException>(func);

		public static IRetryProcessor ExcludeError(this IRetryProcessor retryProcessor, Expression<Func<Exception, bool>> handledErrorFilter) => retryProcessor.ExcludeError<IRetryProcessor>(handledErrorFilter);
	}
}

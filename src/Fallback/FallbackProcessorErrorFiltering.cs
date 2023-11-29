using System;
using System.Linq.Expressions;

namespace PoliNorError
{
	/// <summary>
	///  Provides extension methods to set error filters to IFallbackProcessor.
	/// </summary>
	public static class FallbackProcessorErrorFiltering
	{
		public static IFallbackProcessor IncludeError<TException>(this IFallbackProcessor fallbackProcessor, Func<TException, bool> func = null) where TException : Exception => fallbackProcessor.IncludeError<IFallbackProcessor, TException>(func);

		public static IFallbackProcessor IncludeError(this IFallbackProcessor fallbackProcessor, Expression<Func<Exception, bool>> handledErrorFilter) => fallbackProcessor.IncludeError<IFallbackProcessor>(handledErrorFilter);

		public static IFallbackProcessor IncludeErrorSet<TException1, TException2>(this IFallbackProcessor fallbackProcessor) where TException1 : Exception where TException2 : Exception
			=> fallbackProcessor.IncludeErrorSet<IFallbackProcessor, TException1, TException2>();

		public static IFallbackProcessor ExcludeError<TException>(this IFallbackProcessor fallbackProcessor, Func<TException, bool> func = null) where TException : Exception => fallbackProcessor.ExcludeError<IFallbackProcessor, TException>(func);

		public static IFallbackProcessor ExcludeError(this IFallbackProcessor fallbackProcessor, Expression<Func<Exception, bool>> handledErrorFilter) => fallbackProcessor.ExcludeError<IFallbackProcessor>(handledErrorFilter);

		public static IFallbackProcessor ExcludeErrorSet<TException1, TException2>(this IFallbackProcessor fallbackProcessor) where TException1 : Exception where TException2 : Exception
			=> fallbackProcessor.ExcludeErrorSet<IFallbackProcessor, TException1, TException2>();
	}
}

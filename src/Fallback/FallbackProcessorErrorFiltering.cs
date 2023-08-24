using System;
using System.Linq.Expressions;

namespace PoliNorError
{
	/// <summary>
	///  Provides static methods to set error filters to IFallbackProcessor.
	/// </summary>
	public static class FallbackProcessorErrorFiltering
	{
		public static IFallbackProcessor IncludeError<TException>(this IFallbackProcessor fallbackProcessor, Func<TException, bool> func = null) where TException : Exception => fallbackProcessor.IncludeError<IFallbackProcessor, TException>(func);

		public static IFallbackProcessor IncludeError(this IFallbackProcessor fallbackProcessor, Expression<Func<Exception, bool>> handledErrorFilter) => fallbackProcessor.IncludeError<IFallbackProcessor>(handledErrorFilter);

		public static IFallbackProcessor ExcludeError<TException>(this IFallbackProcessor fallbackProcessor, Func<TException, bool> func = null) where TException : Exception => fallbackProcessor.ExcludeError<IFallbackProcessor, TException>(func);

		public static IFallbackProcessor ExcludeError(this IFallbackProcessor fallbackProcessor, Expression<Func<Exception, bool>> handledErrorFilter) => fallbackProcessor.ExcludeError<IFallbackProcessor>(handledErrorFilter);
	}
}

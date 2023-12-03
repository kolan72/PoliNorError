using System;
using System.Linq.Expressions;

namespace PoliNorError
{
	/// <summary>
	///  Provides extension methods to set error filters to IFallbackProcessor.
	/// </summary>
	public static class FallbackProcessorErrorFiltering
	{
		/// <summary>
		/// Adds condition for exception to have the <typeparamref name="TException"/> type and satisfy the <paramref name="func"/> predicate to the  <paramref name="fallbackProcessor"/> filter.
		/// </summary>
		/// <typeparam name="TException">A type of exception.</typeparam>
		/// <param name="fallbackProcessor">A processor for Fallback policy.</param>
		/// <param name="func">A predicate that an exception should satisfy.</param>
		/// <returns></returns>
		public static IFallbackProcessor IncludeError<TException>(this IFallbackProcessor fallbackProcessor, Func<TException, bool> func = null) where TException : Exception => fallbackProcessor.IncludeError<IFallbackProcessor, TException>(func);

		/// <summary>
		/// Adds condition for exception to satisfy the <paramref name="predicate"/> predicate to the  <paramref name="fallbackProcessor"/> filter.
		/// </summary>
		/// <param name="fallbackProcessor">A processor for Fallback policy.</param>
		/// <param name="predicate">A predicate that an exception should satisfy.</param>
		/// <returns></returns>
		public static IFallbackProcessor IncludeError(this IFallbackProcessor fallbackProcessor, Expression<Func<Exception, bool>> predicate) => fallbackProcessor.IncludeError<IFallbackProcessor>(predicate);

		/// <summary>
		/// Adds a set of two permitted types of an exception that can be processed by the <paramref name="fallbackProcessor"/> processor.
		/// </summary>
		/// <typeparam name="TException1">A type of exception.</typeparam>
		/// <typeparam name="TException2">A type of exception.</typeparam>
		/// <param name="fallbackProcessor">A processor for Fallback policy.</param>
		/// <returns></returns>
		public static IFallbackProcessor IncludeErrorSet<TException1, TException2>(this IFallbackProcessor fallbackProcessor) where TException1 : Exception where TException2 : Exception
			=> fallbackProcessor.IncludeErrorSet<IFallbackProcessor, TException1, TException2>();

		/// <summary>
		/// Adds condition for the exception to not have the <typeparamref name="TException"/> type and not satisfy the <paramref name="func"/> predicate to the  <paramref name="fallbackProcessor"/> filter.
		/// </summary>
		/// <typeparam name="TException">>A type of exception.</typeparam>
		/// <param name="fallbackProcessor">A processor for Fallback policy.</param>
		/// <param name="func">A predicate that an exception should satisfy.</param>
		/// <returns></returns>
		public static IFallbackProcessor ExcludeError<TException>(this IFallbackProcessor fallbackProcessor, Func<TException, bool> func = null) where TException : Exception => fallbackProcessor.ExcludeError<IFallbackProcessor, TException>(func);

		/// <summary>
		/// Adds condition for the exception to not satisfy the <paramref name="predicate"/> predicate to the  <paramref name="fallbackProcessor"/> filter.
		/// </summary>
		/// <param name="fallbackProcessor">A processor for Fallback policy.</param>
		/// <param name="predicate">A predicate that an exception should satisfy.</param>
		/// <returns></returns>
		public static IFallbackProcessor ExcludeError(this IFallbackProcessor fallbackProcessor, Expression<Func<Exception, bool>> predicate) => fallbackProcessor.ExcludeError<IFallbackProcessor>(predicate);

		/// <summary>
		/// Adds a set of two types of an exception that can not be processed by the <paramref name="fallbackProcessor"/> processor.
		/// </summary>
		/// <typeparam name="TException1">>A type of exception.</typeparam>
		/// <typeparam name="TException2">>A type of exception.</typeparam>
		/// <param name="fallbackProcessor">A processor for Fallback policy.</param>
		/// <returns></returns>
		public static IFallbackProcessor ExcludeErrorSet<TException1, TException2>(this IFallbackProcessor fallbackProcessor) where TException1 : Exception where TException2 : Exception
			=> fallbackProcessor.ExcludeErrorSet<IFallbackProcessor, TException1, TException2>();
	}
}

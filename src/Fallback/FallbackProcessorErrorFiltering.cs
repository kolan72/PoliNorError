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
		/// Specifies <typeparamref name="TException"/> type- and optionally <paramref name="func"/> predicate-based filter condition for including exception to the processing by the <paramref name="fallbackProcessor"/> processor.
		/// </summary>
		/// <typeparam name="TException">A type of exception.</typeparam>
		/// <param name="fallbackProcessor">A processor for Fallback policy.</param>
		/// <param name="func">A predicate that an exception should satisfy.</param>
		/// <returns></returns>
		public static IFallbackProcessor IncludeError<TException>(this IFallbackProcessor fallbackProcessor, Func<TException, bool> func = null) where TException : Exception => fallbackProcessor.IncludeError<IFallbackProcessor, TException>(func);

		/// <summary>
		/// Specifies <paramref name="predicate"/> predicate-based filter condition for including exception to the processing by the <paramref name="fallbackProcessor"/> processor.
		/// </summary>
		/// <param name="fallbackProcessor">A processor for Fallback policy.</param>
		/// <param name="predicate">A predicate that an exception should satisfy.</param>
		/// <returns></returns>
		public static IFallbackProcessor IncludeError(this IFallbackProcessor fallbackProcessor, Expression<Func<Exception, bool>> predicate) => fallbackProcessor.IncludeError<IFallbackProcessor>(predicate);

		/// <summary>
		/// Specifies two types-based filter condition for including an exception in the processing performed by the <paramref name="fallbackProcessor"/> processor.
		/// </summary>
		/// <typeparam name="TException1">A type of exception.</typeparam>
		/// <typeparam name="TException2">A type of exception.</typeparam>
		/// <param name="fallbackProcessor">A processor for Fallback policy.</param>
		/// <returns></returns>
		public static IFallbackProcessor IncludeErrorSet<TException1, TException2>(this IFallbackProcessor fallbackProcessor) where TException1 : Exception where TException2 : Exception
			=> fallbackProcessor.IncludeErrorSet<IFallbackProcessor, TException1, TException2>();

		/// <summary>
		/// Specifies the <see cref="IErrorSet"/> interface-based filter conditions for including a set of exceptions in the processing performed by the <paramref name="fallbackProcessor"/> processor.
		/// </summary>
		/// <param name="fallbackProcessor">A processor for Fallback policy.</param>
		/// <param name="errorSet"><see cref="IErrorSet"/></param>
		/// <returns>A processor for Fallback policy.</returns>
		public static IFallbackProcessor IncludeErrorSet(this IFallbackProcessor fallbackProcessor, IErrorSet errorSet)
			=> fallbackProcessor.IncludeErrorSet<IFallbackProcessor>(errorSet);

		/// <summary>
		/// Specifies the type- and optionally <paramref name="predicate"/> predicate-based filter condition for the inner exception of a handling exception to be included in the processing by the <paramref name="fallbackProcessor"/> processor.
		/// </summary>
		/// <typeparam name="TInnerException">A type of an inner exception.</typeparam>
		/// <param name="fallbackProcessor">A processor for Fallback policy.</param>
		/// <param name="predicate">A predicate that an inner exception should satisfy.</param>
		/// <returns></returns>
		public static IFallbackProcessor IncludeInnerError<TInnerException>(this IFallbackProcessor fallbackProcessor, Func<TInnerException, bool> predicate = null) where TInnerException : Exception
			=> fallbackProcessor.IncludeInnerError<IFallbackProcessor, TInnerException>(predicate);

		/// <summary>
		/// Specifies <typeparamref name="TException"/> type- and optionally <paramref name="func"/> predicate-based filter condition for excluding exception from the processing by the <paramref name="fallbackProcessor"/> processor.
		/// </summary>
		/// <typeparam name="TException">>A type of exception.</typeparam>
		/// <param name="fallbackProcessor">A processor for Fallback policy.</param>
		/// <param name="func">A predicate that an exception should satisfy.</param>
		/// <returns></returns>
		public static IFallbackProcessor ExcludeError<TException>(this IFallbackProcessor fallbackProcessor, Func<TException, bool> func = null) where TException : Exception => fallbackProcessor.ExcludeError<IFallbackProcessor, TException>(func);

		/// <summary>
		/// Specifies  <paramref name="predicate"/> predicate-based filter condition for excluding exception from the processing by the <paramref name="fallbackProcessor"/> processor.
		/// </summary>
		/// <param name="fallbackProcessor">A processor for Fallback policy.</param>
		/// <param name="predicate">A predicate that an exception should satisfy.</param>
		/// <returns></returns>
		public static IFallbackProcessor ExcludeError(this IFallbackProcessor fallbackProcessor, Expression<Func<Exception, bool>> predicate) => fallbackProcessor.ExcludeError<IFallbackProcessor>(predicate);

		/// <summary>
		/// Specifies two types-based filter condition for excluding an exception from the processing performed by the <paramref name="fallbackProcessor"/> processor.
		/// </summary>
		/// <typeparam name="TException1">>A type of exception.</typeparam>
		/// <typeparam name="TException2">>A type of exception.</typeparam>
		/// <param name="fallbackProcessor">A processor for Fallback policy.</param>
		/// <returns></returns>
		public static IFallbackProcessor ExcludeErrorSet<TException1, TException2>(this IFallbackProcessor fallbackProcessor) where TException1 : Exception where TException2 : Exception
			=> fallbackProcessor.ExcludeErrorSet<IFallbackProcessor, TException1, TException2>();

		/// <summary>
		/// Specifies the <see cref="IErrorSet"/> interface-based filter conditions for excluding a set of exceptions in the processing performed by the <paramref name="fallbackProcessor"/> processor.
		/// </summary>
		/// <param name="fallbackProcessor">A processor for Fallback policy.</param>
		/// <param name="errorSet"><see cref="IErrorSet"/></param>
		/// <returns>A processor for Fallback policy.</returns>
		public static IFallbackProcessor ExcludeErrorSet(this IFallbackProcessor fallbackProcessor, IErrorSet errorSet)
			=> fallbackProcessor.ExcludeErrorSet<IFallbackProcessor>(errorSet);

		/// <summary>
		/// Specifies the type- and optionally <paramref name="predicate"/> predicate-based filter condition for the inner exception of a handling exception to be excluded from the processing by the <paramref name="fallbackProcessor"/> processor.
		/// </summary>
		/// <typeparam name="TInnerException">A type of an inner exception.</typeparam>
		/// <param name="fallbackProcessor">A processor for Fallback policy.</param>
		/// <param name="predicate">A predicate that an inner exception should satisfy.</param>
		/// <returns></returns>
		public static IFallbackProcessor ExcludeInnerError<TInnerException>(this IFallbackProcessor fallbackProcessor, Func<TInnerException, bool> predicate = null) where TInnerException : Exception
			=> fallbackProcessor.ExcludeInnerError<IFallbackProcessor, TInnerException>(predicate);
	}
}

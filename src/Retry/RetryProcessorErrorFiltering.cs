using System;
using System.Linq.Expressions;

namespace PoliNorError
{
	/// <summary>
	///  Provides extension methods to set error filters to IRetryProcessor.
	/// </summary>
	public static class RetryProcessorErrorFiltering
	{
		/// <summary>
		/// Specifies <typeparamref name="TException"/> type- and optionally <paramref name="func"/> predicate-based filter condition for including exception to the processing by the <paramref name="retryProcessor"/> processor.
		/// </summary>
		/// <typeparam name="TException">A type of exception.</typeparam>
		/// <param name="retryProcessor">A processor for Retry policy.</param>
		/// <param name="func">A predicate that an exception should satisfy.</param>
		/// <returns></returns>
		public static IRetryProcessor IncludeError<TException>(this IRetryProcessor retryProcessor, Func<TException, bool> func = null) where TException : Exception => retryProcessor.IncludeError<IRetryProcessor, TException>(func);

		/// <summary>
		/// Specifies <paramref name="predicate"/> predicate-based filter condition for including exception to the processing by the <paramref name="retryProcessor"/> processor.
		/// </summary>
		/// <param name="retryProcessor">A processor for Retry policy.</param>
		/// <param name="predicate">A predicate that an exception should satisfy.</param>
		/// <returns></returns>
		public static IRetryProcessor IncludeError(this IRetryProcessor retryProcessor, Expression<Func<Exception, bool>> predicate) => retryProcessor.IncludeError<IRetryProcessor>(predicate);

		/// <summary>
		/// Specifies two types-based filter condition for including an exception in the processing performed by the <paramref name="retryProcessor"/> processor.
		/// </summary>
		/// <typeparam name="TException1">A type of exception.</typeparam>
		/// <typeparam name="TException2">A type of exception.</typeparam>
		/// <param name="retryProcessor">A processor for Retry policy.</param>
		/// <returns></returns>
		public static IRetryProcessor IncludeErrorSet<TException1, TException2>(this IRetryProcessor retryProcessor) where TException1 : Exception where TException2 : Exception
			=> retryProcessor.IncludeErrorSet<IRetryProcessor, TException1, TException2>();

		/// <summary>
		/// Specifies the <see cref="IErrorSet"/> interface-based filter conditions for including a set of exceptions in the processing performed by the <paramref name="retryProcessor"/> processor.
		/// </summary>
		/// <param name="retryProcessor">A processor for Retry policy.</param>
		/// <param name="errorSet"><see cref="IErrorSet"/></param>
		/// <returns>A processor for Retry policy.</returns>
		public static IRetryProcessor IncludeErrorSet(this IRetryProcessor retryProcessor, IErrorSet errorSet)
			=> retryProcessor.IncludeErrorSet<IRetryProcessor>(errorSet);

		/// <summary>
		/// Specifies the type- and optionally <paramref name="predicate"/> predicate-based filter condition for the inner exception of a handling exception to be included in the processing by the <paramref name="retryProcessor"/> processor.
		/// </summary>
		/// <typeparam name="TInnerException">A type of an inner exception.</typeparam>
		/// <param name="retryProcessor">A processor for Retry policy.</param>
		/// <param name="predicate">A predicate that an inner exception should satisfy.</param>
		/// <returns></returns>
		public static IRetryProcessor IncludeInnerError<TInnerException>(this IRetryProcessor retryProcessor, Func<TInnerException, bool> predicate = null) where TInnerException : Exception
			=> retryProcessor.IncludeInnerError<IRetryProcessor, TInnerException>(predicate);

		/// <summary>
		/// Specifies <typeparamref name="TException"/> type- and optionally <paramref name="func"/> predicate-based filter condition for excluding exception from the processing by the <paramref name="retryProcessor"/> processor.
		/// </summary>
		/// <typeparam name="TException">>A type of exception.</typeparam>
		/// <param name="retryProcessor">A processor for Retry policy.</param>
		/// <param name="func">A predicate that an exception should satisfy.</param>
		/// <returns></returns>
		public static IRetryProcessor ExcludeError<TException>(this IRetryProcessor retryProcessor, Func<TException, bool> func = null) where TException : Exception => retryProcessor.ExcludeError<IRetryProcessor, TException>(func);

		/// <summary>
		/// Specifies  <paramref name="predicate"/> predicate-based filter condition for excluding exception from the processing by the <paramref name="retryProcessor"/> processor.
		/// </summary>
		/// <param name="retryProcessor">A processor for Retry policy.</param>
		/// <param name="predicate">A predicate that an exception should satisfy.</param>
		/// <returns></returns>
		public static IRetryProcessor ExcludeError(this IRetryProcessor retryProcessor, Expression<Func<Exception, bool>> predicate) => retryProcessor.ExcludeError<IRetryProcessor>(predicate);

		/// <summary>
		/// Specifies two types-based filter condition for excluding an exception from the processing performed by the <paramref name="retryProcessor"/> processor.
		/// </summary>
		/// <typeparam name="TException1">>A type of exception.</typeparam>
		/// <typeparam name="TException2">>A type of exception.</typeparam>
		/// <param name="retryProcessor">A processor for Retry policy.</param>
		/// <returns></returns>
		public static IRetryProcessor ExcludeErrorSet<TException1, TException2>(this IRetryProcessor retryProcessor) where TException1 : Exception where TException2 : Exception
			=> retryProcessor.ExcludeErrorSet<IRetryProcessor, TException1, TException2>();

		/// <summary>
		/// Specifies the <see cref="IErrorSet"/> interface-based filter conditions for excluding a set of exceptions in the processing performed by the <paramref name="retryProcessor"/> processor.
		/// </summary>
		/// <param name="retryProcessor">A processor for Retry policy.</param>
		/// <param name="errorSet"><see cref="IErrorSet"/></param>
		/// <returns>A processor for Retry policy.</returns>
		public static IRetryProcessor ExcludeErrorSet(this IRetryProcessor retryProcessor, IErrorSet errorSet)
			=> retryProcessor.ExcludeErrorSet<IRetryProcessor>(errorSet);

		/// <summary>
		/// Specifies the type- and optionally <paramref name="predicate"/> predicate-based filter condition for the inner exception of a handling exception to be excluded from the processing by the <paramref name="retryProcessor"/> processor.
		/// </summary>
		/// <typeparam name="TInnerException">A type of an inner exception.</typeparam>
		/// <param name="retryProcessor">A processor for Retry policy.</param>
		/// <param name="predicate">A predicate that an inner exception should satisfy.</param>
		/// <returns></returns>
		public static IRetryProcessor ExcludeInnerError<TInnerException>(this IRetryProcessor retryProcessor, Func<TInnerException, bool> predicate = null) where TInnerException : Exception
			=> retryProcessor.ExcludeInnerError<IRetryProcessor, TInnerException>(predicate);
	}
}

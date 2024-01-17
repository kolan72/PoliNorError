using System;
using System.Linq.Expressions;

namespace PoliNorError
{
	/// <summary>
	/// Provides extension methods to set error filters to ISimplePolicyProcessor.
	/// </summary>
	public static class SimplePolicyProcessorErrorFiltering
	{
		/// <summary>
		/// Specifies <typeparamref name="TException"/> type- and optionally <paramref name="func"/> predicate-based filter condition for including exception to the processing by the <paramref name="simplePolicyProcessor"/> processor.
		/// </summary>
		/// <typeparam name="TException">A type of exception.</typeparam>
		/// <param name="simplePolicyProcessor">A processor for Simple policy.</param>
		/// <param name="func">A predicate that an exception should satisfy.</param>
		/// <returns></returns>
		public static ISimplePolicyProcessor IncludeError<TException>(this ISimplePolicyProcessor simplePolicyProcessor, Func<TException, bool> func = null) where TException : Exception => simplePolicyProcessor.IncludeError<ISimplePolicyProcessor, TException>(func);

		/// <summary>
		/// Specifies <paramref name="predicate"/> predicate-based filter condition for including exception to the processing by the <paramref name="simplePolicyProcessor"/> processor.
		/// </summary>
		/// <param name="simplePolicyProcessor">A processor for Simple policy.</param>
		/// <param name="predicate">A predicate that an exception should satisfy.</param>
		/// <returns></returns>
		public static ISimplePolicyProcessor IncludeError(this ISimplePolicyProcessor simplePolicyProcessor, Expression<Func<Exception, bool>> predicate) => simplePolicyProcessor.IncludeError<ISimplePolicyProcessor>(predicate);

		/// <summary>
		/// Specifies two types-based filter condition for including an exception in the processing performed by the <paramref name="simplePolicyProcessor"/> processor.
		/// </summary>
		/// <typeparam name="TException1">A type of exception.</typeparam>
		/// <typeparam name="TException2">A type of exception.</typeparam>
		/// <param name="simplePolicyProcessor">A processor for Simple policy.</param>
		/// <returns></returns>
		public static ISimplePolicyProcessor IncludeErrorSet<TException1, TException2>(this ISimplePolicyProcessor simplePolicyProcessor) where TException1 : Exception where TException2 : Exception
			=> simplePolicyProcessor.IncludeErrorSet<ISimplePolicyProcessor, TException1, TException2>();


		/// <summary>
		/// Specifies the type- and optionally <paramref name="predicate"/> predicate-based filter condition for the inner exception of a handling exception to be included in the processing by the <paramref name="simplePolicyProcessor"/> processor.
		/// </summary>
		/// <typeparam name="TInnerException">A type of an inner exception.</typeparam>
		/// <param name="simplePolicyProcessor">A processor for Simple policy.</param>
		/// <param name="predicate">A predicate that an inner exception should satisfy.</param>
		/// <returns></returns>
		public static ISimplePolicyProcessor IncludeInnerError<TInnerException>(this ISimplePolicyProcessor simplePolicyProcessor, Func<TInnerException, bool> predicate = null) where TInnerException : Exception 
			=> simplePolicyProcessor.IncludeInnerError<ISimplePolicyProcessor, TInnerException>(predicate);

		/// <summary>
		/// Specifies the type- and optionally <paramref name="predicate"/> predicate-based filter condition for the inner exception of a handling exception to be excluded from the processing by the <paramref name="simplePolicyProcessor"/> processor.
		/// </summary>
		/// <typeparam name="TInnerException">A type of an inner exception.</typeparam>
		/// <param name="simplePolicyProcessor">A processor for Simple policy.</param>
		/// <param name="predicate">A predicate that an inner exception should satisfy.</param>
		/// <returns></returns>
		public static ISimplePolicyProcessor ExcludeInnerError<TInnerException>(this ISimplePolicyProcessor simplePolicyProcessor, Func<TInnerException, bool> predicate = null) where TInnerException : Exception
			=> simplePolicyProcessor.ExcludeInnerError<ISimplePolicyProcessor, TInnerException>(predicate);

		/// <summary>
		/// Specifies <typeparamref name="TException"/> type- and optionally <paramref name="func"/> predicate-based filter condition for excluding exception from the processing by the <paramref name="simplePolicyProcessor"/> processor.
		/// </summary>
		/// <typeparam name="TException">>A type of exception.</typeparam>
		/// <param name="simplePolicyProcessor">A processor for Simple policy.</param>
		/// <param name="func">A predicate that an exception should satisfy.</param>
		/// <returns></returns>
		public static ISimplePolicyProcessor ExcludeError<TException>(this ISimplePolicyProcessor simplePolicyProcessor, Func<TException, bool> func = null) where TException : Exception => simplePolicyProcessor.ExcludeError<ISimplePolicyProcessor, TException>(func);

		/// <summary>
		/// Specifies  <paramref name="predicate"/> predicate-based filter condition for excluding exception from the processing by the <paramref name="simplePolicyProcessor"/> processor.
		/// </summary>
		/// <param name="simplePolicyProcessor">A processor for Simple policy.</param>
		/// <param name="predicate">A predicate that an exception should satisfy.</param>
		/// <returns></returns>
		public static ISimplePolicyProcessor ExcludeError(this ISimplePolicyProcessor simplePolicyProcessor, Expression<Func<Exception, bool>> predicate) => simplePolicyProcessor.ExcludeError<ISimplePolicyProcessor>(predicate);

		/// <summary>
		/// Specifies two types-based filter condition for excluding an exception from the processing performed by the <paramref name="simplePolicyProcessor"/> processor.
		/// </summary>
		/// <typeparam name="TException1">>A type of exception.</typeparam>
		/// <typeparam name="TException2">>A type of exception.</typeparam>
		/// <param name="simplePolicyProcessor">A processor for Simple policy.</param>
		/// <returns></returns>
		public static ISimplePolicyProcessor ExcludeErrorSet<TException1, TException2>(this ISimplePolicyProcessor simplePolicyProcessor) where TException1 : Exception where TException2 : Exception
			=> simplePolicyProcessor.ExcludeErrorSet<ISimplePolicyProcessor, TException1, TException2>();
	}
}

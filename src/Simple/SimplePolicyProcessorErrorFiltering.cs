using System;
using System.Linq.Expressions;

namespace PoliNorError
{
	/// <summary>
	/// Provides extension methods to set error filters to ISimplePolicyProcessor.
	/// </summary>
	public static class SimplePolicyProcessorErrorFiltering
	{
		public static ISimplePolicyProcessor IncludeError<TException>(this ISimplePolicyProcessor simplePolicyProcessor, Func<TException, bool> func = null) where TException : Exception => simplePolicyProcessor.IncludeError<ISimplePolicyProcessor, TException>(func);

		public static ISimplePolicyProcessor IncludeError(this ISimplePolicyProcessor simplePolicyProcessor, Expression<Func<Exception, bool>> handledErrorFilter) => simplePolicyProcessor.IncludeError<ISimplePolicyProcessor>(handledErrorFilter);

		public static ISimplePolicyProcessor IncludeErrorSet<TException1, TException2>(this ISimplePolicyProcessor simplePolicyProcessor) where TException1 : Exception where TException2 : Exception
			=> simplePolicyProcessor.IncludeErrorSet<ISimplePolicyProcessor, TException1, TException2>();

		public static ISimplePolicyProcessor ExcludeError<TException>(this ISimplePolicyProcessor simplePolicyProcessor, Func<TException, bool> func = null) where TException : Exception => simplePolicyProcessor.ExcludeError<ISimplePolicyProcessor, TException>(func);

		public static ISimplePolicyProcessor ExcludeError(this ISimplePolicyProcessor simplePolicyProcessor, Expression<Func<Exception, bool>> handledErrorFilter) => simplePolicyProcessor.ExcludeError<ISimplePolicyProcessor>(handledErrorFilter);
	}
}

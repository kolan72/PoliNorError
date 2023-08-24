using System;
using System.Linq.Expressions;

namespace PoliNorError
{
	/// <summary>
	/// Provides static methods to set error filters to ISimplePolicyProcessor.
	/// </summary>
	public static class SimplePolicyProcessorErrorFiltering
	{
		public static ISimplePolicyProcessor IncludeError<TException>(this ISimplePolicyProcessor simplePolicyProcessor, Func<TException, bool> func = null) where TException : Exception => simplePolicyProcessor.IncludeError<ISimplePolicyProcessor, TException>(func);

		public static ISimplePolicyProcessor IncludeError(this ISimplePolicyProcessor simplePolicyProcessor, Expression<Func<Exception, bool>> handledErrorFilter) => simplePolicyProcessor.IncludeError<ISimplePolicyProcessor>(handledErrorFilter);

		public static ISimplePolicyProcessor ExcludeError<TException>(this ISimplePolicyProcessor simplePolicyProcessor, Func<TException, bool> func = null) where TException : Exception => simplePolicyProcessor.ExcludeError<ISimplePolicyProcessor, TException>(func);

		public static ISimplePolicyProcessor ExcludeError(this ISimplePolicyProcessor simplePolicyProcessor, Expression<Func<Exception, bool>> handledErrorFilter) => simplePolicyProcessor.ExcludeError<ISimplePolicyProcessor>(handledErrorFilter);
	}
}

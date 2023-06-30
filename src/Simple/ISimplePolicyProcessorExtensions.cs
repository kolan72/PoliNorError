using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static class ISimplePolicyProcessorExtensions
	{
		public static Task<PolicyResult> ExecuteAsync(this ISimplePolicyProcessor simplePolicyProcessor, Func<CancellationToken, Task> func, CancellationToken token)
															=> simplePolicyProcessor.ExecuteAsync(func, false, token);

		public static Task<PolicyResult<T>> ExecuteAsync<T>(this ISimplePolicyProcessor simplePolicyProcessor, Func<CancellationToken, Task<T>> func, CancellationToken token)
													=> simplePolicyProcessor.ExecuteAsync(func, false, token);

		public static ISimplePolicyProcessor IncludeError<TException>(this ISimplePolicyProcessor simplePolicyProcessor, Func<TException, bool> func = null) where TException : Exception => simplePolicyProcessor.IncludeError<ISimplePolicyProcessor, TException>(func);

		public static ISimplePolicyProcessor IncludeError(this ISimplePolicyProcessor simplePolicyProcessor, Expression<Func<Exception, bool>> handledErrorFilter) => simplePolicyProcessor.IncludeError<ISimplePolicyProcessor>(handledErrorFilter);

		public static ISimplePolicyProcessor ExcludeError<TException>(this ISimplePolicyProcessor simplePolicyProcessor, Func<TException, bool> func = null) where TException : Exception => simplePolicyProcessor.ExcludeError<ISimplePolicyProcessor, TException>(func);

		public static ISimplePolicyProcessor ExcludeError(this ISimplePolicyProcessor simplePolicyProcessor, Expression<Func<Exception, bool>> handledErrorFilter) => simplePolicyProcessor.ExcludeError<ISimplePolicyProcessor>(handledErrorFilter);
	}
}

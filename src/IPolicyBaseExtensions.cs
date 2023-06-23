using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static class IPolicyBaseExtensions
	{
		public static Task<PolicyResult> HandleAsync(this IPolicyBase policyBase, Func<CancellationToken, Task> func, CancellationToken token)
		{
			return policyBase.HandleAsync(func, false, token);
		}

		public static Task<PolicyResult<T>> HandleAsync<T>(this IPolicyBase policyBase, Func<CancellationToken, Task<T>> func, CancellationToken token)
		{
			return policyBase.HandleAsync(func, false, token);
		}

		public static PolicyDelegate ToPolicyDelegate(this IPolicyBase errorPolicy, Action action)
		{
			var res = new PolicyDelegate(errorPolicy);
			res.SetDelegate(action);
			return res;
		}

		public static PolicyDelegate ToPolicyDelegate(this IPolicyBase errorPolicy, Func<CancellationToken, Task> func)
		{
			var res = new PolicyDelegate(errorPolicy);
			res.SetDelegate(func);
			return res;
		}

		public static PolicyDelegate ToPolicyDelegate(this IPolicyBase errorPolicy)
		{
			return new PolicyDelegate(errorPolicy);
		}

		public static PolicyDelegate<T> ToPolicyDelegate<T>(this IPolicyBase errorPolicy, Func<T> func)
		{
			var res = new PolicyDelegate<T>(errorPolicy);
			res.SetDelegate(func);
			return res;
		}

		public static PolicyDelegate<T> ToPolicyDelegate<T>(this IPolicyBase errorPolicy, Func<CancellationToken, Task<T>> func)
		{
			var res = new PolicyDelegate<T>(errorPolicy);
			res.SetDelegate(func);
			return res;
		}

		public static PolicyDelegate<T> ToPolicyDelegate<T>(this IPolicyBase errorPolicy)
		{
			return new PolicyDelegate<T>(errorPolicy);
		}

		public static T WithErrorProcessorOf<T>(this T errorPolicyBase, Func<Exception, Task> func, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable) where T : IPolicyBase
		{
			return WithErrorProcessorOf(errorPolicyBase, func.ToCancelableFunc(convertType));
		}

		public static T WithErrorProcessorOf<T>(this T errorPolicyBase, Action<Exception> action, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable) where T : IPolicyBase
		{
			return WithErrorProcessorOf(errorPolicyBase, action.ToCancelableAction(convertType));
		}

		public static T WithErrorProcessorOf<T>(this T errorPolicyBase, Action<Exception, CancellationToken> onBeforeProcessError) where T : IPolicyBase => WithErrorProcessorOf(errorPolicyBase, onBeforeProcessError, null);

		public static T WithErrorProcessorOf<T>(this T errorPolicyBase, Func<Exception, CancellationToken, Task> onBeforeProcessErrorAsync) where T : IPolicyBase => WithErrorProcessorOf(errorPolicyBase, null, onBeforeProcessErrorAsync);

		public static T WithErrorProcessorOf<T>(this T errorPolicyBase, Action<Exception, CancellationToken> onBeforeProcessError, Func<Exception, CancellationToken, Task> onBeforeProcessErrorAsync) where T : IPolicyBase
		{
			return WithErrorProcessor(errorPolicyBase, new DefaultErrorProcessor(onBeforeProcessError, onBeforeProcessErrorAsync));
		}

		public static T WithErrorProcessor<T>(this T errorPolicyBase, IErrorProcessor errorProcessor) where T : IPolicyBase
		{
			if (errorProcessor == null)
				throw new ArgumentNullException(nameof(errorProcessor));

			errorPolicyBase.PolicyProcessor.WithErrorProcessor(errorProcessor);
			return errorPolicyBase;
		}

		public static T ExcludeError<T>(this T errorPolicy, Expression<Func<Exception, bool>> handledErrorFilter) where T : IPolicyBase
		{
			errorPolicy.PolicyProcessor.AddExcludedErrorFilter(handledErrorFilter);
			return errorPolicy;
		}

		internal static T ExcludeError<T, TException>(this T errorPolicy, Func<TException, bool> func = null) where T : IPolicyBase where TException : Exception
		{
			errorPolicy.PolicyProcessor.AddExcludedErrorFilter(func);
			return errorPolicy;
		}

		public static T IncludeError<T>(this T errorPolicy, Expression<Func<Exception, bool>> handledErrorFilter) where T : IPolicyBase
		{
			errorPolicy.PolicyProcessor.AddIncludedErrorFilter(handledErrorFilter);
			return errorPolicy;
		}

		internal static T IncludeError<T, TException>(this T errorPolicy, Func<TException, bool> func = null) where T : IPolicyBase where TException : Exception
		{
			errorPolicy.PolicyProcessor.AddIncludedErrorFilter(func);
			return errorPolicy;
		}
	}
}

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

		internal static PolicyDelegate ToPolicyDelegate(this IPolicyBase errorPolicy)
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

		internal static PolicyDelegate<T> ToPolicyDelegate<T>(this IPolicyBase errorPolicy)
		{
			return new PolicyDelegate<T>(errorPolicy);
		}

		public static T WithErrorProcessorOf<T>(this T errorPolicyBase, Action<Exception> actionProcessor) where T : IPolicyBase
		{
			errorPolicyBase.PolicyProcessor.WithErrorProcessorOf(actionProcessor);
			return errorPolicyBase;
		}

		public static T WithErrorProcessorOf<T>(this T errorPolicyBase, Action<Exception, CancellationToken> actionProcessor) where T : IPolicyBase
		{
			errorPolicyBase.PolicyProcessor.WithErrorProcessorOf(actionProcessor);
			return errorPolicyBase;
		}

		public static T WithErrorProcessorOf<T>(this T errorPolicyBase, Action<Exception> actionProcessor, CancellationType cancellationType) where T : IPolicyBase
		{
			errorPolicyBase.PolicyProcessor.WithErrorProcessorOf(actionProcessor, cancellationType);
			return errorPolicyBase;
		}

		public static T WithErrorProcessorOf<T>(this T errorPolicyBase, Func<Exception, Task> funcProcessor) where T : IPolicyBase
		{
			errorPolicyBase.PolicyProcessor.WithErrorProcessorOf(funcProcessor);
			return errorPolicyBase;
		}

		public static T WithErrorProcessorOf<T>(this T errorPolicyBase, Func<Exception, Task> funcProcessor, CancellationType convertToCancelableFuncType) where T : IPolicyBase
		{
			errorPolicyBase.PolicyProcessor.WithErrorProcessorOf(funcProcessor, convertToCancelableFuncType);
			return errorPolicyBase;
		}

		public static T WithErrorProcessorOf<T>(this T errorPolicyBase, Func<Exception, CancellationToken, Task> funcProcessor) where T : IPolicyBase
		{
			errorPolicyBase.PolicyProcessor.WithErrorProcessorOf(funcProcessor);
			return errorPolicyBase;
		}

		public static T WithErrorProcessorOf<T>(this T errorPolicyBase, Func<Exception, CancellationToken, Task> funcProcessor, Action<Exception> actionProcessor) where T : IPolicyBase
		{
			errorPolicyBase.PolicyProcessor.WithErrorProcessorOf(funcProcessor, actionProcessor);
			return errorPolicyBase;
		}

		public static T WithErrorProcessorOf<T>(this T errorPolicyBase, Func<Exception, CancellationToken, Task> funcProcessor, Action<Exception> actionProcessor, CancellationType convertToCancelableFuncType) where T : IPolicyBase
		{
			errorPolicyBase.PolicyProcessor.WithErrorProcessorOf(funcProcessor, actionProcessor, convertToCancelableFuncType);
			return errorPolicyBase;
		}

		public static T WithErrorProcessorOf<T>(this T errorPolicyBase, Func<Exception, Task> funcProcessor, Action<Exception> actionProcessor) where T : IPolicyBase
		{
			errorPolicyBase.PolicyProcessor.WithErrorProcessorOf(funcProcessor, actionProcessor);
			return errorPolicyBase;
		}

		public static T WithErrorProcessorOf<T>(this T errorPolicyBase, Func<Exception, Task> funcProcessor, Action<Exception> actionProcessor, CancellationType convertToCancelableFuncType) where T : IPolicyBase
		{
			errorPolicyBase.PolicyProcessor.WithErrorProcessorOf(funcProcessor, actionProcessor, convertToCancelableFuncType);
			return errorPolicyBase;
		}

		public static T WithErrorProcessor<T>(this T errorPolicyBase, IErrorProcessor errorProcessor) where T : IPolicyBase
		{
			errorPolicyBase.PolicyProcessor.WithErrorProcessor(errorProcessor);
			return errorPolicyBase;
		}

		internal static T ExcludeError<T>(this T errorPolicy, Expression<Func<Exception, bool>> handledErrorFilter) where T : IPolicyBase
		{
			errorPolicy.PolicyProcessor.AddExcludedErrorFilter(handledErrorFilter);
			return errorPolicy;
		}

		internal static T ExcludeError<T, TException>(this T errorPolicy, Func<TException, bool> func = null) where T : IPolicyBase where TException : Exception
		{
			errorPolicy.PolicyProcessor.AddExcludedErrorFilter(func);
			return errorPolicy;
		}

		internal static T IncludeError<T>(this T errorPolicy, Expression<Func<Exception, bool>> handledErrorFilter) where T : IPolicyBase
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

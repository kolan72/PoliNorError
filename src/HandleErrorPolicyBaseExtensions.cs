using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static class HandleErrorPolicyBaseExtensions
	{
		public static T WithPolicyResultHandler<T>(this T errorPolicyBase, Action<PolicyResult> action, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable) where T : HandleErrorPolicyBase
		{
			return errorPolicyBase.WithPolicyResultHandler(action.ToCancelableAction(convertType));
		}

		public static T WithPolicyResultHandler<T>(this T errorPolicyBase, Action<PolicyResult, CancellationToken> action) where T : HandleErrorPolicyBase
		{
			errorPolicyBase.AddSyncHandler(action);
			return errorPolicyBase;
		}

		public static T WithPolicyResultHandler<T>(this T errorPolicyBase, Func<PolicyResult, Task> func, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable) where T : HandleErrorPolicyBase
		{
			errorPolicyBase.AddAsyncHandler(func.ToCancelableFunc(convertType));
			return errorPolicyBase;
		}

		public static T WithPolicyResultHandler<T>(this T errorPolicyBase, Func<PolicyResult, CancellationToken, Task> func) where T : HandleErrorPolicyBase
		{
			errorPolicyBase.AddAsyncHandler(func);
			return errorPolicyBase;
		}

		public static T WithPolicyResultErrorsHandler<T>(this T errorPolicyBase, Action<IEnumerable<Exception>> action, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable) where T : HandleErrorPolicyBase
		{
			return errorPolicyBase.WithPolicyResultErrorsHandler(action.ToCancelableAction(convertType));
		}

		public static T WithPolicyResultErrorsHandler<T>(this T errorPolicyBase, Action<IEnumerable<Exception>, CancellationToken> action) where T : HandleErrorPolicyBase
		{
			return errorPolicyBase.WithPolicyResultHandler(action.ToPolicyResultHandlerAction());
		}

		public static T WithPolicyResultErrorsHandler<T>(this T errorPolicyBase, Func<IEnumerable<Exception>, CancellationToken, Task> func) where T : HandleErrorPolicyBase
		{
			return errorPolicyBase.WithPolicyResultHandler(func.ToPolicyResultHandlerAsyncFunc());
		}

		public static T WithPolicyResultErrorsHandler<T>(this T errorPolicyBase, Func<IEnumerable<Exception>, Task> func, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable) where T : HandleErrorPolicyBase
		{
			return errorPolicyBase.WithPolicyResultErrorsHandler(func.ToCancelableFunc(convertType));
		}

		public static T WrapPolicy<T>(this T errorPolicyBase, IPolicyBase wrappedPolicy) where T : HandleErrorPolicyBase
		{
			if (errorPolicyBase._wrappedPolicy != null)
			{
				throw new NotImplementedException("More than one wrapped policy is not supported.");
			}
			errorPolicyBase._wrappedPolicy = wrappedPolicy;
			return errorPolicyBase;
		}

		public static T WithPolicyName<T>(this T errorPolicyBase, string policyName) where T : HandleErrorPolicyBase
		{
			errorPolicyBase.PolicyName = policyName;
			return errorPolicyBase;
		}

		public static T ExcludeError<T>(this T errorPolicy, Expression<Func<Exception, bool>> handledErrorFilter) where T : HandleErrorPolicyBase
		{
			errorPolicy.PolicyProcessor.AddExcludedErrorFilter(handledErrorFilter);
			return errorPolicy;
		}

		internal static T ExcludeError<T, TException>(this T errorPolicy, Func<TException, bool> func = null) where T : HandleErrorPolicyBase where TException : Exception
		{
			errorPolicy.PolicyProcessor.AddExcludedErrorFilter(func);
			return errorPolicy;
		}

		public static T ForError<T>(this T errorPolicy, Expression<Func<Exception, bool>> handledErrorFilter) where T : HandleErrorPolicyBase
		{
			errorPolicy.PolicyProcessor.AddIncludedErrorFilter(handledErrorFilter);
			return errorPolicy;
		}

		internal static T ForError<T, TException>(this T errorPolicy, Func<TException, bool> func = null) where T : HandleErrorPolicyBase where TException : Exception
		{
			errorPolicy.PolicyProcessor.AddIncludedErrorFilter(func);
			return errorPolicy;
		}

	}
}

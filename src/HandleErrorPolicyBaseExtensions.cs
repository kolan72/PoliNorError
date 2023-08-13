using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static class HandleErrorPolicyBaseExtensions
	{
		internal static T AddPolicyResultHandlerInner<T>(this T errorPolicyBase, Action<PolicyResult> action, CancellationType convertType = CancellationType.Precancelable) where T : HandleErrorPolicyBase
		{
			return errorPolicyBase.AddPolicyResultHandlerInner(action.ToCancelableAction(convertType));
		}

		internal static T AddPolicyResultHandlerInner<T>(this T errorPolicyBase, Action<PolicyResult, CancellationToken> action) where T : HandleErrorPolicyBase
		{
			errorPolicyBase.AddSyncHandler(action);
			return errorPolicyBase;
		}

		internal static T AddPolicyResultHandlerInner<T>(this T errorPolicyBase, Func<PolicyResult, Task> func, CancellationType convertType = CancellationType.Precancelable) where T : HandleErrorPolicyBase
		{
			errorPolicyBase.AddPolicyResultHandlerInner(func.ToCancelableFunc(convertType));
			return errorPolicyBase;
		}

		internal static T AddPolicyResultHandlerInner<T>(this T errorPolicyBase, Func<PolicyResult, CancellationToken, Task> func) where T : HandleErrorPolicyBase
		{
			errorPolicyBase.AddAsyncHandler(func);
			return errorPolicyBase;
		}

		internal static T AddPolicyResultHandlerInner<T, U>(this T errorPolicyBase, Func<PolicyResult<U>, Task> func, CancellationType convertType = CancellationType.Precancelable) where T : HandleErrorPolicyBase
		{
			errorPolicyBase.AddPolicyResultHandlerInner(func.ToCancelableFunc(convertType));
			return errorPolicyBase;
		}

		internal static T AddPolicyResultHandlerInner<T, U>(this T errorPolicyBase, Func<PolicyResult<U>, CancellationToken, Task> func) where T : HandleErrorPolicyBase
		{
			errorPolicyBase.AddAsyncHandler(func);
			return errorPolicyBase;
		}

		internal static T AddPolicyResultHandlerInner<T, U>(this T errorPolicyBase, Action<PolicyResult<U>> action, CancellationType convertType = CancellationType.Precancelable) where T : HandleErrorPolicyBase
		{
			return errorPolicyBase.AddPolicyResultHandlerInner(action.ToCancelableAction(convertType));
		}

		internal static T AddPolicyResultHandlerInner<T, U>(this T errorPolicyBase, Action<PolicyResult<U>, CancellationToken> action) where T : HandleErrorPolicyBase
		{
			errorPolicyBase.AddSyncHandler(action);
			return errorPolicyBase;
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
	}
}

using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Extensions.PolicyResultHandling
{
	public static class PolicyResultHandling
	{
		public static T AddHandlerForPolicyResult<T>(this T errorPolicyBase, Action<PolicyResult> action) where T : Policy
		{
			errorPolicyBase.AddSyncHandler(action);
			return errorPolicyBase;
		}

		public static T AddHandlerForPolicyResult<T>(this T errorPolicyBase, Action<PolicyResult> action, CancellationType convertType) where T : Policy
		{
			return errorPolicyBase.AddHandlerForPolicyResult(action.ToCancelableAction(convertType));
		}

		public static T AddHandlerForPolicyResult<T>(this T errorPolicyBase, Action<PolicyResult, CancellationToken> action) where T : Policy
		{
			errorPolicyBase.AddSyncHandler(action);
			return errorPolicyBase;
		}

		public static T AddHandlerForPolicyResult<T>(this T errorPolicyBase, Func<PolicyResult, Task> func) where T : Policy
		{
			errorPolicyBase.AddAsyncHandler(func);
			return errorPolicyBase;
		}

		public static T AddHandlerForPolicyResult<T>(this T errorPolicyBase, Func<PolicyResult, Task> func, CancellationType convertType) where T : Policy
		{
			errorPolicyBase.AddHandlerForPolicyResult(func.ToCancelableFunc(convertType));
			return errorPolicyBase;
		}

		public static T AddHandlerForPolicyResult<T>(this T errorPolicyBase, Func<PolicyResult, CancellationToken, Task> func) where T : Policy
		{
			errorPolicyBase.AddAsyncHandler(func);
			return errorPolicyBase;
		}

		public static T AddHandlerForPolicyResult<T, U>(this T errorPolicyBase, Func<PolicyResult<U>, Task> func) where T : Policy
		{
			errorPolicyBase.AddAsyncHandler(func);
			return errorPolicyBase;
		}

		public static T AddHandlerForPolicyResult<T, U>(this T errorPolicyBase, Func<PolicyResult<U>, Task> func, CancellationType convertType) where T : Policy
		{
			errorPolicyBase.AddHandlerForPolicyResult(func.ToCancelableFunc(convertType));
			return errorPolicyBase;
		}

		public static T AddHandlerForPolicyResult<T, U>(this T errorPolicyBase, Func<PolicyResult<U>, CancellationToken, Task> func) where T : Policy
		{
			errorPolicyBase.AddAsyncHandler(func);
			return errorPolicyBase;
		}

		public static T AddHandlerForPolicyResult<T, U>(this T errorPolicyBase, Action<PolicyResult<U>> action) where T : Policy
		{
			errorPolicyBase.AddSyncHandler(action);
			return errorPolicyBase;
		}

		public static T AddHandlerForPolicyResult<T, U>(this T errorPolicyBase, Action<PolicyResult<U>> action, CancellationType convertType) where T : Policy
		{
			return errorPolicyBase.AddHandlerForPolicyResult(action.ToCancelableAction(convertType));
		}

		public static T AddHandlerForPolicyResult<T, U>(this T errorPolicyBase, Action<PolicyResult<U>, CancellationToken> action) where T : Policy
		{
			errorPolicyBase.AddSyncHandler(action);
			return errorPolicyBase;
		}
	}
}

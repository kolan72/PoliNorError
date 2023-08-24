using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	internal static class PolicyResultHandlerRegistration
	{
		internal static T AddPolicyResultHandlerInner<T>(this T errorPolicyBase, Action<PolicyResult> action) where T : Policy
		{
			errorPolicyBase.AddSyncHandler(action);
			return errorPolicyBase;
		}

		internal static T AddPolicyResultHandlerInner<T>(this T errorPolicyBase, Action<PolicyResult> action, CancellationType convertType) where T : Policy
		{
			return errorPolicyBase.AddPolicyResultHandlerInner(action.ToCancelableAction(convertType));
		}

		internal static T AddPolicyResultHandlerInner<T>(this T errorPolicyBase, Action<PolicyResult, CancellationToken> action) where T : Policy
		{
			errorPolicyBase.AddSyncHandler(action);
			return errorPolicyBase;
		}

		internal static T AddPolicyResultHandlerInner<T>(this T errorPolicyBase, Func<PolicyResult, Task> func) where T : Policy
		{
			errorPolicyBase.AddAsyncHandler(func);
			return errorPolicyBase;
		}

		internal static T AddPolicyResultHandlerInner<T>(this T errorPolicyBase, Func<PolicyResult, Task> func, CancellationType convertType) where T : Policy
		{
			errorPolicyBase.AddPolicyResultHandlerInner(func.ToCancelableFunc(convertType));
			return errorPolicyBase;
		}

		internal static T AddPolicyResultHandlerInner<T>(this T errorPolicyBase, Func<PolicyResult, CancellationToken, Task> func) where T : Policy
		{
			errorPolicyBase.AddAsyncHandler(func);
			return errorPolicyBase;
		}

		internal static T AddPolicyResultHandlerInner<T, U>(this T errorPolicyBase, Func<PolicyResult<U>, Task> func) where T : Policy
		{
			errorPolicyBase.AddAsyncHandler(func);
			return errorPolicyBase;
		}

		internal static T AddPolicyResultHandlerInner<T, U>(this T errorPolicyBase, Func<PolicyResult<U>, Task> func, CancellationType convertType) where T : Policy
		{
			errorPolicyBase.AddPolicyResultHandlerInner(func.ToCancelableFunc(convertType));
			return errorPolicyBase;
		}

		internal static T AddPolicyResultHandlerInner<T, U>(this T errorPolicyBase, Func<PolicyResult<U>, CancellationToken, Task> func) where T : Policy
		{
			errorPolicyBase.AddAsyncHandler(func);
			return errorPolicyBase;
		}

		internal static T AddPolicyResultHandlerInner<T, U>(this T errorPolicyBase, Action<PolicyResult<U>> action) where T : Policy
		{
			errorPolicyBase.AddSyncHandler(action);
			return errorPolicyBase;
		}

		internal static T AddPolicyResultHandlerInner<T, U>(this T errorPolicyBase, Action<PolicyResult<U>> action, CancellationType convertType) where T : Policy
		{
			return errorPolicyBase.AddPolicyResultHandlerInner(action.ToCancelableAction(convertType));
		}

		internal static T AddPolicyResultHandlerInner<T, U>(this T errorPolicyBase, Action<PolicyResult<U>, CancellationToken> action) where T : Policy
		{
			errorPolicyBase.AddSyncHandler(action);
			return errorPolicyBase;
		}
	}
}

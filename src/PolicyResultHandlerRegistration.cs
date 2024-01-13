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

		/// <summary>
		///  Adds a special PolicyResult handler that only sets  <see cref="PolicyResult.IsFailed"/> to true if the executed <paramref name="predicate"/> returns true.
		/// </summary>
		/// <typeparam name="T">The type of the Policy</typeparam>
		/// <param name="errorPolicyBase"></param>
		/// <param name="predicate">A predicate that a PolicyResult should satisfy.</param>
		/// <returns></returns>
		internal static T SetPolicyResultFailedIfInner<T>(this T errorPolicyBase, Func<PolicyResult, bool> predicate) where T : Policy
		{
			void handler(PolicyResult pr)
			{
				if (predicate(pr))
					pr.SetFailed();
			}
			return AddPolicyResultHandlerInner(errorPolicyBase, handler);
		}

		/// <summary>
		/// Adds a special PolicyResult handler that only sets  <see cref="PolicyResult.IsFailed"/> to true if the executed <paramref name="predicate"/> returns true.
		/// </summary>
		/// <typeparam name="T">The type of the Policy.</typeparam>
		/// <typeparam name="U">The type of the result.</typeparam>
		/// <param name="errorPolicyBase"></param>
		/// <param name="predicate">>A predicate that a PolicyResult should satisfy.</param>
		/// <returns></returns>
		internal static T SetPolicyResultFailedIfInner<T, U>(this T errorPolicyBase, Func<PolicyResult<U>, bool> predicate) where T : Policy
		{
			void handler(PolicyResult<U> pr)
			{
				if (predicate(pr))
					pr.SetFailed();
			}
			return AddPolicyResultHandlerInner<T, U>(errorPolicyBase, handler);
		}
	}
}

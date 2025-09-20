using System;
using PoliNorError.Extensions.PolicyResultHandling;
namespace PoliNorError
{
	internal static class PolicyResultHandlerRegistration
	{
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
			return errorPolicyBase.AddHandlerForPolicyResult(handler);
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
			return errorPolicyBase.AddHandlerForPolicyResult<T, U>(handler);
		}

		/// <summary>
		/// Adds a special PolicyResult handler that only sets  <see cref="PolicyResult.IsFailed"/> to true if the executed <paramref name="predicate"/> returns true.
		/// When <see cref="PolicyResult.IsFailed"/> is set to <see langword="true"/> by this <see cref="PolicyResult{T}"/> handler, the <paramref name="onSetPolicyResultFailed"/> handler is called.
		/// </summary>
		/// <typeparam name="T">The type of the Policy.</typeparam>
		/// <typeparam name="U">The type of the result.</typeparam>
		/// <param name="errorPolicyBase">Policy to which handler is added.</param>
		/// <param name="predicate">A predicate that a PolicyResult should satisfy.</param>
		/// <param name="onSetPolicyResultFailed">Delegate that is called when <see cref="PolicyResult.IsFailed"/> is set to true by the <see cref="PolicyResult{T}"/> handler.</param>
		/// <returns></returns>
		internal static T SetPolicyResultFailedWithHandlerIfInner<T, U>(this T errorPolicyBase, Func<PolicyResult<U>, bool> predicate, Action<PolicyResult<U>> onSetPolicyResultFailed) where T : Policy
		{
			void handler(PolicyResult<U> pr)
			{
				if (pr.IsFailed)
				{
					return;
				}
				if (predicate(pr))
				{
					pr.SetFailed();
					onSetPolicyResultFailed(pr);
				}
			}
			return errorPolicyBase.AddHandlerForPolicyResult<T, U>(handler);
		}
	}
}

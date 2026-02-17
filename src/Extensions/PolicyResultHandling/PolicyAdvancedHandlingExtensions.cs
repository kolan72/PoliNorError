using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Extensions.PolicyResultHandling
{
	public static class PolicyAdvancedHandlingExtensions
	{
		public static TPolicy HandleResult<TPolicy>(this TPolicy policy, Func<PolicyResult, bool> resultPredicate)
			where TPolicy : Policy
		{
			if (resultPredicate is null)
			{
				return policy;
			}

			policy.AddHandlerForPolicyResult(result =>
			{
				if (!resultPredicate(result))
				{
					return;
				}

				result.SetFailedInner(PolicyResultFailedReason.PolicyResultHandlerFailed);
			});
			return policy;
		}

		public static PolicyResult<T> HandleValue<T>(
			this IPolicyBase policy,
			Func<T> func,
			Func<T, bool> failurePredicate,
			CancellationToken token = default)
		{
			var result = policy.Handle(func, token);
			if (failurePredicate?.Invoke(result.Result) == true)
			{
				result.SetFailedInner(PolicyResultFailedReason.PolicyResultHandlerFailed);
			}

			return result;
		}

		public static async Task<PolicyResult<T>> HandleValueAsync<T>(
			this IPolicyBase policy,
			Func<CancellationToken, Task<T>> func,
			Func<T, bool> failurePredicate,
			bool configureAwait = false,
			CancellationToken token = default)
		{
			var result = await policy.HandleAsync(func, configureAwait, token).ConfigureAwait(configureAwait);
			if (failurePredicate?.Invoke(result.Result) == true)
			{
				result.SetFailedInner(PolicyResultFailedReason.PolicyResultHandlerFailed);
			}

			return result;
		}
	}
}

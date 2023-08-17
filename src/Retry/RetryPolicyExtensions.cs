using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static class RetryPolicyExtensions
	{
		public static RetryPolicy UseCustomErrorSaverOf(this RetryPolicy policy, Action<Exception> saveError, CancellationType convertToCancelableFuncType = CancellationType.Precancelable)
		{
			policy.RetryProcessor.UseCustomErrorSaverOf(saveError, convertToCancelableFuncType);
			return policy;
		}

		public static RetryPolicy UseCustomErrorSaverOf(this RetryPolicy policy, Action<Exception, CancellationToken> saveError)
		{
			policy.RetryProcessor.UseCustomErrorSaverOf(saveError);
			return policy;
		}

		public static RetryPolicy UseCustomErrorSaverOf(this RetryPolicy policy, Func<Exception, CancellationToken, Task> saveErrorAsync)
		{
			policy.RetryProcessor.UseCustomErrorSaverOf(saveErrorAsync);
			return policy;
		}

		public static RetryPolicy UseCustomErrorSaverOf(this RetryPolicy policy, Func<Exception, CancellationToken, Task> saveErrorAsync, Action<Exception> saveError, CancellationType convertToCancelableFuncType = CancellationType.Precancelable)
		{
			policy.RetryProcessor.UseCustomErrorSaverOf(saveErrorAsync, saveError, convertToCancelableFuncType);
			return policy;
		}

		public static RetryPolicy UseCustomErrorSaverOf(this RetryPolicy policy, Func<Exception, Task> saveErrorAsync, Action<Exception> saveError)
		{
			policy.RetryProcessor.UseCustomErrorSaverOf(saveErrorAsync, saveError);
			return policy;
		}
	}
}

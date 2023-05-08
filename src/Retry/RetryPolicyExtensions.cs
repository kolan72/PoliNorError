using System;

namespace PoliNorError
{
	public static class RetryPolicyExtensions
	{
		public static RetryPolicy ExcludeError<TException>(this RetryPolicy retryPolicy, Func<TException, bool> func = null) where TException : Exception => retryPolicy.ExcludeError<RetryPolicy, TException>(func);

		public static RetryPolicy ForError<TException>(this RetryPolicy retryPolicy, Func<TException, bool> func = null) where TException : Exception => retryPolicy.ForError<RetryPolicy, TException>(func);
	}
}

using System;

namespace PoliNorError
{
	internal static class PolicyResultHandleErrorDelegates
	{
		internal static Action<PolicyResult, Exception> DefaultErrorSaver => (pr, ex) => pr.AddError(ex);

		public static Action<PolicyResult, Exception> GetWrappedErrorSaver(Action<PolicyResult, Exception> action, bool setFailedIfInvocationError = false) => (result, ex) =>
		{
			//In the common case, action could not have an exception handler,
			//so we wrap it here.
			try
			{
				action(result, ex);
			}
			catch (Exception exIn)
			{
				if (setFailedIfInvocationError)
				{
					result.SetFailedWithCatchBlockError(exIn, ex);
				}
				else
				{
					result.AddCatchBlockError(new CatchBlockException(exIn, ex));
				}
			}
		};
	}
}

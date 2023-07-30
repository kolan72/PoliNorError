using System;
using System.Linq;

namespace PoliNorError
{
	internal static class PolicyResultHandleErrorDelegates
	{
		public static Action<PolicyResult, Exception> GetWrappedErrorSaver(Action<Exception> action, bool setFailedIfInvocationError = false) => (result, ex) =>
		{
			//In the common case, action could not have an exception handler,
			//so we wrap it here.
			try
			{
				if (action == null)
				{
					result.AddError(ex);
				}
				else
				{
					//In the common case, action could not have an exception handler,
					action(ex);
					//We set it here to keep UnprocessedError from being lost.
					result.UnprocessedError = ex;
				}
			}
			catch (Exception exIn)
			{
				if (setFailedIfInvocationError)
				{
					result.UnprocessedError = ex;
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

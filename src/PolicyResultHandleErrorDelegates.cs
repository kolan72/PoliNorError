using System;
using System.Linq;

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
				//We don't know if the action saves an error in the Errors collection, 
				//so we set it here to keep UnprocessedError from being lost.
				if(!result.Errors.Contains(ex))
				{
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

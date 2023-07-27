using System;

namespace PoliNorError
{
	internal static class PolicyResultHandleErrorDelegates
	{
		internal static Action<PolicyResult, Exception> GetDefaultErrorSaver() => (pr, ex) => pr.AddError(ex);

		public static Action<PolicyResult, Exception> GetWrappedErrorSaver(Action<PolicyResult, Exception> action) => (result, ex) =>
		{
			//In the common case, action could not have an exception handler,
			//so we wrap it here.
			try
			{
				action(result, ex);
			}
			catch (Exception exIn)
			{
				result.SetFailedWithCatchBlockError(exIn, ex);
				result.UnprocessedError = ex;
			}
		};
	}
}

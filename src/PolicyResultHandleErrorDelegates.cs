using System;

namespace PoliNorError
{
	public static class PolicyResultHandleErrorDelegates
	{
		public static readonly Action<PolicyResult, Exception> DefaultErrorSaver = GetWrappedErrorSaver(GetDefaultErrorSaver());

		private static Action<PolicyResult, Exception> GetDefaultErrorSaver() => (pr, ex) => pr.AddError(ex);

		public static Action<PolicyResult, Exception> GetWrappedErrorSaver(Action<PolicyResult, Exception> action) => (result, ex) =>
		{
			try
			{
				action(result, ex);
			}
			catch (Exception exIn)
			{
				result.SetFailedWithCatchBlockError(exIn, ex, true);
			}
		};
	}
}

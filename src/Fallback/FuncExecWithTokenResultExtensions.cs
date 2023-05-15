using System;

namespace PoliNorError
{
	internal static class FuncExecWithTokenResultExtensions
	{
		internal static void ChangePolicyResult(this FuncExecWithTokenResult funcExecResult, PolicyResult policyResult, Exception ex)
		{
			if (funcExecResult.IsSuccess)
				return;

			SetFailedByCancel(funcExecResult, policyResult, ex);
		}

		internal static void ChangePolicyResult<T>(this FuncExecWithTokenResult<T> funcExecResult, PolicyResult<T> policyResult, Exception ex)
		{
			if (funcExecResult.IsSuccess)
			{
				policyResult.SetResult(funcExecResult.Result);
				return;
			}

			SetFailedByCancel(funcExecResult, policyResult, ex);
		}

		private static void SetFailedByCancel(FuncExecWithTokenResult funcExecResult, PolicyResult policyResult, Exception ex)
		{
			if (funcExecResult.IsCanceled)
				policyResult.SetFailedAndCanceled();
			else
				policyResult.SetFailedWithCatchBlockError(funcExecResult.Error, ex, true);
		}
	}
}

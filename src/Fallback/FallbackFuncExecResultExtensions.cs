using System;

namespace PoliNorError
{
	internal static class FallbackFuncExecResultExtensions
	{
		internal static void ChangePolicyResult(this FallbackFuncExecResult funcExecResult, PolicyResult policyResult, Exception ex)
		{
			if (funcExecResult.IsSuccess)
				return;

			SetFailedByCancel(funcExecResult, policyResult, ex);
		}

		internal static void ChangePolicyResult<T>(this FallbackFuncExecResult<T> funcExecResult, PolicyResult<T> policyResult, Exception ex)
		{
			if (funcExecResult.IsSuccess)
			{
				policyResult.SetResult(funcExecResult.Result);
				return;
			}

			SetFailedByCancel(funcExecResult, policyResult, ex);
		}

		private static void SetFailedByCancel(FallbackFuncExecResult funcExecResult, PolicyResult policyResult, Exception ex)
		{
			if (funcExecResult.IsCanceled)
				policyResult.SetFailedAndCanceled();
			else
				policyResult.SetFailedWithCatchBlockError(funcExecResult.Error, ex);
		}
	}
}

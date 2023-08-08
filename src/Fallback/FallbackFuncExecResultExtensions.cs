using System;

namespace PoliNorError
{
	internal static class FallbackFuncExecResultExtensions
	{
		internal static void ChangePolicyResult(this FallbackFuncExecResult funcExecResult, PolicyResult policyResult, Exception ex)
		{
			if (funcExecResult.IsSuccess)
				return;

			SetFailedByFallbackFuncExecResult(policyResult, funcExecResult, ex);
		}

		internal static void ChangePolicyResult<T>(this FallbackFuncExecResult<T> funcExecResult, PolicyResult<T> policyResult, Exception ex)
		{
			if (funcExecResult.IsSuccess)
			{
				policyResult.SetResult(funcExecResult.Result);
				return;
			}
			SetFailedByFallbackFuncExecResult(policyResult, funcExecResult, ex);
		}

		private static void SetFailedByFallbackFuncExecResult(PolicyResult policyResult, FallbackFuncExecResult funcExecResult, Exception ex)
		{
			if (funcExecResult.IsCanceled)
				policyResult.SetFailedAndCanceled();
			else
				policyResult.SetFailedWithCatchBlockError(funcExecResult.Error, ex, CatchBlockExceptionSource.PolicyRule);
		}
	}
}

using System;

namespace PoliNorError
{
	internal static class FallbackFuncExecResultExtensions
	{
		internal static void ChangePolicyResult(this FallbackFuncExecResult funcExecResult, PolicyResult policyResult, Exception ex)
		{
			if (funcExecResult.IsSuccess)
				return;

			policyResult.SetFailedByFallbackFuncExecResult(funcExecResult, ex);
		}

		internal static void ChangePolicyResult<T>(this FallbackFuncExecResult<T> funcExecResult, PolicyResult<T> policyResult, Exception ex)
		{
			if (funcExecResult.IsSuccess)
			{
				policyResult.SetResult(funcExecResult.Result);
				return;
			}
			policyResult.SetFailedByFallbackFuncExecResult(funcExecResult, ex);
		}
	}
}

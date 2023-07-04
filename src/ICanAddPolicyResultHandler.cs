using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public interface ICanAddPolicyResultHandler<T> where T: ICanAddPolicyResultHandler<T>
	{
		T AddPolicyResultHandler(Action<PolicyResult> action, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable);
		T AddPolicyResultHandler(Action<PolicyResult, CancellationToken> action);

		T AddPolicyResultHandler(Func<PolicyResult, Task> func, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable);
		T AddPolicyResultHandler(Func<PolicyResult, CancellationToken, Task> func);

		T AddPolicyResultHandler<U>(Action<PolicyResult<U>> action, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable);
		T AddPolicyResultHandler<U>(Action<PolicyResult<U>, CancellationToken> action);

		T AddPolicyResultHandler<U>(Func<PolicyResult<U>, Task> func, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable);
		T AddPolicyResultHandler<U>(Func<PolicyResult<U>, CancellationToken, Task> func);
	}
}

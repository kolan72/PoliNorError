using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static class HandleErrorPolicyBaseExtensions
	{
		public static T AddPolicyResultHandler<T>(this T errorPolicyBase, Action<PolicyResult> action, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable) where T : HandleErrorPolicyBase
		{
			return errorPolicyBase.AddPolicyResultHandler(action.ToCancelableAction(convertType));
		}

		public static T AddPolicyResultHandler<T>(this T errorPolicyBase, Action<PolicyResult, CancellationToken> action) where T : HandleErrorPolicyBase
		{
			errorPolicyBase.AddSyncHandler(action);
			return errorPolicyBase;
		}

		public static T AddPolicyResultHandler<T>(this T errorPolicyBase, Func<PolicyResult, Task> func, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable) where T : HandleErrorPolicyBase
		{
			errorPolicyBase.AddAsyncHandler(func.ToCancelableFunc(convertType));
			return errorPolicyBase;
		}

		public static T AddPolicyResultHandler<T>(this T errorPolicyBase, Func<PolicyResult, CancellationToken, Task> func) where T : HandleErrorPolicyBase
		{
			errorPolicyBase.AddAsyncHandler(func);
			return errorPolicyBase;
		}

		public static T WrapPolicy<T>(this T errorPolicyBase, IPolicyBase wrappedPolicy) where T : HandleErrorPolicyBase
		{
			if (errorPolicyBase._wrappedPolicy != null)
			{
				throw new NotImplementedException("More than one wrapped policy is not supported.");
			}
			errorPolicyBase._wrappedPolicy = wrappedPolicy;
			return errorPolicyBase;
		}

		public static T WithPolicyName<T>(this T errorPolicyBase, string policyName) where T : HandleErrorPolicyBase
		{
			errorPolicyBase.PolicyName = policyName;
			return errorPolicyBase;
		}
	}
}

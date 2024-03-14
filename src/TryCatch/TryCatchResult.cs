using System;
using System.Linq;

namespace PoliNorError.TryCatch
{
	public class TryCatchResult : TryCatchResultBase
	{
		internal TryCatchResult(PolicyResult policyResult) : base(policyResult) {}
	}

	public class TryCatchResult<T> : TryCatchResult
	{
		internal TryCatchResult(PolicyResult<T> policyResult) : base(policyResult)
		{
			if (!IsError)
			{
				Result = policyResult.Result;
			}
		}

		public T Result { get; }
	}

	public abstract class TryCatchResultBase
	{
		protected TryCatchResultBase(PolicyResult policyResult)
		{
			IsCanceled = policyResult.IsCanceled;
			Error = !policyResult.NoError ? policyResult.Errors.FirstOrDefault() : policyResult.GetErrorInWrappedResults();
			IsError = !(Error is null);
		}

		public bool IsCanceled { get; }

		public bool IsError { get; }

		public Exception Error { get; }
	}
}

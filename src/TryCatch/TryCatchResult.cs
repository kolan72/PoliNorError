using System;
using System.Linq;

namespace PoliNorError.TryCatch
{
	public class TryCatchResult : TryCatchResultBase
	{
		internal TryCatchResult(PolicyResult policyResult) : base(policyResult)
		{
			Error = !policyResult.NoError ? policyResult.Errors.FirstOrDefault() : policyResult.GetErrorInWrappedResults();
			IsError = !(Error is null);
		}
	}

	public class TryCatchResult<T> : TryCatchResultBase
	{
		internal TryCatchResult(PolicyResult<T> policyResult) : base(policyResult)
		{
			Error = !policyResult.NoError ? policyResult.Errors.FirstOrDefault() : policyResult.GetErrorInWrappedResults();
			IsError = !(Error is null);
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
		}

		public bool IsCanceled { get; }

		public bool IsError { get; protected set; }

		public Exception Error { get; protected set; }
	}
}

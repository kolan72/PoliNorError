using System;
using System.Linq;

namespace PoliNorError.TryCatch
{
	public class TryCatchResult : TryCatchResultBase
	{
		internal TryCatchResult(PolicyResult policyResult) : base(policyResult) {}
	}

	public abstract class TryCatchResultBase
	{
		protected TryCatchResultBase(PolicyResult policyResult)
		{
			IsCanceled = policyResult.IsCanceled;
			IsError = !policyResult.NoError;
			Error = !policyResult.NoError ? policyResult.Errors.FirstOrDefault() : null;
		}

		public bool IsCanceled { get; }

		public bool IsError { get; }

		public Exception Error { get; }
	}
}

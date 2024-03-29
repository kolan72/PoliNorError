using System;
using System.Linq;

namespace PoliNorError.TryCatch
{
	/// <summary>
	/// The result of executing delegates using non-generic methods of the <see cref="ITryCatch"/> interface.
	/// </summary>
	public class TryCatchResult : TryCatchResultBase
	{
		internal TryCatchResult(PolicyResult policyResult, int catchBlockCount) : base(policyResult)
		{
			(Error, ExceptionHandlerIndex) = policyResult.GetErrorInWrappedResults(catchBlockCount - 1);
			IsError = !(Error is null);
		}
	}

	/// <summary>
	/// The result of executing delegates using generic methods of the <see cref="ITryCatch"/> interface.
	/// </summary>
	/// <typeparam name="T">The type of return value of the generic delegate</typeparam>
	public class TryCatchResult<T> : TryCatchResultBase
	{
		internal TryCatchResult(PolicyResult<T> policyResult, int catchBlockCount) : base(policyResult)
		{
			(Error, ExceptionHandlerIndex) = policyResult.GetErrorInWrappedResults(catchBlockCount - 1);
			IsError = !(Error is null);
			if (!IsError)
			{
				Result = policyResult.Result;
			}
		}

		/// <summary>
		/// The return value of the generic method if no exception occurs.
		/// </summary>
		public T Result { get; }
	}

	public abstract class TryCatchResultBase
	{
		protected TryCatchResultBase(PolicyResult policyResult)
		{
			IsCanceled = policyResult.IsCanceled;
		}

		/// <summary>
		/// Indicates whether the execution was canceled.
		/// </summary>
		public bool IsCanceled { get; }

		/// <summary>
		///  Indicates whether the execution ended with an exception.
		/// </summary>
		public bool IsError { get; protected set; }

		/// <summary>
		/// Represents an exception that occurred during execution.
		/// </summary>
		public Exception Error { get; protected set; }

		/// <summary>
		/// Represents the index of the <see cref="CatchBlockHandler"/> that handled an exception.
		/// </summary>
		public int ExceptionHandlerIndex { get; protected set; } = -1;
	}
}

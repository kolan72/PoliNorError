using System;
using System.Linq;

namespace PoliNorError.TryCatch
{
	/// <summary>
	/// The result of executing delegates using non-generic methods of the <see cref="ITryCatch"/> interface.
	/// </summary>
	public class TryCatchResult : TryCatchResultBase
	{
		internal TryCatchResult(PolicyResult policyResult, int catchBlockCount) : base(policyResult, catchBlockCount)
		{
		}
	}

	/// <summary>
	/// The result of executing delegates using generic methods of the <see cref="ITryCatch"/> interface.
	/// </summary>
	/// <typeparam name="T">The type of return value of the generic delegate</typeparam>
	public class TryCatchResult<T> : TryCatchResultBase
	{
		internal TryCatchResult(PolicyResult<T> policyResult, int catchBlockCount) : base(policyResult, catchBlockCount)
		{
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

	/// <summary>
	/// Base class for results of executing delegates using the <see cref="ITryCatch"/> interface.
	/// Provides state information about whether the execution succeeded, was canceled, or encountered an error,
	/// including the exception that was caught and the index of the <see cref="CatchBlockHandler"/> that handled it.
	/// </summary>
	public abstract class TryCatchResultBase
	{
		protected TryCatchResultBase(PolicyResult policyResult)
		{
			IsCanceled = policyResult.IsCanceled;
		}

		protected TryCatchResultBase(PolicyResult policyResult, int catchBlockCount) : this(policyResult)
		{
			InitializeErrorState(policyResult, catchBlockCount);
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
		/// Indicates that the execution did not result in an exception or a cancellation.
		/// </summary>
		public bool IsSuccess => !IsError && !IsCanceled;

		/// <summary>
		/// Represents the index of the <see cref="CatchBlockHandler"/> that handled an exception.
		/// </summary>
		public int ExceptionHandlerIndex { get; protected set; } = -1;

		private void InitializeErrorState(PolicyResult policyResult, int catchBlockCount)
		{
			(Error, ExceptionHandlerIndex) = policyResult.GetErrorInWrappedResults(catchBlockCount - 1);
			IsError = !(Error is null);
		}
	}
}

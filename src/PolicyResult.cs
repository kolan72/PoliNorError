using System;
using System.Collections.Generic;
using System.Linq;

namespace PoliNorError
{
	/// <summary>
	/// Stores execution results after calling the policy's non-generic <see cref="IPolicyBase.Handle"/> or <see cref="IPolicyBase.HandleAsync"/> methods.
	/// </summary>
	public class PolicyResult
	{
		private readonly FlexSyncEnumerable<Exception> _errors;
		private readonly FlexSyncEnumerable<CatchBlockException> _catchBlockErrors;
		private readonly FlexSyncEnumerable<PolicyResultHandlingException> _handleResultErrors;

		private Exception _unprocessedError;

		internal static PolicyResult ForSync() => new PolicyResult();
		internal static PolicyResult ForNotSync() => new PolicyResult(true);
		internal static PolicyResult InitByConfigureAwait(bool configureAwait) => !configureAwait ? ForNotSync() : ForSync();

		/// <summary>
		/// Creates <see cref="PolicyResult"/>.
		/// </summary>
		/// <param name="forAsync">Indicates whether PolicyResult was created to store the results of handling sync or async delegate.</param>
		public PolicyResult(bool forAsync = false)
		{
			Async = forAsync;
			_errors = new FlexSyncEnumerable<Exception>(forAsync);
			FailedReason = PolicyResultFailedReason.None;
			_catchBlockErrors = new FlexSyncEnumerable<CatchBlockException>(forAsync);
			_handleResultErrors = new FlexSyncEnumerable<PolicyResultHandlingException>(forAsync);
		}

		/// <summary>
		/// Represents the collection of <see cref="Exception"/>s where the exception is stored by default.
		/// </summary>
		public IEnumerable<Exception> Errors => _errors;

		/// <summary>
		/// Indicates whether error storage is customised.
		/// </summary>
		public bool ErrorsNotUsed { get; internal set; }

		/// <summary>
		/// Collection of <see cref="CatchBlockException"/> exceptions.
		/// </summary>
		public IEnumerable<CatchBlockException> CatchBlockErrors => _catchBlockErrors;

		/// <summary>
		/// Collection of <see cref="PolicyResultHandlingException"/> exceptions.
		/// </summary>
		public IEnumerable<PolicyResultHandlingException> PolicyResultHandlingErrors => _handleResultErrors;

		/// <summary>
		/// Sets the <see cref="IsFailed"/> property to <see langword="true"/>.
		/// </summary>
		public void SetFailed()
		{
			if (!IsFailed)
			{
				IsFailed = true;
			}
		}

		/// <summary>
		///  Indicates successful handling.
		/// </summary>
		public bool IsSuccess => !IsFailed && !IsCanceled;

		/// <summary>
		/// Indicates that, despite errors during the handling, the policy handled the delegate successfully.
		/// </summary>
		public bool IsPolicySuccess => !NoError && IsSuccess;

		/// <summary>
		/// Indicates whether the handling was canceled.
		/// </summary>
		public bool IsCanceled { get; protected set; }

		/// <summary>
		/// Indicates that there were no exceptions at all when the delegate was called.
		/// </summary>
		public bool NoError { get; protected set; }

		/// <summary>
		/// Indicates whether the delegate has been handled.
		/// </summary>
		public bool IsFailed { get; protected set; }

		///<inheritdoc cref = "PolicyResultFailedReason"/>
		public PolicyResultFailedReason FailedReason { get; internal set; }

		/// <summary>
		/// Indicates that error filter conditions have not been satisfied.
		/// </summary>
		public bool ErrorFilterUnsatisfied { get; internal set; }

		/// <summary>
		/// Represents an exception that was not handled correctly within the catch block. To find out the failed reason, see the <see cref="FailedReason "/> property.
		/// </summary>
		public Exception UnprocessedError
		{
			get { return _unprocessedError ?? ((IsFailed && (FailedReason == PolicyResultFailedReason.DelegateIsNull || FailedReason == PolicyResultFailedReason.PolicyProcessorFailed || FailedReason == PolicyResultFailedReason.UnhandledError))
															? Errors.LastOrDefault()
															: null); }
			internal set { _unprocessedError = value; }
		}

		/// <summary>
		/// An exception in itself or wrapped in the AggregateException caused the processing to break with the <see cref="IsFailed"/>property equaling true.
		/// </summary>
		public Exception CriticalError => CatchBlockErrors.FirstOrDefault(ce => ce.IsCritical)?.ProcessingException;

		/// <summary>
		/// Contains the collection of <see cref="PolicyDelegateResult"/> results of wrapping a policy or <see cref="PolicyCollection"/> collection by the policy
		/// that gives rise to this <see cref="PolicyResult"/> result.
		/// </summary>
		public IEnumerable<PolicyDelegateResult> WrappedPolicyResults { get; internal set; }

		/// <summary>
		/// Represents the name of the policy whose results this PolicyResult contains.
		/// </summary>
		public string PolicyName { get; internal set; }

		/// <summary>
		/// Represents the index of the PolicyResult handler that set <see cref="PolicyResult.IsFailed"/> property to true.
		/// </summary>
		public int FailedHandlerIndex { get; internal set; } = -1;

		internal void SetFailedInner(PolicyResultFailedReason failedReason = PolicyResultFailedReason.PolicyProcessorFailed)
		{
			IsFailed = true;
			FailedReason = failedReason;
		}

		protected void SetErrors(IEnumerable<Exception> exceptions)
		{
			throw new NotImplementedException();
		}

		internal void AddError(Exception exception)
		{
			_errors.Add(exception);
		}

		internal void AddCatchBlockError(CatchBlockException catchBlockException)
		{
			_catchBlockErrors.Add(catchBlockException);
		}

		internal void AddCatchBlockErrors(IEnumerable<CatchBlockException> catchBlockExceptions)
		{
			_catchBlockErrors.AddRange(catchBlockExceptions);
		}

		internal void AddHandleResultError(PolicyResultHandlingException handlePolicyResultException)
		{
			_handleResultErrors.Add(handlePolicyResultException);
		}

		internal void AddHandleResultErrors(IEnumerable<PolicyResultHandlingException> handlePolicyResultExceptions)
		{
			_handleResultErrors.AddRange(handlePolicyResultExceptions);
		}

		internal void SetOk()
		{
			NoError = true;
		}

		internal void SetFailedAndCanceled()
		{
			SetCanceled();
			SetFailedInner();
		}

		internal void SetFailedAndFilterUnsatisfied()
		{
			ErrorFilterUnsatisfied = true;
			SetFailedInner();
		}

		internal void SetCanceled()
		{
			IsCanceled = true;
		}

		internal bool Async { get; }

		internal virtual IEnumerable<PolicyDelegateResultBase> GetWrappedPolicyResults() => WrappedPolicyResults;
	}

	/// <summary>
	/// Stores execution results after calling the policy's generic <see cref="IPolicyBase.Handle{T}"/> or <see cref="IPolicyBase.HandleAsync{T}"/> methods.
	/// </summary>
	/// <typeparam name="T">Type of result.</typeparam>
	public class PolicyResult<T> : PolicyResult
	{
		internal static new PolicyResult<T> ForSync() => new PolicyResult<T>();
		internal static new PolicyResult<T> ForNotSync() => new PolicyResult<T>(true);
		internal static new PolicyResult<T> InitByConfigureAwait(bool configureAwait) => !configureAwait ? ForNotSync() : ForSync();

		///<inheritdoc cref = "PolicyResult(bool)"/>
		public PolicyResult(bool forAsync = false) : base(forAsync){}

		internal void SetResult(T result)
		{
			Result = result;
		}

		/// <summary>
		/// Contains the result of the delegate handling.
		/// </summary>
		public T Result { get; private set; }

		/// <summary>
		/// Contains the collection of <see cref="PolicyDelegateResult{T}"/> results of wrapping a policy or <see cref="PolicyCollection"/> collection by the policy
		/// that gives rise to this <see cref="PolicyResult{T}"/> result.
		/// </summary>
		public new IEnumerable<PolicyDelegateResult<T>> WrappedPolicyResults { get; internal set; }

		internal override IEnumerable<PolicyDelegateResultBase> GetWrappedPolicyResults() => WrappedPolicyResults;
	}

	/// <summary>
	/// Represents the reason why the <see cref="PolicyResult.IsFailed"/> property was set to <see langword="true"/>.
	/// </summary>
	public enum PolicyResultFailedReason
	{
		/// <summary>
		/// Default value.
		/// </summary>
		None,
		/// <summary>
		/// The delegate to handle is null.
		/// </summary>
		DelegateIsNull,
		/// <summary>
		/// The <see cref="PolicyResult.IsFailed"/> property was set to <see langword="true"/> during processing by a policy processor.
		/// </summary>
		PolicyProcessorFailed,
		/// <summary>
		/// The <see cref="PolicyResult.IsFailed"/> property was set to <see langword="true"/> by a PolicyResult handler.
		/// </summary>
		PolicyResultHandlerFailed,
		/// <summary>
		/// Policy  (re-)throws exception.
		/// </summary>
		UnhandledError
	}
}

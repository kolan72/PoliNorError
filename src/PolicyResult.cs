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

		private bool _executed;

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
		/// Indicates failed handling.
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

		/// <summary>
		/// Gets the exception thrown during policy execution when cancellation occurred.
		/// </summary>
		public OperationCanceledException PolicyCanceledError { get; internal set; }

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

		internal void SetFailedAndCanceled(OperationCanceledException exception)
		{
			SetCanceled();
			SetFailedInner();
			PolicyCanceledError = exception;
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

		internal void SetExecuted()
		{
			_executed = true;
		}

		internal bool Async { get; }

		internal PolicyStatus Status
		{
			get
			{
				if (!_executed && !NoError && !IsFailed && !IsCanceled)
				{
					return PolicyStatus.NotExecuted;
				}
				else if (IsFailed)
				{
					if (IsCanceled)
						return PolicyStatus.FailedWithCancellation;
					else
						return PolicyStatus.Failed;
				}
				else if (IsCanceled)
				{
					return PolicyStatus.Canceled;
				}
				else if (NoError)
				{
					return PolicyStatus.NoError;
				}
				else
				{
					return PolicyStatus.PolicySuccess;
				}
			}
		}

		internal virtual PolicyResult GetLastWrappedPolicyResult()
		{
			return WrappedPolicyResults?.LastOrDefault()?.Result;
		}

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

		internal override PolicyResult GetLastWrappedPolicyResult()
		{
			return WrappedPolicyResults?.LastOrDefault()?.Result;
		}

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

	internal class PolicyStatus
	{
		public static readonly PolicyStatus NotExecuted = new PolicyStatus(ResultStatusValue.NotExecuted);
		public static readonly PolicyStatus NoError = new PolicyStatus(ResultStatusValue.NoError);
		public static readonly PolicyStatus PolicySuccess = new PolicyStatus(ResultStatusValue.PolicySuccess);
		public static readonly PolicyStatus Failed = new PolicyStatus(ResultStatusValue.Failed);
		public static readonly PolicyStatus Canceled = new PolicyStatus(ResultStatusValue.Canceled);
		public static readonly PolicyStatus FailedWithCancellation = new PolicyStatus(ResultStatusValue.FailedWithCancellation);

		internal PolicyStatus(ResultStatusValue resultStatus)
		{
			Status = resultStatus;
		}

		internal ResultStatusValue Status { get; }
	}

	public class WrappedPolicyStatus
	{
		public static readonly WrappedPolicyStatus NotExecuted = new WrappedPolicyStatus(ResultStatusValue.NotExecuted);
		public static readonly WrappedPolicyStatus NoError = new WrappedPolicyStatus(ResultStatusValue.NoError);
		public static readonly WrappedPolicyStatus PolicySuccess = new WrappedPolicyStatus(ResultStatusValue.PolicySuccess);
		public static readonly WrappedPolicyStatus Failed = new WrappedPolicyStatus(ResultStatusValue.Failed);
		public static readonly WrappedPolicyStatus Canceled = new WrappedPolicyStatus(ResultStatusValue.Canceled);
		public static readonly WrappedPolicyStatus FailedWithCancellation = new WrappedPolicyStatus(ResultStatusValue.FailedWithCancellation);
		public static readonly WrappedPolicyStatus None = new WrappedPolicyStatus(ResultStatusValue.NoneWrapped);

		internal WrappedPolicyStatus(ResultStatusValue resultStatus)
		{
			Status = resultStatus;
		}

		internal ResultStatusValue Status { get; }
	}

	internal class ResultStatusValue : IEquatable<ResultStatusValue>
	{
		public static readonly ResultStatusValue NotExecuted = new ResultStatusValue(PolicyResultStatus.NotExecuted);
		public static readonly ResultStatusValue NoError = new ResultStatusValue(PolicyResultStatus.NoError);
		public static readonly ResultStatusValue PolicySuccess = new ResultStatusValue(PolicyResultStatus.PolicySuccess);
		public static readonly ResultStatusValue Failed = new ResultStatusValue(PolicyResultStatus.Failed);
		public static readonly ResultStatusValue Canceled = new ResultStatusValue(PolicyResultStatus.Canceled);
		public static readonly ResultStatusValue FailedWithCancellation = new ResultStatusValue(PolicyResultStatus.FailedWithCancellation);

		public static readonly ResultStatusValue NoneWrapped = new ResultStatusValue(WrappedPolicyResultStatusPart.None);

		internal ResultStatusValue(PolicyResultStatus resultStatus)
		{
			Status = (int)resultStatus;
		}

		internal ResultStatusValue(WrappedPolicyResultStatusPart wrappedPolicyStatus)
		{
			Status = ((int)wrappedPolicyStatus + 1) * 100;
		}

		internal ResultStatusValue(int status)
		{
			Status = status;
		}

		internal int Status { get; }

		public bool Equals(ResultStatusValue other) => Status == other.Status;

		public override bool Equals(object obj) => obj is ResultStatusValue s && Equals(s);
		public override int GetHashCode() => Status.GetHashCode();

		public static bool operator ==(ResultStatusValue left, ResultStatusValue right) =>
			left.Equals(right);

		public static bool operator !=(ResultStatusValue left, ResultStatusValue right) =>
			!left.Equals(right);
	}

	internal enum PolicyResultStatus
	{
		NotExecuted = 0,
		NoError,
		PolicySuccess,
		Failed,
		Canceled,
		FailedWithCancellation
	}

	internal enum WrappedPolicyResultStatusPart
	{
		None = 0
	}
}

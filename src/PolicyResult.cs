﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace PoliNorError
{
	public class PolicyResult
	{
		private readonly FlexSyncEnumerable<Exception> _errors;
		private readonly FlexSyncEnumerable<CatchBlockException> _catchBlockErrors;
		private readonly FlexSyncEnumerable<PolicyResultHandlingException> _handleResultErrors;

		private Exception _unprocessedError;

		internal static PolicyResult ForSync() => new PolicyResult();
		internal static PolicyResult ForNotSync() => new PolicyResult(true);
		internal static PolicyResult InitByConfigureAwait(bool configureAwait) => !configureAwait ? ForNotSync() : ForSync();

		public PolicyResult(bool forAsync = false)
		{
			Async = forAsync;
			_errors = new FlexSyncEnumerable<Exception>(forAsync);
			FailedReason = PolicyResultFailedReason.None;
			_catchBlockErrors = new FlexSyncEnumerable<CatchBlockException>(forAsync);
			_handleResultErrors = new FlexSyncEnumerable<PolicyResultHandlingException>(forAsync);
		}

		public IEnumerable<Exception> Errors => _errors;

		public bool ErrorsNotUsed { get; internal set; }

		public IEnumerable<CatchBlockException> CatchBlockErrors => _catchBlockErrors;

		public IEnumerable<PolicyResultHandlingException> PolicyResultHandlingErrors => _handleResultErrors;

		public void SetFailed()
		{
			if (!IsFailed)
			{
				IsFailed = true;
			}
		}

		public bool IsSuccess => !IsFailed && !IsCanceled;

		/// <summary>
		/// Indicates that, despite errors during the handling, the policy handled the delegate successfully.
		/// </summary>
		public bool IsPolicySuccess => !NoError && IsSuccess;

		public bool IsCanceled { get; protected set; }

		public bool NoError { get; protected set; }

		public bool IsFailed { get; protected set; }

		public PolicyResultFailedReason FailedReason { get; internal set; }

		public bool ErrorFilterUnsatisfied { get; internal set; }

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

		public IEnumerable<PolicyDelegateResult> WrappedPolicyResults { get; internal set; }

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

	public class PolicyResult<T> : PolicyResult
	{
		internal static new PolicyResult<T> ForSync() => new PolicyResult<T>();
		internal static new PolicyResult<T> ForNotSync() => new PolicyResult<T>(true);
		internal static new PolicyResult<T> InitByConfigureAwait(bool configureAwait) => !configureAwait ? ForNotSync() : ForSync();

		public PolicyResult(bool forAsync = false) : base(forAsync){}

		internal void SetResult(T result)
		{
			Result = result;
		}

		public T Result { get; private set; }

		public new IEnumerable<PolicyDelegateResult<T>> WrappedPolicyResults { get; internal set; }

		internal override IEnumerable<PolicyDelegateResultBase> GetWrappedPolicyResults() => WrappedPolicyResults;
	}

	public enum PolicyResultFailedReason
	{
		None,
		DelegateIsNull,
		PolicyProcessorFailed,
		PolicyResultHandlerFailed,
		UnhandledError
	}
}

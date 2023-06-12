using System;
using System.Collections.Generic;

namespace PoliNorError
{
	public class PolicyResult
	{
		private FlexSyncEnumerable<Exception> _errors;
		private readonly FlexSyncEnumerable<CatchBlockException> _catchBlockErrors;
		private readonly FlexSyncEnumerable<HandlePolicyResultException> _handleResultErrors;

		public static PolicyResult ForSync() => new PolicyResult();
		public static PolicyResult ForNotSync() => new PolicyResult(true);
		public static PolicyResult InitByConfigureAwait(bool configureAwait) => !configureAwait ? ForNotSync() : ForSync();

		public PolicyResult(bool forAsync = false)
		{
			Async = forAsync;
			_errors = new FlexSyncEnumerable<Exception>(forAsync);
			_catchBlockErrors = new FlexSyncEnumerable<CatchBlockException>(forAsync);
			_handleResultErrors = new FlexSyncEnumerable<HandlePolicyResultException>(forAsync);
		}

		public IEnumerable<Exception> Errors => _errors;

		public IEnumerable<CatchBlockException> CatchBlockErrors => _catchBlockErrors;

		public IEnumerable<HandlePolicyResultException> HandleResultErrors => _handleResultErrors;

		public void SetFailed()
		{
			IsFailed = true;
		}

		public bool IsSuccess => !IsFailed && !IsCanceled;

		public bool IsCanceled { get; protected set; }

		public bool IsOk { get; protected set; }

		public bool IsFailed { get; protected set; }

		public bool ErrorFilterUnsatisfied { get; protected set; }

		public IEnumerable<PolicyDelegateResult> WrappedPolicyResults { get; internal set; }

		protected void SetErrors(IEnumerable<Exception> exceptions)
		{
			if (!(exceptions is FlexSyncEnumerable<Exception>))
			{
				throw new ArgumentException($"Collection of {nameof(exceptions)} is not type of {nameof(FlexSyncEnumerable<Exception>)}.");
			}
			_errors = (FlexSyncEnumerable<Exception>)exceptions;
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

		internal void AddHandleResultError(HandlePolicyResultException handlePolicyResultException)
		{
			_handleResultErrors.Add(handlePolicyResultException);
		}

		internal void AddHandleResultErrors(IEnumerable<HandlePolicyResultException> handlePolicyResultExceptions)
		{
			_handleResultErrors.AddRange(handlePolicyResultExceptions);
		}

		internal void SetOk()
		{
			IsOk = true;
		}

		internal void SetFailedAndCanceled()
		{
			SetCanceled();
			SetFailed();
		}

		internal void SetFailedAndFilterUnsatisfied()
		{
			ErrorFilterUnsatisfied = true;
			SetFailed();
		}

		internal void SetCanceled()
		{
			IsCanceled = true;
		}

		internal bool Async { get; }
	}

	public class PolicyResult<T> : PolicyResult
	{
		public static new PolicyResult<T> ForSync() => new PolicyResult<T>();
		public static new PolicyResult<T> ForNotSync() => new PolicyResult<T>(true);
		public static new PolicyResult<T> InitByConfigureAwait(bool configureAwait) => !configureAwait ? ForNotSync() : ForSync();

		public PolicyResult(bool forAsync = false) : base(forAsync){}

		public void SetResult(T result)
		{
			Result = result;
		}

		internal static PolicyResult<T> FromPolicyResult(PolicyResult policyResult)
		{
			var res = new PolicyResult<T>(policyResult.Async)
			{
				IsCanceled = policyResult.IsCanceled,
				IsFailed = policyResult.IsFailed
			};

			res.SetErrors(policyResult.Errors);

			return res;
		}

		public T Result { get; private set; }
	}
}

using System.Threading;

namespace PoliNorError
{
	internal class RetryErrorContext : ErrorContext<RetryContext>
	{
		private readonly bool _isPolicyAliasSet;

		public RetryErrorContext(int tryCount, bool isPolicyAliasSet = true) : this(new RetryContext(tryCount), isPolicyAliasSet){}

		public RetryErrorContext(RetryContext retryContext, bool isPolicyAliasSet = true) : base(retryContext) => _isPolicyAliasSet = isPolicyAliasSet;

		public override ProcessingErrorContext ToProcessingErrorContext()
		{
			var res = ProcessingErrorContext.FromRetry(Context.CurrentRetryCount);
			if (!_isPolicyAliasSet)
			{
				res.PolicyKind = PolicyAlias.Retry;
			}
			return res;
		}

		internal void IncrementCount() => Context.IncrementCount();

		internal void IncrementCountAtomic() => Context.IncrementCountAtomic();
	}

	internal class RetryContext
	{
		private int _currentRetryCount;

		public RetryContext(int currentRetryCount) => _currentRetryCount = currentRetryCount;

		public int CurrentRetryCount
		{
			get { return _currentRetryCount; }
		}

		internal void IncrementCount() => _currentRetryCount++;

		internal void IncrementCountAtomic() => Interlocked.Increment(ref _currentRetryCount);
	}
}

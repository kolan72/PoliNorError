using System.Threading;

namespace PoliNorError
{
	internal class RetryErrorContext : ErrorContext<RetryContext>
	{
		public RetryErrorContext(int tryCount) : this(new RetryContext(tryCount)){}

		public RetryErrorContext(RetryContext retryContext) : base(retryContext) {}

		public override ProcessingErrorContext ToProcessingErrorContext()
		{
			return new RetryProcessingErrorContext(Context.CurrentRetryCount);
		}

		internal void IncrementCount() => Context.IncrementCount();

		internal void IncrementCountAtomic() => Context.IncrementCountAtomic();
	}

	internal class RetryContext
	{
		private int _currentRetryCount;

		private readonly int _startRetryCount;

		public RetryContext(int currentRetryCount)
		{
			_startRetryCount = currentRetryCount;
			_currentRetryCount = _startRetryCount;
		}

		public bool IsZeroRetry => _currentRetryCount == _startRetryCount;

		public int CurrentRetryCount
		{
			get { return _currentRetryCount; }
		}

		internal void IncrementCount() => _currentRetryCount++;

		internal void IncrementCountAtomic() => Interlocked.Increment(ref _currentRetryCount);
	}
}

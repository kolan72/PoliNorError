using System;

namespace PoliNorError
{
	public class LinearRetryDelay : RetryDelay
    {
        private readonly TimeSpan _baseDelay;

        public LinearRetryDelay(LinearRetryDelayOptions retryDelayOptions) : this(retryDelayOptions.BaseDelay) { }

        internal LinearRetryDelay(TimeSpan baseDelay)
		{
            _baseDelay = baseDelay;
        }

        public override TimeSpan GetDelay(int attempt)
        {
            return TimeSpan.FromMilliseconds((attempt + 1) * _baseDelay.TotalMilliseconds);
        }
    }

    public class LinearRetryDelayOptions : RetryDelayOptions
    {
        public override RetryDelayType DelayType => RetryDelayType.Linear;
    }
}

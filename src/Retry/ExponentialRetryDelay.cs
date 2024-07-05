using System;

namespace PoliNorError
{
	public class ExponentialRetryDelay : RetryDelay
    {
        private readonly TimeSpan _baseDelay;
        private readonly double _exponentialFactor;

        public ExponentialRetryDelay(ExponentialRetryDelayOptions retryDelayOptions)
        {
            _baseDelay = retryDelayOptions.BaseDelay;
            _exponentialFactor = retryDelayOptions.ExponentialFactor;
        }

        public override TimeSpan GetDelay(int attempt)
        {
            return TimeSpan.FromMilliseconds(Math.Pow(_exponentialFactor, attempt) * _baseDelay.TotalMilliseconds);
        }
    }

    public class ExponentialRetryDelayOptions : RetryDelayOptions
    {
        public override RetryDelayType DelayType => RetryDelayType.Exponential;
        public double ExponentialFactor { get; set; } = 2.0;
    }
}

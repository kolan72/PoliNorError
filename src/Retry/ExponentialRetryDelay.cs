using System;

namespace PoliNorError
{
	public class ExponentialRetryDelay : RetryDelay
	{
		private readonly ExponentialRetryDelayOptions _retryDelayOptions;

		public ExponentialRetryDelay(ExponentialRetryDelayOptions retryDelayOptions)
		{
			InnerDelay = this;
			_retryDelayOptions = retryDelayOptions;
		}

		internal ExponentialRetryDelay(TimeSpan baseDelay, double exponentialFactor = 2.0) : this(new ExponentialRetryDelayOptions() { BaseDelay = baseDelay, ExponentialFactor = exponentialFactor }) {}

		protected override TimeSpan GetInnerDelay(int attempt)
		{
			return TimeSpan.FromMilliseconds(Math.Pow(_retryDelayOptions.ExponentialFactor, attempt) * _retryDelayOptions.BaseDelay.TotalMilliseconds);
		}
	}

	public class ExponentialRetryDelayOptions : RetryDelayOptions
	{
		public override RetryDelayType DelayType => RetryDelayType.Exponential;
		public double ExponentialFactor { get; set; } = 2.0;
	}
}

using System;

namespace PoliNorError
{
	/// <summary>
	///  Class to get the delay value calculated exponentially.
	/// </summary>
	public partial class ExponentialRetryDelay : RetryDelay
	{
		private readonly ExponentialRetryDelayOptions _retryDelayOptions;

		/// <summary>
		/// Initializes a new instance of <see cref="ExponentialRetryDelay"/>.
		/// </summary>
		/// <param name="retryDelayOptions"><see cref="ExponentialRetryDelayOptions"/></param>
		public ExponentialRetryDelay(ExponentialRetryDelayOptions retryDelayOptions)
		{
			InnerDelay = this;
			_retryDelayOptions = retryDelayOptions;
		}

		internal ExponentialRetryDelay(TimeSpan baseDelay, double exponentialFactor = 2.0) : this(new ExponentialRetryDelayOptions() { BaseDelay = baseDelay, ExponentialFactor = exponentialFactor }) {}

		protected override TimeSpan GetInnerDelay(int attempt)
		{
			var delay = Math.Pow(_retryDelayOptions.ExponentialFactor, attempt) * _retryDelayOptions.BaseDelay.TotalMilliseconds;
			if (delay > RetryDelayOptions.MaxTimeSpanMs)
			{
				return TimeSpan.MaxValue;
			}
			return TimeSpan.FromMilliseconds(delay);
		}
	}

	/// <summary>
	///  Represents options for the <see cref="ExponentialRetryDelay"/>.
	/// </summary>
	public class ExponentialRetryDelayOptions : RetryDelayOptions
	{
		public override RetryDelayType DelayType => RetryDelayType.Exponential;
		public double ExponentialFactor { get; set; } = 2.0;
	}
}

using System;

namespace PoliNorError
{
	/// <summary>
	///  Class to get the delay value calculated exponentially.
	/// </summary>
	public partial class ExponentialRetryDelay : RetryDelay
	{
		private readonly ExponentialRetryDelayOptions _options;

		private readonly double _adoptedMaxDelayMs;

		/// <summary>
		/// Initializes a new instance of <see cref="ExponentialRetryDelay"/>.
		/// </summary>
		/// <param name="retryDelayOptions"><see cref="ExponentialRetryDelayOptions"/></param>
		public ExponentialRetryDelay(ExponentialRetryDelayOptions retryDelayOptions)
		{
			InnerDelay = this;
			_options = retryDelayOptions;

			if (_options.UseJitter)
			{
				var dj = new DecorrelatedJitter(_options.BaseDelay, _options.ExponentialFactor);
				InnerDelayValueProvider = dj.DecorrelatedJitterBackoffV2;
			}
			else
			{
				InnerDelayValueProvider = GetDelayValue;
				_adoptedMaxDelayMs = retryDelayOptions.MaxDelay.TotalMilliseconds > RetryDelayOptions.MaxTimeSpanMs
																				? RetryDelayOptions.MaxTimeSpanMs : retryDelayOptions.MaxDelay.TotalMilliseconds;
			}
		}

		internal ExponentialRetryDelay(TimeSpan baseDelay, double exponentialFactor = 2.0, TimeSpan? maxDelay = null, bool useJitter = false) :
			this(new ExponentialRetryDelayOptions() { BaseDelay = baseDelay, ExponentialFactor = exponentialFactor, UseJitter = useJitter, MaxDelay = maxDelay ?? TimeSpan.MaxValue}) {}

		protected override TimeSpan GetInnerDelay(int attempt)
		{
			return InnerDelayValueProvider(attempt);
		}

		private TimeSpan GetDelayValue(int attempt)
		{
			var ms = Math.Pow(_options.ExponentialFactor, attempt) * _options.BaseDelay.TotalMilliseconds;
			return (ms >= _adoptedMaxDelayMs) ? _options.MaxDelay : TimeSpan.FromMilliseconds(ms);
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

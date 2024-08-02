using System;

namespace PoliNorError
{
	/// <summary>
	/// Class to get the delay value calculated linearly.
	/// </summary>
	public class LinearRetryDelay : RetryDelay
	{
		private readonly LinearRetryDelayOptions _options;

		private readonly double _adoptedMaxDelayMs;

		/// <summary>
		/// Initializes a new instance of <see cref="LinearRetryDelay"/>.
		/// </summary>
		/// <param name="retryDelayOptions"><see cref="LinearRetryDelayOptions"/></param>
		public LinearRetryDelay(LinearRetryDelayOptions retryDelayOptions)
		{
			InnerDelay = this;
			_options = retryDelayOptions;

			if (_options.UseJitter)
			{
				InnerDelayValueProvider = GetJitteredDelayValue;
			}
			else
			{
				InnerDelayValueProvider = GetDelayValue;
				_adoptedMaxDelayMs = retryDelayOptions.MaxDelay.TotalMilliseconds > RetryDelayConstants.MaxTimeSpanMs
																? RetryDelayConstants.MaxTimeSpanMs : retryDelayOptions.MaxDelay.TotalMilliseconds;
			}
		}

		internal LinearRetryDelay(TimeSpan baseDelay, TimeSpan? maxDelay = null, bool useJitter = false) : this(new LinearRetryDelayOptions() { BaseDelay = baseDelay, UseJitter = useJitter, MaxDelay = maxDelay ?? TimeSpan.MaxValue } ) {}

		protected override TimeSpan GetInnerDelay(int attempt)
		{
			 return InnerDelayValueProvider(attempt);
		}

		private TimeSpan GetDelayValue(int attempt)
		{
			var ms = GetDelayValueInMs(attempt);
			return (ms >= _adoptedMaxDelayMs) ? _options.MaxDelay : TimeSpan.FromMilliseconds(ms);
		}

		private TimeSpan GetJitteredDelayValue(int attempt)
		{
			var ms = ApplyJitter(GetDelayValueInMs(attempt));
			return ms >= RetryDelayConstants.MaxTimeSpanMs ? TimeSpan.MaxValue : TimeSpan.FromMilliseconds(ms);
		}

		private double GetDelayValueInMs(int attempt)
		{
			return (attempt + 1) * _options.BaseDelay.TotalMilliseconds;
		}
	}

	/// <summary>
	/// Represents options for the <see cref="LinearRetryDelay"/>.
	/// </summary>
	public class LinearRetryDelayOptions : RetryDelayOptions
	{
		public override RetryDelayType DelayType => RetryDelayType.Linear;
	}
}

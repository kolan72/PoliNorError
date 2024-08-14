using System;

namespace PoliNorError
{
	/// <summary>
	/// Class to get the delay value calculated linearly.
	/// </summary>
	public class LinearRetryDelay : RetryDelay
	{
		private readonly LinearRetryDelayOptions _options;

		private readonly MaxDelayDelimiter _maxDelayDelimiter;

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
			}
			_maxDelayDelimiter = new MaxDelayDelimiter(retryDelayOptions);
		}

		/// <summary>
		///  Creates <see cref="LinearRetryDelay"/>.
		/// </summary>
		/// <param name="baseDelay">Base delay value between retries.</param>
		/// <param name="maxDelay">Maximum delay value.</param>
		/// <param name="useJitter">Whether jitter is used.</param>
		/// <returns></returns>
		public static LinearRetryDelay Create(TimeSpan baseDelay, TimeSpan? maxDelay = null, bool useJitter = false) => new LinearRetryDelay(baseDelay, maxDelay, useJitter);

		internal LinearRetryDelay(TimeSpan baseDelay, TimeSpan? maxDelay = null, bool useJitter = false) : this(new LinearRetryDelayOptions() { BaseDelay = baseDelay, UseJitter = useJitter, MaxDelay = maxDelay ?? TimeSpan.MaxValue } ) {}

		private TimeSpan GetDelayValue(int attempt)
		{
			return _maxDelayDelimiter.GetDelayLimitedToMaxDelayIfNeed(GetDelayValueInMs(attempt));
		}

		private TimeSpan GetJitteredDelayValue(int attempt)
		{
			return _maxDelayDelimiter.GetDelayLimitedToMaxDelayIfNeed(ApplyJitter(GetDelayValueInMs(attempt)));
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

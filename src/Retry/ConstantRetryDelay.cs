using System;

namespace PoliNorError
{
	/// <summary>
	/// Class to get the constant delay value.
	/// </summary>
	public class ConstantRetryDelay : RetryDelay
	{
		private readonly ConstantRetryDelayOptions _options;

		private readonly MaxDelayDelimiter _maxDelayDelimiter;

		/// <summary>
		/// Initializes a new instance of <see cref="ConstantRetryDelay"/>.
		/// </summary>
		/// <param name="retryDelayOptions"><see cref="ConstantRetryDelayOptions"/></param>
		public ConstantRetryDelay(ConstantRetryDelayOptions retryDelayOptions)
		{
			InnerDelay = this;
			_options = retryDelayOptions;

			if (_options.UseJitter)
			{
				if (_options.MaxDelay < _options.BaseDelay)
					throw new ArgumentOutOfRangeException(nameof(retryDelayOptions), "MaxDelay must be greater than or equal to BaseDelay.");

				InnerDelayValueProvider = GetJitteredDelayValue;
				_maxDelayDelimiter = new MaxDelayDelimiter(retryDelayOptions);
			}
			else
			{
				InnerDelayValueProvider = GetDelayValue;
			}
		}

		internal ConstantRetryDelay(TimeSpan baseDelay, TimeSpan? maxDelay = null, bool useJitter = false) : this(new ConstantRetryDelayOptions() { BaseDelay = baseDelay, UseJitter = useJitter, MaxDelay = maxDelay ?? TimeSpan.MaxValue }){}

		private TimeSpan GetDelayValue(int attempt)
		{
			return _options.BaseDelay;
		}

		private TimeSpan GetJitteredDelayValue(int attempt)
		{
			return _maxDelayDelimiter.GetDelayLimitedToMaxDelayIfNeed(ApplyJitter(GetDelayValueInMs()));
		}

		private double GetDelayValueInMs() => _options.BaseDelay.TotalMilliseconds;
	}

	/// <summary>
	/// Represents options for the <see cref="ConstantRetryDelay"/>.
	/// </summary>
	public class ConstantRetryDelayOptions : RetryDelayOptions
	{
		public override RetryDelayType DelayType => RetryDelayType.Constant;
	}
}

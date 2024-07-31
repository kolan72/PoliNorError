using System;

namespace PoliNorError
{
	/// <summary>
	/// Class to get the constant delay value.
	/// </summary>
	public class ConstantRetryDelay : RetryDelay
	{
		private readonly ConstantRetryDelayOptions _options;

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
				InnerDelayValueProvider = GetJitteredDelayValue;
			}
			else
			{
				InnerDelayValueProvider = GetDelayValue;
			}
		}

		internal ConstantRetryDelay(TimeSpan baseDelay, bool useJitter = false) : this(new ConstantRetryDelayOptions() { BaseDelay = baseDelay, UseJitter = useJitter }){}

		protected override TimeSpan GetInnerDelay(int attempt)
		{
			return InnerDelayValueProvider(attempt);
		}

		private TimeSpan GetDelayValue(int attempt)
		{
			return _options.BaseDelay;
		}

		private TimeSpan GetJitteredDelayValue(int attempt)
		{
			var ms = ApplyJitter(GetDelayValueInMs());
			return ms >= RetryDelayConstants.MaxTimeSpanMs ? TimeSpan.MaxValue : TimeSpan.FromMilliseconds(ms);
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

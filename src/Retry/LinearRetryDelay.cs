using System;

namespace PoliNorError
{
	/// <summary>
	/// Class to get the delay value calculated linearly.
	/// </summary>
	public class LinearRetryDelay : RetryDelay
	{
		private readonly LinearRetryDelayOptions _options;

		/// <summary>
		/// Initializes a new instance of <see cref="LinearRetryDelay"/>.
		/// </summary>
		/// <param name="retryDelayOptions"><see cref="LinearRetryDelayOptions"/></param>
		public LinearRetryDelay(LinearRetryDelayOptions retryDelayOptions)
		{
			InnerDelay = this;
			_options = retryDelayOptions;
		}

		internal LinearRetryDelay(TimeSpan baseDelay) : this(new LinearRetryDelayOptions() { BaseDelay = baseDelay }) {}

		protected override TimeSpan GetInnerDelay(int attempt)
		{
			var delay = (attempt + 1) * _options.BaseDelay.TotalMilliseconds;
			if (delay > RetryDelayOptions.MaxTimeSpanMs)
			{
				return TimeSpan.MaxValue;
			}
			return TimeSpan.FromMilliseconds(delay);
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

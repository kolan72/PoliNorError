using System;

namespace PoliNorError
{
	/// <summary>
	/// Class to get the delay value calculated linearly.
	/// </summary>
	public class LinearRetryDelay : RetryDelay
	{
		private readonly TimeSpan _baseDelay;

		/// <summary>
		/// Initializes a new instance of <see cref="LinearRetryDelay"/>.
		/// </summary>
		/// <param name="retryDelayOptions"><see cref="LinearRetryDelayOptions"/></param>
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

	/// <summary>
	/// Represents options for the <see cref="LinearRetryDelay"/>.
	/// </summary>
	public class LinearRetryDelayOptions : RetryDelayOptions
	{
		public override RetryDelayType DelayType => RetryDelayType.Linear;
	}
}
